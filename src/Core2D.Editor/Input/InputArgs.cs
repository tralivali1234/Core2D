﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Core2D.Editor.Input
{
    /// <summary>
    /// Input arguments.
    /// </summary>
    public struct InputArgs
    {
        /// <summary>
        /// Gets input X position.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets input Y position.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Gets input modifier flags.
        /// </summary>
        public ModifierFlags Modifier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgs"/> struct.
        /// </summary>
        /// <param name="x">The input X position.</param>
        /// <param name="y">The input Y position.</param>
        /// <param name="modifier">The input modifier flags.</param>
        public InputArgs(double x, double y, ModifierFlags modifier)
        {
            this.X = x;
            this.Y = y;
            this.Modifier = modifier;
        }
        
        public void Deconstruct(out double x, out double y)
        {
            x = this.X;
            y = this.Y;
        }
        
        public void Deconstruct(out double x, out double y, out ModifierFlags modifier)
        {
            x = this.X;
            y = this.Y;
            modifier = this.Modifier;
        }
    }
}
