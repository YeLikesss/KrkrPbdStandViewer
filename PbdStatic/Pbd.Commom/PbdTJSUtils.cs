using System;
using System.IO;

namespace Pbd.Commom
{
    public class PbdTJSUtils
    {
        const ulong Signature = 0x6E6942534A547359u;  //YsTJSBin

        /// <summary>
        /// 转换TJS格式
        /// </summary>
        /// <param name="inputDirectory">输入待遍历文件夹</param>
        /// <param name="customParams">游戏参数</param>
        /// <param name="msgCB">消息回调</param>
        public static void Convert(string inputDirectory, PbdCustomParams customParams, IProgress<string>? msgCB)
        {
            string[] files = Directory.GetFiles(inputDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string path in files)
            {
                string relativePath = path[(inputDirectory.Length + 1)..];

                using FileStream fs = File.OpenRead(path);
                if (PbdBinary.Create(fs, customParams) is PbdBinary bin)
                {
                    if (bin.TryGetTJSVariant(out TJSVariant v))
                    {
                        string outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Convert_Export", relativePath);
                        {
                            string dir = Path.GetDirectoryName(outPath)!;
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                        }

                        using FileStream outFs = File.Create(outPath);
                        using BinaryWriter outBw = new(outFs);
                        outBw.Write(PbdTJSUtils.Signature);

                        TJSSerializer serializer = new(outFs);
                        serializer.Serialize(v);

                        outFs.Flush();

                        msgCB?.Report($"转换成功: {relativePath}");
                    }
                    else
                    {
                        msgCB?.Report($"立绘文件TJS解析失败: {relativePath}");
                    }
                }
                else
                {
                    msgCB?.Report($"跳过非立绘文件: {relativePath}");
                }
            }
        }
    }
}
