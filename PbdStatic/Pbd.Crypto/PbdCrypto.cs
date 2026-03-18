using System;
using System.Runtime.InteropServices;
using K4os.Hash.xxHash;
using Blake2Fast;
using Blake2Fast.Implementation;
using Pbd.Commom;

namespace Pbd.Crypto
{
    /// <summary>
    /// 加密接口
    /// </summary>
    internal interface IPbdCryptFilter
    {
        /// <summary>
        /// 解密
        /// </summary>
        void Decrypt(in Span<byte> data);
    }

    /// <summary>
    /// 基本加密
    /// </summary>
    internal abstract class PbdBaseFilter : IPbdCryptFilter
    {
        public abstract void Decrypt(in Span<byte> data);
        /// <summary>
        /// 变换
        /// </summary>
        protected abstract void Transform(in Span<byte> table);
    }

    /// <summary>
    /// Xor加密
    /// </summary>
    internal class PbdXorFilter : PbdBaseFilter
    {
        private readonly byte[] mTable;      //解密表
        private long mPosition;              //解密表位置

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">表大小</param>
        public PbdXorFilter(long size)
        {
            this.mTable = new byte[size];
            this.mPosition = size;
        }

        public override void Decrypt(in Span<byte> data)
        {
            byte[] tbl = this.mTable;
            for (int i = 0; i < data.Length; ++i)
            {
                if (this.mPosition >= tbl.LongLength)
                {
                    this.Transform(tbl);
                    this.mPosition = 0L;
                }
                data[i] ^= tbl[this.mPosition];
                ++this.mPosition;
            }
        }
        protected override void Transform(in Span<byte> table)
        {
        }
    }

    /// <summary>
    /// Chacha加密
    /// </summary>
    internal class PbdChachaFilter: PbdXorFilter
    {
        private readonly int mTblCount;               //表个数
        private readonly uint mSeed;                  //种子
        private ulong mCounter;                       //变换计数器
        private readonly PbdChachaCore mChachaCore;   //加密核

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed">种子</param>
        /// <param name="iv">加密向量</param>
        /// <param name="round">加密轮数</param>
        /// <param name="tblCount">表个数</param>
        public PbdChachaFilter(uint seed, in ReadOnlySpan<byte> iv, int round, int tblCount)
            :base(tblCount * 64L)
        {
            this.mTblCount = tblCount;
            this.mSeed = 0xFFFFFFFFu;
            this.mCounter = 0ul;

            //生成key
            Span<byte> key = stackalloc byte[32];
            PbdChachaFilter.GenerateKey(key, iv, seed);

            //生成nonce
            uint nonce_lo = PbdChachaFilter.GenerateSeed(iv, seed);
            if (tblCount > 1)
            {
                if (nonce_lo != seed)
                {
                    this.mSeed = nonce_lo ^ seed;
                }
                else if (seed != 0u)
                {
                    this.mSeed = seed;
                }
            }

            PbdChachaCore chacha = new();
            chacha.Initialize(key, round, ((ulong)seed << 32) | nonce_lo);

            this.mChachaCore = chacha;
        }

        protected override void Transform(in Span<byte> table)
        {
            //Chacha变换
            this.mChachaCore.Transform(table, this.mCounter);
            ++this.mCounter;

            //Chacha结果表生成所有表
            int tblCount = this.mTblCount;
            if (tblCount > 1)
            {
                Span<uint> src = MemoryMarshal.Cast<byte, uint>(table);
                Span<uint> dst = MemoryMarshal.Cast<byte, uint>(table[64..]);

                for (int i = 0; i < (tblCount - 1) * 16; ++i)
                {
                    uint s = src[i];
                    s = (s << 13 ^ s) >> 17 ^ s << 13 ^ s;
                    s = 32 * s ^ s;
                    s = s == 0 ? this.mSeed : s;
                    dst[i] = s;
                }
            }
        }

        /// <summary>
        /// 生成Key
        /// </summary>
        /// <param name="retValue">32字节Key 返回值</param>
        /// <param name="iv"></param>
        /// <param name="seed"></param>
        /// <returns>True成功 False失败</returns>
        private static unsafe bool GenerateKey(in Span<byte> retValue, in ReadOnlySpan<byte> iv, uint seed)
        {
            Blake2sHashState ctx = Blake2s.CreateIncrementalHasher(32, new ReadOnlySpan<byte>(&seed, sizeof(uint)));
            ctx.Update(iv);
            return ctx.TryFinish(retValue, out _);
        }

        /// <summary>
        /// 生成种子
        /// </summary>
        /// <param name="iv"></param>
        /// <param name="seed"></param>
        private static uint GenerateSeed(in ReadOnlySpan<byte> iv, uint seed)
        {
            XXH32.State ctx = new();
            XXH32.Reset(ref ctx, seed);
            XXH32.Update(ref ctx, iv);
            return XXH32.Digest(ctx);
        }
    }


    /// <summary>
    /// Pbd加密环境
    /// </summary>
    internal class PbdCrypto
    {
        public static IPbdCryptFilter? Create(PbdFile pbd)
        {
            uint seed = pbd.Seed;
            Span<byte> iv = pbd.GetIV();
            return pbd.CryptoMode switch
            {
                1u => new PbdChachaFilter(seed, iv, 8, 16),
                2u => new PbdChachaFilter(seed, iv, 12, 8),
                3u => new PbdChachaFilter(seed, iv, 20, 4),
                4u => new PbdChachaFilter(seed, iv, 8, 1),
                5u => new PbdChachaFilter(seed, iv, 12, 1),
                6u => new PbdChachaFilter(seed, iv, 20, 1),
                _ => null,
            };
        }
    }
}
