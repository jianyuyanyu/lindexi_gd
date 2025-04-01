using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightTextEditorPlus.Core.Attributes;
using LightTextEditorPlus.Core.Carets;
using LightTextEditorPlus.Core.Diagnostics;
using LightTextEditorPlus.Core.Diagnostics.LogInfos;
using LightTextEditorPlus.Core.Document;
using LightTextEditorPlus.Core.Editing;
using LightTextEditorPlus.Core.Events;
using LightTextEditorPlus.Core.Primitive;
using LightTextEditorPlus.Core.Utils;

namespace LightTextEditorPlus.Core;

// 此文件存放文本属性
partial class TextEditorCore
{
    #region 文本属性

    /// <summary>
    /// 获取或设置文本的垂直对齐方式
    /// </summary>
    public VerticalTextAlignment VerticalTextAlignment
    {
        get => _verticalTextAlignment;
        set
        {
            if (CheckFeaturesDisableAndLog(TextFeatures.AlignVertical))
            {
                return;
            }

            if (value == VerticalTextAlignment.Top && CheckFeaturesDisableAndLog(TextFeatures.AlignVerticalTop))
            {
                return;
            }

            if (value == VerticalTextAlignment.Center && CheckFeaturesDisableAndLog(TextFeatures.AlignVerticalCenter))
            {
                return;
            }

            if (value == VerticalTextAlignment.Bottom && CheckFeaturesDisableAndLog(TextFeatures.AlignVerticalBottom))
            {
                return;
            }

            _verticalTextAlignment = value;

            // 实际上可以不布局的，只是修改文档左上角坐标即可
            RequireDispatchReLayoutAllDocument("VerticalTextAlignment Changed");
        }
    }

    private VerticalTextAlignment _verticalTextAlignment;

    /// <summary>
    /// 获取或设置文本的垂直对齐方式。此属性只是为了告诉大家，更加正确的是使用 <see cref="VerticalTextAlignment"/> 属性
    /// </summary>
    [Obsolete("此属性只是为了告诉大家，更加正确的是使用 VerticalTextAlignment 属性")]
    public VerticalTextAlignment VerticalContentAlignment => VerticalTextAlignment;

    /// <summary>
    /// 这个属性在这里只是为了告诉你，文本的水平布局是由段落控制的，不是由整个文本控制的
    /// </summary>
    [Obsolete("这个属性在这里只是为了告诉你，文本的水平布局是由段落控制的，不是由整个文本控制的", error: true)]
    public object TextAlignment => throw new NotSupportedException();

    /// <summary>
    /// 这个属性在这里只是为了告诉你，文本的水平布局是由段落控制的，不是由整个文本控制的
    /// </summary>
    [Obsolete("这个属性在这里只是为了告诉你，文本的水平布局是由段落控制的，不是由整个文本控制的", error: true)]
    public HorizontalTextAlignment HorizontalTextAlignment => throw new NotSupportedException();

    /// <summary>
    /// 获取或设置文本框的尺寸自适应模式
    /// </summary>
    public TextSizeToContent SizeToContent
    {
        set
        {
            if (CheckFeaturesDisableAndLog(TextFeatures.SetSizeToContent))
            {
                return;
            }

            if (_sizeToContent == value) return;
            _sizeToContent = value;
            RequireDispatchReLayoutAllDocument("SizeToContent Changed");
        }
        get => _sizeToContent;
    }

    private TextSizeToContent _sizeToContent = TextSizeToContent.Manual;

    /// <summary>
    /// 行距的配置
    /// </summary>
    public DocumentLineSpacingConfiguration LineSpacingConfiguration
    {
        get => _lineSpacingConfiguration;
        set
        {
            if (CheckFeaturesDisableAndLog(TextFeatures.SetLineSpacing))
            {
                return;
            }

            _lineSpacingConfiguration = value;
            RequireDispatchReLayoutAllDocument("LineSpacingConfiguration Changed");
        }
    }

    private DocumentLineSpacingConfiguration _lineSpacingConfiguration = new();

    /// <summary>
    /// 设置当前多倍行距呈现策略
    /// </summary>
    /// <remarks>
    /// 如需更高级的配置，请使用 <see cref="LineSpacingConfiguration"/> 属性
    /// </remarks>
    public LineSpacingStrategy LineSpacingStrategy
    {
        set
        {
            LineSpacingConfiguration = LineSpacingConfiguration with
            {
                LineSpacingStrategy = value
            };
        }
        get => LineSpacingConfiguration.LineSpacingStrategy;
    }

    /// <summary>
    /// 行距算法
    /// </summary>
    /// <remarks>
    /// 如需更高级的配置，请使用 <see cref="LineSpacingConfiguration"/> 属性
    /// </remarks>
    public LineSpacingAlgorithm LineSpacingAlgorithm
    {
        set
        {
            LineSpacingConfiguration = LineSpacingConfiguration with
            {
                LineSpacingAlgorithm = value
            };
        }
        get => LineSpacingConfiguration.LineSpacingAlgorithm;
    }

    /// <summary>
    /// 布局方式
    /// </summary>
    public ArrangingType ArrangingType
    {
        set
        {
            if (CheckFeaturesDisableAndLog(TextFeatures.ChangeArrangingType))
            {
                return;
            }

            if (_arrangingType == value) return;
            var oldArrangingType = _arrangingType;
            _arrangingType = value;

            ArrangingTypeChanged?.Invoke(this,
                new TextEditorValueChangeEventArgs<ArrangingType>(oldArrangingType, value));

            RequireDispatchReLayoutAllDocument("ArrangingType Changed");
        }
        get => _arrangingType;
    }

    private ArrangingType _arrangingType;

