﻿#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using BujeeberehemnaNurgacolarje;

using SkiaInkCore;
using SkiaInkCore.Diagnostics;
using SkiaInkCore.Interactives;
using SkiaInkCore.Primitive;
using SkiaInkCore.Settings;
using SkiaInkCore.Utils;

using SkiaSharp;

namespace ReewheaberekaiNayweelehe;

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
partial record InkingInputInfo(int Id, StylusPoint StylusPoint, ulong Timestamp)
=======
record InkingModeInputArgs(int Id, StylusPoint StylusPoint, ulong Timestamp)
>>>>>>> 26fc699b4b42ce61c693a35760e56996f085d438
{
    public Point Position => StylusPoint.Point;

    /// <summary>
    /// 是否来自鼠标的输入
    /// </summary>
    public bool IsMouse { init; get; }

    /// <summary>
    /// 被合并的其他历史的触摸点。可能为空
    /// </summary>
    public IReadOnlyList<StylusPoint>? StylusPointList { init; get; }
};

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
partial record InkInfo(int Id);
=======
>>>>>>> 06026b0bbb703276589096b11fd69181ce02f21c

=======
>>>>>>> fc149583aa0a4eb1ed4aa8ca82c20621a7b49d41
/// <summary>
/// 画板的配置
/// </summary>
/// <param name="EnableClippingEraser">是否允许使用裁剪方式的橡皮擦，而不是走静态笔迹层</param>
/// <param name="AutoSoftPen">是否开启自动软笔模式</param>
record SkInkCanvasSettings(bool EnableClippingEraser = true, bool AutoSoftPen = true)
=======
enum InputMode
>>>>>>> 9994af5e3facc399bf93df657e69c36f21288956
{
    Ink,
    Manipulate,
}

class InkingInputManager
{
    public InkingInputManager(SkInkCanvas skInkCanvas)
    {
        SkInkCanvas = skInkCanvas;
    }

    public SkInkCanvas SkInkCanvas { get; }

    public InputMode InputMode { set; get; } = InputMode.Manipulate;

    private int _downCount;

    private StylusPoint _lastStylusPoint;

    public void Down(InkingModeInputArgs args)
    {
        _downCount++;
        if (_downCount > 2)
        {
            InputMode = InputMode.Manipulate;
        }

        if (InputMode == InputMode.Ink)
        {
            SkInkCanvas.DrawStrokeDown(args);
        }
        else if (InputMode == InputMode.Manipulate)
        {
            _lastStylusPoint = args.StylusPoint;
        }
    }

    public void Move(InkingModeInputArgs args)
    {
        if (InputMode == InputMode.Ink)
        {
            SkInkCanvas.DrawStrokeMove(args);
        }
        else if (InputMode == InputMode.Manipulate)
        {
            SkInkCanvas.ManipulateMove(new Point(args.StylusPoint.Point.X - _lastStylusPoint.Point.X, args.StylusPoint.Point.Y - _lastStylusPoint.Point.Y));

            _lastStylusPoint = args.StylusPoint;
        }
    }

    public void Up(InkingModeInputArgs args)
    {
        if (InputMode == InputMode.Ink)
        {
            SkInkCanvas.DrawStrokeUp(args);
        }
        else if (InputMode == InputMode.Manipulate)
        {
            SkInkCanvas.ManipulateMove(new Point(args.StylusPoint.Point.X - _lastStylusPoint.Point.X, args.StylusPoint.Point.Y - _lastStylusPoint.Point.Y));
            SkInkCanvas.ManipulateFinish();

            _lastStylusPoint = args.StylusPoint;
        }
    }
}

