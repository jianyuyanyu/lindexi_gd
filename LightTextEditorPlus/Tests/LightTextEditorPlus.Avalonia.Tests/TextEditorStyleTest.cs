using Avalonia.Threading;

using LightTextEditorPlus.Core.Carets;
using LightTextEditorPlus.Core.Document;
using LightTextEditorPlus.Document;
using SkiaSharp;

namespace LightTextEditorPlus.Avalonia.Tests;

[TestClass]
public class TextEditorStyleTest
{
    [TestMethod("整段文本字符属性设置之后，经过撤销恢复，能够还原状态")]
    public async Task TestSetRunProperty1()
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Arrange
            using var context = TestFramework.CreateTextEditorInNewWindow();
            var textEditor = context.TextEditor;

            SkiaTextRunProperty runProperty = textEditor.CreateRunProperty(property => property with
            {
                FontSize = 62.5,
                FontWeight = SKFontStyleWeight.Bold,
            });
            textEditor.Text = "123\nabc123";

            await textEditor.WaitForRenderCompletedAsync();

            textEditor.CurrentCaretOffset = new CaretOffset("123\n".Length, isAtLineStart: true);
            ITextParagraph paragraph = textEditor.GetCurrentCaretOffsetParagraph();

            var selection = textEditor.GetParagraphSelection(paragraph);
            var originRunProperty = textEditor.GetRunPropertyRange(in selection).First();

            // Action
            textEditor.SetRunProperty(runProperty, selection);

            // Assert
            var runProperty1 = textEditor.GetRunPropertyRange(in selection).First();
            Assert.AreEqual(runProperty, runProperty1, "设置进去的字符属性，应该能够设置成功，能够拿到传入的字符属性");
            // 预期此时就和原始的不相同的了
            Assert.AreNotEqual(originRunProperty, runProperty1);
            // 原始的应该和样式相同
            Assert.AreEqual(textEditor.StyleRunProperty, originRunProperty);

            // 再测试撤销恢复
            for (int i = 0; i < 10; i++)
            {
                // Action
                // 撤销之后，应该和原始的相同
                textEditor.TextEditorCore.UndoRedoProvider.Undo();
                // Assert
                var runProperty2 = textEditor.GetRunPropertyRange(in selection).First();
                Assert.AreEqual(originRunProperty, runProperty2, "撤销之后，应该能还原为和原来的相同的文本字符属性");

                if (i > 5)
                {
                    // 同时也要测试经过了 UI 布局渲染之后的情况
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                }

                // Action
                textEditor.TextEditorCore.UndoRedoProvider.Redo();
                // Assert
                var runProperty3 = textEditor.GetRunPropertyRange(in selection).First();
                Assert.AreEqual(runProperty, runProperty3);

                if (i > 5)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                }
            }

            await TestFramework.FreezeTestToDebug();
        });
    }
}