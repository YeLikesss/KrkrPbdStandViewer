using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using K4os.Compression.LZ4;

namespace Pbd.Commom
{
    /// <summary>
    /// 压缩方式
    /// </summary>
    internal enum PbdCompress : int
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow = -1,
        /// <summary>
        /// 原始
        /// </summary>
        Raw = 0,
        /// <summary>
        /// Lz4压缩
        /// </summary>
        Lz4 = 1,
    }

    internal class PbdCompression
    {
        /// <summary>
        /// Lz4解压缩
        /// </summary>
        /// <param name="rawData">原数据</param>
        public static byte[] DecompressLz4(byte[] rawData)
        {
            byte[] dictionaryBuffer = ArrayPool<byte>.Shared.Rent(0x100000);    //1MB
            byte[] compressBuffer = ArrayPool<byte>.Shared.Rent(0x100000);      //1MB
            byte[] decompressBuffer = ArrayPool<byte>.Shared.Rent(0x100000);    //1MB

            //原始数据
            using MemoryStream output = new(0x100000);          //1MB

            using MemoryStream inMs = new(rawData, false);
            using BinaryReader inBr = new(inMs);

            Span<byte> dictionaryBuf = dictionaryBuffer;
            Span<byte> encodeBuf = compressBuffer;
            Span<byte> decodeBuf = decompressBuffer;

            int encLen = 0;     //当前块压缩长度
            int decLen = 0;     //当前块解压缩长度

            try
            {
                while (inMs.Position < inMs.Length)
                {
                    encLen = inBr.ReadUInt16();       //读取长度(2字节)

                    Span<byte> encMem = encodeBuf[..encLen];
                    inMs.Read(encMem);

                    //Lz4解压
                    decLen = LZ4Codec.Decode(encMem, decodeBuf, dictionaryBuf[..decLen]);

                    Span<byte> decMem = decodeBuf[..decLen];
                    output.Write(decMem);

                    //当前结果作为下一次解压字典
                    decMem.CopyTo(dictionaryBuf[..decLen]);
                }
                return output.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Array.Empty<byte>();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(dictionaryBuffer);
                ArrayPool<byte>.Shared.Return(compressBuffer);
                ArrayPool<byte>.Shared.Return(decompressBuffer);
            }
        }
    }
}
