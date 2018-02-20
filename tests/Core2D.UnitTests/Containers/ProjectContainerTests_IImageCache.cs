﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Containers;
using Core2D.Renderer;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Core2D.UnitTests
{
    public class ProjectContainerTests_IImageCache
    {
        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void Implements_IImageCache_Interface()
        {
            var target = new ProjectContainer();
            Assert.True(target is IImageCache);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void Inherits_From_ObservableObject()
        {
            var target = new ProjectContainer();
            Assert.True(target is ObservableObject);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void Keys_Not_Null()
        {
            var target = new ProjectContainer();
            Assert.NotNull(target.Keys);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void AddImageFromFile_Add_Key_And_Notify()
        {
            var target = new ProjectContainer();
            string actual = null;

            target.PropertyChanged += (sender, e) =>
            {
                actual = e.PropertyName;
            };

            var key = target.AddImageFromFile(@"C:/Images/image.jpg", new byte[] { });

            Assert.Equal(ProjectContainer.ImageEntryNamePrefix + "image.jpg", key);
            Assert.Contains(target.Keys, item => item.Key == key);
            Assert.Equal("Keys", actual);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void AddImageFromFile_Do_Not_Add_Duplicate()
        {
            var target = new ProjectContainer();
            int count = 0;

            target.PropertyChanged += (sender, e) =>
            {
                count++;
            };

            var key = ProjectContainer.ImageEntryNamePrefix + "image.jpg";
            var key1 = target.AddImageFromFile(@"C:/Images/image.jpg", new byte[] { });
            var key2 = target.AddImageFromFile(@"C:/Images/image.jpg", new byte[] { });

            Assert.Equal(key, key1);
            Assert.Equal(key, key2);
            Assert.Contains(target.Keys, item => item.Key == key);
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void AddImage_Add_Key_And_Notify()
        {
            var target = new ProjectContainer();
            string actual = null;

            target.PropertyChanged += (sender, e) =>
            {
                actual = e.PropertyName;
            };

            var key = ProjectContainer.ImageEntryNamePrefix + "image.jpg";

            target.AddImage(key, new byte[] { });

            Assert.Equal(key, target.Keys.First().Key);
            Assert.Contains(target.Keys, item => item.Key == key);
            Assert.Equal("Keys", actual);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void AddImage_Do_Not_Add_Duplicate()
        {
            var target = new ProjectContainer();
            int count = 0;

            target.PropertyChanged += (sender, e) =>
            {
                count++;
            };

            var key = ProjectContainer.ImageEntryNamePrefix + "image.jpg";

            target.AddImage(key, new byte[] { });
            target.AddImage(key, new byte[] { });

            Assert.Equal(key, target.Keys.First().Key);
            Assert.Contains(target.Keys, item => item.Key == key);
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void GetImage_Returns_Byte_Array()
        {
            var project = new ProjectContainer();

            var key = ProjectContainer.ImageEntryNamePrefix + "image.jpg";
            var data = new byte[] { 0x12, 0x34, 0x56, 0x78 };

            project.AddImage(key, data);

            var target = project.GetImage(key);

            Assert.Equal(data, target);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void GetImage_Returns_Null()
        {
            var project = new ProjectContainer();

            var key = ProjectContainer.ImageEntryNamePrefix + "image.jpg";

            var target = project.GetImage(key);

            Assert.Null(target);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void RemoveImage_Remove_Key_And_Notify()
        {
            var target = new ProjectContainer();
            int count = 0;

            target.PropertyChanged += (sender, e) =>
            {
                count++;
            };

            var key = ProjectContainer.ImageEntryNamePrefix + "image.jpg";

            target.AddImage(key, new byte[] { });
            target.RemoveImage(key);

            Assert.Empty(target.Keys);
            Assert.Equal(2, count);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void PurgeUnusedImages_Remove_All_Keys_And_Notify()
        {
            var target = new ProjectContainer();
            int count = 0;

            target.PropertyChanged += (sender, e) =>
            {
                count++;
            };

            var key1 = ProjectContainer.ImageEntryNamePrefix + "image1.jpg";
            var key2 = ProjectContainer.ImageEntryNamePrefix + "image2.jpg";
            var key3 = ProjectContainer.ImageEntryNamePrefix + "image3.jpg";

            target.AddImage(key1, new byte[] { });
            target.AddImage(key2, new byte[] { });
            target.AddImage(key3, new byte[] { });

            var used = Enumerable.Empty<string>().ToImmutableHashSet();

            target.PurgeUnusedImages(used);

            Assert.Empty(target.Keys);
            Assert.Equal(4, count);
        }

        [Fact]
        [Trait("Core2D.Containers", "IImageCache")]
        public void PurgeUnusedImages_Remove_Only_Unused_Keys_And_Notify()
        {
            var target = new ProjectContainer();
            int count = 0;

            target.PropertyChanged += (sender, e) =>
            {
                count++;
            };

            var key1 = ProjectContainer.ImageEntryNamePrefix + "image1.jpg";
            var key2 = ProjectContainer.ImageEntryNamePrefix + "image2.jpg";
            var key3 = ProjectContainer.ImageEntryNamePrefix + "image3.jpg";

            target.AddImage(key1, new byte[] { });
            target.AddImage(key2, new byte[] { });
            target.AddImage(key3, new byte[] { });

            var used = ImmutableHashSet.Create(key2);

            target.PurgeUnusedImages(used);

            Assert.Equal(key2, target.Keys.First().Key);
            Assert.Contains(target.Keys, item => item.Key == key2);
            Assert.Equal(4, count);
        }
    }
}
