using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PbdStandViewerGUI.MVVM.Base;
using Pbd.Commom;

namespace PbdStandViewerGUI
{
    /// <summary>
    /// 游戏窗口后台
    /// </summary>
    internal class SelectTitleWindowViewModel : NotifyPropertyChangedBase
    {
        private static readonly SelectTitleWindowViewModel smInstance = new();
        public static SelectTitleWindowViewModel Instance => smInstance;

        private SelectTitleWindowViewModel()
        {
            ReadOnlyCollection<PbdCustomParams> titles = DataManager.Titles;

            this.mTitles = titles.Select(title => title.Title).ToList();
            this.mParamStrings = titles.Select(title => title.GetParamsString()).ToList();
        }

        private readonly List<string> mTitles;                  //界面combo box所有内容
        private readonly List<string> mParamStrings;            //界面文本框所有内容
        private string mParams = string.Empty;                  //界面参数文本框当前内容
        private int mSelectIndex = -1;                          //界面combo box当前选项

        private int mPrevIndex = -1;                            //上一次选择的选项

        public ReadOnlyCollection<string> Titles => this.mTitles.AsReadOnly();
        public string Params
        {
            get => this.mParams;
            set => this.SetField(ref this.mParams, value);
        }
        public int SelectIndex
        {
            get => this.mSelectIndex;
            set
            {
                if(value < this.mTitles.Count && this.SetField(ref this.mSelectIndex, value))
                {
                    this.Params = value >= 0 ? this.mParamStrings[value] : string.Empty;        //刷新参数文本框
                }
            }
        }

        private ICommand? mOnConfirmClick;                      //确认按钮点击
        private ICommand? mOnWindowClosed;                      //窗口关闭后事件

        public ICommand OnConfirmClick
        {
            get
            {
                this.mOnConfirmClick ??= new DelegateCommand(this.EventConfirmClick);
                return this.mOnConfirmClick;
            }
        }
        public ICommand OnWindowClosed
        {
            get
            {
                this.mOnWindowClosed ??= new DelegateCommand(this.EventWindowClosed);
                return this.mOnWindowClosed;
            }
        }

        private void EventConfirmClick(object? arg)
        {
            Window win = (Window)arg!;
            win.DialogResult = true;
        }
        private void EventWindowClosed(object? arg)
        {
            Window win = (Window)arg!;
            int idx;
            if (win.DialogResult is bool v && v)
            {
                idx = this.mPrevIndex = this.SelectIndex;
            }
            else
            {
                idx = this.SelectIndex = this.mPrevIndex;
            }
            win.Tag = idx;
        }
    }
}
