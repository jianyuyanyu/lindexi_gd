﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

using LightTextEditorPlus.Core;
using LightTextEditorPlus.Core.Carets;
using LightTextEditorPlus.Core.Document;
using LightTextEditorPlus.Core.Primitive;
using LightTextEditorPlus.Utils;

using SizeToContent = LightTextEditorPlus.Core.Primitive.SizeToContent;

// ReSharper disable once CheckNamespace
namespace LightTextEditorPlus;

// 这里存放和 Avalonia 相关的代码
partial class TextEditor : Control
{
    public TextEditor()
    {
        // 属性初始化
        Focusable = true;

        SkiaTextEditor = new SkiaTextEditor();
        SkiaTextEditor.InvalidateVisualRequested += (sender, args) => InvalidateVisual();

        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        //// 调试代码
        //TextEditorCore.AppendText("afg123微软雅黑123123");

        // 设计上会导致 Avalonia 总会调用二级的 SkiaTextEditor 接口实现功能。有开发资源可以做一层代理
    }

    #region 交互

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        this.Focus(NavigationMethod.Directional);
        PointerPoint currentPoint = e.GetCurrentPoint(this);
        Point position = currentPoint.Position;
        TextPoint textPoint = position.ToTextPoint();

        if (TextEditorCore.TryHitTest(textPoint, out var result))
        {
            TextEditorCore.CurrentCaretOffset = result.HitCaretOffset;
        }
        base.OnPointerPressed(e);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        string? inputText = e.Text;
        // todo 处理 emoij 表情符号
        if (inputText != null)
        {
            TextEditorCore.EditAndReplace(inputText);
        }

        base.OnTextInput(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (TextEditorCore.IsDirty)
        {
            // 如果有明确布局的话，可以在这里加上明确布局
            // todo ForceRedraw
            return;
        }

        if (e.Key == Key.Delete)
        {
            TextEditorCore.Delete();
        }
        else if (e.Key == Key.Back)
        {
            TextEditorCore.Backspace();
        }
        else if (e.Key == Key.Enter)
        {
            TextEditorCore.EditAndReplace("\n");
        }
        else if (e.Key == Key.Up)
        {
            TextEditorCore.MoveCaret(CaretMoveType.UpByLine);
        }
        else if (e.Key == Key.Down)
        {
            TextEditorCore.MoveCaret(CaretMoveType.DownByLine);
        }
        else if (e.Key == Key.Left)
        {
            TextEditorCore.MoveCaret(CaretMoveType.LeftByCharacter);
        }
        else if (e.Key == Key.Right)
        {
            TextEditorCore.MoveCaret(CaretMoveType.RightByCharacter);
        }
    }

    #endregion

    #region 状态同步

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        // 获取焦点时，允许用户编辑，才能设置为编辑模式
        IsInEditingInputMode = IsEditable && true;
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        IsInEditingInputMode = false;
        base.OnLostFocus(e);
    }

    #endregion

    #region 布局

    protected override Size MeasureOverride(Size availableSize)
    {
        var result = base.MeasureOverride(availableSize);

        if (TextEditorCore.SizeToContent is SizeToContent.Width or SizeToContent.WidthAndHeight or SizeToContent.Height)
        {
            throw new NotImplementedException();
        }
        //else if (TextEditorCore.SizeToContent is SizeToContent.Width)
        //{
        //    // 宽度自适应，高度固定
        //    if (TextEditorCore.IsDirty)
        //    {
        //        TextEditorCore.ForceRedraw();
        //    }
        //    //TextEditorCore.GetDocumentLayoutBounds()
        //    return new Size(, availableSize.Height);
        //}
        else if (TextEditorCore.SizeToContent == SizeToContent.Manual)
        {
            // 手动的，有多少就要多少
            return availableSize;
        }

        // 文本库，有多少就要多少
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // 实际布局多大就使用多大
        TextEditorCore.DocumentManager.DocumentWidth = finalSize.Width;
        TextEditorCore.DocumentManager.DocumentHeight = finalSize.Height;

        return base.ArrangeOverride(finalSize);
    }

    #endregion

    #region 渲染

    public override void Render(DrawingContext context)
    {
        ITextEditorSkiaRender textEditorSkiaRender = SkiaTextEditor.GetCurrentTextRender();
        context.Custom(new TextEditorCustomDrawOperation(new Rect(DesiredSize), textEditorSkiaRender));

        if (IsInEditingInputMode)
        {
            // 只有编辑模式下才会绘制光标和选择区域
            context.Custom(new TextEditorCustomDrawOperation(new Rect(DesiredSize),
                SkiaTextEditor.GetCurrentCaretAndSelectionRender()));
        }
    }

    #endregion
}

class TextEditorCustomDrawOperation : ICustomDrawOperation
{
    public TextEditorCustomDrawOperation(Rect bounds, ITextEditorSkiaRender render)
    {
        _render = render;
        Bounds = bounds;

        render.AddReference();
    }

    private readonly ITextEditorSkiaRender _render;

    public void Dispose()
    {
        _render.ReleaseReference();
    }

    public bool Equals(ICustomDrawOperation? other)
    {
        return ReferenceEquals(_render, (other as TextEditorCustomDrawOperation)?._render);
    }

    public bool HitTest(Point p)
    {
        return Bounds.Contains(p);
    }

    public void Render(ImmediateDrawingContext context)
    {
        ISkiaSharpApiLeaseFeature? skiaSharpApiLeaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaSharpApiLeaseFeature != null)
        {
            using ISkiaSharpApiLease skiaSharpApiLease = skiaSharpApiLeaseFeature.Lease();
            _render.Render(skiaSharpApiLease.SkCanvas);
        }
        else
        {
            // 不支持 Skia 绘制
        }
    }

    public Rect Bounds { get; }
}