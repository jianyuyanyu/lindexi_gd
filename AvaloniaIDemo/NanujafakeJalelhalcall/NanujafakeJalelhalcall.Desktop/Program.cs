﻿using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;

namespace NanujafakeJalelhalcall.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            // 修复麒麟丢失字体
            .With(new FontManagerOptions()
            {
                DefaultFamilyName = "Noto Sans CJK SC",
                FontFallbacks =
                [
                    new FontFallback { FontFamily = "文泉驿正黑" },
                    new FontFallback { FontFamily = "DejaVu Sans" },
                ],
            })
            .With(new X11PlatformOptions()
            {
                RenderingMode = new List<X11RenderingMode>()
                {
                    X11RenderingMode.Software
                }
            })
            .UseReactiveUI();
}
