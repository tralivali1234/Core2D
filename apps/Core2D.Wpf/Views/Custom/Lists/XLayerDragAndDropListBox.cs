﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Editor;
using Core2D.Containers;
using System.Collections.Immutable;

namespace Core2D.Wpf.Views.Custom.Lists
{
    /// <summary>
    /// The <see cref="ListBox"/> control for <see cref="LayerContainer"/> items with drag and drop support.
    /// </summary>
    public class LayerContainerDragAndDropListBox : DragAndDropListBox<LayerContainer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayerContainerDragAndDropListBox"/> class.
        /// </summary>
        public LayerContainerDragAndDropListBox()
            : base()
        {
            this.Initialized += (s, e) => base.Initialize();
        }

        /// <summary>
        /// Updates DataContext binding to ImmutableArray collection property.
        /// </summary>
        /// <param name="array">The updated immutable array.</param>
        protected override void UpdateDataContext(ImmutableArray<LayerContainer> array)
        {
            var editor = (ProjectEditor)this.Tag;

            var container = editor.Project.CurrentContainer;

            var previous = container.Layers;
            var next = array;
            editor.Project?.History?.Snapshot(previous, next, (p) => container.Layers = p);
            container.Layers = next;
        }
    }
}
