using Avalonia.Controls;
using Avalonia.Media;

using LightTextEditorPlus;
using LightTextEditorPlus.Core;
using LightTextEditorPlus.Core.Carets;
using LightTextEditorPlus.Core.Document.Segments;
using LightTextEditorPlus.Document;
using LightTextEditorPlus.Editing;
using LightTextEditorPlus.Primitive;

using Markdig;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Syntax;

using SimpleWrite.Business.ShortcutManagers;
using SimpleWrite.Business.Snippets;
using SimpleWrite.Utils;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWrite.Business.TextEditors;

/// <summary>
/// 文本编辑器
/// </summary>
internal sealed class SimpleWriteTextEditor : TextEditor
{
    public SimpleWriteTextEditor()
    {
        CaretConfiguration.SelectionBrush = new Color(0x9F, 0x26, 0x3F, 0xC7);

        TextEditorCore.TextChanged += TextEditorCore_TextChanged;

        SizeToContent = SizeToContent.Height;

        var normalFontSize = 20;

        SetStyleTextRunProperty(runProperty => runProperty with
        {
            FontSize = normalFontSize,
            Foreground = new SolidColorSkiaTextBrush(SKColors.Azure)
        });

        var normalTextRunProperty = this.CreateRunProperty(property => property with
        {
            FontSize = normalFontSize
        });
        NormalTextRunProperty = normalTextRunProperty;

        var titleLevel1RunProperty = this.CreateRunProperty(property => property with
        {
            FontSize = normalFontSize + 10,
            FontWeight = SKFontStyleWeight.Bold,
        });

        var titleLevel2RunProperty = this.CreateRunProperty(property => property with
        {
            FontSize = normalFontSize + 7,
            FontWeight = SKFontStyleWeight.Bold,
        });

        var titleLevel3RunProperty = this.CreateRunProperty(property => property with
        {
            FontSize = normalFontSize + 5,
            FontWeight = SKFontStyleWeight.Bold,
        });

        var titleLevel4RunProperty = this.CreateRunProperty(property => property with
        {
            FontSize = normalFontSize + 3,
            FontWeight = SKFontStyleWeight.Bold,
        });

        var titleLevel5RunProperty = this.CreateRunProperty(property => property with
        {
            FontSize = normalFontSize + 1,
            FontWeight = SKFontStyleWeight.Bold,
        });

        TitleLevelRunPropertyList = [titleLevel1RunProperty, titleLevel2RunProperty, titleLevel3RunProperty, titleLevel4RunProperty, titleLevel5RunProperty];

        CodeLangInfoRunProperty = CreateColorRunProperty(new SKColor(0xFFAC90DE));

        SkiaTextRunProperty CreateColorRunProperty(SKColor color)
        {
            return CreateRunProperty(property => property with
            {
                Foreground = new SolidColorSkiaTextBrush(color)
            });
        }
    }

    private void TextEditorCore_TextChanged(object? sender, EventArgs e)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        var markdownText = Text;
        var markdownDocument = Markdown.Parse(markdownText, pipeline);
        var setter = new TextRunPropertySetter(this);

        foreach (Block block in markdownDocument)
        {
            if (block is ParagraphBlock paragraphBlock)
            {
                if (paragraphBlock.Inline is { } inline)
                {

                }

                setter.TrySetRunProperty(NormalTextRunProperty, paragraphBlock.Span);
            }

            if (block is HeadingBlock headingBlock)
            {
                var levelIndex = headingBlock.Level - 1;
                SkiaTextRunProperty runProperty;
                if (TitleLevelRunPropertyList.Count - 1 > levelIndex)
                {
                    runProperty = TitleLevelRunPropertyList[levelIndex];
                }
                else
                {
                    runProperty = TitleLevelRunPropertyList[^1];
                }

                setter.TrySetRunProperty(runProperty, headingBlock.Span);
            }

            if (block is FencedCodeBlock fencedCodeBlock)
            {
                var sourceSpan = fencedCodeBlock.Span;

                //setter.SetRunProperty(property =>
                //    property with
                //    {
                //        Background = CodeBackgroundColor,
                //    }, sourceSpan);

                var codeText = ToText(sourceSpan);

                var codeSetter = setter with
                {
                    StartOffset = sourceSpan.Start
                };

                //var stringReader = new StringReader(codeText);
                //stringReader.ReadLine()
                //var codeLineArray = codeText.Split('\n');

                var fencedChar = fencedCodeBlock.FencedChar;
                var closingFencedCharCount = fencedCodeBlock.ClosingFencedCharCount;
                var langInfo = fencedCodeBlock.Info;

                var lineReader = new Markdig.Helpers.LineReader(codeText);
                StringSlice firstLine = lineReader.ReadLine();
                if (firstLine.Length == closingFencedCharCount + (langInfo?.Length ?? 0))
                {
                    codeSetter.TrySetRunProperty(CodeLangInfoRunProperty, new SourceSpan(closingFencedCharCount, closingFencedCharCount + langInfo?.Length ?? 0));
                }

                // 准备给代码内容着色
            }
        }

        string ToText(SourceSpan span)
        {
            var text = markdownText.AsSpan().Slice(span.Start, span.Length).ToString();
            return text;
        }
    }

    /// <summary>
    /// 快捷键执行器
    /// </summary>
    public required ShortcutExecutor ShortcutExecutor { get; init; }

    /// <summary>
    /// 代码片管理器
    /// </summary>
    public required SnippetManager SnippetManager { get; init; }

    /// <summary>
    /// 正文文本属性
    /// </summary>
    public SkiaTextRunProperty NormalTextRunProperty { get; }
    public IReadOnlyList<SkiaTextRunProperty> TitleLevelRunPropertyList { get; }
    public SkiaTextRunProperty CodeLangInfoRunProperty { get; }
    public SKColor CodeBackgroundColor { get; } = new SKColor(0xFF3B3C37);

    protected override TextEditorHandler CreateTextEditorHandler()
    {
        return new SimpleWriteTextEditorHandler(this);
    }
}

readonly record struct TextRunPropertySetter(TextEditor TextEditor)
{
    public DocumentOffset StartOffset { get; init; } = 0;

    public void SetRunProperty(ConfigRunProperty config, SourceSpan span)
    {
        span = span with
        {
            Start = span.Start + StartOffset,
            End = span.End + StartOffset
        };
        var selection = SourceSpanToSelection(span);

        TextEditor.TextEditorCore.SetUndoRedoEnable(false, "框架内部设置文本样式，防止将内容动作记录");
      
        TextEditor.SetRunProperty(config, selection);

        TextEditor.TextEditorCore.SetUndoRedoEnable(true, "完成框架内部设置文本样式，启用撤销恢复");
    }

    public void TrySetRunProperty(SkiaTextRunProperty runProperty, SourceSpan span)
    {
        span = span with
        {
            Start = span.Start + StartOffset,
            End = span.End + StartOffset
        };
        var selection = SourceSpanToSelection(span);

        TextEditor.TextEditorCore.SetUndoRedoEnable(false, "框架内部设置文本样式，防止将内容动作记录");
        IEnumerable<SkiaTextRunProperty> runPropertyRange = TextEditor.GetRunPropertyRange(selection);
        var same = runPropertyRange.All(t => t == runProperty);
        if (!same)
        {
            TextEditor.SetRunProperty(runProperty, selection);
        }
        TextEditor.TextEditorCore.SetUndoRedoEnable(true, "完成框架内部设置文本样式，启用撤销恢复");
    }

    private Selection SourceSpanToSelection(SourceSpan span) => new Selection(new CaretOffset(span.Start), span.Length);
}