=======
>>>>>>> ce8eee3cf06aef12e1d325fcb5d0e447eac79f34
partial class SkInkCanvas
=======
partial class SkInkCanvas : IInkingInputProcessor, IInkingModeInputDispatcherSensitive
>>>>>>> 80e4690e464b58133b931ec1a63ee7a6c9d5df5e
{
    public SkInkCanvas(SKCanvas skCanvas, SKBitmap applicationDrawingSkBitmap)
    {
        _skCanvas = skCanvas;
        ApplicationDrawingSkBitmap = applicationDrawingSkBitmap;
    }

    public SkInkCanvasSettings Settings { get; set; } = new SkInkCanvasSettings();

    public event EventHandler<Rect>? RenderBoundsChanged;

    private readonly SKCanvas _skCanvas;

    /// <summary>
    /// 原应用输出的内容
    /// </summary>
    public SKBitmap ApplicationDrawingSkBitmap { set; get; }

    /// <summary>
    /// 开始书写时对当前原应用输出的内容 <see cref="ApplicationDrawingSkBitmap"/> 制作的快照，用于解决笔迹的平滑处理，和笔迹算法相关
    /// </summary>
    private SKBitmap? _originBackground;

    /// <summary>
    /// 是否原来的背景，即充当静态层的界面是无效的
    /// </summary>
    private bool _isOriginBackgroundDisable = false;

    /// <summary>
    /// 静态笔迹层
    /// </summary>
    public List<SkiaStrokeSynchronizer> StaticInkInfoList { get; } = new List<SkiaStrokeSynchronizer>();

    private Dictionary<int, DrawStrokeContext> CurrentInputDictionary { get; } =
        new Dictionary<int, DrawStrokeContext>();

    public SKColor Color { set; get; } = SKColors.Red;

    public event EventHandler<SkiaStrokeSynchronizer>? StrokesCollected;

    //public IEnumerable<string> CurrentInkStrokePathEnumerable =>
    //    CurrentInputDictionary.Values.Select(t => t.InkStrokePath).Where(t => t != null).Select(t => t!.ToSvgPathData());


    public void DrawStrokeDown(InkingModeInputArgs args)
    {
        var context = new DrawStrokeContext(InkId.NewId(), args, Color, 20);
        CurrentInputDictionary[args.Id] = context;

        context.AllStylusPoints.Add(args.StylusPoint);
        context.TipStylusPoints.Enqueue(args.StylusPoint);
    }

    public void DrawStrokeMove(InkingModeInputArgs args)
    {
        if (CurrentInputDictionary.TryGetValue(args.Id, out var context))
        {
            context.AllStylusPoints.Add(args.StylusPoint);
            context.TipStylusPoints.Enqueue(args.StylusPoint);

            context.InkStrokePath?.Dispose();

            var outlinePointList = SimpleInkRender.GetOutlinePointList(context.AllStylusPoints.ToArray(), context.InkThickness);

            var skPath = new SKPath();
            skPath.AddPoly(outlinePointList.Select(t => new SKPoint((float) t.X, (float) t.Y)).ToArray());

            context.InkStrokePath = skPath;

            DrawAllInk();

            // 计算脏范围，用于渲染更新
            var additionSize = 100d; // 用于设置比简单计算的范围更大一点的范围，解决重采样之后的模糊
            var (x, y) = args.StylusPoint.Point;

            RenderBoundsChanged?.Invoke(this,
                new Rect(x - additionSize / 2, y - additionSize / 2, additionSize, additionSize));
        }
    }

    public void DrawAllInk()
    {
        var skCanvas = _skCanvas;
        skCanvas.Clear(Settings.ClearColor);

        using var skPaint = new SKPaint();
        skPaint.StrokeWidth = 0;
        skPaint.IsAntialias = true;
        skPaint.FilterQuality = SKFilterQuality.High;
        skPaint.Style = SKPaintStyle.Fill;

        foreach (var drawStrokeContext in CurrentInputDictionary)
        {
            skPaint.Color = drawStrokeContext.Value.StrokeColor;

            if (drawStrokeContext.Value.InkStrokePath is { } path)
            {
                skCanvas.DrawPath(path, skPaint);
            }
        }

        foreach (var skiaStrokeSynchronizer in StaticInkInfoList)
        {
            DrawInk(skCanvas, skPaint, skiaStrokeSynchronizer);
        }

        skCanvas.Flush();
    }

    private static void DrawInk(SKCanvas skCanvas, SKPaint skPaint, SkiaStrokeSynchronizer inkInfo)
    {
        skPaint.Color = inkInfo.StrokeColor;

        if (inkInfo.InkStrokePath is { } path)
        {
            skCanvas.DrawPath(path, skPaint);
        }
    }

    public void DrawStrokeUp(InkingModeInputArgs args)
    {
        if (CurrentInputDictionary.Remove(args.Id, out var context))
        {
            context.IsUp = true;

            StaticInkInfoList.Add(new SkiaStrokeSynchronizer((uint) args.Id, context.InkId, context.StrokeColor, context.InkThickness, context.InkStrokePath, context.AllStylusPoints));
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
    /// <summary>
    /// 绘制使用的上下文信息
    /// </summary>
    /// <param name="inputInfo"></param>
    class DrawStrokeContext(InkingInputInfo inputInfo, SKColor strokeColor) : IDisposable
    {
        public SKColor StrokeColor { get; } = strokeColor;
        public InkingInputInfo InputInfo { set; get; } = inputInfo;
        public int DropPointCount { set; get; }

        /// <summary>
        /// 笔尖的点
        /// </summary>
        public readonly FixedQueue<StylusPoint> TipStylusPoints = new FixedQueue<StylusPoint>(MaxTipStylusCount);

        /// <summary>
        /// 整个笔迹的点，包括笔尖的点
        /// </summary>
        public List<StylusPoint> AllStylusPoints { get; } = new List<StylusPoint>();

        public SKPath? InkStrokePath { set; get; }

        public bool IsUp { set; get; }

        public void Dispose()
        {
            //InkStrokePath?.Dispose();
        }
    }

    /// <summary>
    /// 取多少个点做笔尖
    /// </summary>
    private const int MaxTipStylusCount = 7;
<<<<<<< HEAD
<<<<<<< HEAD
}

partial class SkInkCanvas
{
    public SkInkCanvas()
    {
    }

    public SkInkCanvasSettings Settings { get; set; } = new SkInkCanvasSettings();

    public void SetCanvas(SKCanvas canvas)
    {
        _skCanvas = canvas;
    }

<<<<<<< HEAD
    public SKCanvas? SkCanvas => _skCanvas;
    private SKCanvas? _skCanvas;
=======
>>>>>>> 423d0d697db86f6b4bda17c5b4e51fb5d4a99877


    /// <summary>
    /// 原来的背景
    /// </summary>
    private SKBitmap? _originBackground;

    private bool _isOriginBackgroundDisable = false;

    //public SKSurface? SkSurface { set; get; }

<<<<<<< HEAD
    public event EventHandler<Rect>? RenderBoundsChanged;
    public void RaiseRenderBoundsChanged(Rect rect) => RenderBoundsChanged?.Invoke(this, rect);
=======
>>>>>>> 5f60ba247fc4ed21a521355bcad673325d210fa3


<<<<<<< HEAD
    public IEnumerable<SKPath> CurrentInkStrokePathEnumerable => CurrentInputDictionary.Values.Select(t => t.InkStrokePath)
        .Where(t => t != null)!;

    /// <summary>
    /// 取多少个点做笔尖
    /// </summary>
    private const int MaxTipStylusCount = 7;
=======
>>>>>>> 06026b0bbb703276589096b11fd69181ce02f21c


    private readonly StylusPoint[] _cache = new StylusPoint[MaxTipStylusCount + 1];

    private int MainInputId { set; get; }

    private void InputStart()
    {
        // 这是浅拷贝
        //_originBackground = SkBitmap?.Copy();

        Console.WriteLine("==========InputStart============");

        if (ApplicationDrawingSkBitmap is null)
        {
            return;
        }

        _originBackground ??= new SKBitmap(new SKImageInfo(ApplicationDrawingSkBitmap.Width,
            ApplicationDrawingSkBitmap.Height, ApplicationDrawingSkBitmap.ColorType,
            ApplicationDrawingSkBitmap.AlphaType,
            ApplicationDrawingSkBitmap.ColorSpace), SKBitmapAllocFlags.None);
        _isOriginBackgroundDisable = false;

        using var skCanvas = new SKCanvas(_originBackground);
        skCanvas.Clear();
        skCanvas.DrawBitmap(ApplicationDrawingSkBitmap, 0, 0);
    }

    public void Down(InkingInputInfo info)
    {
        CurrentInputDictionary.Add(info.Id, new DrawStrokeContext(info, Color));

        if (CurrentInputDictionary.Count == 1)
        {
            InputStart();
            MainInputId = info.Id;
        }
    }

    public void Move(InkingInputInfo info)
    {
        if (!CurrentInputDictionary.ContainsKey(info.Id))
        {
            // 如果丢失按下，那就不能画
            // 解决鼠标在其他窗口按下，然后移动到当前窗口
            return;
        }

        var context = UpdateInkingStylusPoint(info);

        if (IsInEraserMode)
        {
            if (info.Id == MainInputId)
            {
                MoveEraser(info);
            }
        }
        else
        {
            if (DrawStroke(context, out var rect))
            {
                RenderBoundsChanged?.Invoke(this, rect);
            }
        }
    }

    public void Up(InkingInputInfo info)
    {
        var context = UpdateInkingStylusPoint(info);

        if (IsInEraserMode)
        {
            if (info.Id == MainInputId)
            {
                UpEraser(info);
            }
        }
        else
        {
            if (DrawStroke(context, out var rect))
            {
                RenderBoundsChanged?.Invoke(this, rect);
            }
        }


        context.DropPointCount = 0;
        context.TipStylusPoints.Clear();

        context.IsUp = true;

        if (CurrentInputDictionary.All(t => t.Value.IsUp))
        {
            InputComplete();
        }
    }

    private void InputComplete()
    {
        // 不加这句话，将会在 foreach 里炸掉，不知道是不是 CLR 的坑
        var enumerator = CurrentInputDictionary.GetEnumerator();

        foreach (var drawStrokeContext in CurrentInputDictionary)
        {
            drawStrokeContext.Value.Dispose();
        }

        CurrentInputDictionary.Clear();

        _isOriginBackgroundDisable = true;

        InputCompleted?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? InputCompleted;

    /// <summary>
    /// 这是 WPF 的概念，那就继续用这个概念
    /// </summary>
    public void Leave()
    {
        InputComplete();

        if (_isOriginBackgroundDisable)
        {
            return;
        }

        if (_skCanvas is null || _originBackground is null)
        {
            Console.WriteLine(
                $"Leave-------- 进入非预期分支，除非是初始化 _skCanvas is null = {_skCanvas is null} _originBackground is null={_originBackground is null}");
            return;
        }

        Console.WriteLine("Leave--------");

        var skCanvas = _skCanvas;
        skCanvas.Clear(SKColors.Transparent);
        skCanvas.DrawBitmap(_originBackground, 0, 0);
        // 完全重绘，丢掉画出来的笔迹
        RenderBoundsChanged?.Invoke(this, new Rect(0, 0, _originBackground.Width, _originBackground.Height));
    }

    private DrawStrokeContext UpdateInkingStylusPoint(InkingInputInfo info)
    {
        if (CurrentInputDictionary.TryGetValue(info.Id, out var context))
        {
            var lastInfo = context.InputInfo;
            var stylusPoint = info.StylusPoint;
            var lastStylusPoint = lastInfo.StylusPoint;
            stylusPoint = stylusPoint with
            {
                Pressure = stylusPoint.IsPressureEnable ? stylusPoint.Pressure : lastStylusPoint.Pressure,
                Width = stylusPoint.Width ?? lastStylusPoint.Width,
                Height = stylusPoint.Height ?? lastStylusPoint.Height,
            };

            info = info with
            {
                StylusPoint = stylusPoint
            };

            context.InputInfo = info;
            return context;
        }
        else
        {
            context = new DrawStrokeContext(info, Color);
            CurrentInputDictionary.Add(info.Id, context);
            return context;
        }
    }

    /// <summary>
    /// 按照德熙的玄幻算法，决定传入的点是否能丢掉
    /// </summary>
    /// <param name="pointList"></param>
    /// <param name="currentStylusPoint"></param>
    /// <returns></returns>
    private bool CanDropLastPoint(Span<StylusPoint> pointList, StylusPoint currentStylusPoint)
    {
        if (pointList.Length < 2)
        {
            return false;
        }

        var lastPoint = pointList[^1];

        // 后续可以优化算法参考 Sia 的
        // 简单判断点的距离
        if (Math.Pow(lastPoint.Point.X - currentStylusPoint.Point.X, 2) +
            Math.Pow(lastPoint.Point.Y - currentStylusPoint.Point.Y, 2) < 100)
        {
            return true;
        }

        return false;
    }

    private bool DrawStroke(DrawStrokeContext context, out Rect drawRect)
    {
        StylusPoint currentStylusPoint = context.InputInfo.StylusPoint;

        drawRect = Rect.Zero;
        if (context.TipStylusPoints.Count == 0)
        {
            context.TipStylusPoints.Enqueue(currentStylusPoint);

            return false;
        }
=======
=======
>>>>>>> dcb1584ba73e3bd6236ef5a0cbf85c073e36ce5e
=======
    /// <summary>
    /// 绘制使用的上下文信息
    /// </summary>
    class DrawStrokeContext : IDisposable
    {
        /// <summary>
        /// 绘制使用的上下文信息
        /// </summary>
        public DrawStrokeContext(InkId inkId, InkingModeInputArgs modeInputArgs, SKColor strokeColor, double inkThickness)
        {
            InkId = inkId;
            InkThickness = inkThickness;
            StrokeColor = strokeColor;
<<<<<<< HEAD
<<<<<<< HEAD
            InputInfo = inputInfo;
>>>>>>> 3fa23c5db39211ac70b181c2423a7ab1a163836e
=======
            ModeInputArgs = modeInputArgs;
>>>>>>> 26fc699b4b42ce61c693a35760e56996f085d438
=======
            InputInfo = modeInputArgs;
>>>>>>> 2ade76db594383844b39be7c43bacf08c910c678

            List<StylusPoint> historyDequeueList = [];
            TipStylusPoints = new InkingFixedQueue<StylusPoint>(MaxTipStylusCount, historyDequeueList);
            _historyDequeueList = historyDequeueList;
        }

        /// <summary>
        /// 笔迹的 Id 号，基本上每个笔迹都是不相同的。和输入的 Id 是不相同的，这是给每个 Stroke 一个的，不同的 Stroke 是不同的。除非有人能够一秒一条笔迹，写 60 多年才能重复
        /// </summary>
        public InkId InkId { get; }


        public double InkThickness { get; }

        public SKColor StrokeColor { get; }
        public InkingModeInputArgs InputInfo { set; get; }


        /// <summary>
        /// 丢点的数量
        /// </summary>
        public int DropPointCount { set; get; }

        /// <summary>
        /// 笔尖的点
        /// </summary>
        public InkingFixedQueue<StylusPoint> TipStylusPoints { get; }

        public List<StylusPoint> AllStylusPoints { get; } = new List<StylusPoint>();

        /// <summary>
        /// 存放笔迹的笔尖的点丢出来的点
        /// </summary>
        private List<StylusPoint>? _historyDequeueList;

        /// <summary>
        /// 整个笔迹的点，包括笔尖的点
        /// </summary>
        public List<StylusPoint> GetAllStylusPointsOnFinish()
        {
            if (_historyDequeueList is null)
            {
                // 为了减少 List 对象的申请，这里将复用 _historyDequeueList 的 List 对象。这就导致了一旦上层调用过此方法，将不能重复调用，否则将会炸掉逻辑
                throw new InvalidOperationException("此方法只能在完成的时候调用一次，禁止多次调用");
            }

            // 将笔尖的点合并到 _historyDequeueList 里面，这样就可以一次性返回所有的点。减少创建一个比较大的数组。缺点是这么做将不能多次调用，否则数据将会不正确
            var historyDequeueList = _historyDequeueList;
            //historyDequeueList.AddRange(TipStylusPoints);
            int count = TipStylusPoints.Count; // 为什么需要取出来？因为会越出队越小
            for (int i = 0; i < count; i++)
            {
                // 全部出队列，即可确保数据全取出来
                TipStylusPoints.Dequeue();
            }

            // 防止被多次调用
            _historyDequeueList = null;
            return historyDequeueList;
        }

        public SKPath? InkStrokePath { set; get; }

        public bool IsUp { set; get; }

        public bool IsLeave { set; get; }

        public void Dispose()
        {
            // 不释放，否则另一个线程使用可能炸掉
            // 如 cee6070566964a8143b235e10f90dda9907e6e22 的测试
            //InkStrokePath?.Dispose();
        }
    }

    /// <summary>
    /// 取多少个点做笔尖
    /// </summary>
    /// 经验值，原本只是想取 5 + 1 个点，但是发现这样笔尖太短了，于是再加一个点
    private const int MaxTipStylusCount = 7;


    #region IInputProcessor

    void IInkingInputProcessor.InputStart()
    {
        StaticDebugLogger.WriteLine("==========InputStart============");

        // 这是浅拷贝
        //_originBackground = SkBitmap?.Copy();

        UpdateOriginBackground();
    }

    void IInkingInputProcessor.Down(InkingModeInputArgs info)
    {
        var inkId = CreateInkId();
        var drawStrokeContext = new DrawStrokeContext(inkId, info, Settings.Color, Settings.InkThickness);
        CurrentInputDictionary.Add(info.Id, drawStrokeContext);

        StaticDebugLogger.WriteLine($"Down {info.Position.X:0.00},{info.Position.Y:0.00} CurrentInputDictionaryCount={CurrentInputDictionary.Count}");
        //_outputMove = false;
        _moveCount = 0;

        // 以下逻辑由框架层处理
        //if (CurrentInputDictionary.Count == 1)
        //{
        //    MainInputId = info.Id;
        //}

        if ((IsInEraserMode || IsInEraserGestureMode) && !_isErasing)
        {
            // 首次就进入橡皮擦
            DownEraser(in info);
        }
        else
        {
            // 笔模式
            if (Settings.ShouldDrawStrokeOnDown)
            {
                var result = DrawStroke(drawStrokeContext, out _);
                // 必定不会成功，但是将点进行收集
                System.Diagnostics.Debug.Assert(!result);
            }
        }
    }

    //private bool _outputMove;

    private StepCounter _stepCounter = new StepCounter();
    private int _moveCount = 0;

    void IInkingInputProcessor.Move(InkingModeInputArgs info)
    {
        if (!CurrentInputDictionary.ContainsKey(info.Id))
        {
            // 如果丢失按下，那就不能画
            // 解决鼠标在其他窗口按下，然后移动到当前窗口
            StaticDebugLogger.WriteLine($"Lost Input Id={info.Id}");
            return;
        }

        if (_moveCount == 0)
        {
            //_stepCounter.Start();
        }
        _stepCounter.Record($"StartMove{_moveCount}");

        //if (!_outputMove)
        //{
        //    StaticDebugLogger.WriteLine($"IInputProcessor.Move {info.Position.X:0.00},{info.Position.Y:0.00}");
        //}

        //_outputMove = true;

        var context = UpdateInkingStylusPoint(info);
        // 重新赋值 info 值，因此旧的这个值没有处理宽度高度是空的情况，使用上一个点的宽度高度而让橡皮擦闪烁
        info = context.InputInfo;

        if (IsInEraserMode || IsInEraserGestureMode)
        {
            if (!_isErasing)
            {
                // 如果是手指按下，然后再判断是橡皮擦的，则进入此分支
                // 或者上个橡皮擦抬起，下一次执行则也进入此分支
                StaticDebugLogger.WriteLine($"[{nameof(SkInkCanvas)}] Move DownEraser");
                DownEraser(context.InputInfo);
            }

            if (info.Id == _eraserDeviceId)
            {
                MoveEraser(info);
            }

#if DEBUG
            string modeName = "NONE";
            if (IsInEraserMode && IsInEraserGestureMode)
            {
                modeName = "[IsInEraserMode&&IsInEraserGestureMode]";
            }
            else if (IsInEraserMode)
            {
                modeName = nameof(IsInEraserMode);
            }
            else if (IsInEraserGestureMode)
            {
                modeName = nameof(IsInEraserGestureMode);
            }

            if (info.Id == _eraserDeviceId)
            {
                StaticDebugLogger.WriteLine($"[{modeName}] Id==_eraserDeviceId Id={info.Id} _eraserDeviceId={_eraserDeviceId}");
            }
            else
            {
                StaticDebugLogger.WriteLine(
                    $"[{modeName}] Id!=_eraserDeviceId Id={info.Id} _eraserDeviceId={_eraserDeviceId} _eraserDeviceIdUp?={!ModeInputDispatcher.ContainsDeviceId(_eraserDeviceId)} DeviceCount={ModeInputDispatcher.CurrentDeviceCount}");
            }
#endif
        }
        else
        {
            var stylusPoint = context.InputInfo.StylusPoint;
            if (Settings.EnableEraserGesture // 启用手势橡皮擦的前提下，通过尺寸进行判断
                && stylusPoint.Width != null
                && stylusPoint.Height != null
                && stylusPoint.Width >= Settings.MinEraserGesturePixelSize.Width
                && stylusPoint.Height >= Settings.MinEraserGesturePixelSize.Height
                // 如果输入较长，则不能进入橡皮擦模式
                && ModeInputDispatcher.InputDuring > Settings.DisableEnterEraserGestureAfterInputDuring)
            {
                EnterEraserGestureMode(context.InputInfo);
                return;
            }

            var stylusPointList = context.InputInfo.StylusPointList;
            if (stylusPointList != null)
            {
                // 这里是一个补丁的实现，因为现在底层没有处理多点计算的功能
                // 如果底层能够在 DrawStroke 处理多点，预计性能比当前好非常多
                // 至少可以减少重复清空和拼接创建路径

                // 大部分情况下都不会进入此分支，除非是卡了

                // 先执行丢点算法，避免进入太多的点
                var result = new List<StylusPoint>();
                result.Add(stylusPointList[0]);
                // 这里使用新的丢点的算法
                // 可以丢掉更多的点
                for (var i = 1; i < stylusPointList.Count; i++)
                {
                    var lastPoint = result[^1];
                    var currentPoint = stylusPointList[i];
                    var length = Math.Pow((lastPoint.Point.X - currentPoint.Point.X), 2)
                                 + Math.Pow((lastPoint.Point.Y - currentPoint.Point.Y), 2);
                    if (length < 4)
                    {
                        // 太近了
                        continue;
                    }

                    if (length < 10)
                    {
                        // 太近了
                        continue;
                    }
                    if (result.Count < 2)
                    {

                    }

                    result.Add(currentPoint);
                }

                _stepCounter.Record("完成丢点");

                StaticDebugLogger.WriteLine($"完成丢点 丢点数量：{stylusPointList.Count - result.Count}  实际参与绘制点数：{result.Count}");

                Rect currentRect = new Rect();
                bool isFirst = true;

                float pressure = stylusPoint.Pressure;
                double width = stylusPoint.Width ?? 0;
                double height = stylusPoint.Height ?? 0;

                foreach (var point in result)
                {
                    pressure = point.IsPressureEnable ? point.Pressure : pressure;
                    width = point.Width ?? width;
                    height = point.Height ?? height;

                    context.InputInfo = context.InputInfo with
                    {
                        StylusPoint = point with
                        {
                            Pressure = pressure,
                            Width = width,
                            Height = height,
                        }
                    };

                    if (DrawStroke(context, out var rect))
                    {
                        if (isFirst)
                        {
                            currentRect = rect;
                        }
                        else
                        {
                            currentRect = currentRect.Union(rect);
                        }
                    }

                    isFirst = false;
                }
                _stepCounter.Record("完成绘制");

                RenderBoundsChanged?.Invoke(this, currentRect);
            }
            else
            {
                if (DrawStroke(context, out var rect))
                {
                    RenderBoundsChanged?.Invoke(this, rect);
                }
            }
        }

        _stepCounter.Record($"EndMove{_moveCount}");

        _stepCounter.OutputToConsole();
        //_stepCounter.Restart();
    }

    void IInkingInputProcessor.Hover(InkingModeInputArgs args)
    {
        // 没有什么作用
    }

    void IInkingInputProcessor.Up(InkingModeInputArgs info)
    {
        var context = UpdateInkingStylusPoint(info);
        info = context.InputInfo;

        if (IsInEraserMode || IsInEraserGestureMode)
        {
            if (info.Id == _eraserDeviceId)
            {
                StaticDebugLogger.WriteLine($"[{nameof(SkInkCanvas)}] UpEraser _eraserDeviceId={_eraserDeviceId}");
                UpEraser(in info);
            }
        }
        else
        {
            if (DrawStroke(context, out var rect))
            {
                RenderBoundsChanged?.Invoke(this, rect);
            }
        }

        // 获取所有的点，包括笔尖的点
        var allStylusPoints = context.GetAllStylusPointsOnFinish();

        context.DropPointCount = 0;
        // 清空笔尖的点，属于多余逻辑，因为在 GetAllStylusPointsOnFinish 里已经包含清空笔尖逻辑。反正多做一次也没损失，还能让逻辑更加清晰
        context.TipStylusPoints.Clear();
        context.IsUp = true;

        var strokesCollectionInfo = new SkiaStrokeSynchronizer((uint) info.Id, context.InkId, context.StrokeColor, context.InkThickness, context.InkStrokePath, allStylusPoints);
        StaticInkInfoList.Add(strokesCollectionInfo);
        StrokesCollected?.Invoke(this, strokesCollectionInfo);

        //if (CurrentInputDictionary.All(t => t.Value.IsUp))
        //{
        //    //完成等待清理
        //}
    }

    void IInkingInputProcessor.InputComplete()
    {
        Clean();

        StaticDebugLogger.WriteLine($"InputComplete\r\n==========\r\n");

        InputCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void Clean()
    {
        // 不加这句话，将会在 foreach 里炸掉，不知道是不是 CLR 的坑
        using var enumerator = CurrentInputDictionary.GetEnumerator();

        foreach (var drawStrokeContext in CurrentInputDictionary)
        {
            if (!drawStrokeContext.Value.IsUp)
            {
                // 如果还没有 Up 的状态，那就是 Leave 状态了
                drawStrokeContext.Value.IsLeave = true;
            }

            drawStrokeContext.Value.Dispose();
        }

        CurrentInputDictionary.Clear();

        if (IsInEraserMode || IsInEraserGestureMode)
        {
            // 当前是橡皮擦模式，需要清理橡皮擦，修复 1382 对窗口在顶的工具条进行手势橡皮，橡皮擦不消失
            CleanEraser();
        }

        _isOriginBackgroundDisable = true;
        IsInEraserGestureMode = false;

        _isErasing = false;
    }

    public event EventHandler? InputCompleted;

    /// <summary>
    /// 这是 WPF 的概念，那就继续用这个概念
    /// </summary>
    void IInkingInputProcessor.Leave()
    {
        StaticDebugLogger.WriteLine($"{DateTime.Now:hh-MM-ss} IInputProcessor.Leave");

        Clean();

        if (_isOriginBackgroundDisable)
        {
            // 几乎必定进入此分支，除非 Clean 方法后续改错了
            //Console.WriteLine("_isOriginBackgroundDisable=true");
            return;
        }

        if (_originBackground is null)
        {
            StaticDebugLogger.WriteLine(
                $"Leave-------- 进入非预期分支，除非是初始化 _originBackground is null={_originBackground is null}");
            return;
        }

        StaticDebugLogger.WriteLine("Leave--------");

        var skCanvas = _skCanvas;
        skCanvas.Clear(SKColors.Transparent);
        skCanvas.DrawBitmap(_originBackground, 0, 0);
        // 完全重绘，丢掉画出来的笔迹
        RenderBoundsChanged?.Invoke(this, new Rect(0, 0, _originBackground.Width, _originBackground.Height));
    }

    #endregion

    private DrawStrokeContext UpdateInkingStylusPoint(InkingModeInputArgs info)
    {
        if (CurrentInputDictionary.TryGetValue(info.Id, out var context))
        {
            var lastInfo = context.InputInfo;
            var stylusPoint = info.StylusPoint;
            var lastStylusPoint = lastInfo.StylusPoint;
            stylusPoint = stylusPoint with
            {
                Pressure = stylusPoint.IsPressureEnable ? stylusPoint.Pressure : lastStylusPoint.Pressure,
                Width = stylusPoint.Width ?? lastStylusPoint.Width,
                Height = stylusPoint.Height ?? lastStylusPoint.Height,
            };

            info = info with
            {
                StylusPoint = stylusPoint
            };

            context.InputInfo = info;
            return context;
        }
        else
        {
            // 理论上不会进入此分支
            StaticDebugLogger.WriteLine($"UpdateInkingStylusPoint 找不到笔迹点");
            var inkId = CreateInkId();
            context = new DrawStrokeContext(inkId, info, Settings.Color, Settings.InkThickness);
            CurrentInputDictionary.Add(info.Id, context);
            return context;
        }
    }

    private int _currentInkId;

    private InkId CreateInkId()
    {
        var currentInkId = _currentInkId;
        _currentInkId++;
        return new InkId(currentInkId); // return _currentInkId++ 的意思，只是这个可读性太垃圾了
    }

    private bool DrawStroke(DrawStrokeContext context, [NotNullWhen(true)] out Rect? rect)
    {
        var skCanvas = _skCanvas;
        var skPaint = new SKPaint
        {
            Color = context.StrokeColor,
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float) context.InkThickness,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
        };

        var inkStrokePath = context.InkStrokePath;
        if (inkStrokePath is null)
        {
            rect = null;
            return false;
        }

        skCanvas.DrawPath(inkStrokePath, skPaint);

        var bounds = inkStrokePath.Bounds;
        rect = new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        return true;
    }


    public static unsafe bool ReplacePixels(uint* destinationBitmap, uint* sourceBitmap, SKRectI destinationRectI,
        SKRectI sourceRectI, uint destinationPixelWidthLengthOfUint, uint sourcePixelWidthLengthOfUint)
    {
        if (destinationRectI.Width != sourceRectI.Width || destinationRectI.Height != sourceRectI.Height)
        {
            return false;
        }

        //for(var sourceRow = sourceRectI.Top; sourceRow< sourceRectI.Bottom; sourceRow++)
        //{
        //    for (var sourceColumn = sourceRectI.Left; sourceColumn < sourceRectI.Right; sourceColumn++)
        //    {
        //        var sourceIndex = sourceRow * sourceRectI.Width + sourceColumn;

        //        var destinationRow = destinationRectI.Top + sourceRow - sourceRectI.Top;
        //        var destinationColumn = destinationRectI.Left + sourceColumn - sourceRectI.Left;
        //        var destinationIndex = destinationRow * destinationRectI.Width + destinationColumn;

        //        destinationBitmap[destinationIndex] = sourceBitmap[sourceIndex];
        //    }
        //}

        for (var sourceRow = sourceRectI.Top; sourceRow < sourceRectI.Bottom; sourceRow++)
        {
            var sourceStartColumn = sourceRectI.Left;
            var sourceStartIndex = sourceRow * destinationPixelWidthLengthOfUint + sourceStartColumn;

            var destinationRow = destinationRectI.Top + sourceRow - sourceRectI.Top;
            var destinationStartColumn = destinationRectI.Left;
            var destinationStartIndex = destinationRow * sourcePixelWidthLengthOfUint + destinationStartColumn;

            Unsafe.CopyBlockUnaligned((destinationBitmap + destinationStartIndex), (sourceBitmap + sourceStartIndex), (uint) (destinationRectI.Width * sizeof(uint)));

            //for (var sourceColumn = sourceRectI.Left; sourceColumn < sourceRectI.Right; sourceColumn++)
            //{
            //    var sourceIndex = sourceRow * destinationPixelWidthLengthOfUint + sourceColumn;

            //    var destinationColumn = destinationRectI.Left + sourceColumn - sourceRectI.Left;
            //    var destinationIndex = destinationRow * sourcePixelWidthLengthOfUint + destinationColumn;

            //    destinationBitmap[destinationIndex] = sourceBitmap[sourceIndex];
            //}
        }

        return true;
    }

    [MemberNotNull(nameof(_originBackground))]
    private unsafe void UpdateOriginBackground()
    {
        // 需要使用 SKCanvas 才能实现拷贝
        _originBackground ??= new SKBitmap(new SKImageInfo(ApplicationDrawingSkBitmap.Width,
            ApplicationDrawingSkBitmap.Height, ApplicationDrawingSkBitmap.ColorType,
            ApplicationDrawingSkBitmap.AlphaType,
            ApplicationDrawingSkBitmap.ColorSpace), SKBitmapAllocFlags.None);
        _isOriginBackgroundDisable = false;

        //using var skCanvas = new SKCanvas(_originBackground);
        //skCanvas.Clear();
        //skCanvas.DrawBitmap(ApplicationDrawingSkBitmap, 0, 0);
        var applicationPixelHandler = ApplicationDrawingSkBitmap.GetPixels(out var length);
        var originBackgroundPixelHandler = _originBackground.GetPixels();
        Unsafe.CopyBlock((void*) originBackgroundPixelHandler, (void*) applicationPixelHandler, (uint) length);
    }
}

<<<<<<< HEAD
partial class SkInkCanvas
{
    // 漫游相关

    /// <summary>
    /// 漫游完成，需要将内容重新使用路径绘制，保持清晰
    /// </summary>
    public void ManipulateFinish()
    {
        var skCanvas = _skCanvas;
        skCanvas.Clear(Settings.ClearColor);

        skCanvas.Save();
        skCanvas.SetMatrix(_totalMatrix);

        DrawAllInk();

        skCanvas.Restore();
        _isOriginBackgroundDisable = true;
    }

    public void ManipulateScale(ScaleContext scale)
    {
        StaticDebugLogger.WriteLine($"[ManipulateScale] {scale}");

        var scaleMatrix = SKMatrix.CreateScale(scale.X, scale.Y, scale.PivotX, scale.PivotY);
        _totalMatrix = SKMatrix.Concat(_totalMatrix, scaleMatrix);

        var skCanvas = _skCanvas;
        skCanvas.Clear(Settings.ClearColor);

        skCanvas.Save();
        skCanvas.SetMatrix(_totalMatrix);

        DrawAllInk();

        skCanvas.Restore();

        _isOriginBackgroundDisable = true;
    }

    readonly record struct ManipulationInfo(Point StartAbsPoint, SKMatrix StartMatrix, Point LastAbsPoint);

    private ManipulationInfo _manipulationInfo = default;

    public void ManipulateMoveStart(Point startPoint)
    {
        _manipulationInfo = new ManipulationInfo(StartAbsPoint: startPoint, StartMatrix: _totalMatrix, LastAbsPoint: startPoint);
    }

    public void ManipulateMove(Point absPoint)
    {
        //StaticDebugLogger.WriteLine($"[ManipulateMove] {delta.X:0.00},{delta.Y:0.00}");

        var x = absPoint.X - _manipulationInfo.LastAbsPoint.X;
        var y = absPoint.Y - _manipulationInfo.LastAbsPoint.Y;

        x = Math.Floor(x);
        y = Math.Floor(y);

        if (Math.Abs(x) < 0.01 && Math.Abs(y) < 0.01)
        {
            return;
        }

        var lastAbsPoint = new Point(_manipulationInfo.LastAbsPoint.X + x, _manipulationInfo.LastAbsPoint.Y + y);
        _manipulationInfo = _manipulationInfo with
        {
            LastAbsPoint = lastAbsPoint
        };

        // 需要解决缩放之后的平移，如果直接使用 Concat 方法，那将会在原有的基础上，叠加上缩放后的平移，导致平移的距离不准确
        //_totalMatrix = _totalMatrix * SKMatrix.CreateTranslation((float) delta.X, (float) delta.Y);
        var translation = SKMatrix.CreateTranslation((float) x / _totalMatrix.ScaleX, (float) y / _totalMatrix.ScaleY);
        _totalMatrix = SKMatrix.Concat(_totalMatrix, translation);


        // 像素漫游的方法
        MoveWithPixel(new Point(x, y));

        // 这是用来测试几何漫游的方法
        //// 几何漫游的方法
        //ManipulateFinish();

        _isOriginBackgroundDisable = true;
    }

    private SKMatrix _totalMatrix = SKMatrix.CreateIdentity();

    private unsafe void MoveWithPixel(Point delta)
    {
        var pixels = ApplicationDrawingSkBitmap.GetPixels(out var length);

        UpdateOriginBackground();

        //var pixelLengthOfUint = length / 4;
        //if (_cachePixel is null || _cachePixel.Length != pixelLengthOfUint)
        //{
        //    _cachePixel = new uint[pixelLengthOfUint];
        //}

        //fixed (uint* pCachePixel = _cachePixel)
        //{
        //    //var byteCount = (uint) length * sizeof(uint);
        //    ////Buffer.MemoryCopy((uint*) pixels, pCachePixel, byteCount, byteCount);
        //    //////Buffer.MemoryCopy((uint*) pixels, pCachePixel, 0, byteCount);
        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    var pixel = ((uint*) pixels)[i];
        //    //    pCachePixel[i] = pixel;
        //    //}

        //    var byteCount = (uint) length;
        //    Unsafe.CopyBlock(pCachePixel, (uint*) pixels, byteCount);
        //}

        int destinationX, destinationY, destinationWidth, destinationHeight;
        int sourceX, sourceY, sourceWidth, sourceHeight;

        if (delta.X > 0)
        {
            // 不能直接做加法，这是不对的
            //delta.X += 20;

            destinationX = (int) delta.X;
            destinationWidth = ApplicationDrawingSkBitmap.Width - destinationX;
            sourceX = 0;
        }
        else
        {
            destinationX = 0;
            destinationWidth = ApplicationDrawingSkBitmap.Width - ((int) -delta.X);

            sourceX = (int) -delta.X;
        }

        if (delta.Y > 0)
        {
            destinationY = (int) delta.Y;
            destinationHeight = ApplicationDrawingSkBitmap.Height - destinationY;
            sourceY = 0;
        }
        else
        {
            destinationY = 0;
            destinationHeight = ApplicationDrawingSkBitmap.Height - (int) -delta.Y;

            sourceY = (int) -delta.Y;
        }

        sourceWidth = destinationWidth;
        sourceHeight = destinationHeight;

        SKRectI destinationRectI = SKRectI.Create(destinationX, destinationY, destinationWidth, destinationHeight);
        SKRectI sourceRectI = SKRectI.Create(sourceX, sourceY, sourceWidth, sourceHeight);

        // 计算脏范围，用于在此绘制笔迹
        var topRect = SKRect.Create(0, 0, ApplicationDrawingSkBitmap.Width, destinationY);
        var bottomRect = SKRect.Create(0, destinationY + destinationHeight, ApplicationDrawingSkBitmap.Width, ApplicationDrawingSkBitmap.Height - destinationY - destinationHeight);
        var leftRect = SKRect.Create(0, destinationY, destinationX, destinationHeight);
        var rightRect = SKRect.Create(destinationX + destinationWidth, destinationY, ApplicationDrawingSkBitmap.Width - destinationX - destinationWidth, destinationHeight);

        var hitRectList = new List<SKRect>(4);
        var matrix = _totalMatrix.Invert();
        Span<SKRect> hitRectSpan = [topRect, bottomRect, leftRect, rightRect];
        foreach (var skRect in hitRectSpan)
        {
            if (!IsEmptySize(skRect))
            {
                hitRectList.Add(matrix.MapRect(skRect));
            }
        }

        var hitInk = new List<SkiaStrokeSynchronizer>();
        foreach (var skiaStrokeSynchronizer in StaticInkInfoList)
        {
            foreach (var skRect in hitRectList)
            {
                if (IsHit(skiaStrokeSynchronizer, skRect))
                {
                    hitInk.Add(skiaStrokeSynchronizer);
                    break;
                }
            }
        }

        //var skCanvas = _skCanvas;
        //skCanvas.Clear();
        //foreach (var skRectI in (Span<SKRectI>) [topRectI, bottomRectI, leftRectI, rightRectI])
        //{
        //    using var skPaint = new SKPaint();
        //    skPaint.StrokeWidth = 0;
        //    skPaint.IsAntialias = true;
        //    skPaint.FilterQuality = SKFilterQuality.High;
        //    skPaint.Style = SKPaintStyle.Fill;
        //    skPaint.Color = SKColors.Blue;
        //    var skRect = SKRect.Create(skRectI.Left, skRectI.Top, skRectI.Width, skRectI.Height);

        //    skCanvas.DrawRect(skRect, skPaint);
        //}
        //skCanvas.Flush();

        var skCanvas = _skCanvas;
        skCanvas.Clear(Settings.ClearColor);
        skCanvas.Save();
        skCanvas.SetMatrix(_totalMatrix);
        using var skPaint = new SKPaint();
        skPaint.StrokeWidth = 0;
        skPaint.IsAntialias = true;
        skPaint.FilterQuality = SKFilterQuality.High;
        skPaint.Style = SKPaintStyle.Fill;

        foreach (var skiaStrokeSynchronizer in hitInk)
        {
            DrawInk(skCanvas, skPaint, skiaStrokeSynchronizer);
        }

        skCanvas.Restore();
        skCanvas.Flush();

        var cachePixel = _originBackground.GetPixels();
        uint* pCachePixel = (uint*) cachePixel;
        var pixelLength = (uint) (ApplicationDrawingSkBitmap.Width);

        ReplacePixels((uint*) pixels, pCachePixel, destinationRectI, sourceRectI, pixelLength, pixelLength);

        RenderBoundsChanged?.Invoke(this,
            new Rect(0, 0, ApplicationDrawingSkBitmap.Width, ApplicationDrawingSkBitmap.Height));

        static bool IsEmptySize(SKRect skRect) => skRect.Width == 0 || skRect.Height == 0;

        static bool IsHit(SkiaStrokeSynchronizer inkInfo, SKRect skRect)
        {
            if (inkInfo.InkStrokePath is { } path)
            {
                var bounds = path.Bounds;
                if (skRect.IntersectsWith(bounds))
                {
                    return true;
                }
            }

            return false;
        }
    }
<<<<<<< HEAD

    private void MoveWithPath(Point delta)
    {
        _totalTransform = new Point(_totalTransform.X + delta.X, _totalTransform.Y + delta.Y);
>>>>>>> 9994af5e3facc399bf93df657e69c36f21288956

<<<<<<< HEAD
        if (_skCanvas is null)
        {
            // 理论上不可能进入这里
<<<<<<< HEAD
            return false;
        }

        // 拷贝笔尖到缓存范围，方便计算。后续可以考虑不要缓存，减少拷贝，提升几乎可以忽略的性能
        context.TipStylusPoints.CopyTo(_cache, 0);

        if (CanDropLastPoint(_cache.AsSpan(0, context.TipStylusPoints.Count), currentStylusPoint) &&
            context.DropPointCount < 3)
        {
            // 丢点是为了让 SimpleInkRender 可以绘制更加平滑的折线。但是不能丢太多的点，否则将导致看起来断线
            context.DropPointCount++;
            return false;
        }

        context.DropPointCount = 0;

        var lastPoint = _cache[context.TipStylusPoints.Count - 1];
        if (currentStylusPoint == lastPoint)
        {
            return false;
        }

        _cache[context.TipStylusPoints.Count] = currentStylusPoint;
        context.TipStylusPoints.Enqueue(currentStylusPoint);

        // 是否开启自动模拟软笔效果
        if (Settings.AutoSoftPen)
        {
            for (int i = 0; i < 10; i++)
            {
                if (context.TipStylusPoints.Count - i - 1 < 0)
                {
                    break;
                }

                // 简单的算法…就是越靠近笔尖的点的压感越小
                _cache[context.TipStylusPoints.Count - i - 1] = _cache[context.TipStylusPoints.Count - i - 1] with
                {
                    Pressure = Math.Max(Math.Min(0.1f * i, 0.5f), 0.01f)
                    //Pressure = 0.3f,
                };
            }
        }

        var pointList = _cache.AsSpan(0, context.TipStylusPoints.Count);

        var outlinePointList = SimpleInkRender.GetOutlinePointList(pointList, 20);

        using var skPath = new SKPath();
        skPath.AddPoly(outlinePointList.Select(t => new SKPoint((float) t.X, (float) t.Y)).ToArray());
        //skPath.Close();

        if (Settings.DynamicRenderType == InkCanvasDynamicRenderTipStrokeType.RenderAllTouchingStrokeWithoutTipStroke)
        {
            // 将计算出来的笔尖部分叠加回去原先的笔身，这个方式对画长线性能不好
            context.InkStrokePath ??= new SKPath();
            context.InkStrokePath.AddPath(skPath);

            var skPathBounds = skPath.Bounds;

            // 计算脏范围，用于渲染更新
            var additionSize = 10; // 用于设置比简单计算的范围更大一点的范围，解决重采样之后的模糊
            drawRect = new Rect(skPathBounds.Left - additionSize, skPathBounds.Top - additionSize,
                skPathBounds.Width + additionSize * 2, skPathBounds.Height + additionSize * 2);

            var skCanvas = _skCanvas;
            // 以下代码用于解决绘制的笔迹边缘锐利的问题。原因是笔迹执行了重采样，但是边缘如果没有被覆盖，则重采样的将会重复叠加，导致锐利
            // 根据 Skia 的官方文档，建议是走清空重新绘制。在不清屏的情况下，除非能够获取到原始的像素点。尽管这是能够计算的，但是先走清空开发速度比较快
            skCanvas.Clear(SKColors.Transparent);
            skCanvas.DrawBitmap(_originBackground, 0, 0);

            // 将所有的笔迹绘制出来，作为动态笔迹层。后续抬手的笔迹需要重新写入到静态笔迹层
            using var skPaint = new SKPaint();
            skPaint.StrokeWidth = 0.1f;
            skPaint.IsAntialias = true;
            skPaint.FilterQuality = SKFilterQuality.High;
            skPaint.Style = SKPaintStyle.Fill;
            var enumerator = CurrentInputDictionary.GetEnumerator();

            foreach (var drawStrokeContext in CurrentInputDictionary)
            {
                skPaint.Color = drawStrokeContext.Value.StrokeColor;

                if (drawStrokeContext.Value.InkStrokePath is { } path)
                {
                    skCanvas.DrawPath(path, skPaint);
                }
            }

            return true;
        }

        return false;
    }

    public SKColor Color { get; set; } = SKColors.Red;

    // 以下是橡皮擦系列逻辑
    // 橡皮擦根据给定尺寸缩放

    /// <summary>
    /// 进入橡皮擦模式
    /// </summary>
    public void EnterEraserMode()
    {
        IsInEraserMode = true;
    }

    public void EnterPenMode()
    {
        IsInEraserMode = false;
    }

    private bool IsInEraserMode { set; get; }

    /// <summary>
    /// 移动橡皮擦的计时器，用于丢点
    /// </summary>
    private Stopwatch MoveEraserStopwatch { get; } = new Stopwatch();

    private void MoveEraser(InkingInputInfo info)
    {
        if (_skCanvas is not { } canvas || _originBackground is null)
        {
            return;
        }

        if (Settings.EnableClippingEraser)
        {
            if (!MoveEraserStopwatch.IsRunning)
            {
                MoveEraserStopwatch.Restart();
            }
            else if (MoveEraserStopwatch.Elapsed < TimeSpan.FromMilliseconds(20))
            {
                // 如果时间距离过近，则忽略
                // 由于触摸屏大量触摸点输入，而 DrawBitmap 需要 20 毫秒，导致性能过于差
                if (Settings.ShouldCollectDropErasePoint)
                {
                    _eraserDropPointList ??= new List<StylusPoint>();
                    _eraserDropPointList.Add(info.StylusPoint);
                }

                return;
            }
            else
            {
                MoveEraserStopwatch.Restart();
            }

            if (EraserPath is null)
            {
                EraserPath = new SKPath();
                EraserPath.AddRect(new SKRect(0, 0, _originBackground.Width, _originBackground.Height));
            }

            double width = 30;
            double height = 45;

            using var skRoundRect = new SKPath();

            if (Settings.ShouldCollectDropErasePoint && _eraserDropPointList != null)
            {
                // 如果有收集丢点的点，则加入计算
                foreach (var stylusPoint in _eraserDropPointList)
                {
                    var dropPoint = stylusPoint.Point;
                    var xDropPoint = (float) dropPoint.X;
                    var yDropPoint = (float) dropPoint.Y;

                    xDropPoint -= (float) width / 2;
                    yDropPoint -= (float) height / 2;

                    var skRectDropPoint = new SKRect(xDropPoint, yDropPoint, (float) (xDropPoint + width), (float) (yDropPoint + height));

                    skRoundRect.Reset();
                    skRoundRect.AddRoundRect(skRectDropPoint, 5, 5);

                    EraserPath.Op(skRoundRect, SKPathOp.Difference, EraserPath);
                }

                _eraserDropPointList.Clear();
            }

            var point = info.StylusPoint.Point;
            var x = (float) point.X;
            var y = (float) point.Y;

            x -= (float) width / 2;
            y -= (float) height / 2;

            var skRect = new SKRect(x, y, (float) (x + width), (float) (y + height));

            skRoundRect.AddRoundRect(skRect, 5, 5);
            EraserPath.Op(skRoundRect, SKPathOp.Difference, EraserPath);

            canvas.Clear();
            canvas.Save();
            canvas.ClipPath(EraserPath, antialias: true);
            canvas.DrawBitmap(_originBackground, 0, 0);
            canvas.Restore();

            // 画出橡皮擦
            canvas.Save();
            canvas.Translate(x, y);
            EraserView.DrawEraserView(canvas, 30, 45);
            canvas.Restore();

            // 更新范围
            var addition = 20;
            var currentEraserRenderBounds = new Rect(skRect.Left - addition, skRect.Top - addition, skRect.Width + addition * 2,
                skRect.Height + addition * 2);

            var rect = currentEraserRenderBounds;

            if (_lastEraserRenderBounds != null)
            {
                // 如果将上次的绘制范围进行重新绘制，防止出现橡皮擦图标
                rect = rect.Union(_lastEraserRenderBounds.Value);
            }

            _lastEraserRenderBounds = currentEraserRenderBounds;
            RenderBoundsChanged?.Invoke(this, rect);

            MoveEraserStopwatch.Stop();
            Console.WriteLine($"EraserPath time={MoveEraserStopwatch.ElapsedMilliseconds}ms RenderBounds={rect.X} {rect.Y} {rect.Width} {rect.Height} EraserPathPointCount={EraserPath.PointCount}");
            MoveEraserStopwatch.Restart();
        }
    }

    private SKPath? EraserPath { set; get; }

    private EraserView EraserView { get; } = new EraserView();

    /// <summary>
    /// 在橡皮擦丢点进行收集，进行一次性处理
    /// </summary>
    private List<StylusPoint>? _eraserDropPointList;

    /// <summary>
    /// 上一次的橡皮擦渲染范围
    /// </summary>
    private Rect? _lastEraserRenderBounds;

    private void UpEraser(InkingInputInfo info)
    {
        if (_skCanvas is not { } canvas || _originBackground is null || EraserPath is null)
        {
            return;
        }

        canvas.Clear();

        canvas.Save();
        canvas.ClipPath(EraserPath, antialias: true);
        canvas.DrawBitmap(_originBackground, 0, 0);
        canvas.Restore();

        EraserPath?.Dispose();
        EraserPath = null;

        _lastEraserRenderBounds = null;

        // 完全重绘，修复可能存在的丢失裁剪
        RenderBoundsChanged?.Invoke(this, new Rect(0, 0, _originBackground.Width, _originBackground.Height));
    }
}

class EraserView
{
    public SKBitmap GetEraserView(int width, int height)
    {
        var skBitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));

        using var skCanvas = new SKCanvas(skBitmap);
        DrawEraserView(skCanvas, width, height);

        return skBitmap;
    }

    public void DrawEraserView(SKCanvas skCanvas, int width, int height)
    {
        var pathWidth = 30;
        var pathHeight = 45;

        using var path1 = SKPath.ParseSvgPathData(
            "M0,5.0093855C0,2.24277828,2.2303666,0,5.00443555,0L24.9955644,0C27.7594379,0,30,2.23861485,30,4.99982044L30,17.9121669C30,20.6734914,30,25.1514578,30,27.9102984L30,40.0016889C30,42.7621799,27.7696334,45,24.9955644,45L5.00443555,45C2.24056212,45,0,42.768443,0,39.9906145L0,5.0093855z");
        using var skPaint = new SKPaint();
        skPaint.IsAntialias = true;
        skPaint.Style = SKPaintStyle.Fill;
        skPaint.Color = new SKColor(0, 0, 0, 0x33);
        skCanvas.DrawPath(path1, skPaint);

        skPaint.Color = new SKColor(0xF2, 0xEE, 0xEB, 0xFF);
        skCanvas.DrawRoundRect(1, 1, 28, 43, 4, 4, skPaint);

        using var path2 = SKPath.ParseSvgPathData(
            "M20,29.1666667L20,16.1666667C20,15.3382395 19.3284271,14.6666667 18.5,14.6666667 17.6715729,14.6666667 17,15.3382395 17,16.1666667L17,29.1666667C17,29.9950938 17.6715729,30.6666667 18.5,30.6666667 19.3284271,30.6666667 20,29.9950938 20,29.1666667z M13,29.1666667L13,16.1666667C13,15.3382395 12.3284271,14.6666667 11.5,14.6666667 10.6715729,14.6666667 10,15.3382395 10,16.1666667L10,29.1666667C10,29.9950938 10.6715729,30.6666667 11.5,30.6666667 12.3284271,30.6666667 13,29.9950938 13,29.1666667z");
        skPaint.Color = new SKColor(0x00, 0x00, 0x00, 0x26);
        skCanvas.DrawPath(path2, skPaint);
    }
=======
>>>>>>> fc149583aa0a4eb1ed4aa8ca82c20621a7b49d41
=======
            return;
        }

=======
>>>>>>> 1fa364004d83a43c1852c65c679500b8585260f6
        var skCanvas = _skCanvas;
        skCanvas.Clear();

        skCanvas.Save();

        skCanvas.Translate((float) _totalTransform.X, (float) _totalTransform.Y);

        DrawAllInk();

        skCanvas.Restore();

        RenderBoundsChanged?.Invoke(this,
            new Rect(0, 0, ApplicationDrawingSkBitmap.Width, ApplicationDrawingSkBitmap.Height));
    }

    private Point _totalTransform;
<<<<<<< HEAD
}

