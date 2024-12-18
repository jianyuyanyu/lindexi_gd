﻿using System;
using LightTextEditorPlus.Core.Primitive;
using LightTextEditorPlus.Core.Utils.Maths;

namespace LightTextEditorPlus.Core.Document
{
    /// <summary>
    /// 仅布局支持的文本字符属性
    /// </summary>
    // todo 考虑属性系统支持设置是否影响布局，不影响布局的，例如改个颜色，可以不重新布局
    public record LayoutOnlyRunProperty : IReadOnlyRunProperty
    {
        /// <summary>
        /// 创建仅布局支持的文本字符属性
        /// </summary>
        public LayoutOnlyRunProperty()
        {
        }

        /// <inheritdoc />
        public double FontSize
        {
            init
            {
                var valueToSet = value.CoerceValue(1, 65536);
                _fontSize = valueToSet;
                //RaiseOnTextRunPropertyChanged();
            }
            get => _fontSize ?? DefaultFontSize;
        }

        private readonly double? _fontSize;

        private const double DefaultFontSize = 15;

        /// <inheritdoc />
        public virtual FontName FontName
        {
            init
            {
                _fontFamily = value;
                //RaiseOnTextRunPropertyChanged();
            }
            get => _fontFamily ?? FontName.DefaultNotDefineFontName;
        }

        private readonly FontName? _fontFamily;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(FontSize, FontName);
        }

        /// <summary>
        /// 判断相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(LayoutOnlyRunProperty? other)
        {
            if(other is null) return false;

            // 先判断一定存在的属性，再判断业务端注入的属性
            if 
            (
                FontSize.Equals(other.FontSize)
                && FontName.Equals(other.FontName)
            )
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public virtual bool Equals(IReadOnlyRunProperty? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is LayoutOnlyRunProperty runProperty)
            {
                return Equals(runProperty);
            }
            else
            {
                return false;
            }
        }
    }
}