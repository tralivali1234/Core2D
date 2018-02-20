﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Markup;
using Avalonia.Controls;

namespace Core2D.Avalonia.Converters
{
    /// <summary>
    /// Return valid object when using TreeView.SelectedItem.DataContext source.
    /// </summary>
    public class TreeViewSelectedItemToObjectConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of a <see cref="TreeViewSelectedItemToObjectConverter"/>.
        /// </summary>
        public static readonly TreeViewSelectedItemToObjectConverter Instance = new TreeViewSelectedItemToObjectConverter();

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="parameter">A user-defined parameter.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is TreeViewItem)
                {
                    return (value as TreeViewItem).DataContext;
                }
                else
                {
                    return value;
                }
            }
            return AvaloniaProperty.UnsetValue;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="parameter">A user-defined parameter.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
