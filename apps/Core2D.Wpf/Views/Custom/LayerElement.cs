﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Data;
using Core2D.Containers;
using Core2D.Renderer;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Media;

namespace Core2D.Wpf.Views.Custom
{
    /// <summary>
    /// The custom layer control.
    /// </summary>
    public class LayerElement : FrameworkElement
    {
        /// <summary>
        /// Gets the <see cref="Context"/> from <see cref="DependencyProperty"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyProperty"/> object.</param>
        /// <returns>The <see cref="Context"/> value.</returns>
        public static Context GetData(DependencyObject obj)
        {
            return (Context)obj.GetValue(DataProperty);
        }

        /// <summary>
        /// Sets the <see cref="DependencyProperty"/> object value as <see cref="Context"/>.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyProperty"/> object.</param>
        /// <param name="value">The <see cref="Context"/> value.</param>
        public static void SetData(DependencyObject obj, Context value)
        {
            obj.SetValue(DataProperty, value);
        }

        /// <summary>
        /// The attached <see cref="DependencyProperty"/> for <see cref="Context"/> type.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached(
                "Data",
                typeof(Context),
                typeof(LayerElement),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        /// <summary>
        /// Gets the <see cref="Core2D.Renderer"/> from <see cref="DependencyProperty"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyProperty"/> object.</param>
        /// <returns>The <see cref="ShapeRenderer"/> value.</returns>
        public static ShapeRenderer GetRenderer(DependencyObject obj)
        {
            return (ShapeRenderer)obj.GetValue(RendererProperty);
        }

        /// <summary>
        /// Sets the <see cref="DependencyProperty"/> object value as <see cref="ShapeRenderer"/>.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyProperty"/> object.</param>
        /// <param name="value">The <see cref="ShapeRenderer"/> value.</param>
        public static void SetRenderer(DependencyObject obj, ShapeRenderer value)
        {
            obj.SetValue(RendererProperty, value);
        }

        /// <summary>
        /// The attached <see cref="DependencyProperty"/> for <see cref="ShapeRenderer"/> type.
        /// </summary>
        public static readonly DependencyProperty RendererProperty =
            DependencyProperty.RegisterAttached(
                "Renderer",
                typeof(ShapeRenderer),
                typeof(LayerElement),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        private bool _isLoaded = false;
        private LayerContainer _layer = default(LayerContainer);

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerElement"/> class.
        /// </summary>
        public LayerElement()
        {
            Loaded +=
                (s, e) =>
                {
                    if (_isLoaded)
                        return;
                    else
                        _isLoaded = true;

                    Initialize();
                };

            Unloaded +=
                (s, e) =>
                {
                    if (!_isLoaded)
                        return;
                    else
                        _isLoaded = false;

                    DeInitialize();
                };

            DataContextChanged +=
                (s, e) =>
                {
                    if (!_isLoaded)
                        _isLoaded = true;

                    if (_layer != null)
                    {
                        var layer = DataContext as LayerContainer;
                        if (layer == _layer)
                            return;
                    }

                    Initialize();
                };

            RenderOptions.SetBitmapScalingMode(
                this,
                BitmapScalingMode.HighQuality);
        }

        private void Invalidate(object sender, InvalidateLayerEventArgs e) => Dispatcher.Invoke(() => InvalidateVisual());

        private void Initialize()
        {
            if (_layer != null)
            {
                DeInitialize();
            }

            if (DataContext is LayerContainer layer)
            {
                _layer = layer;
                _layer.InvalidateLayer += Invalidate;
            }
        }

        private void DeInitialize()
        {
            if (_layer != null)
            {
                _layer.InvalidateLayer -= Invalidate;
                _layer = default(LayerContainer);
            }
        }

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Render(drawingContext);
        }

        private void Render(DrawingContext drawingContext)
        {
            if (DataContext is LayerContainer layer && layer.IsVisible)
            {
                var renderer = LayerElement.GetRenderer(this);
                if (renderer != null)
                {
                    var data = LayerElement.GetData(this);
                    var properties = data != null ? data.Properties : default(ImmutableArray<Property>);
                    var record = data != null ? data.Record : default(Record);
                    renderer.Draw(drawingContext, layer, 0.0, 0.0, properties, record);
                }
            }
        }
    }
}
