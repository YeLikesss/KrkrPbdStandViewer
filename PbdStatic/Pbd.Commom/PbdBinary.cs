using System;
using System.Buffers.Binary;
using System.IO;
using Pbd.Crypto;

namespace Pbd.Commom
{
    /// <summary>
    /// pbd-TJS数据
    /// </summary>
    internal class PbdBinary
    {
        private readonly byte[] mData;          //TJS数据
        private readonly PbdFile mPbd;          //pbd信息

        /// <summary>
        /// 尝试获取TJS对象
        /// </summary>
        /// <param name="v">返回TJS对象</param>
        /// <returns>True校验成功 False校验失败</returns>
        public unsafe bool TryGetTJSVariant(out TJSVariant v)
        {
            using MemoryStream stream = new(this.mData, false);

            PbdByteChecker checker = new(this.mPbd.Seed);
            TJSDeserializer tjs = new(stream, this.mPbd.BigEndian, checker.CheckByte);
            v = tjs.Deserialize();

            //最终校验
            uint checkSum;
            stream.Read(new Span<byte>(&checkSum, sizeof(uint)));
            if (this.mPbd.BigEndian)
            {
                checkSum = BinaryPrimitives.ReverseEndianness(checkSum);
            }
            return checker.CheckFinal(checkSum) || this.mPbd.NoCheck;
        }

        private PbdBinary(byte[] data, PbdFile info)
        {
            this.mData = data;
            this.mPbd = info;
        }

        /// <summary>
        /// 创建Pbd
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="param">游戏信息</param>
        public static PbdBinary? Create(Stream stream, PbdCustomParams param)
        {
            PbdFile? pbd = PbdFile.Create(stream);
            if (pbd is null)
            {
                return null;
            }
            pbd.SetCustomParams(param);

            //获取数据
            byte[] rawData = new byte[stream.Length - stream.Position];
            stream.Read(rawData);

            //解密数据
            IPbdCryptFilter? filter = PbdCrypto.Create(pbd);
            filter?.Decrypt(rawData);

            //解压
            rawData = pbd.CompressMode switch
            {
                PbdCompress.Lz4 => PbdCompression.DecompressLz4(rawData),
                _ => rawData,
            };

            if (rawData.Length == 0)
            {
                return null;
            }

            return new(rawData, pbd);
        }
    }
}
