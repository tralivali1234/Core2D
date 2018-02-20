﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Core2D.Interfaces;
using Core2D.Containers;
using Core2D.Renderer;
using SkiaSharp;

namespace Core2D.FileWriter.SkiaSharpSvg
{
    /// <summary>
    /// SkiaSharp svg <see cref="IProjectExporter"/> implementation.
    /// </summary>
    public sealed class SvgSkiaSharpExporter : IProjectExporter
    {
        private readonly ShapeRenderer _renderer;
        private readonly ContainerPresenter _presenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgSkiaSharpExporter"/> class.
        /// </summary>
        /// <param name="renderer">The shape renderer.</param>
        /// <param name="presenter">The container presenter.</param>
        public SvgSkiaSharpExporter(ShapeRenderer renderer, ContainerPresenter presenter)
        {
            _renderer = renderer;
            _presenter = presenter;
        }

        /// <inheritdoc/>
        void IProjectExporter.Save(string path, PageContainer container)
        {
            Save(path, container);
        }

        /// <inheritdoc/>
        void IProjectExporter.Save(string path, DocumentContainer document)
        {
            throw new NotSupportedException("Saving documents as svg drawing is not supported.");
        }

        /// <inheritdoc/>
        void IProjectExporter.Save(string path, ProjectContainer project)
        {
            throw new NotSupportedException("Saving projects as svg drawing is not supported.");
        }

        void Save(string path, PageContainer container)
        {
            using (var stream = new SKFileWStream(path))
            using (var writer = new SKXmlStreamWriter(stream))
            using (var canvas = SKSvgCanvas.Create(SKRect.Create(0, 0, (int)container.Width, (int)container.Height), writer))
            {
                _presenter.Render(canvas, _renderer, container, 0, 0);
            }
        }
    }
}
