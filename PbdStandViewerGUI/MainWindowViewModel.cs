using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PbdStandViewerGUI.MVVM.Base;

namespace PbdStandViewerGUI
{
    /// <summary>
    /// 主窗口后台
    /// </summary>
    internal class MainWindowViewModel : NotifyPropertyChangedBase
    {
        private readonly static MainWindowViewModel smInstance = new();
        public static MainWindowViewModel Instance => smInstance;

        private MainWindowViewModel() 
        {
            this.mStandProcFormat = GlobalVariable.CurrentProcessFormat.ToString();
        }


        //界面操作启用禁用
        private bool mOperateEnable = true;
        //立绘图层信息
        private ObservableCollection<PbdViewModel> mLayerList = new();
        //图层Z轴信息
        private ObservableCollection<LayerLevelViewModel> mLevelList = new();
        //预览图像
        private WriteableBitmap? mPreviewImage;
        //日志文本
        private string mLogText = string.Empty;
        //立绘名称
        private string mStandName = string.Empty;
        //立绘大小
        private string mStandSize = string.Empty;
        //立绘合成格式
        private string mStandProcFormat = string.Empty;
        //立绘处理进度
        private string mStandProgress = string.Empty;

        public bool OperateEnable
        {
            get => this.mOperateEnable;
            set => this.SetField(ref this.mOperateEnable, value);
        }
        public ObservableCollection<PbdViewModel> LayerList
        {
            get => this.mLayerList;
            set => this.SetField(ref this.mLayerList, value);
        }
        public ObservableCollection<LayerLevelViewModel> LevelList
        {
            get => this.mLevelList;
            set => this.SetField(ref this.mLevelList, value);
        }
        public WriteableBitmap? PreviewImage
        {
            get => this.mPreviewImage;
            set => this.SetField(ref this.mPreviewImage, value);
        }
        public string LogText
        {
            get => this.mLogText;
            set => this.SetField(ref this.mLogText, value);
        }
        public string StandName
        {
            get => this.mStandName;
            set => this.SetField(ref this.mStandName, value);
        }
        public string StandSize
        {
            get => this.mStandSize;
            set => this.SetField(ref this.mStandSize, value);
        }
        public string StandProcFormat
        {
            get => this.mStandProcFormat;
            set => this.SetField(ref this.mStandProcFormat, value);
        }
        public string StandProgress
        {
            get => this.mStandProgress;
            set => this.SetField(ref this.mStandProgress, value);
        }

        private ICommand? mOnOpenFile;                      //打开文件
        private ICommand? mOnSelectTitle;                   //选择游戏
        private ICommand? mOnLevelInc;                      //图层Z自增
        private ICommand? mOnLevelDec;                      //图层Z自减
        private ICommand? mOnProcFmtClick;                  //处理格式点击
        private ICommand? mOnPreviewSwitch;                 //预览复选框切换
        private ICommand? mOnExportPreview;                 //输出预览图
        private ICommand? mOnExportStand;                   //输出合成立绘

        public ICommand OnOpenFile 
        {
            get
            {
                this.mOnOpenFile ??= new DelegateCommand(this.EventOpenFile);
                return this.mOnOpenFile;
            }
        }
        public ICommand OnSelectTitle
        {
            get
            {
                this.mOnSelectTitle ??= new DelegateCommand(this.EventSelectTitle);
                return this.mOnSelectTitle;
            }
        }
        public ICommand OnLevelInc
        {
            get
            {
                this.mOnLevelInc ??= new DelegateCommand(this.EventLevelInc);
                return this.mOnLevelInc;
            }
        }
        public ICommand OnLevelDec
        {
            get
            {
                this.mOnLevelDec ??= new DelegateCommand(this.EventLevelDec);
                return this.mOnLevelDec;
            }
        }
        public ICommand OnProcFmtClick
        {
            get
            {
                this.mOnProcFmtClick ??= new DelegateCommand(this.EventProcessFormatClick);
                return this.mOnProcFmtClick;
            }
        }
        public ICommand OnPreviewSwitch
        {
            get
            {
                this.mOnPreviewSwitch ??= new DelegateCommand(this.EventPreviewSwitch);
                return this.mOnPreviewSwitch;
            }
        }
        public ICommand OnExportPreview
        {
            get
            {
                this.mOnExportPreview ??= new DelegateCommand(this.EventExportPreview);
                return this.mOnExportPreview;
            }
        }
        public ICommand OnExportStand
        {
            get
            {
                this.mOnExportStand ??= new DelegateCommand(this.EventExportStand);
                return this.mOnExportStand;
            }
        }

