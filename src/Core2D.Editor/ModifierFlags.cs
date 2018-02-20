﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Core2D.Editor
{
    /// <summary>
    /// Specifies modifier flags.
    /// </summary>
    [Flags]
    public enum ModifierFlags
    {
        /// <summary>
        /// No modifiers.
        /// </summary>
        None = 0,

        /// <summary>
        /// Alt modifier.
        /// </summary>
        Alt = 1,

        /// <summary>
        /// Control modifier.
        /// </summary>
        Control = 2,

        /// <summary>
        /// Shift modifier.
        /// </summary>
        Shift = 4
    }
}
