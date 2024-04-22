﻿// See https://aka.ms/new-console-template for more information

using EDIDParser;

var file = "edid";


unsafe
{
    int[] n = [1, 2, 3];

    var value = n.AsSpan();
    if (value is [2, 2, 3])
    {
        // 底层
        /*
           IL_0021: ldloca.s     'value'
           IL_0023: call         instance int32 valuetype [System.Runtime]System.Span`1<int32>::get_Length()
           IL_0028: ldc.i4.3
           IL_0029: bne.un.s     IL_0051
           IL_002b: ldloca.s     'value'
           IL_002d: ldc.i4.0
           IL_002e: call         instance !0/*int32* /& valuetype [System.Runtime]System.Span`1<int32>::get_Item(int32)
           IL_0033: ldind.i4
           IL_0034: ldc.i4.2
           IL_0035: bne.un.s     IL_0051
           IL_0037: ldloca.s     'value'
           IL_0039: ldc.i4.1
           IL_003a: call         instance !0/*int32* /& valuetype [System.Runtime]System.Span`1<int32>::get_Item(int32)
           IL_003f: ldind.i4
           IL_0040: ldc.i4.2
           IL_0041: bne.un.s     IL_0051
           IL_0043: ldloca.s     'value'
           IL_0045: ldc.i4.2
           IL_0046: call         instance !0/*int32* /& valuetype [System.Runtime]System.Span`1<int32>::get_Item(int32)
           IL_004b: ldind.i4
           IL_004c: ldc.i4.3
           IL_004d: ceq
           IL_004f: br.s         IL_0052
           IL_0051: ldc.i4.0
           IL_0052: stloc.s      V_5
         */
        // 重新转换为低级 C# 代码
        /*
             if (value.Length != 3 || value[0] != 2 || value[1] != 2 || value[2] != 3)
         */

    }
}
// 内容很小，全部读取出来也不怕
var data = File.ReadAllBytes(file);
var edid = new EDID(data);

Console.WriteLine("Hello, World!");