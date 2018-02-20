﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Core2D.Shapes;

namespace Core2D.Path.Segments
{
    /// <summary>
    /// Cubic bezier path segment.
    /// </summary>
    public class CubicBezierSegment : PathSegment, ICopyable
    {
        private PointShape _point1;
        private PointShape _point2;
        private PointShape _point3;

        /// <summary>
        /// Gets or sets first control point.
        /// </summary>
        public PointShape Point1
        {
            get => _point1;
            set => Update(ref _point1, value);
        }

        /// <summary>
        /// Gets or sets second control point.
        /// </summary>
        public PointShape Point2
        {
            get => _point2;
            set => Update(ref _point2, value);
        }

        /// <summary>
        /// Gets or sets end point.
        /// </summary>
        public PointShape Point3
        {
            get => _point3;
            set => Update(ref _point3, value);
        }

        /// <inheritdoc/>
        public override IEnumerable<PointShape> GetPoints()
        {
            yield return Point1;
            yield return Point2;
            yield return Point3;
        }

        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object> shared)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new <see cref="CubicBezierSegment"/> instance.
        /// </summary>
        /// <param name="point1">The first control point.</param>
        /// <param name="point2">The second control point.</param>
        /// <param name="point3">The end point.</param>
        /// <param name="isStroked">The flag indicating whether shape is stroked.</param>
        /// <param name="isSmoothJoin">The flag indicating whether shape is smooth join.</param>
        /// <returns>The new instance of the <see cref="CubicBezierSegment"/> class.</returns>
        public static CubicBezierSegment Create(PointShape point1, PointShape point2, PointShape point3, bool isStroked, bool isSmoothJoin)
        {
            return new CubicBezierSegment()
            {
                Point1 = point1,
                Point2 = point2,
                Point3 = point3,
                IsStroked = isStroked,
                IsSmoothJoin = isSmoothJoin
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("C{1}{0}{2}{0}{2}", " ", Point1, Point2, Point3);
        }
        
        /// <summary>
        /// Check whether the <see cref="Point1"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePoint1() => _point1 != null;

        /// <summary>
        /// Check whether the <see cref="Point2"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePoint2() => _point2 != null;

        /// <summary>
        /// Check whether the <see cref="Point3"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePoint3() => _point3 != null;
    }
}