<<<<<<< HEAD
<<<<<<< HEAD
/// <summary>
/// 绘制使用的上下文信息
/// </summary>
/// <param name="inputInfo"></param>
class DrawStrokeContext(InkingInputInfo inputInfo, SKColor strokeColor) : IDisposable
{
    public SKColor StrokeColor { get; } = strokeColor;
    public InkingInputInfo InputInfo { set; get; } = inputInfo;
    public int DropPointCount { set; get; }

    /// <summary>
    /// 笔尖的点
    /// </summary>
    public readonly FixedQueue<StylusPoint> TipStylusPoints = new FixedQueue<StylusPoint>(SkInkCanvas.MaxTipStylusCount);

    /// <summary>
    /// 整个笔迹的点，包括笔尖的点
    /// </summary>
    public List<StylusPoint> AllStylusPoints { get; } = new List<StylusPoint>();

    public SKPath? InkStrokePath { set; get; }

    public bool IsUp { set; get; }

    public void Dispose()
    {
        //InkStrokePath?.Dispose();
    }
}

<<<<<<< HEAD
    #endregion
>>>>>>> 9994af5e3facc399bf93df657e69c36f21288956
}
=======
record InkInfo(int Id, DrawStrokeContext Context);
>>>>>>> dcb1584ba73e3bd6236ef5a0cbf85c073e36ce5e
=======
>>>>>>> 3fa23c5db39211ac70b181c2423a7ab1a163836e
=======
readonly partial record struct InkId(int Value);
<<<<<<< HEAD
>>>>>>> f329874ef34966a91e0ca1f358ededd562aa428b
=======

