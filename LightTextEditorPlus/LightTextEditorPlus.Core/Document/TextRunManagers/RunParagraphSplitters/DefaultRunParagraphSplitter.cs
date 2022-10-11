﻿using System;
using System.Collections.Generic;

namespace LightTextEditorPlus.Core.Document;

internal class DefaultRunParagraphSplitter : IRunParagraphSplitter
{
    public IEnumerable<IRun> Split(IRun run)
    {
        if (run is TextRun textRun)
        {
            var text = textRun.Text;
            foreach (var subText in Split(text))
            {
                yield return new TextRun(subText, textRun.RunProperty);
            }
        }
        else
        {
            // todo 处理非文本的情况
            throw new NotImplementedException();
        }
    }

    private IEnumerable<string> Split(string text)
    {
        throw new NotImplementedException();
    }
}