﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LightTextEditorPlus.Core.Document.DocumentManagers;
using LightTextEditorPlus.Core.Document.Segments;
using LightTextEditorPlus.Core.Utils;

namespace LightTextEditorPlus.Core.Document;

internal class TextRunManager
{
    public TextRunManager(TextEditorCore textEditor)
    {
        TextEditor = textEditor;
        ParagraphManager = new ParagraphManager(textEditor);
    }

    public ParagraphManager ParagraphManager { get; }
    public TextEditorCore TextEditor { get; }

    public void Replace(SelectionSegment selection, IRun run)
    {
        // 先执行删除，再执行插入
        if (selection.SectionLength != 0)
        {
            RemoveInner(selection.SelectionStart, selection.SectionLength);
        }
        else
        {
            // 没有替换的长度，加入即可
        }

        InsertInner(selection.SelectionStart, run);
    }

    /// <summary>
    /// 在文档指定位移<paramref name="offset"/>处插入一段文本
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="run"></param>
    private void InsertInner(int offset, IRun run)
    {
        // 插入的逻辑，找到插入变更的行
        var paragraphData = ParagraphManager.GetParagraphData(offset);

        // 看看是不是在段落中间插入的，如果在段落中间插入的，需要将段落中间移除掉
        // 在插入完成之后，重新加入
        var lastParagraphRunList = paragraphData.SplitRemoveByDocumentOffset(offset);

        // 当前的段落，如果插入的分行的内容，那自然需要自动分段
        var currentParagraph = paragraphData;
        // 看起来不需要中间插入逻辑，只需要插入到最后
        //var insertOffset = offset;

        // 获取 run 的分段逻辑，大部分情况下都是按照 \r\n 作为分段逻辑
        var runParagraphSplitter = TextEditor.PlatformProvider.GetRunParagraphSplitter();
        bool isFirstSubRun = true;
        foreach (var subRun in runParagraphSplitter.Split(run))
        {
            if (subRun is LineBreakRun || !isFirstSubRun)
            {
                // 如果这是一个分段，那直接插入新的段落
                var newParagraph = new ParagraphData(paragraphData.ParagraphProperty);
                ParagraphManager.InsertParagraphAfter(currentParagraph, newParagraph);

                currentParagraph = newParagraph;

                //insertOffset = ParagraphManager.GetParagraphStartDocumentOffset(currentParagraph);
            }
            else
            {
                //paragraphData.InsertRun(insertOffset,subRun);
                //insertOffset += subRun.Count;
                paragraphData.AppendRun(subRun);
            }

            isFirstSubRun = false;
        }

        if (lastParagraphRunList != null)
        {
            // 如果是从一段的中间插入的，需要将这一段在插入点后面的内容继续放入到当前的段落
            currentParagraph.AppendRun(lastParagraphRunList);
        }
    }

    private void RemoveInner(int offset, int length)
    {
        // todo 实现删除逻辑
    }
}

/// <summary>
/// 段落管理
/// </summary>
class ParagraphManager
{
    public ParagraphManager(TextEditorCore textEditor)
    {
        TextEditor = textEditor;
    }

    public TextEditorCore TextEditor { get; }

    public ParagraphData GetParagraphData(DocumentOffset offset)
    {
        if (ParagraphList.Count == 0)
        {
            // 获取当前的段落属性作为默认段落属性
            var paragraphProperty = TextEditor.DocumentManager.CurrentParagraphProperty;

            var paragraphData = new ParagraphData(paragraphProperty);
            ParagraphList.Add(paragraphData);
            return paragraphData;
        }
        else
        {
            if (offset.Offset == 0)
            {
                return ParagraphList[0];
            }

            //todo 还没实现非空行的逻辑
            throw new NotImplementedException();
        }
    }

    public IReadOnlyList<ParagraphData> GetParagraphList() => ParagraphList;

    private List<ParagraphData> ParagraphList { get; } = new List<ParagraphData>();

    public void InsertParagraphAfter(ParagraphData currentParagraph, ParagraphData newParagraph)
    {
        var index = ParagraphList.IndexOf(currentParagraph);
        ParagraphList.Insert(index + 1, newParagraph);
    }

    /// <summary>
    /// 获取某个段落的起始的文档偏移
    /// </summary>
    /// <param name="currentParagraph"></param>
    /// <returns></returns>
    public DocumentOffset GetParagraphStartDocumentOffset(ParagraphData currentParagraph)
    {
        // todo 完成获取某个段落的起始的文档偏移
        return 0;
    }
}

