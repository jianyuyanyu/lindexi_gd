﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LightTextEditorPlus.Core;
using LightTextEditorPlus.Core.Platform;
using LightTextEditorPlus.Core.Rendering;

namespace LightTextEditorPlus;

public partial class SkiaTextEditor : IRenderManager
{
    public SkiaTextEditor()
    {
        var skiaTextEditorPlatformProvider = new SkiaTextEditorPlatformProvider(this);
        TextEditorCore = new TextEditorCore(skiaTextEditorPlatformProvider);
    }

    public TextEditorCore TextEditorCore { get; }

    void IRenderManager.Render(RenderInfoProvider renderInfoProvider)
    {

    }


}


internal class SkiaTextEditorPlatformProvider : PlatformProvider
{
    public SkiaTextEditorPlatformProvider(SkiaTextEditor textEditor)
    {
        TextEditor = textEditor;
    }

    private SkiaTextEditor TextEditor { get; }
}