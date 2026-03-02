using Avalonia;
using LightTextEditorPlus.Core.Primitive;

namespace LightTextEditorPlus.Utils;

public static class TextUnitConverter
{
    internal static TextPoint ToTextPoint(this Point point)
        => new(point.X, point.Y);

    public static Point ToAvaloniaPoint(this TextPoint textPoint)
        => new(textPoint.X, textPoint.Y);

    internal static TextRect ToTextRect(this Rect rect)
        => TextRect.FromLeftTopRightBottom(rect.Left, rect.Top, rect.Right, rect.Bottom);

    public static Rect ToAvaloniaRect(this TextRect textRect)
        => new Rect(textRect.X, textRect.Y, textRect.Width, textRect.Height);
}