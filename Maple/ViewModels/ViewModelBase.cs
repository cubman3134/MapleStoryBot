using Maple.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple.ViewModels
{
    public class ViewModelBase : ModelBase
    {
        

        public bool IsClosing;
        public bool IsActive;

        

        public void InitializeData()
        {
            IsClosing = false;
            IsActive = true;
        }


        public class CommandHandler : ICommand
        {
            private Action _action;
            private Func<bool> _canExecute;

            /// <summary>
            /// Creates instance of the command handler
            /// </summary>
            /// <param name="action">Action to be executed by the command</param>
            /// <param name="canExecute">A bolean property to containing current permissions to execute the command</param>
            public CommandHandler(Action action, Func<bool> canExecute)
            {
                _action = action;
                _canExecute = canExecute;
            }

            /// <summary>
            /// Wires CanExecuteChanged event 
            /// </summary>
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            /// <summary>
            /// Forcess checking if execute is allowed
            /// </summary>
            /// <param name="parameter"></param>
            /// <returns></returns>
            public bool CanExecute(object parameter)
            {
                return _canExecute.Invoke();
            }

            public void Execute(object parameter)
            {
                _action();
            }
        }
    }
}
