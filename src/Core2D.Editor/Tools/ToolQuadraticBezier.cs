﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Core2D.Editor.Input;
using Core2D.Editor.Tools.Selection;
using Core2D.Editor.Tools.Settings;
using Core2D.Shape;
using Core2D.Shapes;

namespace Core2D.Editor.Tools
{
    /// <summary>
    /// Quadratic bezier tool.
    /// </summary>
    public class ToolQuadraticBezier : ToolBase
    {
        public enum State { Point1, Point3, Point2 }
        private readonly IServiceProvider _serviceProvider;
        private ToolSettingsQuadraticBezier _settings;
        private State _currentState = State.Point1;
        private QuadraticBezierShape _quadraticBezier;
        private ToolQuadraticBezierSelection _selection;

        /// <inheritdoc/>
        public override string Title => "QuadraticBezier";

        /// <summary>
        /// Gets or sets the tool settings.
        /// </summary>
        public ToolSettingsQuadraticBezier Settings
        {
            get => _settings;
            set => Update(ref _settings, value);
        }

        /// <summary>
        /// Initialize new instance of <see cref="ToolQuadraticBezier"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ToolQuadraticBezier(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
            _settings = new ToolSettingsQuadraticBezier();
        }

        /// <inheritdoc/>
        public override void LeftDown(InputArgs args)
        {
            base.LeftDown(args);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            (double sx, double sy) = editor.TryToSnap(args);
            switch (_currentState)
            {
                case State.Point1:
                    {
                        var style = editor.Project.CurrentStyleLibrary.Selected;
                        _quadraticBezier = QuadraticBezierShape.Create(
                            sx, sy,
                            editor.Project.Options.CloneStyle ? style.Clone() : style,
                            editor.Project.Options.PointShape,
                            editor.Project.Options.DefaultIsStroked,
                            editor.Project.Options.DefaultIsFilled);

                        var result = editor.TryToGetConnectionPoint(sx, sy);
                        if (result != null)
                        {
                            _quadraticBezier.Point1 = result;
                        }

                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Add(_quadraticBezier);
                        editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        ToStatePoint3();
                        Move(_quadraticBezier);
                        _currentState = State.Point3;
                        editor.IsToolIdle = false;
                    }
                    break;
                case State.Point3:
                    {
                        if (_quadraticBezier != null)
                        {
                            _quadraticBezier.Point2.X = sx;
                            _quadraticBezier.Point2.Y = sy;
                            _quadraticBezier.Point3.X = sx;
                            _quadraticBezier.Point3.Y = sy;

                            var result = editor.TryToGetConnectionPoint(sx, sy);
                            if (result != null)
                            {
                                _quadraticBezier.Point3 = result;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                            ToStatePoint2();
                            Move(_quadraticBezier);
                            _currentState = State.Point2;
                        }
                    }
                    break;
                case State.Point2:
                    {
                        if (_quadraticBezier != null)
                        {
                            _quadraticBezier.Point2.X = sx;
                            _quadraticBezier.Point2.Y = sy;

                            var result = editor.TryToGetConnectionPoint(sx, sy);
                            if (result != null)
                            {
                                _quadraticBezier.Point2 = result;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_quadraticBezier);
                            Remove();
                            base.Finalize(_quadraticBezier);
                            editor.Project.AddShape(editor.Project.CurrentContainer.CurrentLayer, _quadraticBezier);
                            _currentState = State.Point1;
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
                case State.Point1:
                    break;
                case State.Point3:
                case State.Point2:
                    {
                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_quadraticBezier);
                        editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        Remove();
                        _currentState = State.Point1;
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
                case State.Point1:
                    {
                        if (editor.Project.Options.TryToConnect)
                        {
                            editor.TryToHoverShape(sx, sy);
                        }
                    }
                    break;
                case State.Point3:
                    {
                        if (_quadraticBezier != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape(sx, sy);
                            }
                            _quadraticBezier.Point2.X = sx;
                            _quadraticBezier.Point2.Y = sy;
                            _quadraticBezier.Point3.X = sx;
                            _quadraticBezier.Point3.Y = sy;
                            editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                            Move(_quadraticBezier);
                        }
                    }
                    break;
                case State.Point2:
                    {
                        if (_quadraticBezier != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape(sx, sy);
                            }
                            _quadraticBezier.Point2.X = sx;
                            _quadraticBezier.Point2.Y = sy;
                            editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                            Move(_quadraticBezier);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Transfer tool state to <see cref="State.Point3"/>.
        /// </summary>
        public void ToStatePoint3()
        {
            var editor = _serviceProvider.GetService<ProjectEditor>();
            _selection = new ToolQuadraticBezierSelection(
                editor.Project.CurrentContainer.HelperLayer,
                _quadraticBezier,
                editor.Project.Options.HelperStyle,
                editor.Project.Options.PointShape);

            _selection.ToStatePoint3();
        }

        /// <summary>
        /// Transfer tool state to <see cref="State.Point2"/>.
        /// </summary>
        public void ToStatePoint2()
        {
            _selection.ToStatePoint2();
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
