using System;
using System.Collections.Generic;
using LightTextEditorPlus.Core.Primitive.Collections;
using System.Text;
using LightTextEditorPlus.Core.Utils;

namespace LightTextEditorPlus.Core.Document;

/// <summary>
/// 字符数据只读列表的扩展方法
/// </summary>
public static class CharDataReadOnlyListSpanExtension
{
    /// <summary>
    /// 将字符数据列表转换为文本
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string ToText(this TextReadOnlyListSpan<CharData> list)
    {
        var stringBuilder = new StringBuilder();
        foreach (CharData charData in list)
        {
            stringBuilder.Append(charData.CharObject.ToText());
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 转换为限制字符数量的文本
    /// </summary>
    /// <param name="list"></param>
    /// <param name="limitCharCount"></param>
    /// <param name="replaceText"></param>
    /// <param name="saveStartAndEnd">保留前后，删除中间</param>
    /// <returns></returns>
    internal static string ToLimitText(this TextReadOnlyListSpan<CharData> list, int limitCharCount, string? replaceText = null, bool saveStartAndEnd = true)
    {
        // 这个方法现在只有调试下调用进来，先不管其性能
        string text = list.ToText();
        return text.LimitTrim(limitCharCount, replaceText, saveStartAndEnd);
    }

    /// <inheritdoc cref="GetFirstCharSpanContinuous"/>
    public static IEnumerable<TextReadOnlyListSpan<CharData>> GetCharSpanContinuous(this TextReadOnlyListSpan<CharData> charList, CheckCharDataContinuous? checker = null)
    {
        var currentList = charList;
        while (currentList.Count > 0)
        {
            var firstSpan = currentList.GetFirstCharSpanContinuous(checker);
            yield return firstSpan;
            currentList = currentList.Slice(start: firstSpan.Count);
        }
    }

    /// <summary>
    /// 获取首段连续的字符数据。从传入的 <paramref name="charList"/> 里截取首段连续的字符数据
    /// </summary>
    /// <param name="charList"></param>
    /// <param name="checker">默认不传将只判断 <see cref="CharData.RunProperty"/> 部分</param>
    /// <returns></returns>
    public static TextReadOnlyListSpan<CharData> GetFirstCharSpanContinuous(this TextReadOnlyListSpan<CharData> charList, CheckCharDataContinuous? checker = null)
    {
        if (charList.Count == 0)
        {
            return charList;
        }

        var count = 1;
        var current = charList[0];
        for (int i = 1; i < charList.Count; i++)
        {
            var next = charList[i];
            if (checker is null)
            {
                if (current.RunProperty != next.RunProperty)
                {
                    break;
                }
            }
            else
            {
                if (!checker(current, next))
                {
                    break;
                }
            }
            count++;
            current = next; // 这步有些多余，但是为了代码的可读性，还是加上了
        }

        return charList.Slice(0, count);
    }
}

/// <summary>
/// 判断两个字符数据是否连续，即判断是否属性等相同或可以作为连续的字符
/// </summary>
/// <returns></returns>
public delegate bool CheckCharDataContinuous(CharData current, CharData next);
