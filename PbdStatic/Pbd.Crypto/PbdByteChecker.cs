using System;

namespace Pbd.Crypto
{
    /// <summary>
    /// Pbd校验
    /// </summary>
    internal class PbdByteChecker
    {
        /// <summary>
        /// 检查类型
        /// <para>检查每个字节 == 1</para>
        /// <para>检查最终结果 == 0</para>
        /// <para>检查失败 == -1</para>
        /// </summary>
        private int mFlag;
        private readonly byte[] mSeed = new byte[3];

        /// <summary>
        /// 计算轮
        /// </summary>
        /// <param name="seed">3字节种子</param>
        private static byte Round(Span<byte> seed)
        {
            byte a, b;
            b = a = (byte)(seed[0] ^ (byte)(seed[0] * 2));

            b >>= 2;
            b ^= seed[2];
            b >>= 3;
            b ^= seed[2];
            b ^= a;

            seed[0] = seed[1];
            seed[1] = seed[2];
            seed[2] = b;

            return b;
        }

        /// <summary>
        /// 最后变换
        /// </summary>
        private unsafe void Final()
        {
            Span<byte> seed = this.mSeed;
            PbdByteChecker.Round(seed);
            PbdByteChecker.Round(seed);
            PbdByteChecker.Round(seed);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        private byte Update(byte code)
        {
            if (this.mFlag == 0)
            {
                return 0;       //不检查单字节
            }

            Span<byte> seed = this.mSeed;
            if (code == 0)
            {
                return seed[2];
            }
            else
            {
                return PbdByteChecker.Round(seed);
            }
        }

        /// <summary>
        /// 校验当前字节
        /// </summary>
        /// <param name="code">待检查字节</param>
        /// <param name="chkValue">校验值</param>
        /// <returns>True校验成功 False校验失败</returns>
        public bool CheckByte(byte code, byte chkValue)
        {
            byte chksum = this.Update(code);
            if (this.mFlag == 0 || chkValue == chksum)
            {
                return true;
            }
            else
            {
                this.mFlag = -1;
                return false;
            }
        }

        /// <summary>
        /// 检查校验
        /// </summary>
        /// <param name="checkSum">校验和</param>
        /// <returns>True校验成功 False校验失败</returns>
        public unsafe bool CheckFinal(uint checkSum)
        {
            this.Final();
            ReadOnlySpan<byte> seed = this.mSeed;

            uint checkFin = 0u;
            Span<byte> p = new(&checkFin, sizeof(uint));
            p[0] = seed[2];
            p[1] = seed[1];
            p[2] = seed[0];

            return this.mFlag >= 0 && checkSum == checkFin;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="seed">种子</param>
        public PbdByteChecker(uint seed)
        {
            Span<byte> p = this.mSeed;
            p[0] = (byte)((seed >> 24) ^ seed);
            p[1] = (byte)(seed >> 8);
            p[2] = (byte)(seed >> 16);

            this.mFlag = 1;
        }
    }
}
