﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Core2D.Avalonia.Views.Data
{
    /// <summary>
    /// Interaction logic for <see cref="RecordsControl"/> xaml.
    /// </summary>
    public class RecordsControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordsControl"/> class.
        /// </summary>
        public RecordsControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initialize the Xaml components.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
