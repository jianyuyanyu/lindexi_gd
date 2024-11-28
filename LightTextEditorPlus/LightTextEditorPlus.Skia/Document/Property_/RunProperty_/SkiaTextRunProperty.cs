﻿using LightTextEditorPlus.Core.Document;
using LightTextEditorPlus.Core.Primitive;
using SkiaSharp;

namespace LightTextEditorPlus.Document;

[APIConstraint("RunProperty.txt")]
public record SkiaTextRunProperty : LayoutOnlyRunProperty
{
    internal SkiaTextRunProperty(SkiaPlatformFontManager skiaPlatformFontManager)
    {
        FontManager = skiaPlatformFontManager;
    }

    public override FontName FontName
    {
        get => _fontName;
        init
        {
            if (value.Equals(_fontName))
            {
                return;
            }

            _fontName = value;
            InvalidateFont();
        }
    }

    private readonly FontName _fontName;
    private SkiaPlatformFontManager FontManager { get; }

    public SKTypeface GetRenderSKTypeface(char unicodeChar = '1')
    {
        RenderingFontInfo renderingFontInfo = FontManager.GetRenderingFontInfo(this, unicodeChar, ref _skTypeface);

        return renderingFontInfo.Typeface;
    }

    /// <summary>
    /// 字体
    /// </summary>
    /// 在 Skia 底层会自己释放。只是需要等 GC 才释放资源
    private SKTypeface? _skTypeface;

    public SKFont GetRenderSKFont(char unicodeChar = '1')
    {
        // todo 处理对齐情况
        // todo 处理缓存
        SKFont renderSkFont = new SKFont(GetRenderSKTypeface(unicodeChar), (float)FontSize);
        // From Avalonia
        // Ideally the requested edging should be passed to the glyph run.
        // Currently the edging is computed dynamically inside the drawing context, so we can't know it in advance.
        // But the bounds depends on the edging: for now, always use SubpixelAntialias so we have consistent values.
        // The resulting bounds may be shifted by 1px on some fonts:
        // "F" text with Inter size 14 has a 0px left bound with SubpixelAntialias but 1px with Antialias.

        var edging = SKFontEdging.SubpixelAntialias;

        renderSkFont.Hinting = SKFontHinting.Full;
        renderSkFont.Edging = edging;
        renderSkFont.Subpixel = edging != SKFontEdging.Alias;

        return renderSkFont;
    }

    public double Opacity { get; init; } = 1;
    public SKColor Foreground { get; init; } = SKColors.Black;
    public SKColor Background { get; init; } = SKColors.Transparent;

    /// <summary>
    /// 获取描述与某个字体与该字体的正常纵横比相比的拉伸程度
    /// </summary>
    public SKFontStyleWidth Stretch
    {
        get => _stretch;
        init
        {
            _stretch = value;
            InvalidateFont();
        }
    }

    private readonly SKFontStyleWidth _stretch = SKFontStyleWidth.Normal;

    /// <summary>
    /// 字的粗细度，默认值为Normal
    /// </summary>
    public SKFontStyleWeight FontWeight
    {
        get => _fontWeight;
        init
        {
            _fontWeight = value;
            InvalidateFont();
        }
    }

    private readonly SKFontStyleWeight _fontWeight = SKFontStyleWeight.Normal;

    /// <summary>
    /// 斜体
    /// </summary>
    public SKFontStyleSlant FontStyle
    {
        get => _fontStyle;
        init
        {
            _fontStyle = value;
            InvalidateFont();
        }
    }

    private readonly SKFontStyleSlant _fontStyle = SKFontStyleSlant.Upright;

    private void InvalidateFont()
    {
        // 由于 record 会自动拷贝字段，所以这里需要手动设置为 null 的值
        _skTypeface = null;
    }
}