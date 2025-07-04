﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using LightTextEditorPlus.Core.Document;
using LightTextEditorPlus.Core.Primitive;
using LightTextEditorPlus.Document;
using LightTextEditorPlus.Document.Decorations;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;
using Size = System.Windows.Size;

namespace LightTextEditorPlus.Demo;
/// <summary>
/// TextEditorSettingsControl.xaml 的交互逻辑
/// </summary>
public partial class TextEditorSettingsControl : UserControl
{
    public TextEditorSettingsControl()
    {
        InitializeComponent();

        FontNameComboBox.ItemsSource = Fonts.SystemFontFamilies
            .Where(t => t.FamilyNames.Values is not null)
            .SelectMany(t => t.FamilyNames.Values!).Distinct();

        BulletMarkerStackPanel.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(BulletMarkerButton_OnClick));
    }

    public static readonly DependencyProperty TextEditorProperty = DependencyProperty.Register(
        nameof(TextEditor), typeof(TextEditor), typeof(TextEditorSettingsControl), new PropertyMetadata(default(TextEditor)));

    public TextEditor TextEditor
    {
        get { return (TextEditor) GetValue(TextEditorProperty); }
        set { SetValue(TextEditorProperty, value); }
    }

    private void FontNameComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var fontFamily = (string) e.AddedItems[0]!;

        TextEditor.SetFontName(fontFamily);
    }

    private void ApplyFontSizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(FontSizeTextBox.Text, out var fontSize))
        {
            TextEditor.SetFontSize(fontSize);
        }
        else
        {
            // 别逗
        }
    }

    private void ToggleBoldButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleBold();
    }

    private void ToggleItalicButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleItalic();
    }

    private void ForegroundButton_OnClick(object sender, RoutedEventArgs e)
    {
        Button button = (Button) sender;
        var brush = (Brush) button.DataContext;
        TextEditor.SetForeground(new ImmutableBrush(brush));
    }

    private void FullExpandButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.TextEditorCore.LineSpacingStrategy = LineSpacingStrategy.FullExpand;
    }

    private void FirstLineShrinkButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.TextEditorCore.LineSpacingStrategy = LineSpacingStrategy.FirstLineShrink;
    }

    #region 布局方式

    private void HorizontalArrangingTypeButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ArrangingType = ArrangingType.Horizontal;
    }

    private void VerticalArrangingTypeButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ArrangingType = ArrangingType.Vertical;
    }

    private void MongolianArrangingTypeButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ArrangingType = ArrangingType.Mongolian;
    }
    #endregion

    #region 行距

    private void LineSpacingButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(LineSpacingTextBox.Text, out var lineSpacing))
        {
            SetCurrentParagraphProperty(GetCurrentParagraphProperty() with
            {
                LineSpacing = TextLineSpacings.MultipleLineSpace(lineSpacing)
            });
        }
        else
        {
            LineSpacingTextBox.Text = 1.ToString();
        }
    }

    private void FixedLineSpacingButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(FixedLineSpacingTextBox.Text, out var fixedLineSpacing))
        {
            SetCurrentParagraphProperty(GetCurrentParagraphProperty() with
            {
                LineSpacing = TextLineSpacings.ExactlyLineSpace(fixedLineSpacing)
            });
        }
        else
        {
            FixedLineSpacingTextBox.Text = string.Empty;
        }
    }

    private void FixedLineSpacingResetButton_OnClick(object sender, RoutedEventArgs e)
    {
        // 等价于调用 ConfigCurrentCaretOffsetParagraphProperty 方法
        SetCurrentParagraphProperty(GetCurrentParagraphProperty() with
        {
            LineSpacing = TextLineSpacings.SingleLineSpace(),
        });

        FixedLineSpacingTextBox.Text = string.Empty;
    }

    #endregion

    #region 行距算法

    private void WPFLineSpacingAlgorithmButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.UseWpfLineSpacingStyle();
    }

    private void PPTLineSpacingAlgorithmButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.UsePptLineSpacingStyle();
    }
    #endregion

    #region 水平对齐

    private void LeftHorizontalTextAlignmentButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            HorizontalTextAlignment = HorizontalTextAlignment.Left
        });
    }

    private void CenterHorizontalTextAlignmentButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            HorizontalTextAlignment = HorizontalTextAlignment.Center
        });
    }

    private void RightHorizontalTextAlignmentButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            HorizontalTextAlignment = HorizontalTextAlignment.Right
        });
    }
    #endregion

    #region 垂直对齐

    private void TopVerticalTextAlignmentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TextEditor.VerticalTextAlignment = VerticalAlignment.Top;
    }

    private void CenterVerticalTextAlignmentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TextEditor.VerticalTextAlignment = VerticalAlignment.Center;
    }

    private void BottomVerticalTextAlignmentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TextEditor.VerticalTextAlignment = VerticalAlignment.Bottom;
    }

    #endregion

    #region 辅助方法

    private ParagraphProperty GetCurrentParagraphProperty() => TextEditor.GetCurrentCaretOffsetParagraphProperty();

    private void SetCurrentParagraphProperty(ParagraphProperty paragraphParagraph) =>
        TextEditor.SetParagraphProperty(TextEditor.CurrentCaretOffset, paragraphParagraph);

    #endregion

    #region 缩进

    private void IndentButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(IndentTextBox.Text, out var value))
        {
            SetIndent(value);
        }
        else
        {
            // 别逗
            IndentTextBox.Text = 0.ToString();
        }
    }

    private void AddIndentButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(IndentTextBox.Text, out var value))
        {
            value++;
            SetIndent(value);
            IndentTextBox.Text = value.ToString("#.##");
        }
        else
        {
            // 别逗
            IndentTextBox.Text = 0.ToString();
        }
    }

    private void SubtractIndentButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(IndentTextBox.Text, out var value))
        {
            value--;
            SetIndent(value);
            IndentTextBox.Text = value.ToString("#.##");
        }
        else
        {
            // 别逗
            IndentTextBox.Text = 0.ToString();
        }
    }

    private void SetIndent(double indent)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            Indent = indent,
            IndentType = GetIndentType(),
        });
    }

    private void IndentTypeButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            IndentType = GetIndentType()
        });
    }

    private IndentType GetIndentType()
    {
        string? text = IndentTypeComboBox.SelectionBoxItem.ToString();
        return text switch
        {
            "首行缩进" => IndentType.FirstLine,
            "悬挂缩进" => IndentType.Hanging,
            _ => IndentType.FirstLine
        };
    }

    #endregion

    #region 边距

    private void LeftIndentationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(LeftIndentationTextBox.Text, out var value))
        {
            SetLeftIndentation(value);
        }
        else
        {
            // 别逗
            LeftIndentationTextBox.Text = 0.ToString();
        }
    }

    private void AddLeftIndentationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(LeftIndentationTextBox.Text, out var value))
        {
            value++;
            SetLeftIndentation(value);
            LeftIndentationTextBox.Text = value.ToString("#.##");
        }
        else
        {
            // 别逗
            LeftIndentationTextBox.Text = 0.ToString();
        }
    }

    private void SubtractLeftIndentationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(LeftIndentationTextBox.Text, out var value))
        {
            value--;
            SetLeftIndentation(value);
            LeftIndentationTextBox.Text = value.ToString("#.##");
        }
        else
        {
            // 别逗
            LeftIndentationTextBox.Text = 0.ToString();
        }
    }

    private void SetLeftIndentation(double leftIndentation)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            LeftIndentation = leftIndentation
        });
    }

    private void RightIndentationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(RightIndentationTextBox.Text, out var value))
        {
            SetRightIndentation(value);
        }
        else
        {
            // 别逗
            RightIndentationTextBox.Text = 0.ToString();
        }
    }

    private void AddRightIndentationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(RightIndentationTextBox.Text, out var value))
        {
            value++;
            SetRightIndentation(value);
            RightIndentationTextBox.Text = value.ToString("#.##");
        }
        else
        {
            // 别逗
            RightIndentationTextBox.Text = 0.ToString();
        }
    }

    private void SubtractRightIndentationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(RightIndentationTextBox.Text, out var value))
        {
            value--;
            SetRightIndentation(value);
            RightIndentationTextBox.Text = value.ToString("#.##");
        }
        else
        {
            // 别逗
        }
    }

    private void SetRightIndentation(double rightIndentation)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            RightIndentation = rightIndentation
        });
    }
    #endregion

    #region 自适应
    private void ManualSizeToContentButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.SizeToContent = SizeToContent.Manual;
        TextEditor.HorizontalAlignment = HorizontalAlignment.Stretch;
        TextEditor.VerticalAlignment = VerticalAlignment.Stretch;
    }

    private void WidthSizeToContentButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.SizeToContent = SizeToContent.Width;
        TextEditor.HorizontalAlignment = HorizontalAlignment.Left;
        TextEditor.Width = double.NaN;
    }

    private void HeightSizeToContentButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.SizeToContent = SizeToContent.Height;
        TextEditor.VerticalAlignment = VerticalAlignment.Top;
        TextEditor.Height = double.NaN;
    }

    private void WidthAndHeightSizeToContentButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.SizeToContent = SizeToContent.WidthAndHeight;
        TextEditor.HorizontalAlignment = HorizontalAlignment.Left;
        TextEditor.VerticalAlignment = VerticalAlignment.Top;
        TextEditor.Width = double.NaN;
        TextEditor.Height = double.NaN;
    }
    #endregion

    #region 项目符号

    /// <summary>
    /// 无符号项目符号按钮点击
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BulletMarkerButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button) e.Source;
        var textMarker = (string) button.Content;
        FontFamily fontFamily = button.FontFamily;

        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property => property with
        {
            Marker = new BulletMarker()
            {
                MarkerText = textMarker,
                ShouldFollowParagraphFirstCharRunProperty = true,
                RunProperty = TextEditor.CreateRunProperty(styleRunProperty => styleRunProperty with
                {
                    FontName = new FontName(fontFamily.Source)
                })
            }
        });
    }

    private void ArabicPeriodButton_OnClick(object sender, RoutedEventArgs e)
    {
        // 带点号的阿拉伯数字，如 1.、 2.、 3.
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property =>
            property with
            {
                Marker = new NumberMarker()
                {
                    AutoNumberType = AutoNumberType.ArabicPeriod
                }
            });
    }

    private void AlphaLowerCharacterPeriodButton_OnClick(object sender, RoutedEventArgs e)
    {
        // 带点号的小写字母，如 a.、 b.、 c.
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property =>
            property with
            {
                Marker = new NumberMarker()
                {
                    AutoNumberType = AutoNumberType.AlphaLowerCharacterPeriod
                }
            });
    }

    private void CircleNumberDoubleBytePlainButton_OnClick(object sender, RoutedEventArgs e)
    {
        // 带圈的双字节阿拉伯数字，如 ①、 ②、 ③
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property =>
            property with
            {
                Marker = new NumberMarker()
                {
                    AutoNumberType = AutoNumberType.CircleNumberDoubleBytePlain
                }
            });
    }

    /// <summary>
    /// 清理项目符号
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MarkerButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ConfigCurrentCaretOffsetParagraphProperty(property =>
            property with
            {
                Marker = null,
            });
    }

    #endregion

    #region 文本装饰

    /// <summary>
    /// 删除线
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleStrikeThroughButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleStrikethrough();
    }

    /// <summary>
    /// 波浪线
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleWaveLineButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleTextDecoration(WaveLineTextEditorDecoration.Instance);
    }

    private void ToggleUnderlineButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleUnderline();
    }

    /// <summary>
    /// 着重号，中文着重号
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EmphasisDotsButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleEmphasisDots();
    }

    #endregion

    #region 上下标

    private void ToggleFontSuperscriptButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleSuperscript();
    }

    private void ToggleFontSubscriptButton_OnClick(object sender, RoutedEventArgs e)
    {
        TextEditor.ToggleSubscript();
    }

    #endregion
}
