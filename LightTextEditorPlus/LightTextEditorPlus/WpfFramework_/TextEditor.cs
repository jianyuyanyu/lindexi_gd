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
            property.FontSize = 40;
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
        _runPropertyCreator = new RunPropertyCreator(textEditor);
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

                var pixelsPerDip = (float) VisualTreeHelper.GetDpi(_textEditor).PixelsPerDip;
                var glyphIndices = new[] { glyphIndex };
                var advanceWidths = new[] { width };
                var characters = new[] { c };

                var location = new System.Windows.Point(0, 0);
                //var glyphRun = new GlyphRun
                //(
                //    glyphTypeface,
                //    bidiLevel: 0,
                //    isSideways: false,
                //    renderingEmSize: fontSize,
                //    pixelsPerDip: pixelsPerDip,
                //    glyphIndices: glyphIndices,
                //    baselineOrigin: location, // 设置文本的偏移量
                //    advanceWidths: advanceWidths, // 设置每个字符的字宽，也就是字号
                //    glyphOffsets: null, // 设置每个字符的偏移量，可以为空
                //    characters: characters,
                //    deviceFontName: null,
                //    clusterMap: null,
                //    caretStops: null,
                //    language: DefaultXmlLanguage
                //);
                //var computeInkBoundingBox = glyphRun.ComputeInkBoundingBox();

                //var matrix = new Matrix();
                //matrix.Translate(location.X, location.Y);
                //computeInkBoundingBox.Transform(matrix);
                ////相对于run.BuildGeometry().Bounds方法，run.ComputeInkBoundingBox()会多出一个厚度为1的框框，所以要减去
                //if (computeInkBoundingBox.Width >= 2 && computeInkBoundingBox.Height >= 2)
                //{
                //    computeInkBoundingBox.Inflate(-1, -1);
                //}

                //var bounds = computeInkBoundingBox;
                // 此方法计算的尺寸远远大于视觉效果

                //// 根据 EN 行高算法 height = fontSize * fontFamily.LineSpacing
                //// 不等于 glyphTypeface.AdvanceHeights[glyphIndex] * fontSize 的值
                //var fontFamily = new FontFamily("微软雅黑"); // 这里强行使用微软雅黑，只是为了测试
                //height = fontSize * fontFamily.LineSpacing;

                // 根据 PPT 行高算法
                // PPTPixelLineSpacing = (a * PPTFL * OriginLineSpacing + b) * FontSize
                // 其中 PPT 的行距计算的 a 和 b 为一次线性函数的方法，而 PPTFL 是 PPT Font Line Spacing 的意思，在 PPT 所有文字的行距都是这个值
                // 可以将 a 和 PPTFL 合并为 PPTFL 然后使用 a 代替，此时 a 和 b 是常量
                // PPTPixelLineSpacing = (a * OriginLineSpacing + b) * FontSize
                // 常量 a 和 b 的值如下
                // a = 1.2018;
                // b = 0.0034;
                // PPTFontLineSpacing = a;

                const double pptFontLineSpacing = 1.2018;
                const double b = 0.0034;
                const int lineSpacing = 1;

                height = (pptFontLineSpacing * lineSpacing + b) * fontSize;


                //return (bounds.Width, bounds.Height);
                return (width, height);
            }
        }
        else
        {
            throw new NotImplementedException("还没有实现竖排的文本测量");
        }

        return new CharInfoMeasureResult(new Rect(new Point(), size));
    }

    protected static XmlLanguage DefaultXmlLanguage { get; } =
        XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);
}