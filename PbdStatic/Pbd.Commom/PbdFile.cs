using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Buffers.Binary;

namespace Pbd.Commom
{
    /// <summary>
    /// Pbd文件信息
    /// </summary>
    internal class PbdFile
    {
        /// <summary>
        /// 不检查标记
        /// </summary>
        public bool NoCheck { get; private set; }

        /// <summary>
        /// 检查失败标记
        /// </summary>
        public bool CheckFail { get; private set; }

        /// <summary>
        /// 获取大端模式
        /// </summary>
        public bool BigEndian { get; private set; }

        /// <summary>
        /// 压缩模式
        /// </summary>
        public PbdCompress CompressMode { get; private set; }

        /// <summary>
        /// 种子
        /// </summary>
        public uint Seed { get; private set; }

        /// <summary>
        /// 加密模式
        /// </summary>
        public uint CryptoMode { get; private set; }

        /// <summary>
        /// 文件加密向量
        /// </summary>
        public byte[] FileIV { get; private set; } = Array.Empty<byte>();

        /// <summary>
        /// 客户加密向量
        /// </summary>
        public byte[] OuterIV { get; private set; } = Array.Empty<byte>();

        /// <summary>
        /// 信息大小
        /// </summary>
        public long HdrSize { get; private set; } = 0;

        /// <summary>
        /// 设置用户参数
        /// </summary>
        public void SetCustomParams(PbdCustomParams param)
        {
            this.OuterIV = param.CustomIV;
            this.NoCheck = param.NoCheck;
        }

        /// <summary>
        /// 获取IV
        /// </summary>
        public byte[] GetIV()
        {
            if (this.OuterIV.Length != 0)
            {
                return this.OuterIV;
            }
            else
            {
                return this.FileIV;
            }
        }

        private PbdFile()
        {
        }

        /// <summary>
        /// 创建Pbd文件信息
        /// </summary>
        /// <param name="stream">流</param>
        public static PbdFile? Create(Stream stream)
        {
            PbdFile pbd = new();
            Span<byte> hdr = stackalloc byte[16];

            long start = stream.Position;
            stream.Read(hdr);

            //标记1
            uint sign = MemoryMarshal.Read<uint>(hdr[0..4]);
            if (sign == 0x5C534A54u)       // TJS\
            {
                pbd.BigEndian = true;
            }
            else if (sign == 0x2F534A54u)  // TJS/
            {
                pbd.BigEndian = false;
            }
            else
            {
                return null;
            }

            //标记2
            if (hdr[5] != 0x73 || hdr[6] != 0x30 || hdr[7] != 0x00)
            {
                return null;
            }

            //压缩   
            pbd.CompressMode = hdr[4] switch
            {
                0x6E => PbdCompress.Raw,
                0x34 => PbdCompress.Lz4,
                _ => PbdCompress.Unknow,
            };
            if (pbd.CompressMode == PbdCompress.Unknow)
            {
                return null;
            }

            //种子
            {
                uint seed = MemoryMarshal.Read<uint>(hdr[8..12]);
                pbd.Seed = pbd.BigEndian ? BinaryPrimitives.ReverseEndianness(seed) : seed;
            }

            //加密
            {
                ushort cryptoMode = MemoryMarshal.Read<ushort>(hdr[12..14]);
                pbd.CryptoMode = pbd.BigEndian ? BinaryPrimitives.ReverseEndianness(cryptoMode) : cryptoMode;
            }

            //文件IV
            {
                ushort ivLen = MemoryMarshal.Read<ushort>(hdr[14..16]);
                ivLen = pbd.BigEndian ? BinaryPrimitives.ReverseEndianness(ivLen) : ivLen;
                if (ivLen != 0)
                {
                    pbd.FileIV = new byte[ivLen];
                    stream.Read(pbd.FileIV);
                }
            }

            pbd.CheckFail = false;
            pbd.HdrSize = stream.Position - start;

            return pbd;
        }
    }
}