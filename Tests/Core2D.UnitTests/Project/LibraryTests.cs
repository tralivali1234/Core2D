﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Core2D.UnitTests
{
    public class LibraryTests
    {
        [Fact]
        public void Inherits_From_ObservableObject()
        {
            var target = new Library<Template>();
            Assert.True(target is ObservableObject);
        }

        [Fact]
        public void Items_Not_Null()
        {
            var target = new Library<Template>();
            Assert.NotNull(target.Items);
        }

        [Fact]
        public void Selected_Is_Null()
        {
            var target = new Library<Template>();
            Assert.Null(target.Selected);
        }

        [Fact]
        public void SetSelected_Sets_Selected()
        {
            var target = new Library<Template>();

            var item = Template.Create();
            target.Items = target.Items.Add(item);

            target.SetSelected(item);

            Assert.Equal(item, target.Selected);
        }
    }
}
