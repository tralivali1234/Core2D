﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Core2D.Shape;
using Core2D.Shapes;
using Spatial;

namespace Core2D.Editor.Bounds.Shapes
{
    public class HitTestPoint : HitTestBase
    {
        public override Type TargetType => typeof(PointShape);

        public override PointShape TryToGetPoint(BaseShape shape, Point2 target, double radius, IDictionary<Type, HitTestBase> registered)
        {
            var point = shape as PointShape;
            if (point == null)
                throw new ArgumentNullException(nameof(shape));

            if (Point2.FromXY(point.X, point.Y).ExpandToRect(radius).Contains(target.X, target.Y))
            {
                return point;
            }

            return null;
        }

        public override bool Contains(BaseShape shape, Point2 target, double radius, IDictionary<Type, HitTestBase> registered)
        {
            var point = shape as PointShape;
            if (point == null)
                throw new ArgumentNullException(nameof(shape));

            return Point2.FromXY(point.X, point.Y).ExpandToRect(radius).Contains(target.X, target.Y);
        }

        public override bool Overlaps(BaseShape shape, Rect2 target, double radius, IDictionary<Type, HitTestBase> registered)
        {
            var point = shape as PointShape;
            if (point == null)
                throw new ArgumentNullException(nameof(shape));

            return Point2.FromXY(point.X, point.Y).ExpandToRect(radius).IntersectsWith(target);
        }
    }
}
