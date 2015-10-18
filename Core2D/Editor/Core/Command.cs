﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Core2D
{
    /// <summary>
    /// 
    /// </summary>
    public class Command : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private Action _execute;
        private Func<bool> _canExecute;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public Command(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (_execute == null)
                return;
            _execute();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        /// <returns></returns>
        public static ICommand Create(Action execute, Func<bool> canExecute = null)
        {
            return new Command(execute, canExecute);
        }
    }
}
