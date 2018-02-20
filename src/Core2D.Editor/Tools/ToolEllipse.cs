﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Core2D.Editor.Input;
using Core2D.Editor.Tools.Selection;
using Core2D.Editor.Tools.Settings;
using Core2D.Shape;
using Core2D.Shapes;
using static System.Math;

namespace Core2D.Editor.Tools
{
    /// <summary>
    /// Ellipse tool.
    /// </summary>
    public class ToolEllipse : ToolBase
    {
        public enum State { TopLeft, BottomRight }
        public enum Mode { Rectangle, Circle }
        private readonly IServiceProvider _serviceProvider;
        private ToolSettingsEllipse _settings;
        private State _currentState = State.TopLeft;
        private Mode _currentMode = Mode.Rectangle;
        private EllipseShape _ellipse;
        private ToolEllipseSelection _selection;
        private double _centerX;
        private double _centerY;

        /// <inheritdoc/>
        public override string Title => "Ellipse";

        /// <summary>
        /// Gets or sets the tool settings.
        /// </summary>
        public ToolSettingsEllipse Settings
        {
            get => _settings;
            set => Update(ref _settings, value);
        }

        /// <summary>
        /// Initialize new instance of <see cref="ToolEllipse"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ToolEllipse(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
            _settings = new ToolSettingsEllipse();
        }

        private static void CircleConstrain(PointShape tl, PointShape br, double cx, double cy, double px, double py)
        {
            double r = Max(Abs(cx - px), Abs(cy - py));
            tl.X = cx - r;
            tl.Y = cy - r;
            br.X = cx + r;
            br.Y = cy + r;
        }

        /// <inheritdoc/>
        public override void LeftDown(InputArgs args)
        {
            base.LeftDown(args);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            (double sx, double sy) = editor.TryToSnap(args);
            switch (_currentState)
            {
                case State.TopLeft:
                    {
                        if (_currentMode == Mode.Circle)
                        {
                            _centerX = sx;
                            _centerY = sy;
                        }

                        var style = editor.Project.CurrentStyleLibrary.Selected;
                        _ellipse = EllipseShape.Create(
                            sx, sy,
                            editor.Project.Options.CloneStyle ? style.Clone() : style,
                            editor.Project.Options.PointShape,
                            editor.Project.Options.DefaultIsStroked,
                            editor.Project.Options.DefaultIsFilled);

                        var result = editor.TryToGetConnectionPoint(sx, sy);
                        if (result != null)
                        {
                            _ellipse.TopLeft = result;
                        }

                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Add(_ellipse);
                        editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        ToStateBottomRight();
                        Move(_ellipse);
                        _currentState = State.BottomRight;
                        editor.IsToolIdle = false;
                    }
                    break;
                case State.BottomRight:
                    {
                        if (_ellipse != null)
                        {
                            if (_currentMode == Mode.Circle)
                            {
                                CircleConstrain(_ellipse.TopLeft, _ellipse.BottomRight, _centerX, _centerY, sx, sy);
                            }
                            else
                            {
                                _ellipse.BottomRight.X = sx;
                                _ellipse.BottomRight.Y = sy;
                            }

                            var result = editor.TryToGetConnectionPoint(sx, sy);
                            if (result != null)
                            {
                                _ellipse.BottomRight = result;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_ellipse);
                            Remove();
                            base.Finalize(_ellipse);
                            editor.Project.AddShape(editor.Project.CurrentContainer.CurrentLayer, _ellipse);
                            _currentState = State.TopLeft;
                            editor.IsToolIdle = true;
                        }
                    }
                    break;
            }

        }

        /// <inheritdoc/>
        public override void RightDown(InputArgs args)
        {
            base.RightDown(args);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            switch (_currentState)
            {
                case State.TopLeft:
                    break;
                case State.BottomRight:
                    {
                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_ellipse);
                        editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        Remove();
                        _currentState = State.TopLeft;
                        editor.IsToolIdle = true;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override void Move(InputArgs args)
        {
            base.Move(args);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            (double sx, double sy) = editor.TryToSnap(args);
            switch (_currentState)
            {
                case State.TopLeft:
                    {
                        if (editor.Project.Options.TryToConnect)
                        {
                            editor.TryToHoverShape(sx, sy);
                        }
                    }
                    break;
                case State.BottomRight:
                    {
                        if (_ellipse != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape(sx, sy);
                            }

                            if (_currentMode == Mode.Circle)
                            {
                                CircleConstrain(_ellipse.TopLeft, _ellipse.BottomRight, _centerX, _centerY, sx, sy);
                            }
                            else
                            {
                                _ellipse.BottomRight.X = sx;
                                _ellipse.BottomRight.Y = sy;
                            }
                            editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                            Move(_ellipse);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Transfer tool state to <see cref="State.BottomRight"/>.
        /// </summary>
        public void ToStateBottomRight()
        {
            var editor = _serviceProvider.GetService<ProjectEditor>();
            _selection = new ToolEllipseSelection(
                editor.Project.CurrentContainer.HelperLayer,
                _ellipse,
                editor.Project.Options.HelperStyle,
                editor.Project.Options.PointShape);

            _selection.ToStateBottomRight();
        }

        /// <inheritdoc/>
        public override void Move(BaseShape shape)
        {
            base.Move(shape);

            _selection.Move();
        }

        /// <inheritdoc/>
        public override void Remove()
        {
            base.Remove();

            _selection.Remove();
            _selection = null;
        }
    }
}
