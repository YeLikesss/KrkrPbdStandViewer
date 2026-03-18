using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PbdStandViewerGUI.MVVM.Base
{
    /// <summary>
    /// 属性通知封装
    /// </summary>
    internal class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            { 
                return false;
            }
            field = value;
            this.OnPropertyChanged(propName);
            return true;
        }
    }
}
