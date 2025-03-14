using System;
using System.Collections.Generic;
using System.Diagnostics;

using LightTextEditorPlus.Core.Document;
using LightTextEditorPlus.Core.Document.Segments;
using LightTextEditorPlus.Core.Exceptions;
using LightTextEditorPlus.Core.Layout.LayoutUtils;
using LightTextEditorPlus.Core.Layout.LayoutUtils.WordDividers;
using LightTextEditorPlus.Core.Platform;
using LightTextEditorPlus.Core.Primitive;
using LightTextEditorPlus.Core.Primitive.Collections;
using LightTextEditorPlus.Core.Utils;
using LightTextEditorPlus.Core.Utils.Maths;

namespace LightTextEditorPlus.Core.Layout;

// ReSharper 禁用不可达代码提示
// ReSharper disable HeuristicUnreachableCode

/// <summary>
/// 水平方向布局的提供器
/// </summary>
class HorizontalArrangingLayoutProvider : ArrangingLayoutProvider
// 这是为分词器提供的接口。现在分词器只做分词，不做布局，所以这个接口不需要
//, IInternalCharDataSizeMeasurer
{
    public HorizontalArrangingLayoutProvider(LayoutManager layoutManager) : base(layoutManager)
    {
        //_divider = new DefaultWordDivider(layoutManager.TextEditor, this);
    }

    public override ArrangingType ArrangingType => ArrangingType.Horizontal;

    #region 02 预布局阶段

    #region 更新非脏的段落和行

    /// <summary>
    /// 更新非脏的段落和行的起始点
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    protected override ParagraphLayoutResult UpdateParagraphStartPoint(in ParagraphLayoutArgument argument)
    {
        var paragraph = argument.ParagraphData;

        if (TextEditor.IsInDebugMode && paragraph.IsDirty())
        {
            throw new TextEditorInnerDebugException("更新非脏的段落和行时，段落是脏的");
        }

        // 先设置是脏的，然后再更新，这样即可更新段落版本号
        paragraph.SetDirty();

        paragraph.SetLayoutDirty(exceptTextSize: true);
        Debug.Assert(paragraph.ParagraphLayoutData.StartPointInDocumentContentCoordinate.IsInvalid);
        UpdateParagraphLayoutData(in argument);

        //var layoutArgument = argument with
        //{
        //    //CurrentStartPoint = currentStartPoint
        //};

        var nextLineStartPoint = UpdateParagraphLineLayoutDataStartPoint(in argument);
        // 设置当前段落已经布局完成
        paragraph.SetFinishLayout();

        // 转换为下一段的坐标
        TextPointInDocumentContentCoordinate nextParagraphStartPoint = nextLineStartPoint.ToDocumentContentCoordinate(paragraph);
        // 加上段后间距
        nextParagraphStartPoint = nextParagraphStartPoint.Offset(0, argument.GetParagraphAfter());
        return new ParagraphLayoutResult(nextParagraphStartPoint);
    }

    /// <summary>
    /// 更新段落的布局信息
    /// </summary>
    /// <param name="argument"></param>
    private static void UpdateParagraphLayoutData(in ParagraphLayoutArgument argument)
    {
        var paragraph = argument.ParagraphData;
        //double paragraphBefore = argument.IsFirstParagraph ? 0 /*首段不加段前间距*/  : paragraph.ParagraphProperty.ParagraphBefore;
        //var currentStartPoint = argument.CurrentStartPoint with
        //{
        //    Y = argument.CurrentStartPoint.Y + paragraphBefore
        //};
        paragraph.UpdateParagraphLayoutStartPoint(argument.CurrentStartPoint);

        double paragraphBefore = argument.GetParagraphBefore();
        // 只加上段前后距离，左右边距现在不加上，因为左右边距在行里进行计算
        // 左右边距影响行的可用宽度，这就是为什么放在行进行计算的原因。既然放在行进行计算了，那就顺带叠加在行的布局属性
        double paragraphAfter = argument.GetParagraphAfter();
        var contentThickness =
            new TextThickness(0, paragraphBefore, 0, paragraphAfter);
        paragraph.SetParagraphLayoutContentThickness(contentThickness);
    }

    /// <summary>
    /// 更新段落里面的所有行的起始点
    /// </summary>
    /// <param name="argument"></param>
    /// <returns>下一行的坐标。不包括段后间距</returns>
    private TextPointInParagraphCoordinate UpdateParagraphLineLayoutDataStartPoint(in ParagraphLayoutArgument argument)
    {
        var paragraph = argument.ParagraphData;
        var currentStartPoint = new TextPointInParagraphCoordinate(0, 0, paragraph);

        foreach (LineLayoutData lineLayoutData in paragraph.LineLayoutDataList)
        {
            //UpdateLineLayoutDataStartPoint(lineVisualData, currentStartPoint);
            // 更新行内的所有字符的版本
            // 由于现在的行是相对段落坐标，因此段落坐标变更即可，不需要再变更到具体的行的坐标

            TextReadOnlyListSpan<CharData> list = lineLayoutData.GetCharList();
            foreach (CharData charData in list)
            {
                charData.CharLayoutData!.UpdateVersion();
            }

            lineLayoutData.UpdateVersion();

            currentStartPoint = GetNextLineStartPoint(currentStartPoint, lineLayoutData);
        }

        return currentStartPoint;
    }

    ///// <summary>
    ///// 重新更新每一行的起始点。例如现在第一段的文本加了一行，那第二段的所有文本都需要更新每一行的起始点，而不需要重新布局第二段
    ///// </summary>
    ///// <param name="lineLayoutData"></param>
    ///// <param name="startPoint"></param>
    //private void UpdateLineLayoutDataStartPoint(LineLayoutData lineLayoutData, TextPoint startPoint)
    //{
    //    // 更新包括两个方面：
    //    // 1. 此行的起点
    //    // 2. 更新行内的所有字符的版本
    //    //// 2. 此行内的所有字符的起点坐标
    //    lineLayoutData.CharStartPoint = startPoint;

    //    // 行内字符相对于行的坐标，只需更新行的起始点即可
    //    //// 更新行内所有字符的坐标
    //    //TextReadOnlyListSpan<CharData> list = lineLayoutData.GetCharList();
    //    //var lineHeight = lineLayoutData.LineContentSize.Height;
    //    //UpdateTextLineStartPoint(list, startPoint, lineHeight,
    //    //    // 这里只是更新行的起始点，行内的字符坐标不需要变更，因此不需要重新排列 X 坐标
    //    //    reArrangeXPosition: false,
    //    //    needUpdateCharLayoutDataVersion: true);

    //    // 更新行内的所有字符的版本
    //    TextReadOnlyListSpan<CharData> list = lineLayoutData.GetCharList();
    //    foreach (CharData charData in list)
    //    {
    //        charData.CharLayoutData!.UpdateVersion();
    //    }

    //    lineLayoutData.UpdateVersion();
    //}

    #endregion

    #region 段落布局核心

    /// <summary>
    /// 布局段落的核心逻辑
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="startParagraphOffset">开始布局的字符，刚好是一行的开始</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>
    /// 逻辑上是：
    /// 布局按照： 文本-段落-行-Run-字符
    /// 布局整个文本
    /// 布局文本的每个段落 <see cref="UpdateParagraphLayoutCore"/>
    /// 段落里面，需要对每一行进行布局 <see cref="UpdateWholeLineLayout"/>
    /// 每一行里面，需要对每个 Char 字符进行布局 <see cref="UpdateSingleCharInLineLayout"/>
    /// </remarks>
    protected override ParagraphLayoutResult UpdateParagraphLayoutCore(in ParagraphLayoutArgument argument,
        in ParagraphCharOffset startParagraphOffset)
    {
        UpdateParagraphLayoutData(in argument);

        var paragraph = argument.ParagraphData;

        // 预布局过程中，不考虑边距的影响。但只考虑缩进等对可用尺寸的影响
        // 在回溯过程中，才赋值给到边距。详细请参阅 《文本库行布局信息定义.enbx》 维护文档
        //// 更新段左边距
        //currentStartPoint = currentStartPoint with
        //{
        //    X = paragraph.ParagraphProperty.LeftIndentation
        //};

        // 下一段的起始点
        TextPointInDocumentContentCoordinate nextParagraphStartPoint;
        // 如果是空段的话，那就进行空段布局，否则布局段落里面每一行
        if (paragraph.IsEmptyParagraph)
        {
            // 空段布局
            nextParagraphStartPoint = UpdateEmptyParagraphLayout(argument, argument.CurrentStartPoint);
        }
        else
        {
            // 布局段落里面每一行

            // 先更新非脏的行的坐标
            // 布局左上角坐标，当前行的坐标点。行的坐标点是相对于段落的
            TextPointInParagraphCoordinate currentLinePoint;
            // 根据是否存在缓存行决定是否需要计算段前间距
            if (paragraph.LineLayoutDataList.Count == 0)
            {
                //// 一行都没有的情况下，需要计算段前间距
                //double paragraphBefore = argument.GetParagraphBefore();

                //currentStartPoint = argument.CurrentStartPoint with
                //{
                //    Y = argument.CurrentStartPoint.Y + paragraphBefore
                //};
                // 行的坐标点是相对于段落文本范围的，~~只需加上段前间距即可~~ 不能加上段落前后间距
                var x = 0;
                //var y = paragraphBefore;
                var y = 0;
                currentLinePoint = new TextPointInParagraphCoordinate(x, y, paragraph);
            }
            else
            {
                // 有缓存的行，证明段落属性没有更改~~，不需要计算段前间距~~
                // 只需要更新缓存的行，获取到首个需要布局的行的坐标点
                currentLinePoint = UpdateParagraphLineLayoutDataStartPoint(argument);
            }

            // 布局段落里面的每一行
            nextParagraphStartPoint = UpdateParagraphLinesLayout(argument, startParagraphOffset, currentLinePoint);

            // 下一段的距离需要加上段后间距
            double paragraphAfter =
                argument.GetParagraphAfter();
            var offsetX = 0;
            var offsetY = paragraphAfter;
            nextParagraphStartPoint = nextParagraphStartPoint.Offset(offsetX, offsetY);
        }

        //// 考虑行复用，例如刚好添加的内容是一行。或者在一行内做文本替换等
        //// 这个没有啥优先级。测试了 SublimeText 和 NotePad 工具，都没有做此复用，预计有坑

        // 计算段落的文本尺寸
        //TextPoint paragraphTextStartPoint = argument.ParagraphData.LineLayoutDataList[0].CharStartPoint;
        TextSize paragraphTextSize = BuildParagraphSize(argument);
        //TextRect paragraphTextBounds = new TextRect(paragraphTextStartPoint, paragraphTextSize);
        paragraph.SetParagraphLayoutTextSize(paragraphTextSize);

        return new ParagraphLayoutResult(nextParagraphStartPoint);
    }

    /// <summary>
    /// 布局空段
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="currentStartPoint"></param>
    /// <returns></returns>
    private TextPointInDocumentContentCoordinate UpdateEmptyParagraphLayout(in ParagraphLayoutArgument argument, TextPointInDocumentContentCoordinate currentStartPoint)
    {
        double paragraphBefore = argument.GetParagraphBefore();

        var paragraph = argument.ParagraphData;
        // 如果是空段的话，如一段只是一个 \n 而已，那就需要执行空段布局逻辑
        Debug.Assert(paragraph.LineLayoutDataList.Count == 0, "空段布局时一定是一行都不存在");
        var emptyParagraphLineHeightMeasureResult = MeasureEmptyParagraphLineHeight(
            new EmptyParagraphLineHeightMeasureArgument(paragraph, argument.ParagraphIndex, argument.UpdateLayoutContext));
        double lineHeight = emptyParagraphLineHeightMeasureResult.LineHeight;

        // 加上空行
        var lineLayoutData = new LineLayoutData(paragraph)
        {
            CharStartParagraphIndex = 0,
            CharEndParagraphIndex = 0,
            CharStartPointInParagraphCoordinate = new TextPointInParagraphCoordinate(0, 0, paragraph),
            LineContentSize = new TextSize(0, lineHeight)
        };
        paragraph.LineLayoutDataList.Add(lineLayoutData);

        // 下一段的起始坐标
        //  = 当前的坐标 + 段前 + 行高 + 段后
        double paragraphAfter = argument.GetParagraphAfter();
        const double offsetX = 0;
        double offsetY = paragraphBefore + lineHeight + paragraphAfter;
        var nextParagraphStartPoint = currentStartPoint.Offset(offsetX, offsetY);
        return nextParagraphStartPoint;
    }

    /// <summary>
    /// 布局段落里面的每一行
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="startParagraphOffset"></param>
    /// <param name="currentStartPoint"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="TextEditorDebugException"></exception>
    /// <exception cref="TextEditorInnerException"></exception>
    /// 每一行的布局都是相对于文本的每个对应的段落的坐标点。更具体来说是相对于段落的文本范围的坐标点。即不包括段前间距和段后间距的坐标点
    private TextPointInDocumentContentCoordinate UpdateParagraphLinesLayout(in ParagraphLayoutArgument argument, in ParagraphCharOffset startParagraphOffset,
        TextPointInParagraphCoordinate currentStartPoint)
    {
        // 当前的坐标点，这是相对于段落的坐标点
        _ = currentStartPoint;

        ParagraphData paragraph = argument.ParagraphData;
        // 获取最大宽度信息
        double lineMaxWidth = GetLineMaxWidth();

        var wholeRunLineLayouter = TextEditor.PlatformProvider.GetWholeRunLineLayouter();
        for (var i = startParagraphOffset.Offset; i < paragraph.CharCount;)
        {
            // 开始行布局
            // 第一个 Run 就是行的开始
            TextReadOnlyListSpan<CharData> charDataList = paragraph.ToReadOnlyListSpan(new ParagraphCharOffset(i));

            if (IsInDebugMode)
            {
                // 这是调试代码，判断是否在布局过程，漏掉某个字符
                foreach (var charData in charDataList)
                {
                    charData.IsSetStartPointInDebugMode = false;
                }
            }

            int lineIndex = paragraph.LineLayoutDataList.Count;

            argument.UpdateLayoutContext.RecordDebugLayoutInfo($"第 {lineIndex} 行开始布局",
                LayoutDebugCategory.PreWholeLine);

            var isFirstLine = lineIndex == 0;
            ParagraphProperty paragraphProperty = paragraph.ParagraphProperty;

            var usableLineMaxWidth = paragraphProperty.GetUsableLineMaxWidth(lineMaxWidth, isFirstLine);

            WholeLineLayoutResult result;
            var wholeRunLineLayoutArgument = new WholeLineLayoutArgument(argument.ParagraphIndex,
                lineIndex, paragraph, charDataList,
                usableLineMaxWidth, currentStartPoint, argument.UpdateLayoutContext);
            if (wholeRunLineLayouter != null)
            {
                result = wholeRunLineLayouter.LayoutWholeLine(in wholeRunLineLayoutArgument);
            }
            else
            {
                // 继续往下执行，如果没有注入自定义的行布局层的话，则使用默认的行布局器
                // 为什么不做默认实现？因为默认实现会导致默认逻辑写在外面，而不是相同一个文件里面，没有内聚性，对于文本排版布局内部重调试的情况下，不友好。即，尽管代码结构是清晰了，但实际调试体验却下降了，一个调试或阅读代码需要跳转多个文件，复杂度提升
                result = UpdateWholeLineLayout(in wholeRunLineLayoutArgument);
            }

            // 当前的行布局信息
            var currentLineLayoutData = new LineLayoutData(paragraph)
            {
                CharStartParagraphIndex = i,
                CharEndParagraphIndex = i + result.CharCount,
                LineContentSize = result.LineSize,
                LineCharTextSize = result.TextSize,
                CharStartPointInParagraphCoordinate = currentStartPoint,
                LineSpacingThickness = result.LineSpacingThickness,
            };

            // 更新字符信息
            Debug.Assert(result.CharCount <= charDataList.Count, "所获取的行的字符数量不能超过可提供布局的行的字符数量");
            for (var index = 0; index < result.CharCount; index++)
            {
                var charData = charDataList[index];

                if (IsInDebugMode)
                {
                    if (charData.IsSetStartPointInDebugMode == false)
                    {
                        throw new TextEditorDebugException($"存在某个字符没有在布局时设置坐标",
                            (charData, currentLineLayoutData, i + index));
                    }
                }

                Debug.Assert(charData.CharLayoutData != null, "经过行布局，字符存在行布局信息");
                charData.CharLayoutData!.CharIndex = new ParagraphCharOffset(i + index);
                charData.CharLayoutData.CurrentLine = currentLineLayoutData;
                charData.CharLayoutData.UpdateVersion();
            }

            paragraph.LineLayoutDataList.Add(currentLineLayoutData);

            i += result.CharCount;

            if (result.CharCount == 0)
            {
                // todo 理论上不可能，表示行布局出错了
                // 支持文本宽度小于一个字符的宽度的布局
                throw new TextEditorInnerException($"某一行在布局时，只采用了零个字符");
            }

            currentStartPoint = GetNextLineStartPoint(currentStartPoint, currentLineLayoutData);

            argument.UpdateLayoutContext.RecordDebugLayoutInfo($"第 {lineIndex} 行完成布局，字符数：{currentLineLayoutData.CharCount}。LineContentSize：{currentLineLayoutData.LineContentSize.ToCommaSplitWidthAndHeight()}，LineCharTextSize:{currentLineLayoutData.LineCharTextSize.ToCommaSplitWidthAndHeight()}，下一行起点 {currentStartPoint}，包含文本：'{currentLineLayoutData.GetCharList().ToLimitText(replaceText: "…", limitCharCount: 7)}'",
                LayoutDebugCategory.PreWholeLine);
        }

        // 下一段的起始坐标。从行进行转换
        var nextParagraphStartPoint = currentStartPoint.ToDocumentContentCoordinate(argument.ParagraphData);
        return nextParagraphStartPoint;
    }

    #endregion

    #region LayoutWholeLine 布局一行的字符

    /// <summary>
    /// 布局一行的字符
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// 1. 布局一行的字符，分行算法
    /// 2. 处理行高，行距算法
    /// 3. 设置每个字符在行内的坐标
    ///
    /// | -------- 顶部行距 ------- |
    /// | 左侧缩进 | 文本 | 右侧缩进 |
    /// | -------- 底部行距 ------- |
    private WholeLineLayoutResult UpdateWholeLineLayout(in WholeLineLayoutArgument argument)
    {
        UpdateLayoutContext updateLayoutContext = argument.UpdateLayoutContext;
        var charDataList = argument.CharDataList;
        //var currentStartPoint = argument.CurrentStartPoint;

        if (charDataList.Count == 0)
        {
            // 理论上不会进入这里，如果没有任何的字符，那就不需要进行行布局
            return new WholeLineLayoutResult(TextSize.Zero, TextSize.Zero, 0, default);
        }

        // 1. 布局一行的字符，分行算法
        var layoutResult = UpdateWholeLineCharsLayout(in argument);
        updateLayoutContext.RecordDebugLayoutInfo($"第 {argument.LineIndex} 行，分行算法返回 {layoutResult.WholeTakeCount} 个字符。字符尺寸：{layoutResult.CurrentLineCharTextSize.ToCommaSplitWidthAndHeight()}",
            LayoutDebugCategory.PreWholeLine);
#if DEBUG
        if (layoutResult.CurrentLineCharTextSize.Width > 0 && layoutResult.CurrentLineCharTextSize.Height == 0)
        {
            // 这可能是不正常的情况
            // 可以在这里打断点看
            TextEditor.Logger.LogDebug($"单行布局结果是有宽度没高度，预计是不正确的情况。仅调试下输出");
        }
#endif
        // 2. 处理行高，行距算法
        int wholeCharCount = layoutResult.WholeTakeCount;
        TextSize currentTextSize = layoutResult.CurrentLineCharTextSize;

        if (wholeCharCount == 0)
        {
            // 这一行一个字符都不能拿
            Debug.Assert(currentTextSize == TextSize.Zero);
            // 有可能这一行一个字符都不能拿，但是还是有行高的
            var currentLineSize = currentTextSize; // todo 这里需要重新确认一下，行高应该是多少，行距是多少
            return new WholeLineLayoutResult(currentLineSize, currentTextSize, wholeCharCount, default);
        }

        // 遍历一次，用来取出其中 FontSize 最大的字符，此字符的对应字符属性就是所期望的参与后续计算的字符属性
        // 遍历这一行的所有字符，找到最大字符的字符属性
        var charDataTakeList = charDataList.Slice(0, wholeCharCount);
        CharData maxFontSizeCharData = GetMaxFontSizeCharData(charDataTakeList);
        IReadOnlyRunProperty maxFontSizeCharRunProperty = maxFontSizeCharData.RunProperty;

        // 处理行距
        var lineSpacingCalculateArgument = new LineSpacingCalculateArgument(argument.ParagraphIndex, argument.LineIndex, argument.ParagraphProperty, maxFontSizeCharRunProperty);
        LineSpacingCalculateResult lineSpacingCalculateResult = CalculateLineSpacing(lineSpacingCalculateArgument);

        // 获取到行高，行高的意思是整行的高度，包括空白行距+字符高度
        double lineHeight = lineSpacingCalculateResult.TotalLineHeight;
        if (lineSpacingCalculateResult.ShouldUseCharLineHeight)
        {
            lineHeight = currentTextSize.Height;
        }

        var lineSpacing = lineHeight - currentTextSize.Height; // 行距值，现在仅调试用途
        GC.KeepAlive(lineSpacing);
        // 不能使用 lineSpacing 作为计算参考，因为在 Skia 平台下 TextSize 会更大，超过了布局行高的值，导致 lineSpacing 为负数
        // 正确的应该是使用 MaxFontHeight 进行计算。尽管这个计算可能算出负数
        var maxFontHeight = maxFontSizeCharData.Size!.Value.Height;
        // 行距的空白。正常 MaxFontHeight 小于 LineHeight 的情况下，可以认为这就是行距的空白
        var lineSpacingGap = lineHeight - maxFontHeight;
        RatioVerticalCharInLineAlignment verticalCharInLineAlignment = TextEditor.LineSpacingConfiguration.VerticalCharInLineAlignment;
        // 计算出行距的顶部空白
        var topLineSpacingGap = lineSpacingGap * verticalCharInLineAlignment.LineSpaceRatio;
        var bottomLineSpacingGap = lineSpacingGap - topLineSpacingGap;

        updateLayoutContext.RecordDebugLayoutInfo($"行高：{lineHeight:0.##}，字高：{currentTextSize.Height}，行距：{lineSpacing:0.##}，行距的空白：{lineSpacingGap:0.##}，上边距：{topLineSpacingGap:0.##}，下边距：{bottomLineSpacingGap:0.##}",
            LayoutDebugCategory.PreLineSpacingInWholeLine);

        // 3. 设置每个字符在行内的坐标
        //TextPoint charLineStartPoint = currentStartPoint with
        //{
        //    Y = topLineSpacingGap, // 相对于行的坐标，叠加上了行距
        //};
        TextPoint charLineStartPoint = new TextPoint(0, topLineSpacingGap /*topLineSpacingGap*/);

        // 具体设置每个字符的坐标的逻辑
        UpdateTextLineStartPoint(charDataTakeList, charLineStartPoint, maxFontSizeCharData);

        // 行的尺寸
        var lineSize = new TextSize(currentTextSize.Width, lineHeight);

        return new WholeLineLayoutResult(lineSize, currentTextSize, wholeCharCount,
            new TextThickness(0, topLineSpacingGap, 0, bottomLineSpacingGap));
    }

    /// <summary>
    /// 布局一行里面有哪些字符。只有字符排列，决定哪些字符可以排在当前行里面，不包括行距等信息，不包括将每个字放在行内的具体位置
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    private WholeLineCharsLayoutResult UpdateWholeLineCharsLayout(in WholeLineLayoutArgument argument)
    {
        IWholeLineCharsLayouter? wholeLineCharsLayouter = TextEditor.PlatformProvider.GetWholeLineCharsLayouter();
        if (wholeLineCharsLayouter != null)
        {
            return wholeLineCharsLayouter.UpdateWholeLineCharsLayout(in argument);
        }

        TextReadOnlyListSpan<CharData> charDataList = argument.CharDataList;
        double lineMaxWidth = argument.LineMaxWidth;
        UpdateLayoutContext context = argument.UpdateLayoutContext;

#if DEBUG
        // 调试下显示当前这一行的文本，方便了解当前在哪一行
        string currentLineText = argument.DebugText;
        GC.KeepAlive(currentLineText);
#endif

        var singleRunLineLayouter = TextEditor.PlatformProvider.GetSingleRunLineLayouter();

        // RunLineMeasurer
        // 行还剩余的空闲宽度
        double lineRemainingWidth = lineMaxWidth;

        // 当前相对于 charDataList 的当前序号
        int currentIndex = 0;
        // 当前的字符布局尺寸
        var currentSize = TextSize.Zero;

        while (currentIndex < charDataList.Count)
        {
            // 一行里面需要逐个字符进行布局
            var arguments = new SingleCharInLineLayoutArgument(charDataList, currentIndex, lineRemainingWidth,
                argument.Paragraph, context);

            SingleCharInLineLayoutResult result;
            if (singleRunLineLayouter is not null)
            {
                result = singleRunLineLayouter.LayoutSingleCharInLine(arguments);

#if DEBUG
                if (result.TotalSize.Width > 0 && result.TotalSize.Height == 0)
                {
                    // 这可能是不正常的情况
                    // 可以在这里打断点看
                    TextEditor.Logger.LogDebug($"单行布局结果是有宽度没高度，预计是不正确的情况。仅调试下输出");
                }
#endif
            }
            else
            {
                result = UpdateSingleCharInLineLayout(in arguments);
            }

            if (result.CanTake)
            {
                currentSize = currentSize.HorizontalUnion(result.TotalSize);

                currentIndex += result.TakeCount;

                // 计算此行剩余的宽度
                lineRemainingWidth -= result.TotalSize.Width;
            }

            if (result.ShouldBreakLine)
            {
                // 换行，这一行布局完成
                break;
            }
        }

        // 整个行所使用的字符数量
        var wholeCharCount = currentIndex;
        return new WholeLineCharsLayoutResult(currentSize, wholeCharCount);
    }

    /// <summary>
    /// 更新行的起始点
    /// </summary>
    /// <param name="lineCharList"></param>
    /// <param name="charLineStartPoint">文档布局给到行的距离</param>
    /// <param name="maxFontSizeCharData"></param>
    private void UpdateTextLineStartPoint(TextReadOnlyListSpan<CharData> lineCharList, TextPoint charLineStartPoint, CharData maxFontSizeCharData)
    {
        // 是否需要重新排列 X 坐标。对于只是更新行的 Y 坐标的情况，是不需要重新排列的
        // 需要重新排列 X 坐标，因为这一行的字符可能是新加入的或修改的，需要重新排列 X 坐标
        const bool reArrangeXPosition = true;
        // 是否需要更新 CharLayoutData 版本
        // 不需要。 这时候不需要更新 CharLayoutData 版本，因为这个版本是在布局行的时候更新的
        // 这时候必定这一行需要重新更新渲染，不需要标记脏，这是新加入的布局行
        const bool needUpdateCharLayoutDataVersion = false;

        var lineTop = charLineStartPoint.Y; // 相对于行的坐标，绝大部分情况下应该是 0 的值
        var currentX = charLineStartPoint.X;

        // 求基线的行内偏移量
        double maxFontYOffset = lineTop;
        // 计算出最大字符的基线坐标
        maxFontYOffset += maxFontSizeCharData.Baseline;

        foreach (CharData charData in lineCharList)
        {
            // 计算和更新每个字符的相对文本框的坐标
            Debug.Assert(charData.Size != null, "charData.LineCharSize != null");
            var charDataSize = charData.Size!.Value;

            double xOffset;
            if (reArrangeXPosition)
            {
                xOffset = currentX;
            }
            else
            {
                // 保持 X 不变
                //xOffset = charData.CharLayoutData!.CharLineStartPoint.X;
            }

            // 通过将最大字符的基线和当前字符的基线的差，来计算出当前字符的偏移量
            // 如此可以实现字体的基线对齐
            double yOffset = maxFontYOffset - charData.Baseline;

            charData.SetLayoutCharLineStartPoint(new TextPointInLineCoordinate(xOffset, yOffset)/*, new TextPoint(xOffset, yOffset)*/);

            if (needUpdateCharLayoutDataVersion)
            {
                charData.CharLayoutData.UpdateVersion();
            }

            currentX += charDataSize.Width;
        }
    }

    #endregion  

    /// <summary>
    /// 布局一行里面的单个字符
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    private SingleCharInLineLayoutResult UpdateSingleCharInLineLayout(in SingleCharInLineLayoutArgument argument)
    {
        // LayoutRule 布局规则
        // 可选无规则-直接字符布局，预计没有人使用
        // 调用分词规则-支持注入分词换行规则
        UpdateLayoutContext updateLayoutContext = argument.UpdateLayoutContext;
        TextReadOnlyListSpan<CharData> currentRunList = argument.SliceFromCurrentRunList();

#if DEBUG
        var currentCharData = argument.CurrentCharData;
        Debug.Assert(ReferenceEquals(currentCharData, currentRunList[0]));
#endif
        IWordDivider wordDivider = updateLayoutContext.PlatformProvider.GetWordDivider();
        DivideWordResult divideWordResult = wordDivider.DivideWord(new DivideWordArgument(currentRunList, updateLayoutContext));

        // todo 后续连字符的情况也要考虑

        int takeCount = divideWordResult.TakeCount;
        // 测量 takeCount 下的字符宽度
        var totalSize = TextSize.Zero;
        for (int i = 0; i < takeCount; i++)
        {
            var charData = currentRunList[i];

            if (charData.Size is null)
            {
                var fillSizeOfRunArgument = new FillSizeOfRunArgument(currentRunList.Slice(i), updateLayoutContext);
                Debug.Assert(ReferenceEquals(charData, fillSizeOfRunArgument.CurrentCharData));

                MeasureAndFillSizeOfRun(fillSizeOfRunArgument);

                // 不用扔调试异常了，在 MeasureAndFillSizeOfRun 方法里面已经扔过了。只加一个额外调试判断就好了。以下是一个多余的判断，只是为了不耦合 MeasureAndFillSizeOfRun 方法的判断而已
                Debug.Assert(charData.Size != null, $"经过 {nameof(MeasureAndFillSizeOfRun)} 方法可确保 CurrentCharData 的 Size 一定不空");
            }

            // todo 考虑字间距的情况
            // 这里直接加等合并，是不包含 Kern 字间距的情况
            totalSize = totalSize.HorizontalUnion(charData.Size.Value);
        }

        if (argument.LineRemainingWidth >= totalSize.Width)
        {
            // 完全能放下当前单词
            return new SingleCharInLineLayoutResult(takeCount, totalSize);
        }
        else
        {
            // 这个单词不能获取到。此时需要额外判断逻辑
            if (argument.ParagraphProperty.AllowHangingPunctuation)
            {
                // todo 是否允许符号溢出边界的情况也需要考虑
                // 需要 DivideWordResult 报告是否末尾是符号的情况
            }

            // 是否这一行一个单词都没有。如果不能在一行放下，需要判断 IsTakeEmpty 属性。防止一行行都放不下的情况
            if (argument.IsTakeEmpty)
            {
                // 空行强行换行，否则下一行说不定也不够放
                // LayoutCharWithoutCulture
                int i = 0;
                totalSize = TextSize.Zero;
                var testSize = TextSize.Zero;
                for (; i < takeCount/*这个限制是多余的，必定最终小于 takeCount 宽度*/; i++)
                {
                    // 单个字符直接布局，无视语言文化。快，但是诡异

                    var charData = currentRunList[i];
                    Debug.Assert(charData.Size != null, "进入当前逻辑里，必然已经完成字符尺寸测量");

                    testSize = testSize.HorizontalUnion(charData.Size.Value);

                    if (testSize.Width > argument.LineRemainingWidth)
                    {
                        break;
                    }
                    else
                    {
                        totalSize = testSize;
                    }
                }

                // 刚好第 i 个就超过了可用宽度。于是在 i - 1 个时，还是满足条件的。数量是序号加 1 的值，因此就是在 (i - 1) 的序号上加 1 就是当前获取的数量
                var actualTakeCount = (i - 1) + 1;
                // 至少强行取一个字符
                actualTakeCount = Math.Max(actualTakeCount, 1);
                return new SingleCharInLineLayoutResult(actualTakeCount, totalSize);
            }
            else
            {
                // 那就是拿不到一个字符啦
                // 如果尺寸不足，也就是一个都拿不到
                return new SingleCharInLineLayoutResult(takeCount: 0, default);
            }
        }
    }

    /// <inheritdoc />
    protected override TextSize CalculateDocumentContentSize(IReadOnlyList<ParagraphData> paragraphList, UpdateLayoutContext updateLayoutContext)
    {
        double documentContentWidth = 0;
        double documentContentHeight = 0;
        foreach (ParagraphData paragraphData in paragraphList)
        {
            IParagraphLayoutData layoutData = paragraphData.ParagraphLayoutData;

            documentContentWidth = Math.Max(documentContentWidth, layoutData.TextSize.Width);

            TextThickness contentThickness = layoutData.TextContentThickness;
            documentContentHeight += contentThickness.Top + layoutData.TextSize.Height + contentThickness.Bottom;
        }

        return new TextSize(documentContentWidth, documentContentHeight);
    }

    #region 辅助方法

    // 这是为分词器提供的接口。现在分词器只做分词，不做布局，所以这个接口不需要
    // [DebuggerStepThrough] // 别跳太多层
    //TextSize IInternalCharDataSizeMeasurer.GetCharSize(CharData charData) => GetCharSize(charData);

    /// <summary>
    /// 获取下一行的起始点
    /// </summary>
    /// 对于横排布局来说，只是更新 Y 值即可
    /// <param name="currentStartPoint"></param>
    /// <param name="currentLineLayoutData"></param>
    /// <returns></returns>
    private static TextPointInParagraphCoordinate GetNextLineStartPoint(TextPointInParagraphCoordinate currentStartPoint, LineLayoutData currentLineLayoutData)
    {
        //currentStartPoint = new TextPoint(currentStartPoint.X, currentStartPoint.Y + currentLineLayoutData.LineContentSize.Height);

        return currentStartPoint.Offset(0, currentLineLayoutData.LineContentSize.Height);
    }

    private static TextSize BuildParagraphSize(in ParagraphLayoutArgument argument)
    {
        var paragraphSize = new TextSize(0, 0);
        foreach (var lineVisualData in argument.ParagraphData.LineLayoutDataList)
        {
            var width = Math.Max(paragraphSize.Width, lineVisualData.LineContentSize.Width);
            var height = paragraphSize.Height + lineVisualData.LineContentSize.Height;

            paragraphSize = new TextSize(width, height);
        }

        return paragraphSize;
    }

    /// <summary>
    /// 获取行的最大宽度
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private double GetLineMaxWidth()
    {
        double lineMaxWidth = TextEditor.SizeToContent switch
        {
            TextSizeToContent.Manual => TextEditor.DocumentManager.DocumentWidth,
            TextSizeToContent.Width => double.PositiveInfinity,
            TextSizeToContent.Height => TextEditor.DocumentManager.DocumentWidth,
            TextSizeToContent.WidthAndHeight => double.PositiveInfinity,
            _ => throw new ArgumentOutOfRangeException()
        };
        return lineMaxWidth;
    }

    #endregion

    /// <inheritdoc />
    protected override TextPointInDocumentContentCoordinate GetNextParagraphLineStartPoint(in TextPointInDocumentContentCoordinate currentPoint,ParagraphData paragraphData)
    {
        const double x = 0;
        var layoutData = paragraphData.ParagraphLayoutData;
        //TextRect textBounds = layoutData.TextContentBounds;
        if (IsInDebugMode)
        {
            if (layoutData.OutlineSize == TextSize.Invalid)
            {
                throw new TextEditorDebugException($"只有完全完成布局的段落才能进入此分支，获取下一段的行起始点");
            }
        }

        return currentPoint.Offset(0, layoutData.OutlineSize.Height);

        // 以下是通过最后一行的值进行计算的。不足的是需要判断空段，因此不如使用段落偏移加上段落高度进行计算
        //if (paragraphData.LineVisualDataList.Count == 0)
        //{
        //    // 这一段没有包含任何的行。这一段里面可能没有任何文本，只是一个 \r\n 的空段
        //    Debug.Assert(paragraphData.CharCount == 0,"只有空段才没有包含行");

        //    const double x = 0;
        //    var layoutData = paragraphData.ParagraphLayoutData;
        //    var y = layoutData.CharStartPoint.Y + layoutData.LineCharSize.Height;
        //    return new Point(x, y);
        //}
        //else
        //{
        //    var lineVisualData = paragraphData.LineVisualDataList.Last();
        //    const double x = 0;
        //    var y = lineVisualData.CharStartPoint.Y + lineVisualData.LineCharSize.Height;
        //    return new Point(x, y);
        //}
    }

    #endregion 02 预布局阶段

    #region 03 回溯最终布局阶段

    /// <summary>
    /// 回溯最终布局文档
    /// </summary>
    /// 布局过程是： 文档-段落-行
    /// <param name="preUpdateDocumentLayoutResult"></param>
    /// <param name="updateLayoutContext"></param>
    protected override FinalUpdateDocumentLayoutResult FinalUpdateDocumentLayout(PreUpdateDocumentLayoutResult preUpdateDocumentLayoutResult, UpdateLayoutContext updateLayoutContext)
    {
        updateLayoutContext.RecordDebugLayoutInfo($"FinalLayoutDocument 进入最终布局阶段", LayoutDebugCategory.FinalDocument);

        TextSize documentContentSize = preUpdateDocumentLayoutResult.DocumentContentSize;
        TextSize documentOutlineSize = CalculateDocumentOutlineSize(in documentContentSize);

        var documentWidth = documentOutlineSize.Width;
        IReadOnlyList<ParagraphData> paragraphList = updateLayoutContext.InternalParagraphList;

        for (var paragraphIndex = 0/*为什么从首段开始？如右对齐情况下，被撑大文档范围，则即使没有变脏也需要更新坐标*/; paragraphIndex < paragraphList.Count; paragraphIndex++)
        {
            ParagraphData paragraphData = paragraphList[paragraphIndex];

            var paragraphLayoutArgument = new FinalParagraphLayoutArgument(paragraphData,
                new ParagraphIndex(paragraphIndex), documentWidth, updateLayoutContext);
            FinalUpdateParagraphLayout(in paragraphLayoutArgument);
        }

        //if (IsInDebugMode)
        //{
        //    // 调试逻辑，理论上下一段的起始点就是等于本段最低点
        //    var lastParagraphOutlineBounds = paragraphList[0].ParagraphLayoutData.OutlineBounds;
        //    for (var paragraphIndex = 1;
        //         paragraphIndex < paragraphList.Count;
        //         paragraphIndex++)
        //    {
        //        // 当前段落的起始点就等于上一段的最低点
        //        ParagraphData paragraphData = paragraphList[paragraphIndex];
        //        var startPoint = paragraphData.ParagraphLayoutData.StartPointInDocumentContentCoordinate;

        //        if(!startPoint.NearlyEqualsY(lastParagraphOutlineBounds.Bottom))
        //        {
        //            // 如果不相等，则证明计算不正确
        //            throw new TextEditorInnerDebugException($"文本段落计算之间存在空隙。当前第 {paragraphIndex} 段。上一段范围： {lastParagraphOutlineBounds} ，当前段的起始点 {startPoint}");
        //        }

        //        lastParagraphOutlineBounds = paragraphData.ParagraphLayoutData.OutlineBounds;
        //    }
        //}

        // 计算内容的左上角起点。处理垂直居中、底部对齐等情况
        var documentStartPoint = CalculateDocumentContentLeftTopStartPoint(in documentContentSize, in documentOutlineSize);
        // 内容范围
        TextRect documentContentBounds = new TextRect(documentStartPoint, documentContentSize);
        // 外接范围
        TextRect documentOutlineBounds = new TextRect(TextPoint.Zero, documentOutlineSize);
        DocumentLayoutBounds documentLayoutBounds = new DocumentLayoutBounds(documentContentBounds,documentOutlineBounds);

        updateLayoutContext.RecordDebugLayoutInfo($"FinalLayoutDocument 完成最终布局阶段。文档内容范围：{documentContentBounds} 文档外接范围：{documentOutlineBounds}", LayoutDebugCategory.FinalDocument);

        return new FinalUpdateDocumentLayoutResult(documentLayoutBounds);
    }

    readonly record struct FinalParagraphLayoutArgument(ParagraphData Paragraph, ParagraphIndex ParagraphIndex, double DocumentWidth, UpdateLayoutContext UpdateLayoutContext);

    /// <summary>
    /// 回溯最终布局段落
    /// </summary>
    /// <param name="argument"></param>
    private static void FinalUpdateParagraphLayout(in FinalParagraphLayoutArgument argument)
    {
        UpdateLayoutContext updateLayoutContext = argument.UpdateLayoutContext;
        int paragraphIndex = argument.ParagraphIndex.Index;
        double documentWidth = argument.DocumentWidth;

        updateLayoutContext.RecordDebugLayoutInfo($"开始回溯第 {paragraphIndex} 段", LayoutDebugCategory.FinalParagraph);

        ParagraphData paragraph = argument.Paragraph;
        Debug.Assert(paragraphIndex == paragraph.Index.Index, "参数拿到的 ParagraphIndex 是不用计算的，而 paragraph.Index 是需要遍历计算的。这两个值应该是相同的");

        IParagraphLayoutData layoutData = paragraph.ParagraphLayoutData;

        for (int lineIndex = 0; lineIndex < paragraph.LineLayoutDataList.Count; lineIndex++)
        {
            updateLayoutContext.RecordDebugLayoutInfo($"开始回溯第 {paragraphIndex} 段的第 {lineIndex} 行", LayoutDebugCategory.FinalLine);

            LineLayoutData lineLayoutData = paragraph.LineLayoutDataList[lineIndex];
            var lineLayoutArgument = new FinalParagraphLineLayoutArgument(lineIndex, lineLayoutData, argument);

            FinalUpdateParagraphLineLayout(in lineLayoutArgument);
        }

        // 给定段落的尺寸
        TextThickness contentThickness = layoutData.TextContentThickness;
        paragraph.SetParagraphLayoutOutlineSize(new TextSize
        {
            Width = documentWidth,
            Height = contentThickness.Top + layoutData.TextSize.Height + contentThickness.Bottom
        });

        updateLayoutContext.RecordDebugLayoutInfo($"完成回溯第 {paragraphIndex} 段", LayoutDebugCategory.FinalParagraph);

        // 设置当前段落已经布局完成
        paragraph.SetFinishLayout();
    }

    readonly record struct FinalParagraphLineLayoutArgument
    (
        int LineIndex,
        LineLayoutData LineLayoutData,
        FinalParagraphLayoutArgument FinalParagraphLayoutArgument
    )
    {
        public bool IsFirstLine => LineIndex == 0;
        public bool IsLastLine => LineIndex == FinalParagraphLayoutArgument.Paragraph.LineLayoutDataList.Count - 1;
    }

    /// <summary>
    /// 回溯最终布局行
    /// </summary>
    /// <param name="lineLayoutArgument"></param>
    /// <exception cref="NotSupportedException"></exception>
    private static void FinalUpdateParagraphLineLayout(in FinalParagraphLineLayoutArgument lineLayoutArgument)
    {
        FinalParagraphLayoutArgument paragraphLayoutArgument = lineLayoutArgument.FinalParagraphLayoutArgument;
        UpdateLayoutContext updateLayoutContext = paragraphLayoutArgument.UpdateLayoutContext;
        ParagraphProperty paragraphProperty = paragraphLayoutArgument.Paragraph.ParagraphProperty;
        double documentWidth = paragraphLayoutArgument.DocumentWidth;

        var isFirstLine = lineLayoutArgument.IsFirstLine;
        // 是否最后一行
        var isLastLine = lineLayoutArgument.IsLastLine;
        _ = isLastLine;

        LineLayoutData lineLayoutData = lineLayoutArgument.LineLayoutData;

        // 空白的宽度
        var gapWidth = documentWidth - lineLayoutData.LineContentSize.Width;
        double leftIndentation = paragraphProperty.LeftIndentation;
        double indent = paragraphProperty.GetIndent(isFirstLine);

        var indentationThickness =
            new TextThickness(leftIndentation + indent, 0, paragraphProperty.RightIndentation, 0);

        // 可用的空白宽度。即空白宽度减去左缩进和右缩进
        double usableGapWidth = gapWidth - indentationThickness.Left - indentationThickness.Right;

        TextThickness horizontalTextAlignmentGapThickness = paragraphProperty.GetHorizontalTextAlignmentGapThickness(usableGapWidth);

        lineLayoutData
            .SetLineFinalLayoutInfo(indentationThickness, horizontalTextAlignmentGapThickness);

        // 计算 Outline 的范围
        var outlineStartPoint = lineLayoutData.CharStartPointInParagraphCoordinate.ResetX(0);
        var outlineWidth = documentWidth;
        var outlineHeight = lineLayoutData.LineContentSize.Height;

        lineLayoutData.SetOutlineBounds(outlineStartPoint,new TextSize(outlineWidth,outlineHeight));

        if (updateLayoutContext.IsInDebugMode)
        {
            // 调试模式，校验一下宽度
            var contentWidth = lineLayoutData.LineContentSize.Width;

            Debug.Assert(Nearly.Equals(outlineWidth, documentWidth));

            var contentWidthAddThickness = contentWidth + indentationThickness.Left + indentationThickness.Right;

            // 以下的等于关系是不正确的。因为一行里面有文本的内容可能不满行，如以下例子
            // 123123
            // 123\n
            // 可见末行的内容只有 `123` 无法占满行内容，则此时以下判断条件不相等
            //Debug.Assert(Nearly.Equals(outlineWidth, contentWidthAddThickness),"外接的宽度等于内容框架加上各个边距");
            Debug.Assert(!paragraphProperty.AllowHangingPunctuation && contentWidthAddThickness <= outlineWidth,
                "什么时候小于？一行字符不满的时候。什么时候等于？刚好一行字符刚好满，这里还要求行宽度是字符宽度的倍数。什么时候可能大于？允许标点溢出边界");
        }
    }

    /// <summary>
    /// 计算文档内容的左上角点，处理垂直居中、底部对齐等情况
    /// </summary>
    /// <returns></returns>
    private TextPoint CalculateDocumentContentLeftTopStartPoint(in TextSize documentContentSize, in TextSize documentOutlineSize)
    {
        VerticalTextAlignment verticalTextAlignment = TextEditor.VerticalTextAlignment;
        // 如果是顶部对齐，那么直接返回 0,0 即可
        if (verticalTextAlignment == VerticalTextAlignment.Top)
        {
            return TextPoint.Zero;
        }

        double documentHeight = documentContentSize.Height;
        double outlineHeight = documentOutlineSize.Height;
        var gapHeight = outlineHeight - documentHeight;
        Debug.Assert(gapHeight>=0,"外接的尺寸高度肯定大于等于内容尺寸");

        const int left = 0; // 水平方向不需要处理
        var top = verticalTextAlignment switch
        {
            //VerticalTextAlignment.Top => 0, // 这条已经提前处理了
            VerticalTextAlignment.Center => gapHeight / 2,
            VerticalTextAlignment.Bottom => gapHeight,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new TextPoint(left, top);
    }

    #endregion 03 回溯最终布局阶段

    #region 通用辅助方法

    private TextSize CalculateDocumentOutlineSize(in TextSize documentContentSize)
    {
        // 获取当前文档的大小，对水平布局来说，只取横排的最大值即可
        double lineMaxWidth = GetLineMaxWidth();
        var documentWidth = lineMaxWidth;
        if (!double.IsFinite(documentWidth))
        {
            // 非有限宽度，则采用文档的宽度
            documentWidth = documentContentSize.Width;
        }

        var documentHeight = TextEditor.DocumentManager.DocumentHeight;
        if (!double.IsFinite(documentHeight))
        {
            documentHeight = documentContentSize.Height;
        }

        return new TextSize(documentWidth, documentHeight);
    }

    ///// <inheritdoc />
    //protected override TextRect CalculateHitBounds(in TextRect documentBounds)
    //{
    //    // 获取当前文档的大小，对水平布局来说，只取横排的最大值即可
    //    double lineMaxWidth = GetLineMaxWidth();
    //    var documentWidth = lineMaxWidth;
    //    if (!double.IsFinite(documentWidth))
    //    {
    //        // 非有限宽度，则采用文档的宽度
    //        documentWidth = documentBounds.Width;
    //    }

    //    // 高度不改变。这样即可让命中的时候，命中到文档下方时，命中可计算超过命中范围
    //    return documentBounds with { Width = documentWidth };
    //}

    #endregion 通用辅助方法
}
