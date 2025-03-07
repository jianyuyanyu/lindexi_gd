using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightTextEditorPlus.Core.Document;

namespace LightTextEditorPlus.Core.Primitive.Collections;

/// <summary>
/// 表示一个 List 里面一段只读集合
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>无法处理传入的 source 实际源被更改问题。在 source 不变更情况下，读取是线程安全。使用结构体能够减少 GC 压力</remarks>
public readonly struct TextReadOnlyListSpan<T> : IReadOnlyList<T>, IEquatable<TextReadOnlyListSpan<T>>
{
    /// <inheritdoc cref="TextReadOnlyListSpan{T}"/>
    public TextReadOnlyListSpan(IReadOnlyList<T> source, int start, int length)
    {
        _source = source;
        _start = start;
        _length = length;
    }

    private readonly IReadOnlyList<T> _source;
    private readonly int _start;
    private readonly int _length;

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return _source.Skip(_start).Take(_length).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public int Count => _length;

    /// <inheritdoc />
    public T this[int index] => _source[index + _start];

    /// <summary>
    /// 分出给定范围的新列表
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public TextReadOnlyListSpan<T> Slice(int start)
    {
        var length = _length - start;
        return Slice(start, length);
    }

    /// <inheritdoc cref="Slice(int)"/>
    public TextReadOnlyListSpan<T> Slice(int start, int length)
    {
        if (length + start > _length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        return new TextReadOnlyListSpan<T>(_source, _start + start, length);
    }

    /// <inheritdoc />
    public bool Equals(TextReadOnlyListSpan<T> other)
    {
        return ReferenceEquals(_source, other._source) && _start == other._start && _length == other._length;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TextReadOnlyListSpan<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_source, _start, _length);
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        if (this is TextReadOnlyListSpan<CharData> textReadOnlyListSpan)
        {
            return textReadOnlyListSpan.ToText();
        }
        else if (this is TextReadOnlyListSpan<ICharObject> charObjectListSpan)
        {
            var stringBuilder = new StringBuilder();
            foreach (ICharObject charObject in charObjectListSpan)
            {
                stringBuilder.Append(charObject.ToText());
            }
            return stringBuilder.ToString();
        }

        return base.ToString();
    }

    internal TextReadOnlyListSpan<TOther> Cast<TOther>(Func<T, TOther> converter)
    {
        var list = new ReadOnlyListConverter<T, TOther>(_source, converter);
        return new TextReadOnlyListSpan<TOther>(list, _start, _length);
    }
}

file class ReadOnlyListConverter<TOrigin, TOther> : IReadOnlyList<TOther>
{
    public ReadOnlyListConverter(IReadOnlyList<TOrigin> list, Func<TOrigin, TOther> converter)
    {
        _list = list;
        _converter = converter;
    }

    private readonly IReadOnlyList<TOrigin> _list;

    private readonly Func<TOrigin, TOther> _converter;

    public IEnumerator<TOther> GetEnumerator()
    {
        return _list.Select(origin => _converter(origin)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _list.Count;

    public TOther this[int index]
    {
        get => _converter(_list[index]);
    }
}