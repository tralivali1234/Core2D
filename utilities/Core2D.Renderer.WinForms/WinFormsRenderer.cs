﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Drawing2D;
using Core2D.Data;
using Spatial;
using Spatial.Arc;
using Core2D.Shape;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Renderer.WinForms
{
    /// <summary>
    /// Native Windows Forms shape renderer.
    /// </summary>
    public class WinFormsRenderer : ShapeRenderer
    {
        private bool _enableImageCache = true;
        private IDictionary<string, Image> _biCache;
        private Func<double, float> _scaleToPage;
        private double _textScaleFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsRenderer"/> class.
        /// </summary>
        /// <param name="textScaleFactor"></param>
        public WinFormsRenderer(double textScaleFactor = 1.0)
        {
            ClearCache(isZooming: false);
            _textScaleFactor = textScaleFactor;
            _scaleToPage = (value) => (float)(value);
        }

        /// <summary>
        /// Creates a new <see cref="WinFormsRenderer"/> instance.
        /// </summary>
        /// <returns>The new instance of the <see cref="WinFormsRenderer"/> class.</returns>
        public static ShapeRenderer Create() => new WinFormsRenderer();

        private static Color ToColor(ArgbColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        private Pen ToPen(BaseStyle style, Func<double, float> scale)
        {
            var pen = new Pen(ToColor(style.Stroke), (float)(style.Thickness / State.ZoomX));
            switch (style.LineCap)
            {
                case Core2D.Style.LineCap.Flat:
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
                    break;
                case Core2D.Style.LineCap.Square:
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Square;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Square;
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
                    break;
                case Core2D.Style.LineCap.Round:
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                    break;
            }
            if (style.Dashes != null)
            {
                // TODO: Convert to correct dash values.
                pen.DashPattern = ShapeStyle.ConvertDashesToFloatArray(style.Dashes);
            }
            pen.DashOffset = (float)style.DashOffset;
            return pen;
        }

        private SolidBrush ToSolidBrush(ArgbColor color) => new SolidBrush(ToColor(color));

        private static Rect2 CreateRect(PointShape tl, PointShape br, double dx, double dy) => Rect2.FromPoints(tl.X, tl.Y, br.X, br.Y, dx, dy);

        private static void DrawLineInternal(Graphics gfx, Pen pen, bool isStroked, ref PointF p0, ref PointF p1)
        {
            if (isStroked)
            {
                gfx.DrawLine(pen, p0, p1);
            }
        }

        private static void DrawLineCurveInternal(Graphics gfx, Pen pen, bool isStroked, ref PointF pt1, ref PointF pt2, double curvature, CurveOrientation orientation, PointAlignment pt1a, PointAlignment pt2a)
        {
            if (isStroked)
            {
                double p1x = pt1.X;
                double p1y = pt1.Y;
                double p2x = pt2.X;
                double p2y = pt2.Y;
                LineShapeExtensions.GetCurvedLineBezierControlPoints(orientation, curvature, pt1a, pt2a, ref p1x, ref p1y, ref p2x, ref p2y);
                gfx.DrawBezier(
                    pen,
                    pt1.X, pt1.Y,
                    (float)p1x,
                    (float)p1y,
                    (float)p2x,
                    (float)p2y,
                    pt2.X, pt2.Y);
            }
        }

        private void DrawLineArrowsInternal(LineShape line, double dx, double dy, Graphics gfx, out PointF pt1, out PointF pt2)
        {
            Brush fillStartArrow = ToSolidBrush(line.Style.StartArrowStyle.Fill);
            Pen strokeStartArrow = ToPen(line.Style.StartArrowStyle, _scaleToPage);

            Brush fillEndArrow = ToSolidBrush(line.Style.EndArrowStyle.Fill);
            Pen strokeEndArrow = ToPen(line.Style.EndArrowStyle, _scaleToPage);

            double _x1 = line.Start.X + dx;
            double _y1 = line.Start.Y + dy;
            double _x2 = line.End.X + dx;
            double _y2 = line.End.Y + dy;

            line.GetMaxLength(ref _x1, ref _y1, ref _x2, ref _y2);

            float x1 = _scaleToPage(_x1);
            float y1 = _scaleToPage(_y1);
            float x2 = _scaleToPage(_x2);
            float y2 = _scaleToPage(_y2);

            var sas = line.Style.StartArrowStyle;
            var eas = line.Style.EndArrowStyle;
            float a1 = (float)(Math.Atan2(y1 - y2, x1 - x2) * 180.0 / Math.PI);
            float a2 = (float)(Math.Atan2(y2 - y1, x2 - x1) * 180.0 / Math.PI);

            // Draw start arrow.
            pt1 = DrawLineArrowInternal(gfx, strokeStartArrow, fillStartArrow, x1, y1, a1, sas);

            // Draw end arrow.
            pt2 = DrawLineArrowInternal(gfx, strokeEndArrow, fillEndArrow, x2, y2, a2, eas);

            fillStartArrow.Dispose();
            strokeStartArrow.Dispose();

            fillEndArrow.Dispose();
            strokeEndArrow.Dispose();
        }

        private static PointF DrawLineArrowInternal(Graphics gfx, Pen pen, Brush brush, float x, float y, float angle, ArrowStyle style)
        {
            PointF pt;
            var rt = new Matrix();
            rt.RotateAt(angle, new PointF(x, y));
            double rx = style.RadiusX;
            double ry = style.RadiusY;
            double sx = 2.0 * rx;
            double sy = 2.0 * ry;

            switch (style.ArrowType)
            {
                default:
                case ArrowType.None:
                    {
                        pt = new PointF(x, y);
                    }
                    break;
                case ArrowType.Rectangle:
                    {
                        var pts = new PointF[] { new PointF(x - (float)sx, y) };
                        rt.TransformPoints(pts);
                        pt = pts[0];
                        var rect = new Rect2(x - sx, y - ry, sx, sy);
                        var gs = gfx.Save();
                        gfx.MultiplyTransform(rt);
                        DrawRectangleInternal(gfx, brush, pen, style.IsStroked, style.IsFilled, ref rect);
                        gfx.Restore(gs);
                    }
                    break;
                case ArrowType.Ellipse:
                    {
                        var pts = new PointF[] { new PointF(x - (float)sx, y) };
                        rt.TransformPoints(pts);
                        pt = pts[0];
                        var gs = gfx.Save();
                        gfx.MultiplyTransform(rt);
                        var rect = new Rect2(x - sx, y - ry, sx, sy);
                        DrawEllipseInternal(gfx, brush, pen, style.IsStroked, style.IsFilled, ref rect);
                        gfx.Restore(gs);
                    }
                    break;
                case ArrowType.Arrow:
                    {
                        var pts = new PointF[]
                        {
                            new PointF(x, y),
                            new PointF(x - (float)sx, y + (float)sy),
                            new PointF(x, y),
                            new PointF(x - (float)sx, y - (float)sy),
                            new PointF(x, y)
                        };
                        rt.TransformPoints(pts);
                        pt = pts[0];
                        var p11 = pts[1];
                        var p21 = pts[2];
                        var p12 = pts[3];
                        var p22 = pts[4];
                        DrawLineInternal(gfx, pen, style.IsStroked, ref p11, ref p21);
                        DrawLineInternal(gfx, pen, style.IsStroked, ref p12, ref p22);
                    }
                    break;
            }

            return pt;
        }

        private static void DrawRectangleInternal(Graphics gfx, Brush brush, Pen pen, bool isStroked, bool isFilled, ref Rect2 rect)
        {
            if (isFilled)
            {
                gfx.FillRectangle(
                    brush,
                    (float)rect.X,
                    (float)rect.Y,
                    (float)rect.Width,
                    (float)rect.Height);
            }

            if (isStroked)
            {
                gfx.DrawRectangle(
                    pen,
                    (float)rect.X,
                    (float)rect.Y,
                    (float)rect.Width,
                    (float)rect.Height);
            }
        }

        private static void DrawEllipseInternal(Graphics gfx, Brush brush, Pen pen, bool isStroked, bool isFilled, ref Rect2 rect)
        {
            if (isFilled)
            {
                gfx.FillEllipse(
                    brush,
                    (float)rect.X,
                    (float)rect.Y,
                    (float)rect.Width,
                    (float)rect.Height);
            }

            if (isStroked)
            {
                gfx.DrawEllipse(
                    pen,
                    (float)rect.X,
                    (float)rect.Y,
                    (float)rect.Width,
                    (float)rect.Height);
            }
        }

        private void DrawGridInternal(Graphics gfx, Pen stroke, ref Rect2 rect, double offsetX, double offsetY, double cellWidth, double cellHeight, bool isStroked)
        {
            double ox = rect.X;
            double oy = rect.Y;
            double sx = ox + offsetX;
            double sy = oy + offsetY;
            double ex = ox + rect.Width;
            double ey = oy + rect.Height;

            for (double x = sx; x < ex; x += cellWidth)
            {
                var p0 = new PointF(
                    _scaleToPage(x),
                    _scaleToPage(oy));
                var p1 = new PointF(
                    _scaleToPage(x),
                    _scaleToPage(ey));
                DrawLineInternal(gfx, stroke, isStroked, ref p0, ref p1);
            }

            for (double y = sy; y < ey; y += cellHeight)
            {
                var p0 = new PointF(
                    _scaleToPage(ox),
                    _scaleToPage(y));
                var p1 = new PointF(
                    _scaleToPage(ex),
                    _scaleToPage(y));
                DrawLineInternal(gfx, stroke, isStroked, ref p0, ref p1);
            }
        }

        private Matrix ToMatrix(MatrixObject matrix)
        {
            return new Matrix(
                (float)matrix.M11, (float)matrix.M12,
                (float)matrix.M21, (float)matrix.M22,
                (float)matrix.OffsetX, (float)matrix.OffsetY);
        }

        /// <inheritdoc/>
        public override void ClearCache(bool isZooming)
        {
            if (!isZooming)
            {
                if (_biCache != null)
                {
                    foreach (var kvp in _biCache)
                    {
                        kvp.Value.Dispose();
                    }
                    _biCache.Clear();
                }
                _biCache = new Dictionary<string, Image>();
            }
        }

        /// <inheritdoc/>
        public override void Fill(object dc, double x, double y, double width, double height, ArgbColor color)
        {
            var _gfx = dc as Graphics;
            Brush brush = ToSolidBrush(color);
            _gfx.FillRectangle(
                brush,
                (float)x,
                (float)y,
                (float)width,
                (float)height);
            brush.Dispose();
        }

        /// <inheritdoc/>
        public override object PushMatrix(object dc, MatrixObject matrix)
        {
            var _gfx = dc as Graphics;
            var state = _gfx.Save();
            _gfx.MultiplyTransform(ToMatrix(matrix));
            return state;
        }

        /// <inheritdoc/>
        public override void PopMatrix(object dc, object state)
        {
            var _gfx = dc as Graphics;
            var _state = state as GraphicsState;
            _gfx.Restore(_state);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, LineShape line, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            Pen strokeLine = ToPen(line.Style, _scaleToPage);
            DrawLineArrowsInternal(line, dx, dy, _gfx, out PointF pt1, out PointF pt2);

            if (line.Style.LineStyle.IsCurved)
            {
                DrawLineCurveInternal(
                    _gfx,
                    strokeLine, line.IsStroked,
                    ref pt1, ref pt2,
                    line.Style.LineStyle.Curvature,
                    line.Style.LineStyle.CurveOrientation,
                    line.Start.Alignment,
                    line.End.Alignment);
            }
            else
            {
                DrawLineInternal(_gfx, strokeLine, line.IsStroked, ref pt1, ref pt2);
            }

            strokeLine.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, RectangleShape rectangle, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            Brush brush = ToSolidBrush(rectangle.Style.Fill);
            Pen pen = ToPen(rectangle.Style, _scaleToPage);

            var rect = CreateRect(
                rectangle.TopLeft,
                rectangle.BottomRight,
                dx, dy);

            if (rectangle.IsFilled)
            {
                _gfx.FillRectangle(
                    brush,
                    _scaleToPage(rect.X),
                    _scaleToPage(rect.Y),
                    _scaleToPage(rect.Width),
                    _scaleToPage(rect.Height));
            }

            if (rectangle.IsStroked)
            {
                _gfx.DrawRectangle(
                    pen,
                    _scaleToPage(rect.X),
                    _scaleToPage(rect.Y),
                    _scaleToPage(rect.Width),
                    _scaleToPage(rect.Height));
            }

            if (rectangle.IsGrid)
            {
                DrawGridInternal(
                    _gfx,
                    pen,
                    ref rect,
                    rectangle.OffsetX, rectangle.OffsetY,
                    rectangle.CellWidth, rectangle.CellHeight,
                    true);
            }

            brush.Dispose();
            pen.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, EllipseShape ellipse, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            Brush brush = ToSolidBrush(ellipse.Style.Fill);
            Pen pen = ToPen(ellipse.Style, _scaleToPage);

            var rect = CreateRect(
                ellipse.TopLeft,
                ellipse.BottomRight,
                dx, dy);

            if (ellipse.IsFilled)
            {
                _gfx.FillEllipse(
                    brush,
                    _scaleToPage(rect.X),
                    _scaleToPage(rect.Y),
                    _scaleToPage(rect.Width),
                    _scaleToPage(rect.Height));
            }

            if (ellipse.IsStroked)
            {
                _gfx.DrawEllipse(
                    pen,
                    _scaleToPage(rect.X),
                    _scaleToPage(rect.Y),
                    _scaleToPage(rect.Width),
                    _scaleToPage(rect.Height));
            }

            brush.Dispose();
            pen.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, ArcShape arc, double dx, double dy, object db, object r)
        {
            var a = new GdiArc(
                Point2.FromXY(arc.Point1.X, arc.Point1.Y),
                Point2.FromXY(arc.Point2.X, arc.Point2.Y),
                Point2.FromXY(arc.Point3.X, arc.Point3.Y),
                Point2.FromXY(arc.Point4.X, arc.Point4.Y));
            if (a.Width <= 0.0 || a.Height <= 0.0)
                return;

            var _gfx = dc as Graphics;

            Brush brush = ToSolidBrush(arc.Style.Fill);
            Pen pen = ToPen(arc.Style, _scaleToPage);

            if (arc.IsFilled)
            {
                var path = new GraphicsPath();
                path.AddArc(
                    _scaleToPage(a.X + dx),
                    _scaleToPage(a.Y + dy),
                    _scaleToPage(a.Width),
                    _scaleToPage(a.Height),
                    (float)a.StartAngle,
                    (float)a.SweepAngle);
                _gfx.FillPath(brush, path);
            }

            if (arc.IsStroked)
            {
                _gfx.DrawArc(
                    pen,
                    _scaleToPage(a.X + dx),
                    _scaleToPage(a.Y + dy),
                    _scaleToPage(a.Width),
                    _scaleToPage(a.Height),
                    (float)a.StartAngle,
                    (float)a.SweepAngle);
            }

            brush.Dispose();
            pen.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, CubicBezierShape cubicBezier, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            Brush brush = ToSolidBrush(cubicBezier.Style.Fill);
            Pen pen = ToPen(cubicBezier.Style, _scaleToPage);

            if (cubicBezier.IsFilled)
            {
                var path = new GraphicsPath();
                path.AddBezier(
                    _scaleToPage(cubicBezier.Point1.X),
                    _scaleToPage(cubicBezier.Point1.Y),
                    _scaleToPage(cubicBezier.Point2.X),
                    _scaleToPage(cubicBezier.Point2.Y),
                    _scaleToPage(cubicBezier.Point3.X),
                    _scaleToPage(cubicBezier.Point3.Y),
                    _scaleToPage(cubicBezier.Point4.X),
                    _scaleToPage(cubicBezier.Point4.Y));
                _gfx.FillPath(brush, path);
            }

            if (cubicBezier.IsStroked)
            {
                _gfx.DrawBezier(
                    pen,
                    _scaleToPage(cubicBezier.Point1.X),
                    _scaleToPage(cubicBezier.Point1.Y),
                    _scaleToPage(cubicBezier.Point2.X),
                    _scaleToPage(cubicBezier.Point2.Y),
                    _scaleToPage(cubicBezier.Point3.X),
                    _scaleToPage(cubicBezier.Point3.Y),
                    _scaleToPage(cubicBezier.Point4.X),
                    _scaleToPage(cubicBezier.Point4.Y));
            }

            brush.Dispose();
            pen.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, QuadraticBezierShape quadraticBezier, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            Brush brush = ToSolidBrush(quadraticBezier.Style.Fill);
            Pen pen = ToPen(quadraticBezier.Style, _scaleToPage);

            double x1 = quadraticBezier.Point1.X;
            double y1 = quadraticBezier.Point1.Y;
            double x2 = quadraticBezier.Point1.X + (2.0 * (quadraticBezier.Point2.X - quadraticBezier.Point1.X)) / 3.0;
            double y2 = quadraticBezier.Point1.Y + (2.0 * (quadraticBezier.Point2.Y - quadraticBezier.Point1.Y)) / 3.0;
            double x3 = x2 + (quadraticBezier.Point3.X - quadraticBezier.Point1.X) / 3.0;
            double y3 = y2 + (quadraticBezier.Point3.Y - quadraticBezier.Point1.Y) / 3.0;
            double x4 = quadraticBezier.Point3.X;
            double y4 = quadraticBezier.Point3.Y;

            if (quadraticBezier.IsFilled)
            {
                var path = new GraphicsPath();
                path.AddBezier(
                    _scaleToPage(x1 + dx),
                    _scaleToPage(y1 + dy),
                    _scaleToPage(x2 + dx),
                    _scaleToPage(y2 + dy),
                    _scaleToPage(x3 + dx),
                    _scaleToPage(y3 + dy),
                    _scaleToPage(x4 + dx),
                    _scaleToPage(y4 + dy));
                _gfx.FillPath(brush, path);
            }

            if (quadraticBezier.IsStroked)
            {
                _gfx.DrawBezier(
                    pen,
                    _scaleToPage(x1 + dx),
                    _scaleToPage(y1 + dy),
                    _scaleToPage(x2 + dx),
                    _scaleToPage(y2 + dy),
                    _scaleToPage(x3 + dx),
                    _scaleToPage(y3 + dy),
                    _scaleToPage(x4 + dx),
                    _scaleToPage(y4 + dy));
            }

            brush.Dispose();
            pen.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, TextShape text, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            var properties = (ImmutableArray<Property>)db;
            var record = (Record)r;
            var tbind = text.BindText(properties, record);
            if (string.IsNullOrEmpty(tbind))
                return;

            Brush brush = ToSolidBrush(text.Style.Stroke);

            var fontStyle = System.Drawing.FontStyle.Regular;
            if (text.Style.TextStyle.FontStyle != null)
            {
                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(Core2D.Style.FontStyleFlags.Bold))
                {
                    fontStyle |= System.Drawing.FontStyle.Bold;
                }

                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(Core2D.Style.FontStyleFlags.Italic))
                {
                    fontStyle |= System.Drawing.FontStyle.Italic;
                }

                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(Core2D.Style.FontStyleFlags.Underline))
                {
                    fontStyle |= System.Drawing.FontStyle.Underline;
                }

                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(Core2D.Style.FontStyleFlags.Strikeout))
                {
                    fontStyle |= System.Drawing.FontStyle.Strikeout;
                }
            }

            Font font = new Font(
                text.Style.TextStyle.FontName,
                (float)(text.Style.TextStyle.FontSize * _textScaleFactor),
                fontStyle);

            var rect = CreateRect(
                text.TopLeft,
                text.BottomRight,
                dx, dy);

            var srect = new RectangleF(
                _scaleToPage(rect.X),
                _scaleToPage(rect.Y),
                _scaleToPage(rect.Width),
                _scaleToPage(rect.Height));

            var format = new StringFormat();
            switch (text.Style.TextStyle.TextHAlignment)
            {
                case TextHAlignment.Left:
                    format.Alignment = StringAlignment.Near;
                    break;
                case TextHAlignment.Center:
                    format.Alignment = StringAlignment.Center;
                    break;
                case TextHAlignment.Right:
                    format.Alignment = StringAlignment.Far;
                    break;
            }

            switch (text.Style.TextStyle.TextVAlignment)
            {
                case TextVAlignment.Top:
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case TextVAlignment.Center:
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case TextVAlignment.Bottom:
                    format.LineAlignment = StringAlignment.Far;
                    break;
            }

            format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
            format.Trimming = StringTrimming.None;

            _gfx.DrawString(
                tbind,
                font,
                ToSolidBrush(text.Style.Stroke),
                srect,
                format);

            brush.Dispose();
            font.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, ImageShape image, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            Brush brush = ToSolidBrush(image.Style.Stroke);

            var rect = CreateRect(
                image.TopLeft,
                image.BottomRight,
                dx, dy);

            var srect = new RectangleF(
                _scaleToPage(rect.X),
                _scaleToPage(rect.Y),
                _scaleToPage(rect.Width),
                _scaleToPage(rect.Height));

            if (image.IsFilled)
            {
                _gfx.FillRectangle(
                    ToSolidBrush(image.Style.Fill),
                    srect);
            }

            if (image.IsStroked)
            {
                _gfx.DrawRectangle(
                    ToPen(image.Style, _scaleToPage),
                    srect.X,
                    srect.Y,
                    srect.Width,
                    srect.Height);
            }

            if (_enableImageCache
                && _biCache.ContainsKey(image.Key))
            {
                _gfx.DrawImage(_biCache[image.Key], srect);
            }
            else
            {
                if (State.ImageCache == null || string.IsNullOrEmpty(image.Key))
                    return;

                var bytes = State.ImageCache.GetImage(image.Key);
                if (bytes != null)
                {
                    var ms = new System.IO.MemoryStream(bytes);
                    var bi = Image.FromStream(ms);
                    ms.Dispose();

                    if (_enableImageCache)
                        _biCache[image.Key] = bi;

                    _gfx.DrawImage(bi, srect);

                    if (!_enableImageCache)
                        bi.Dispose();
                }
            }

            brush.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, PathShape path, double dx, double dy, object db, object r)
        {
            var _gfx = dc as Graphics;

            var gp = path.Geometry.ToGraphicsPath(dx, dy, _scaleToPage);

            if (path.IsFilled && path.IsStroked)
            {
                var brush = ToSolidBrush(path.Style.Fill);
                var pen = ToPen(path.Style, _scaleToPage);
                _gfx.FillPath(
                    brush,
                    gp);
                _gfx.DrawPath(
                    pen,
                    gp);
                brush.Dispose();
                pen.Dispose();
            }
            else if (path.IsFilled && !path.IsStroked)
            {
                var brush = ToSolidBrush(path.Style.Fill);
                _gfx.FillPath(
                    brush,
                    gp);
                brush.Dispose();
            }
            else if (!path.IsFilled && path.IsStroked)
            {
                var pen = ToPen(path.Style, _scaleToPage);
                _gfx.DrawPath(
                    pen,
                    gp);
                pen.Dispose();
            }
        }
    }
}