        private void EventOpenFile(object? arg)
        {
            OpenFileDialog ofd = new()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Title = "选择立绘文件",
                DefaultExt = ".pbd",
                Filter = "立绘文件|*.pbd|所有文件|*.*",
            };
            if (ofd.ShowDialog() is bool result && result)
            {
                string fn = ofd.FileName;
                PbdProc proc = GlobalVariable.CurrentPbdStand;
                if (proc.Load(fn))
                {
                    this.Update();
                }
                else
                {
                    MessageBox.Show(proc.LastError, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void EventSelectTitle(object? arg)
        {
            SelectTitleWindow dlg = new()
            {
                Owner = arg as Window,
            };
            if (dlg.ShowDialog() is bool result && result)
            {
                if(dlg.Tag is int idx)
                {
                    //切换游戏
                    GlobalVariable.CurrentParamIndex = idx;
                }
            }
        }
        private void EventLevelInc(object? arg)
        {
            if(arg is PbdViewModel pbdVM)
            {
                int prev = pbdVM.Level++;
                if (prev != pbdVM.Level && pbdVM.IsPreview)
                {
                    //Z轴变化 预览启用
                    this.UpdatePreviewImage();
                }
            }
        }
        private void EventLevelDec(object? arg)
        {
            if (arg is PbdViewModel pbdVM)
            {
                int prev = pbdVM.Level--;
                if (prev != pbdVM.Level && pbdVM.IsPreview)
                {
                    //Z轴变化 预览启用
                    this.UpdatePreviewImage();
                }
            }
        }
        private void EventProcessFormatClick(object? arg)
        {
            if(arg is MenuItem menu)
            {
                if(menu.Tag is PbdProcFormat fmt)
                {
                    //未选中时切换
                    if (!menu.IsChecked)
                    {
                        if(menu.Parent is MenuItem parent)
                        {
                            //同组取消选中 保证单项选择
                            foreach(MenuItem sub in parent.Items)
                            {
                                if (sub != menu)
                                {
                                    sub.IsChecked = false;
                                }
                            }
                            menu.IsChecked = true;
                            this.StandProcFormat = fmt.ToString();

                            //切换导出格式
                            GlobalVariable.CurrentProcessFormat = fmt;
                        }
                    }
                }
            }
        }
        private void EventPreviewSwitch(object? arg)
        {
            this.UpdatePreviewImage();
        }
        private async void EventExportPreview(object? arg)
        {
            PbdProc proc = GlobalVariable.CurrentPbdStand;

            this.AppendLog("开始导出预览图");

            this.OperateEnable = false;
            bool result = await proc.SavePreviewImageAsync();
            this.OperateEnable = true;
            if (result)
            {
                this.AppendLog("预览图导出成功");
            }
            else
            {
                this.AppendLog($"预览图导出失败: {proc.LastError}");
            }
        }
        private async void EventExportStand(object? arg)
        {
            int taskCnt = 0;
            int processedCnt = 0;
            Progress<int> initCB = new((int v) =>
            {
                taskCnt = v;
                this.SetStandProgress(processedCnt, taskCnt);
            });
            Progress<int> procCB = new((int v) =>
            {
                processedCnt++;
                this.SetStandProgress(processedCnt, taskCnt);
            });
            Progress<string> errCB = new((string s) =>
            {
                this.AppendLog(s);
            });
            PbdProc proc = GlobalVariable.CurrentPbdStand;

            this.AppendLog("开始导出合成立绘");

            this.OperateEnable = false;
            bool result = await proc.SaveStandImageAsync(initCB, procCB, errCB);
            this.OperateEnable = true;
            if (result)
            {
                this.AppendLog("合成立绘成功");
            }
            else
            {
                this.AppendLog($"合成立绘失败: {proc.LastError}");
            }
        }

        /// <summary>
        /// 刷新界面数据
        /// </summary>
        private void Update()
        {
            PbdProc proc = GlobalVariable.CurrentPbdStand;
            this.LayerList = proc.LayerList;
            this.LevelList = proc.LevelList;
            this.StandName = proc.Name;
            this.StandSize = $"{proc.Width} x {proc.Height}";
            this.StandProgress = string.Empty;

            this.PreviewImage = null;       //清空图像
            this.LogText = string.Empty;    //清空日志
        }

        /// <summary>
        /// 刷新预览图像
        /// </summary>
        private unsafe void UpdatePreviewImage()
        {
            PbdProc proc = GlobalVariable.CurrentPbdStand;
            if (proc.UpdatePreviewImage())
            {
                int w = proc.Width;
                int h = proc.Height;
                ReadOnlySpan<byte> pixels = proc.PreviewPixels;

                WriteableBitmap bmp;
                if (this.PreviewImage == null)
                {
                    bmp = this.PreviewImage = new WriteableBitmap(w, h, 96.0, 96.0, PixelFormats.Bgra32, null);
                }
                else
                {
                    bmp = this.PreviewImage;
                }

                bmp.Lock();
                pixels.CopyTo(new Span<byte>(bmp.BackBuffer.ToPointer(), pixels.Length));
                bmp.AddDirtyRect(new(0, 0, w, h));
                bmp.Unlock();
            }
            else
            {
                this.AppendLog(proc.LastError);
            }
        }

        /// <summary>
        /// 设置立绘合成进度
        /// </summary>
        private void SetStandProgress(int cur, int max)
        {
            this.StandProgress = $"{cur} / {max}";
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        private void AppendLog(string s)
        {
            this.LogText += $"{DateTime.Now:HH:mm:ss} | {s}\r\n";
        }
    }
}
