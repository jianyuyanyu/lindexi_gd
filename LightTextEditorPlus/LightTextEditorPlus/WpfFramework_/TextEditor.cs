﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Threading;

using LightTextEditorPlus.Core;
using LightTextEditorPlus.Core.Carets;
using LightTextEditorPlus.Core.Document;
using LightTextEditorPlus.Core.Layout;
using LightTextEditorPlus.Core.Platform;
using LightTextEditorPlus.Core.Primitive;
using LightTextEditorPlus.Core.Rendering;
using LightTextEditorPlus.Core.Utils;
using LightTextEditorPlus.Document;
using LightTextEditorPlus.Layout;
using LightTextEditorPlus.TextEditorPlus.Render;
using LightTextEditorPlus.Utils.Threading;

using Microsoft.Win32;

using Point = LightTextEditorPlus.Core.Primitive.Point;
using Rect = LightTextEditorPlus.Core.Primitive.Rect;
using Size = LightTextEditorPlus.Core.Primitive.Size;

namespace LightTextEditorPlus;

public partial class TextEditor : FrameworkElement, IRenderManager
{
    public TextEditor()
    {
        TextView = new TextView(this);
        // 加入视觉树，方便调试和方便触发视觉变更
        AddVisualChild(TextView);
        AddLogicalChild(TextView);

        #region 清晰文本

        SnapsToDevicePixels = true;
        RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

        #endregion

        #region 配置文本

        var textEditorPlatformProvider = new TextEditorPlatformProvider(this);
        TextEditorCore = new TextEditorCore(textEditorPlatformProvider);
        SetDefaultTextRunProperty(property =>
        {
            property.FontSize = 30;
        });

        TextEditorPlatformProvider = textEditorPlatformProvider;

        #endregion

        Loaded += TextEditor_Loaded;
    }

    private void TextEditor_Loaded(object sender, RoutedEventArgs e)
    {
    }

    #region 公开属性

    public TextEditorCore TextEditorCore { get; }

    /// <summary>
    /// 文本库的静态配置
    /// </summary>
    public static StaticConfiguration StaticConfiguration { get; } = new StaticConfiguration();

    #endregion

    #region 公开方法

    /// <summary>
    /// 设置当前文本的默认字符属性
    /// </summary>
    public void SetDefaultTextRunProperty(Action<RunProperty> config)
    {
        TextEditorCore.DocumentManager.SetDefaultTextRunProperty<RunProperty>(config);
    }

    /// <summary>
    /// 设置当前光标的字符属性。在光标切走之后，自动失效
    /// </summary>
    public void SetCurrentCaretRunProperty(Action<RunProperty> config)
        => TextEditorCore.DocumentManager.SetCurrentCaretRunProperty<RunProperty>(config);

    public void SetRunProperty(Action<RunProperty> config, Selection? selection = null)
        => TextEditorCore.DocumentManager.SetRunProperty(config, selection);

    #endregion

    #region 框架
    /// <summary>
    /// 视觉呈现容器
    /// </summary>
    private TextView TextView { get; }
    protected override int VisualChildrenCount => 1; // 当前只有视觉呈现容器一个而已
    protected override Visual GetVisualChild(int index) => TextView;

    internal TextEditorPlatformProvider TextEditorPlatformProvider { get; }

    void IRenderManager.Render(RenderInfoProvider renderInfoProvider)
    {
        TextView.Render(renderInfoProvider);
    }

    #endregion
}

internal class TextEditorPlatformProvider : PlatformProvider
{
    public TextEditorPlatformProvider(TextEditor textEditor)
    {
        TextEditor = textEditor;

        _textLayoutDispatcherRequiring = new DispatcherRequiring(UpdateLayout, DispatcherPriority.Render);
        _charInfoMeasurer = new CharInfoMeasurer(textEditor);
        _runPropertyCreator = new RunPropertyCreator();
    }

    private void UpdateLayout()
    {
        Debug.Assert(_lastTextLayout is not null);
        _lastTextLayout?.Invoke();
    }

    private TextEditor TextEditor { get; }
    private readonly DispatcherRequiring _textLayoutDispatcherRequiring;
    private Action? _lastTextLayout;

    public override void RequireDispatchUpdateLayout(Action textLayout)
    {
        _lastTextLayout = textLayout;
        _textLayoutDispatcherRequiring.Require();
    }

    public override ICharInfoMeasurer? GetCharInfoMeasurer()
    {
        return _charInfoMeasurer;
    }

    private readonly CharInfoMeasurer _charInfoMeasurer;

    public override IRenderManager? GetRenderManager()
    {
        return TextEditor;
    }

    public override IPlatformRunPropertyCreator GetPlatformRunPropertyCreator() => _runPropertyCreator;

    private readonly RunPropertyCreator _runPropertyCreator; //= new RunPropertyCreator();
}

class CharInfoMeasurer : ICharInfoMeasurer
{
    public CharInfoMeasurer(TextEditor textEditor)
    {
        _textEditor = textEditor;
    }
    private readonly TextEditor _textEditor;

    public CharInfoMeasureResult MeasureCharInfo(in CharInfo charInfo)
    {
        GlyphTypeface glyphTypeface = charInfo.RunProperty.AsRunProperty().GetGlyphTypeface();
        var fontSize = charInfo.RunProperty.FontSize;

        Size size;

        if (_textEditor.TextEditorCore.ArrangingType == ArrangingType.Horizontal)
        {
            if (charInfo.CharObject is SingleCharObject singleCharObject)
            {
                var (width, height) = MeasureChar(singleCharObject.GetChar());
                size = new Size(width, height);
            }
            else
            {
                size = Size.Zero;

                var text = charInfo.CharObject.ToText();

                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];

                    var (width, height) = MeasureChar(c);

                    size = size.HorizontalUnion(width, height);
                }
            }

            (double width, double height) MeasureChar(char c)
            {
                var glyphIndex = glyphTypeface.CharacterToGlyphMap[c];

                var width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
                width = GlyphExtension.RefineValue(width);
                var height = glyphTypeface.AdvanceHeights[glyphIndex] * fontSize;

                return (width, height);
            }
        }
        else
        {
            throw new NotImplementedException("还没有实现竖排的文本测量");
        }

        return new CharInfoMeasureResult(new Rect(new Point(), size));
    }
}