/// <summary>
/// 笔迹信息 用于静态笔迹层
/// </summary>
record SkiaStrokeSynchronizer(uint StylusDeviceId,
    InkId InkId,
    SKColor StrokeColor,
    double StrokeInkThickness,
    SKPath? InkStrokePath,
    List<StylusPoint> StylusPoints)
    ;
<<<<<<< HEAD
>>>>>>> 72ed49de4be8929bf6ab6fd3dfd6535e2ecdf686
=======

static class StaticDebugLogger
{
    //[Conditional("DEBUG")]
    public static void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
}

/// <summary>
/// 线性步骤记录器
/// </summary>
class StepCounter
{
    /// <summary>
    /// 开始
    /// </summary>
    /// 开始和记录分离，开始不一定是某个步骤。这样业务方修改开始对应的步骤时，可以能够更好的被约束，明确一个开始的时机
    public void Start()
    {
        Stopwatch.Restart();
        IsStart = true;
    }

    public void Restart()
    {
        IsStart = true;
        StepDictionary.Clear();
        Stopwatch.Restart();
    }

    public Stopwatch Stopwatch => _stopwatch ??= new Stopwatch();
    private Stopwatch? _stopwatch;

    /// <summary>
    /// 记录某个步骤。默认就是一个步骤将会延续到下个步骤，两个步骤之间的耗时就是步骤耗时
    /// 实在不行，那你就加上 “Xx开始” 和 “Xx结束”好了
    /// </summary>
    /// <param name="step"></param>
    public void Record(string step)
    {
        if (!IsStart)
        {
            return;
        }

        Stopwatch.Stop();
        StepDictionary[step] = Stopwatch.ElapsedTicks;
        Stopwatch.Restart();
    }

