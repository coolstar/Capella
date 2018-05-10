using System;
using System.Windows;
using System.Windows.Input;

namespace Capella.Models
{
    class TrayIconViewModel
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow.WindowState == WindowState.Minimized,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                    }
                };
            }
        }

        /// <summary>
        /// Check for application updates.
        /// </summary>
        public ICommand UpdatesApplicationCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Console.Beep()
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }


    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
