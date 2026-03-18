using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Pbd.Commom;
using Pbd.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pbd.Layer
{
    /// <summary>
    /// 图层类型
    /// Krkr tTVPLayerType [ltXXXXX]
    /// </summary>
    public enum PbdLayerType
    {
        Unknow = -1,      //未知类型

        Binder = 0,
        //CoverRect = 1,
        Opaque = 1, // the same as ltCoverRect
        //Transparent = 2, // alpha blend
        Alpha = 2, // the same as ltTransparent
        Additive = 3,
        Subtractive = 4,
        Multiplicative = 5,
        Effect = 6,
        Filter = 7,
        Dodge = 8,
        Darken = 9,
        Lighten = 10,
        Screen = 11,
        AddAlpha = 12, // additive alpha blend
        PsNormal = 13,
        PsAdditive = 14,
        PsSubtractive = 15,
        PsMultiplicative = 16,
        PsScreen = 17,
        PsOverlay = 18,
        PsHardLight = 19,
        PsSoftLight = 20,
        PsColorDodge = 21,
        PsColorDodge5 = 22,
        PsColorBurn = 23,
        PsLighten = 24,
        PsDarken = 25,
        PsDifference = 26,
        PsDifference5 = 27,
        PsExclusion = 28
    }

    /// <summary>
    /// Pbd-psd图层信息
    /// </summary>
    public class PbdPSDInfo
    {
        private readonly int m_layer_type = -1;
        private readonly string m_name = string.Empty;
        private readonly int m_left = 0;
        private readonly int m_top = 0;
        private readonly int m_width = 0;
        private readonly int m_height = 0;
        private readonly PbdLayerType m_type = PbdLayerType.Unknow;
        private readonly int m_opacity = -1;
        private readonly int m_visible = -1;
        private readonly int m_layer_id = -1;
        private readonly int m_group_layer_id = -1;
        private readonly int m_base = -1;
        private readonly string m_images = string.Empty;

        private string mFilename = string.Empty;

        /// <summary>
        /// ID
        /// </summary>
        public int ID => this.m_layer_id;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => this.m_name;
        /// <summary>
        /// X偏移
        /// </summary>
        public int Left => this.m_left;
        /// <summary>
        /// Y偏移
        /// </summary>
        public int Top => this.m_top;
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width => this.m_width;
        /// <summary>
        /// 高度
        /// </summary>
        public int Height => this.m_height;
        /// <summary>
        /// 图层类型
        /// </summary>
        public PbdLayerType Type => this.m_type;
        /// <summary>
        /// 不透明度
        /// </summary>
        public int Opacity => this.m_opacity;

        /// <summary>
        /// 文件名[无后缀]
        /// </summary>
        public string Filename => this.mFilename;

        /// <summary>
        /// 图像内存大小
        /// </summary>
        public int MemoryBytes => this.m_width * this.m_height * 4;

        /// <summary>
        /// 设置图层文件名
        /// </summary>
        /// <param name="baseName">立绘文件名</param>
        internal void SetFileName(string baseName)
        {
            this.mFilename = $"{baseName}_{this.m_layer_id}";
        }

        internal PbdPSDInfo(TJSVariant v)
        {
            Dictionary<string, TJSVariant> dic = v.AsDictionary();

            if (dic.ContainsKey("layer_type"))
            {
                this.m_layer_type = dic["layer_type"].ToInt32();
            }
            if (dic.ContainsKey("name"))
            {
                this.m_name = dic["name"].AsString();
            }
            if (dic.ContainsKey("left"))
            {
                this.m_left = dic["left"].ToInt32();
            }
            if (dic.ContainsKey("top"))
            {
                this.m_top = dic["top"].ToInt32();
            }
            if (dic.ContainsKey("width"))
            {
                this.m_width = dic["width"].ToInt32();
            }
            if (dic.ContainsKey("height"))
            {
                this.m_height = dic["height"].ToInt32();
            }
            if (dic.ContainsKey("type"))
            {
                int t = dic["type"].ToInt32();
                if (Enum.IsDefined(typeof(PbdLayerType), t))
                {
                    this.m_type = (PbdLayerType)Enum.ToObject(typeof(PbdLayerType), t);
                }
            }
            if (dic.ContainsKey("opacity"))
            {
                this.m_opacity = dic["opacity"].ToInt32();
            }
            if (dic.ContainsKey("visible"))
            {
                this.m_visible = dic["visible"].ToInt32();
            }
            if (dic.ContainsKey("layer_id"))
            {
                this.m_layer_id = dic["layer_id"].ToInt32();
            }
            if (dic.ContainsKey("group_layer_id"))
            {
                this.m_group_layer_id = dic["group_layer_id"].ToInt32();
            }
            if (dic.ContainsKey("base"))
            {
                this.m_base = dic["base"].ToInt32();
            }
            if (dic.ContainsKey("images"))
            {
                this.m_images = dic["images"].AsString();
            }
        }
    }

    /// <summary>
    /// Pbd立绘画布
    /// </summary>
    public class PbdStandLayer
    {
        private readonly int m_width = 0;
        private readonly int m_height = 0;
        private readonly List<PbdPSDInfo> mLayers = new(64);
        private readonly string mName = string.Empty;

        private readonly Dictionary<string, PbdPSDInfo> mLayerMap = new(64);        //图层映射表

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width => this.m_width;
        /// <summary>
        /// 高度
        /// </summary>
        public int Height => this.m_height;
        /// <summary>
        /// 图层
        /// </summary>
        public ReadOnlyCollection<PbdPSDInfo> Layers => this.mLayers.AsReadOnly();
        /// <summary>
        /// 立绘名
        /// </summary>
        public string Name => this.mName;

        /// <summary>
        /// 图像内存大小
        /// </summary>
        public int MemoryBytes => this.m_width * this.m_height * 4;

        private PbdStandLayer(TJSVariant v, string name)
        {
            if(v.Type != TJSVariantType.ArrayObject)
            {
                throw new ArgumentException("传入TJS变量类型错误, 必须为Array类型");
            }

            this.mName = name;

            List<TJSVariant> infos = v.AsArray();
            foreach(TJSVariant info in infos)
            {
                if(info.Type != TJSVariantType.DictionaryObject)
                {
                    throw new ArgumentException("图层项必须为Dictionary类型");
                }
                Dictionary<string, TJSVariant> dic = info.AsDictionary();
                if (dic.TryGetValue("layer_type", out TJSVariant? lt))
                {
                    //获取图层项
                    int psdType = lt.ToInt32();
                    if(psdType == 0)
                    {
                        PbdPSDInfo psd = new(info);
                        psd.SetFileName(name);
                        this.mLayers.Add(psd);
                        this.mLayerMap.Add(psd.Name, psd);
                    }
                }
                else
                {
                    //获取画布大小
                    if (dic.TryGetValue("width", out TJSVariant? w))
                    {
                        this.m_width = w.ToInt32();
                    }
                    if (dic.TryGetValue("height", out TJSVariant? h))
                    {
                        this.m_height = h.ToInt32();
                    }
                }
            }
        }

        /// <summary>
        /// 加载图像
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <param name="index">索引</param>
        /// <param name="psd">返回psd信息</param>
        internal Image<Bgra32> Load(string directory, int index, out PbdPSDInfo psd)
        {
            List<PbdPSDInfo> layers = this.mLayers;
            if(index < 0 || index >= layers.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "图层索引范围");
            }
            psd = layers[index];

            string fn = Path.Combine(directory, psd.Filename);
            using FileStream? fs = PbdLayerFileStream.OpenStream(fn);
            if (fs is not null)
            {
                try
                {
                    return Image.Load<Bgra32>(fs);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new ArgumentException($"图层文件不存在或打开失败:{fn}");
            }
        }

        /// <summary>
        /// 加载图像
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <param name="name">名称</param>
        /// <param name="psd">返回psd信息</param>
        internal Image<Bgra32> Load(string directory, string name, out PbdPSDInfo psd)
        {
            Dictionary<string, PbdPSDInfo> layerMap = this.mLayerMap;
            if(!layerMap.TryGetValue(name, out PbdPSDInfo? info))
            {
                throw new ArgumentOutOfRangeException(nameof(name), name, "不存在此图层");
            }
            psd = info;

            string fn = Path.Combine(directory, psd.Filename);
            using FileStream? fs = PbdLayerFileStream.OpenStream(fn);
            if (fs is not null)
            {
                try
                {
                    return Image.Load<Bgra32>(fs);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new ArgumentException($"打开文件失败:{fn}");
            }
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="dst">目标数组</param>
        /// <param name="directory">文件夹路径</param>
        /// <param name="names">名称</param>
        public void Draw(byte[] dst, string directory, List<string> names)
        {
            try
            {
                Memory<byte> output = new(dst, 0, this.MemoryBytes);
                output.Span.Clear();

                int w = this.m_width;
                int h = this.m_height;

                using Image<Bgra32> main = Image.WrapMemory<Bgra32>(output, w, h);
                foreach (string name in names)
                {
                    using Image<Bgra32> sub = this.Load(directory, name, out PbdPSDInfo psd);
                    PbdRender.BlendAlpha(main, sub, new(psd.Left, psd.Top, psd.Width, psd.Height), sub.Bounds, (byte)psd.Opacity);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 创建立绘图层
        /// </summary>
        /// <param name="stream">输入流</param>
        /// <param name="pbdParams">pbd参数</param>
        /// <param name="name">立绘名</param>
        public static PbdStandLayer? Create(Stream stream, PbdCustomParams pbdParams, string name)
        {
            PbdBinary? bin = PbdBinary.Create(stream, pbdParams);
            if(bin is null)
            {
                return null;
            }

            if(!bin.TryGetTJSVariant(out TJSVariant v))
            {
                return null;
            }

            return new(v, name);
        }
    }
}
