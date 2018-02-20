﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Editor;
using Core2D.Editor.Commands;
using Core2D.Editor.Input;
using Core2D.Containers;
using Core2D.Wpf.Windows;
using Microsoft.Win32;

namespace Core2D.Wpf.Commands
{
    /// <inheritdoc/>
    public class ImportDataCommand : Command<ProjectContainer>, IImportDataCommand
    {
        /// <inheritdoc/>
        public override bool CanRun(ProjectContainer project)
        {
            return ServiceProvider.GetService<ProjectEditor>().IsEditMode();
        }

        /// <inheritdoc/>
        public override void Run(ProjectContainer project)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Csv (*.csv)|*.csv|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = ""
            };

            if (dlg.ShowDialog(ServiceProvider.GetService<MainWindow>()) == true)
            {
                ServiceProvider.GetService<ProjectEditor>().OnImportData(project, dlg.FileName);
            }
        }
    }
}
