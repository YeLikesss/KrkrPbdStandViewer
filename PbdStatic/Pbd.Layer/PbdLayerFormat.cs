using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pbd.Layer
{
    /// <summary>
    /// 格式类型
    /// </summary>
    public enum PbdFormat
    {
        Webp,
        Png,
        Bmp,
        Tga,
    }

    public class PbdLayerFormat
    {
        private static readonly Dictionary<PbdFormat, ImageEncoder> smEncodeProvider;
        private static readonly Dictionary<PbdFormat, string> smExtensionProvider;

        static PbdLayerFormat()
        {
            {
                Dictionary<PbdFormat, ImageEncoder> encodes = new(8);
                WebpEncoder webp = new()
                {
                    FileFormat = WebpFileFormatType.Lossless,
                    FilterStrength = 0,
                    Method = WebpEncodingMethod.BestQuality,
                    NearLossless = false,
                    Quality = 100,
                    TransparentColorMode = WebpTransparentColorMode.Clear,
                    SkipMetadata = true,
                };
                PngEncoder png = new()
                {
                    BitDepth = PngBitDepth.Bit8,
                    ColorType = PngColorType.RgbWithAlpha,
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    TransparentColorMode = PngTransparentColorMode.Preserve,
                    SkipMetadata = true,
                };
                TgaEncoder tga = new()
                {
                    BitsPerPixel = TgaBitsPerPixel.Pixel32,
                    Compression = TgaCompression.RunLength,
                    SkipMetadata = true,
                };
                BmpEncoder bmp = new()
                {
                    BitsPerPixel = BmpBitsPerPixel.Pixel32,
                    SupportTransparency = true,
                    SkipMetadata = true,
                };
                encodes.Add(PbdFormat.Webp, webp);
                encodes.Add(PbdFormat.Png, png);
                encodes.Add(PbdFormat.Tga, tga);
                encodes.Add(PbdFormat.Bmp, bmp);
                PbdLayerFormat.smEncodeProvider = encodes;
            }

            {
                Dictionary<PbdFormat, string> extensions = new(8)
                {
                    { PbdFormat.Webp, ".webp" },
                    { PbdFormat.Png, ".png" },
                    { PbdFormat.Tga, ".tga" },
                    { PbdFormat.Bmp, ".bmp" },
                };
                PbdLayerFormat.smExtensionProvider = extensions;
            }
        }


        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="img">图片对象</param>
        /// <param name="fmt">格式</param>
        /// <param name="directory">文件夹</param>
        /// <param name="name">名字</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Save(Image<Bgra32> img, PbdFormat fmt, string directory, string name)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("文件夹为空", nameof(directory));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("名称为空", nameof(name));
            }
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string filename = name + PbdLayerFormat.smExtensionProvider[fmt];
            string path = Path.Combine(directory, filename);

            img.Save(path, PbdLayerFormat.smEncodeProvider[fmt]);
        }
    }
}
