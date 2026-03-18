using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Pbd.Crypto
{
    /// <summary>
    /// Pbd Chacha核心
    /// </summary>
    internal class PbdChachaCore
    {
        private static readonly byte[] Sigma = Encoding.UTF8.GetBytes("expand 32-byte k");

        private readonly byte[] mState = new byte[64];      //状态
        private int mRound;                                 //轮数

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="key">加密key 32字节</param>
        /// <param name="round">加密轮数</param>
        /// <param name="nonce">加密变量</param>
        /// <param name="counter">计数</param>
        public void Initialize(in ReadOnlySpan<byte> key, int round, ulong nonce, ulong counter = 0ul)
        {
            this.mRound = round;

            //[0:15]    常量
            //[16:47]   密钥
            //[48:55]   计数
            //[56:63]   加密变量

            Span<byte> state = this.mState;

            PbdChachaCore.Sigma.AsSpan().CopyTo(state[0..16]);
            key.CopyTo(state[16..48]);
            BitConverter.TryWriteBytes(state[56..64], nonce);

            this.SetCounter(counter);
        }

        /// <summary>
        /// 设置计数器
        /// </summary>
        /// <param name="counter">计数</param>
        public void SetCounter(ulong counter)
        {
            Span<byte> state = this.mState;
            BitConverter.TryWriteBytes(state[48..56], counter);
        }

        /// <summary>
        /// 变换
        /// </summary>
        /// <param name="output">输出</param>
        /// <param name="counter">计数器</param>
        public void Transform(in Span<byte> output, ulong counter)
        {
            this.SetCounter(counter);

            Span<uint> src = MemoryMarshal.Cast<byte, uint>(this.mState);
            Span<uint> dst = MemoryMarshal.Cast<byte, uint>(output);
            uint z0, z1, z2, z3, z4, z5, z6, z7, z8, z9, za, zb, zc, zd, ze, zf;
            z0 = src[0];
            z1 = src[1];
            z2 = src[2];
            z3 = src[3];
            z4 = src[4];
            z5 = src[5];
            z6 = src[6];
            z7 = src[7];
            z8 = src[8];
            z9 = src[9];
            za = src[10];
            zb = src[11];
            zc = src[12];
            zd = src[13];
            ze = src[14];
            zf = src[15];

            for (int i = 0; i < this.mRound; i += 2)
            {
                // QUARTER(z0, z4, z8, zc);
                z0 += z4; zc = BitOperations.RotateLeft(zc ^ z0, 16);
                z8 += zc; z4 = BitOperations.RotateLeft(z4 ^ z8, 12);
                z0 += z4; zc = BitOperations.RotateLeft(zc ^ z0, 8);
                z8 += zc; z4 = BitOperations.RotateLeft(z4 ^ z8, 7);
                // QUARTER(z1, z5, z9, zd);
                z1 += z5; zd = BitOperations.RotateLeft(zd ^ z1, 16);
                z9 += zd; z5 = BitOperations.RotateLeft(z5 ^ z9, 12);
                z1 += z5; zd = BitOperations.RotateLeft(zd ^ z1, 8);
                z9 += zd; z5 = BitOperations.RotateLeft(z5 ^ z9, 7);
                // QUARTER(z2, z6, za, ze);
                z2 += z6; ze = BitOperations.RotateLeft(ze ^ z2, 16);
                za += ze; z6 = BitOperations.RotateLeft(z6 ^ za, 12);
                z2 += z6; ze = BitOperations.RotateLeft(ze ^ z2, 8);
                za += ze; z6 = BitOperations.RotateLeft(z6 ^ za, 7);
                // QUARTER(z3, z7, zb, zf);
                z3 += z7; zf = BitOperations.RotateLeft(zf ^ z3, 16);
                zb += zf; z7 = BitOperations.RotateLeft(z7 ^ zb, 12);
                z3 += z7; zf = BitOperations.RotateLeft(zf ^ z3, 8);
                zb += zf; z7 = BitOperations.RotateLeft(z7 ^ zb, 7);
                // QUARTER(z0, z5, za, zf);
                z0 += z5; zf = BitOperations.RotateLeft(zf ^ z0, 16);
                za += zf; z5 = BitOperations.RotateLeft(z5 ^ za, 12);
                z0 += z5; zf = BitOperations.RotateLeft(zf ^ z0, 8);
                za += zf; z5 = BitOperations.RotateLeft(z5 ^ za, 7);
                // QUARTER(z1, z6, zb, zc);
                z1 += z6; zc = BitOperations.RotateLeft(zc ^ z1, 16);
                zb += zc; z6 = BitOperations.RotateLeft(z6 ^ zb, 12);
                z1 += z6; zc = BitOperations.RotateLeft(zc ^ z1, 8);
                zb += zc; z6 = BitOperations.RotateLeft(z6 ^ zb, 7);
                // QUARTER(z2, z7, z8, zd);
                z2 += z7; zd = BitOperations.RotateLeft(zd ^ z2, 16);
                z8 += zd; z7 = BitOperations.RotateLeft(z7 ^ z8, 12);
                z2 += z7; zd = BitOperations.RotateLeft(zd ^ z2, 8);
                z8 += zd; z7 = BitOperations.RotateLeft(z7 ^ z8, 7);
                // QUARTER(z3, z4, z9, ze);
                z3 += z4; ze = BitOperations.RotateLeft(ze ^ z3, 16);
                z9 += ze; z4 = BitOperations.RotateLeft(z4 ^ z9, 12);
                z3 += z4; ze = BitOperations.RotateLeft(ze ^ z3, 8);
                z9 += ze; z4 = BitOperations.RotateLeft(z4 ^ z9, 7);
            }

            dst[0] = z0;
            dst[1] = z1;
            dst[2] = z2;
            dst[3] = z3;
            dst[4] = z4;
            dst[5] = z5;
            dst[6] = z6;
            dst[7] = z7;
            dst[8] = z8;
            dst[9] = z9;
            dst[10] = za;
            dst[11] = zb;
            dst[12] = zc;
            dst[13] = zd;
            dst[14] = ze;
            dst[15] = zf;

            for (int i = 0; i < 16; ++i)
            {
                dst[i] += src[i];
            }
        }
    }
}
