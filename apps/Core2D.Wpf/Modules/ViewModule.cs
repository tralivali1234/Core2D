﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Windows.Input;
using Autofac;
using Core2D.Editor.Views.Interfaces;
using Core2D.Wpf.Windows;

namespace Core2D.Wpf.Modules
{
    /// <summary>
    /// View components module.
    /// </summary>
    public class ViewModule : Module
    {
        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(App).Assembly).AssignableTo<ICommand>().AsImplementedInterfaces().AsSelf().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(App).Assembly).As<IView>().InstancePerLifetimeScope();
            builder.RegisterType<MainWindow>().As<MainWindow>().InstancePerLifetimeScope();
        }
    }
}
