﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Editor;
using Core2D.Style;
using System.Collections.Immutable;

namespace Core2D.Wpf.Views.Custom.Lists
{
    /// <summary>
    /// The <see cref="ListBox"/> control for <see cref="ShapeStyle"/> items with drag and drop support.
    /// </summary>
    public class ShapeStyleDragAndDropListBox : DragAndDropListBox<ShapeStyle>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeStyleDragAndDropListBox"/> class.
        /// </summary>
        public ShapeStyleDragAndDropListBox()
            : base()
        {
            this.Initialized += (s, e) => base.Initialize();
        }

        /// <summary>
        /// Updates DataContext binding to ImmutableArray collection property.
        /// </summary>
        /// <param name="array">The updated immutable array.</param>
        protected override void UpdateDataContext(ImmutableArray<ShapeStyle> array)
        {
            var editor = (ProjectEditor)this.Tag;

            var sg = editor.Project.CurrentStyleLibrary;

            var previous = sg.Items;
            var next = array;
            editor.Project?.History?.Snapshot(previous, next, (p) => sg.Items = p);
            sg.Items = next;
        }
    }
}
