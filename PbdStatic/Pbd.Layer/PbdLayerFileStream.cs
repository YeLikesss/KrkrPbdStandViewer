using System.Collections.Generic;
using System.IO;

namespace Pbd.Layer
{
    internal class PbdLayerFileStream
    {
        private static readonly List<string> smExtension = new()
        {
            ".bmp", ".png", ".webp", ".tiff", ".tif", ".tga",
        };

        /// <summary>
        /// 尝试打开文件流
        /// </summary>
        /// <param name="fullnameNoExtension">不带后缀的全路径</param>
        public static FileStream? OpenStream(string fullnameNoExtension)
        {
            foreach(string s in PbdLayerFileStream.smExtension)
            {
                string path = fullnameNoExtension + s;
                if (File.Exists(path))
                {
                    return File.OpenRead(path);
                }
            }
            return null;
        }
    }
}
