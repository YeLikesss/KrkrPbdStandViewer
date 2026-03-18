using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pbd.Commom;
using Pbd.Layer;
using PbdStandViewerGUI.MVVM.Base;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PbdStandViewerGUI
{
    /// <summary>
    /// 全局变量
    /// </summary>
    internal class GlobalVariable
    {
        /// <summary>
        /// 当前参数索引
        /// </summary>
        public static int CurrentParamIndex { get; internal set; } = -1;
        /// <summary>
        /// 当前立绘
        /// </summary>
        public static PbdProc CurrentPbdStand { get; } = new();
        /// <summary>
        /// 当前处理格式
        /// </summary>
        public static PbdProcFormat CurrentProcessFormat { get; internal set; } = PbdProcFormat.PNG;
    }

    /// <summary>
    /// 处理格式
    /// </summary>
    internal enum PbdProcFormat
    {
        WEBP,
        PNG,
        BMP,
        TGA,
    }

    internal class LayerLevelViewModel
    {
        private int mLevel;
        private ObservableCollection<PbdViewModel> mLayers;

        public int Level
        {
            get => this.mLevel;
        }

        public ObservableCollection<PbdViewModel> Layers
        {
            get => this.mLayers;
        }

        internal LayerLevelViewModel(int level, ObservableCollection<PbdViewModel> layers)
        {
            this.mLevel = level;
            this.mLayers = layers;
        }
    }

    internal class PbdViewModel : NotifyPropertyChangedBase
    {
        private string mName;
        private string mType;
        private int mOffsetX;
        private int mOffsetY;
        private int mWidth;
        private int mHeight;
        private int mOpacity;
        private int mID;

        public string Name
        {
            get => this.mName;
        }
        public string Type
        {
            get => this.mType;
        }
        public int OffsetX
        {
            get => this.mOffsetX;
        }
        public int OffsetY
        {
            get => this.mOffsetY;
        }
        public int Width
        {
            get => this.mWidth;
        }
        public int Height
        {
            get => this.mHeight;
        }
        public int Opacity
        {
            get => this.mOpacity;
        }
        public int ID
        {
            get => this.mID;
        }

        private bool mIsPreview = false;                          //启用预览
        private bool mIsProcess = false;                          //启用合成
        private int mLevel = 0;                                   //层级
        private ObservableCollection<LayerLevelViewModel> mLevels = new();       //层级组

        public bool IsPreview
        {
            get => this.mIsPreview;
            set => this.SetField(ref this.mIsPreview, value);
        }
        public bool IsProcess
        {
            get => this.mIsProcess;
            set => this.SetField(ref this.mIsProcess, value);
        }
        public int Level
        {
            get => this.mLevel;
            set
            {
                if (PbdProc.CheckLevelLimited(value))
                {
                    int prev = this.mLevel;
                    if(this.SetField(ref this.mLevel, value))
                    {
                        //刷新Z轴层级
                        ObservableCollection<LayerLevelViewModel> lvs = this.mLevels;
                        lvs[prev].Layers.Remove(this);
                        lvs[value].Layers.Add(this);
                    }
                }
            }
        }

        internal void SetBindingData(ObservableCollection<LayerLevelViewModel> levelVMs)
        {
            this.mLevels = levelVMs;
        }

        public PbdViewModel(string name, string type, int x, int y, int w, int h, int opa, int id)
        {
            this.mName = name;
            this.mType = type;
            this.mOffsetX = x;
            this.mOffsetY = y;
            this.mWidth = w;
            this.mHeight = h;
            this.mOpacity = opa;
            this.mID = id;
        }
    }

    /// <summary>
    /// Pbd调用
    /// </summary>
    internal class PbdProc
    {
        /// <summary>
        /// 图层最大值
        /// </summary>
        public const int CLayerLevelMax = 16;

        private PbdStandLayer? mLayer = null;           //当前图层
        private string mLastError = string.Empty;       //错误信息

        private ObservableCollection<PbdViewModel> mLayerList = new();
        private ObservableCollection<LayerLevelViewModel> mLevelList = new();

        private string mCurrentDirectory = string.Empty;        //立绘文件文件夹
        private byte[] mPreviewImage = Array.Empty<byte>();     //预览图像素

        /// <summary>
        /// 最后一次错误信息
        /// </summary>
        public string LastError => this.mLastError;

        /// <summary>
        /// 图层列表
        /// </summary>
        public ObservableCollection<PbdViewModel> LayerList => this.mLayerList;

        /// <summary>
        /// 图层Z轴列表
        /// </summary>
        public ObservableCollection<LayerLevelViewModel> LevelList => this.mLevelList;

        /// <summary>
        /// 立绘名称
        /// </summary>
        public string Name
        {
            get
            {
                if (this.mLayer != null)
                {
                    return this.mLayer.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 画布宽度
        /// </summary>
        public int Width
        {
            get
            {
                if (this.mLayer != null)
                {
                    return this.mLayer.Width;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 画布高度
        /// </summary>
        public int Height
        {
            get
            {
                if (this.mLayer != null)
                {
                    return this.mLayer.Height;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 预览像素
        /// </summary>
        public ReadOnlySpan<byte> PreviewPixels => this.mPreviewImage;

        /// <summary>
        /// 刷新
        /// </summary>
        private void Update()
        {
            if (this.mLayer is null)
            {
                return;
            }

            List<PbdViewModel> pbdVM = this.mLayer.Layers.Select((PbdPSDInfo info) =>
            {
                return new PbdViewModel(info.Name, info.Type.ToString(), info.Left, info.Top, info.Width, info.Height, info.Opacity, info.ID);
            }).ToList();
            this.mLayerList = new(pbdVM);

            //构建图层关系
            List<LayerLevelViewModel> layersVM = new(PbdProc.CLayerLevelMax)
            {
                new(0, new(pbdVM)),          //初始全部0图层级别
            };
            for (int i = 1; i < PbdProc.CLayerLevelMax; ++i)
            {
                layersVM.Add(new(i, new()));
            }
            this.mLevelList = new(layersVM);

            //绑定组 图层组
            foreach (PbdViewModel m in pbdVM)
            {
                m.SetBindingData(this.mLevelList);
            }

            //更新预览像素缓存
            this.mPreviewImage = new byte[this.mLayer.MemoryBytes];
        }

        /// <summary>
        /// 加载立绘
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>True加载成功 False加载失败</returns>
        public bool Load(string filename)
        {
            ReadOnlyCollection<PbdCustomParams> titles = DataManager.Titles;
            int idx = GlobalVariable.CurrentParamIndex;

            if (idx < 0 || idx >= titles.Count)
            {
                this.mLastError = "请选择游戏";
                return false;
            }
            if (!File.Exists(filename))
            {
                this.mLastError = "文件不存在";
                return false;
            }

            PbdCustomParams param = titles[idx];
            try
            {
                using FileStream inFs = File.OpenRead(filename);
                if(PbdStandLayer.Create(inFs, param, Path.GetFileNameWithoutExtension(filename)) is PbdStandLayer layer)
                {
                    this.mLayer = layer;
                    this.mCurrentDirectory = Path.GetDirectoryName(filename)!;
                    this.mLastError = string.Empty;
                    this.Update();
                    return true;
                }
                else
                {
                    this.mLastError = "立绘文件解析失败";
                    return false;
                }
            }
            catch(Exception e)
            {
                this.mLastError = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 刷新预览图层
        /// </summary>
        public bool UpdatePreviewImage()
        {
            this.mLastError = string.Empty;
            if(this.mLayer is null)
            {
                this.mLastError = "未加载立绘文件";
                return false;
            }
            List<string> prevNames = this.mLayerList.Where(pbdVM => pbdVM.IsPreview)
                                                    .GroupBy(pbdVM => pbdVM.Level)
                                                    .OrderBy(g => g.Key)
                                                    .Select(g => g.First().Name)
                                                    .ToList();
            if (!prevNames.Any())
            {
                //没有预览清空图片内容
                this.mPreviewImage.AsSpan().Clear();
                return true;
            }
            try
            {
                this.mLayer.Draw(this.mPreviewImage, this.mCurrentDirectory, prevNames);
                return true;
            }
            catch(Exception e)
            {
                this.mLastError = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取保存参数
        /// </summary>
        /// <param name="w">宽</param>
        /// <param name="h">高</param>
        /// <param name="bytes">图像大小</param>
        /// <param name="fmt">格式</param>
        private bool GetImageSaveOptions(out int w, out int h, out int bytes, out PbdFormat fmt)
        {
            w = h = -1;
            bytes = -1;
            fmt = PbdProc.FormatConvert(GlobalVariable.CurrentProcessFormat);

            if (this.mLayer is null)
            {
                this.mLastError = "未加载立绘文件";
                return false;
            }

            w = this.mLayer.Width;
            h = this.mLayer.Height;
            bytes = this.mLayer.MemoryBytes;
            if (w <= 0 || h <= 0)
            {
                this.mLastError = "无效的图像";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 保存预览图(同步)
        /// </summary>
        public bool SavePreviewImage()
        {
            if(!this.GetImageSaveOptions(out int w, out int h, out _, out PbdFormat fmt))
            {
                return false;
            }

            using Image<Bgra32> img = Image.WrapMemory<Bgra32>(this.mPreviewImage, w, h);
            try
            {
                PbdLayerFormat.Save(img, fmt, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Preview_Export"), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                this.mLastError = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                this.mLastError = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 保存预览图(异步)
        /// </summary>
        public Task<bool> SavePreviewImageAsync()
        {
            return Task.Run(this.SavePreviewImage);
        }

        /// <summary>
        /// 获取立绘合成栈
        /// </summary>
        private List<List<string>> GetProcessStandStack()
        {
            if (this.mLayer is null)
            {
                return new();
            }

            return this.mLayerList.Where(pbdVM => pbdVM.IsProcess)
                                  .GroupBy(pbdVM => pbdVM.Level)
                                  .OrderBy(g => g.Key)
                                  .Select(g => g.Select(pbdVM => pbdVM.Name).ToList()).ToList();
        }

        /// <summary>
        /// 保存立绘(同步)
        /// </summary>
        public bool SaveStandImage(IProgress<int>? initCB, IProgress<int>? processedCB, IProgress<string>? errorCB)
        {
            if (this.mLayer is null)
            {
                this.mLastError = "未加载立绘文件";
                return false;
            }
            PbdStandLayer stand = this.mLayer;

            if (stand.Width <= 0 || stand.Height <= 0)
            {
                this.mLastError = "无效的图像";
                return false;
            }
            
            List<List<string>> nameStack = this.GetProcessStandStack();
            if(!nameStack.Any())
            {
                this.mLastError = "未选中任何图像";
                return false;
            }

            int total = 1;      //图片数
            foreach(List<string> ns in nameStack)
            {
                total *= ns.Count;
            }
            initCB?.Report(total);      //汇报任务数量

            //格式
            PbdFormat fmt = PbdProc.FormatConvert(GlobalVariable.CurrentProcessFormat);
            //输入文件夹
            string inportDir = this.mCurrentDirectory;
            //输出文件夹
            string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stand_Export", stand.Name);

            ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            ParallelLoopResult loopResult = Parallel.For(0, total, options, (int idx) =>
            {
                int w = stand.Width;
                int h = stand.Height;
                int bytes = stand.MemoryBytes;

                List<string> names = new(nameStack.Count);
                {
                    //从最上层遍历
                    int sel = idx;
                    for (int i = nameStack.Count - 1; i >= 0; --i)
                    {
                        List<string> subs = nameStack[i];
                        names.Add(subs[sel % subs.Count]);
                        sel /= subs.Count;
                    }
                    names.Reverse();
                }

                byte[] baseImg = ArrayPool<byte>.Shared.Rent(bytes);
                try
                {
                    stand.Draw(baseImg, inportDir, names);
                    using Image<Bgra32> img = Image.WrapMemory<Bgra32>(baseImg.AsMemory(), w, h);
                    PbdLayerFormat.Save(img, fmt, exportDir, $"{idx:D5}");
                }
                catch(Exception e)
                {
                    errorCB?.Report(e.Message);     //汇报错误
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(baseImg);
                }
                processedCB?.Report(idx);           //汇报进度
            });

            this.mLastError = string.Empty;
            return loopResult.IsCompleted;
        }

        /// <summary>
        /// 保存立绘(异步)
        /// </summary>
        public Task<bool> SaveStandImageAsync(IProgress<int>? initCB, IProgress<int>? processedCB, IProgress<string>? errorCB)
        {
            return Task.Run(() =>
            {
                return this.SaveStandImage(initCB, processedCB, errorCB);
            });
        }

        /// <summary>
        /// 导出格式转换
        /// </summary>
        private static PbdFormat FormatConvert(PbdProcFormat fmt)
        {
            return fmt switch
            {
                PbdProcFormat.WEBP => PbdFormat.Webp,
                PbdProcFormat.PNG => PbdFormat.Png,
                PbdProcFormat.BMP => PbdFormat.Bmp,
                PbdProcFormat.TGA => PbdFormat.Tga,
                _ => PbdFormat.Png,
            };
        }

        /// <summary>
        /// 检查图层Z轴限制
        /// </summary>
        internal static bool CheckLevelLimited(int level)
        {
            return level >= 0 && level < PbdProc.CLayerLevelMax;
        }
    }
}
