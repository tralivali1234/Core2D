﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Interfaces;
using Core2D.Containers;
using Core2D.Renderer;
using Core2D.Shape;
using Core2D.Renderer.PdfSharp;

namespace Core2D.FileWriter.PdfSharp
{
    /// <summary>
    /// PdfSharp file writer.
    /// </summary>
    public sealed class PdfSharpWriter : IFileWriter
    {
        /// <inheritdoc/>
        string IFileWriter.Name { get; } = "Pdf (PdfSharp)";

        /// <inheritdoc/>
        string IFileWriter.Extension { get; } = "pdf";

        /// <inheritdoc/>
        void IFileWriter.Save(string path, object item, object options)
        {
            if (string.IsNullOrEmpty(path) || item == null)
                return;

            var ic = options as IImageCache;
            if (options == null)
                return;

            IProjectExporter exporter = new PdfSharpRenderer();

            ShapeRenderer renderer = (PdfSharpRenderer)exporter;
            renderer.State.DrawShapeState.Flags = ShapeStateFlags.Printable;
            renderer.State.ImageCache = ic;

            if (item is PageContainer)
            {
                exporter.Save(path, item as PageContainer);
            }
            else if (item is DocumentContainer)
            {
                exporter.Save(path, item as DocumentContainer);
            }
            else if (item is ProjectContainer)
            {
                exporter.Save(path, item as ProjectContainer);
            }
        }
    }
}
