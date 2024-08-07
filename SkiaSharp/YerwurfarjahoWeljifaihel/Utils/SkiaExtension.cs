﻿using System.Runtime.CompilerServices;
using SkiaSharp;

namespace SkiaInkCore.Utils;

static class SkiaExtension
{
    /// <summary>
    /// 从 <paramref name="sourceBitmap"/> 拷贝所有像素覆盖原本的像素
    /// </summary>
    /// <param name="destinationBitmap"></param>
    /// <param name="sourceBitmap"></param>
    /// <returns></returns>
    public static unsafe bool ReplacePixels(this SKBitmap destinationBitmap, SKBitmap sourceBitmap)
    {
        var destinationPixelPtr = (byte*) destinationBitmap.GetPixels(out var length).ToPointer();
        var sourcePixelPtr = (byte*) sourceBitmap.GetPixels().ToPointer();

        Unsafe.CopyBlockUnaligned(destinationPixelPtr, sourcePixelPtr, (uint) length);
        return true;
    }

    /// <summary>
    /// 从 <paramref name="sourceBitmap"/> 拷贝指定范围 <paramref name="rect"/> 像素过来覆盖指定范围 <paramref name="rect"/> 的像素
    /// </summary>
    /// <param name="destinationBitmap"></param>
    /// <param name="sourceBitmap"></param>
    /// <param name="rect"></param>
    public static unsafe bool ReplacePixels(this SKBitmap destinationBitmap, SKBitmap sourceBitmap, SKRectI rect)
    {
        uint* basePtr = (uint*) destinationBitmap.GetPixels().ToPointer();
        uint* sourcePtr = (uint*) sourceBitmap.GetPixels().ToPointer();
        //Console.WriteLine($"ReplacePixels Rect={rect.Left},{rect.Top},{rect.Right},{rect.Bottom} wh={rect.Width},{rect.Height} BitmapWH={destinationBitmap.Width},{destinationBitmap.Height} D={destinationBitmap.RowBytes == (destinationBitmap.Width * sizeof(uint))}");

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            if (row >= destinationBitmap.Height)
            {
                return false;
            }

            var col = rect.Left;
            uint* destinationPixelPtr = basePtr + destinationBitmap.Width * row + col;
            uint* sourcePixelPtr = sourcePtr + sourceBitmap.Width * row + col;

            var length = rect.Width;

            if (col + length > destinationBitmap.Width)
            {
                return false;
            }

            var byteCount = (uint) length * sizeof(uint);
            Unsafe.CopyBlockUnaligned(destinationPixelPtr, sourcePixelPtr, byteCount);
        }

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

            //Unsafe.CopyBlockUnaligned((destinationBitmap + destinationStartIndex), (sourceBitmap + sourceStartIndex), (uint) (destinationRectI.Width * sizeof(uint)));

            Buffer.MemoryCopy((sourceBitmap + sourceStartIndex), (destinationBitmap + destinationStartIndex),
                (uint)(destinationRectI.Width * sizeof(uint)), (uint)(destinationRectI.Width * sizeof(uint)));

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

    /// <summary>
    /// 清理指定范围
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="rect"></param>
    public static unsafe void ClearBounds(this SKBitmap bitmap, SKRectI rect)
    {
        uint* basePtr = (uint*) bitmap.GetPixels().ToPointer();
        // Loop through the rows
        //var stopwatch = Stopwatch.StartNew();
        //for (int row = 0; row < bitmap.Height; row++)
        //{
        //    for (int col = 0; col < bitmap.Width; col++)
        //    {
        //        uint* ptr = basePtr + bitmap.Width * row + col;
        //        *ptr = unchecked((uint)(0xFF << 24 + ((byte)col) <<
        //                                16 + (byte) row));
        //    }
        //}

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            var col = rect.Left;
            uint* ptr = basePtr + bitmap.Width * row + col;

            var length = rect.Width;
            Unsafe.InitBlock(ptr, 0, (uint) length * sizeof(uint));
            //var span = new Span<uint>(ptr, length);
            //span.Clear();
        }

        //Console.WriteLine($"耗时 {stopwatch.ElapsedMilliseconds}"); // 差不多一秒
    }
}