/// <summary>
/// 段落数据
/// </summary>
class ParagraphData
{
    public ParagraphData(ParagraphProperty paragraphProperty)
    {
        ParagraphProperty = paragraphProperty;
    }

    public ParagraphProperty ParagraphProperty { set; get; }

    public List<IRun> TextRunList { get; } = new List<IRun>();

    /// <summary>
    /// 这一段的字符长度
    /// </summary>
    /// todo 考虑缓存字符数量，不需要每次都计算
    public int CharCount => TextRunList.Sum(t => t.Count);

    /// <summary>
    /// 获取这个文本行是否已经从文档中删除.
    /// </summary>
    public bool IsDeleted { set; get; }

    /// <summary>
    /// 是否是脏的，需要重新布局渲染
    /// </summary>
    public bool IsDirty { set; get; } = true;

    /// <summary>
    /// 获取行分隔符的长度
    /// 0 为文档中的最后一行， 1 for <c>"\r"</c> or <c>"\n"</c>, 2 for <c>"\r\n"</c> 
    /// 当本行被删除后这个属性值依然有效，这种情况下，它包含在删除之前的行分隔符的长度
    /// </summary>
    /// 无论在哪个平台上，都统一为 \r\n 两个字符
    public static int DelimiterLength => TextContext.NewLine.Length;


    // 不合适，将会让段落必须知道文档坐标
    ///// <summary>
    ///// 从哪个开始
    ///// </summary>
    //public DocumentOffset? DirtyOffset { set; get; }

    /// <summary>
    /// 在段落中间插入的时候，需要将段落在插入后面的内容分割删除
    /// </summary>
    /// <param name="offset"></param>
    /// <exception cref="NotImplementedException"></exception>
    public IList<IRun>? SplitRemoveByDocumentOffset(int offset)
    {
        // todo 设置LineVisualData是脏的
        if (offset == CharCount)
        {
            // 如果插入在最后，那就啥都不需要做
            return null;
        }
        else
        {
            // 需要考虑将原本合并的 IRun 拆开为多个
            throw new NotImplementedException($"还没实现段落分割的功能");
        }
    }

    /// <summary>
    /// 在指定的地方插入一段文本
    /// </summary>
    /// <param name="insertOffset"></param>
    /// <param name="run"></param>
    public void InsertRun(int insertOffset, IRun run)
    {
        if (insertOffset == CharCount)
        {
            TextRunList.Add(run);
        }
        else
        {
            throw new NotImplementedException($"还没实现从段落中间插入的功能");
        }

        // todo 设置LineVisualData是脏的
    }

    public void AppendRun(IRun run)
    {
        TextRunList.Add(run);
    }

    public void AppendRun(IList<IRun> runList)
    {
        foreach (var run in runList)
        {
            AppendRun(run);
        }
    }

    #region 渲染排版数据

    public List<LineVisualData> LineVisualDataList { get; } = new List<LineVisualData>();

    #endregion

    public ParagraphOffset GetParagraphOffset(IRun run)
    {
        var paragraphOffset = 0;

        foreach (var currentRun in TextRunList)
        {
            if (ReferenceEquals(currentRun, run))
            {
                return new ParagraphOffset(paragraphOffset);
            }
            else
            {
                paragraphOffset += currentRun.Count;
            }
        }

        throw new ArgumentException($"传入的 Run 不是此段落的元素", nameof(run));
    }

    /// <summary>
    /// 给定传入的段落偏移获取是对应 <see cref="TextRunList"/> 的从哪项开始
    /// </summary>
    /// <param name="paragraphOffset"></param>
    /// <returns></returns>
    public int GetRunIndex(ParagraphOffset paragraphOffset)
    {
        var paragraph = this;
        int currentParagraphOffset = 0;
        for (var i = 0; i < paragraph.TextRunList.Count; i++)
        {
            var length = paragraph.TextRunList[i].Count;
            var behindOffset = currentParagraphOffset + length;
            if (behindOffset >= paragraphOffset.Offset)
            {
                var dirtyTextRunIndex = i;
                return dirtyTextRunIndex;
            }
            else
            {
                currentParagraphOffset = behindOffset;
            }
        }

        return -1;
    }
}

/// <summary>
/// 行渲染信息
/// </summary>
class LineVisualData
{
    /// <summary>
    /// 是否是脏的，需要重新布局渲染
    /// </summary>
    public bool IsDirty { set; get; }

    /// <summary>
    /// 这一行的字符长度
    /// </summary>
    public int CharCount => LineRunList?.Sum(t => t.Count) ?? 0;

    /// <summary>
    /// 行里面的文本
    /// </summary>
    public List<IRun>? LineRunList { set; get; }
}