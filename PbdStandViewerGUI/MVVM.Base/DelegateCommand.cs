using System;
using System.Windows.Input;

namespace PbdStandViewerGUI.MVVM.Base
{
    /// <summary>
    /// 命令封装
    /// </summary>
    internal class DelegateCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add
            { 
                CommandManager.RequerySuggested += value;
            }
            remove 
            { 
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object? parameter)
        {
            if(this.mCanExecute is null)
            {
                return true;
            }
            else
            {
                return this.mCanExecute(parameter);
            }
        }

        public void Execute(object? parameter)
        {
            this.mExecute(parameter);
        }

        public DelegateCommand(Action<object?> executeFunc, Func<object?, bool>? canExecuteFunc = null)
        {
            this.mExecute = executeFunc;
            this.mCanExecute = canExecuteFunc;
        }
        private readonly Action<object?> mExecute;
        private readonly Func<object?, bool>? mCanExecute;
    }
}