    public void OutputToConsole()
    {
        if (!IsStart)
        {
            return;
        }
        Console.WriteLine(BuildStepResult());
    }

    /// <summary>
    /// 进行耗时对比，用于对比两个模块或者两个版本的各个步骤的耗时差
    /// </summary>
    /// <param name="other"></param>
    public void CompareToConsole(StepCounter other)
    {
        if (!IsStart)
        {
            return;
        }
        Console.WriteLine(Compare(other));
    }

    public string Compare(StepCounter other)
    {
        if (!IsStart)
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder();
        foreach (var (step, tick) in StepDictionary)
        {
            if (other.StepDictionary.TryGetValue(step, out var otherTick))
            {
                var sign = tick > otherTick ? "+" : "";
                stringBuilder.AppendLine($"{step} {TickToMillisecond(tick):0.000}ms {TickToMillisecond(otherTick):0.000}ms {sign}{TickToMillisecond(tick - otherTick):0.000}ms");
            }
            else
            {
                stringBuilder.AppendLine($"{step} {tick * 1000d / Stopwatch.Frequency}ms");
            }
        }
        return stringBuilder.ToString();
    }

    public string BuildStepResult()
    {
        if (!IsStart)
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder();
        foreach (var (step, tick) in StepDictionary)
        {
            stringBuilder.AppendLine($"{step} {TickToMillisecond(tick)}ms");
        }
        return stringBuilder.ToString();
    }

    public Dictionary<string /*Step*/, long /*耗时*/> StepDictionary => _stepDictionary ??= new Dictionary<string, long>();
    private Dictionary<string, long>? _stepDictionary;

    /// <summary>
    /// 是否开始，如果没有开始则啥都不做，用于性能优化，方便一次性注释决定是否测试性能
    /// </summary>
    public bool IsStart { get; private set; }

    private const double SecondToMillisecond = 1000d;
    private static double TickToMillisecond(long tick) => tick * SecondToMillisecond / Stopwatch.Frequency;
}
>>>>>>> 478bd8d9d7d6a5df4c6d0d86ed2b849c955082e0
=======
}
>>>>>>> c54fc6f0540f239c00c47c3883fcaa800ae7f1bf
=======
}
>>>>>>> 7e713e9e6340c66abbc9377edcf43f7b77c137e3
=======
>>>>>>> ebae0743ab6691dfe8aa3e8e9cf4b98a657b52ad