    /// <summary>
    /// 文本的当前语言文化，此属性会影响文本的排版或渲染
    /// </summary>
    public CultureInfo CurrentCulture
    {
        get => _cultureInfo ?? CultureInfo.CurrentCulture;
        set
        {
            if (value.Equals(CurrentCulture))
            {
                return;
            }

            _cultureInfo = value;
            // 变更语言文化，需要重新布局
            RequireDispatchReLayoutAllDocument("CurrentCultureChanged");
        }
    }

    private CultureInfo? _cultureInfo;

    #endregion

    #region 状态属性

    // 存放文本当前的状态

    /// <summary>
    /// 是否正在更新布局。更新布局的过程中，不允许修改文档
    /// </summary>
    public bool IsUpdatingLayout { get; private set; }

    /// <summary>
    /// 文本是不是脏的，需要等待布局完成。可选使用 <see cref="WaitLayoutCompletedAsync"/> 等待布局完成
    /// </summary>
    // ReSharper disable once RedundantDefaultMemberInitializer
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            _isDirty = value;

            if (_renderInfoProvider != null)
            {
                _renderInfoProvider.IsDirty = true;
                _renderInfoProvider = null;
            }
        }
    }

    /// <summary>
    /// 文本是不是脏的
    /// </summary>
    /// 创建出来的文本就是脏的，需要等待布局完成才能获取到布局信息
    /// 根据 README.md 文档约定： “默认创建出来的文本是脏的，需要布局完成之后，才不是脏的”
    private bool _isDirty = true;

    #endregion

    // todo 考虑设置可见范围，用来支持长文本

    #region 调试属性
    // 调试属性放在 TextEditorCore.Diagnostics.cs 文件中
    #endregion

    #region 文本对外属性

    #region 光标和选择

    /// <summary>
    /// 获取或设置当前光标位置
    /// </summary>
    /// 这是对外调用的，非框架内使用
    [TextEditorPublicAPI]
    public CaretOffset CurrentCaretOffset
    {
        set => CaretManager.CurrentCaretOffset = value;
        get => CaretManager.CurrentCaretOffset;
    }

    /// <summary>
    /// 获取或设置当前的选择范围
    /// </summary>
    /// 这是对外调用的，非框架内使用
    [TextEditorPublicAPI]
    public Selection CurrentSelection
    {
        set => CaretManager.SetSelection(in value);
        get => CaretManager.CurrentSelection;
    }

    /// <summary>
    /// 选择文本
    /// </summary>
    /// <remarks>完全等同于 <see cref="CurrentSelection"/> 的 set 方法</remarks>
    /// <param name="selection"></param>
    [TextEditorPublicAPI]
    public void Select(in Selection selection) => CaretManager.SetSelection(in selection);

    /// <summary>
    /// 移动光标。如已知 <see cref="CaretOffset"/> 可直接给 <see cref="CurrentCaretOffset"/> 属性赋值
    /// </summary>
    /// <param name="type"></param>
    [TextEditorPublicAPI]
    public void MoveCaret(CaretMoveType type)
    {
        var caretOffset = GetNewCaretOffset(type);
        CaretManager.CurrentCaretOffset = caretOffset;
    }

    /// <summary>
    /// 移动光标
    /// </summary>
    /// <param name="caretOffset"></param>
    [Obsolete(
        "如已知 CaretOffset 的值，则可直接给 CurrentCaretOffset 属性赋值。此方法仅仅只是用来告诉你正确的方法应该是给 CurrentCaretOffset 属性赋值，无需再调用任何方法")]
    public void MoveCaret(CaretOffset caretOffset) => CaretManager.CurrentCaretOffset = caretOffset;

    /// <summary>
    /// 根据键盘操作获取光标导航
    /// </summary>
    /// <param name="caretMoveType"></param>
    /// <returns></returns>
    public CaretOffset GetNewCaretOffset(CaretMoveType caretMoveType)
    {
        return KeyboardCaretNavigationHelper.GetNewCaretOffset(this, caretMoveType);
    }

    #endregion

    #region 段落

    /// <summary>
    /// 获取文档的段落列表
    /// </summary>
    public ReadOnlyParagraphList ParagraphList => _paragraphList ??= new ReadOnlyParagraphList(this);
    private ReadOnlyParagraphList? _paragraphList;

    #endregion

    #endregion

    #region 功能特性

    /// <summary>
    /// 功能特性
    /// </summary>
    public TextFeatures Features
    {
        get => _features;
        set
        {
            if (value == _features)
            {
                return;
            }

            var oldValue = _features;
            _features = value;
            FeaturesChanged?.Invoke(this, new TextEditorValueChangeEventArgs<TextFeatures>(oldValue, value));
        }
    }

    private TextFeatures _features = TextFeatures.All;

    public event EventHandler<TextEditorValueChangeEventArgs<TextFeatures>>? FeaturesChanged;

    public void EnableFeatures(TextFeatures features)
    {
        Features = Features.EnableFeatures(features);
    }

    public void DisableFeatures(TextFeatures features)
    {
        Features = Features.DisableFeatures(features);
    }

    public bool IsFeaturesEnable(TextFeatures features)
    {
        return Features.IsFeaturesEnable(features);
    }

    /// <summary>
    /// 判断功能是否被禁用，如被禁用则记录日志
    /// </summary>
    /// <param name="features"></param>
    /// <returns></returns>
    public bool CheckFeaturesDisableAndLog(TextFeatures features)
    {
        if (IsFeaturesEnable(features))
        {
            return true;
        }
        else
        {
            Logger.Log(new TextFeaturesBeDisabledLogInfo(features));
            return false;
        }
    }

    #endregion
}
