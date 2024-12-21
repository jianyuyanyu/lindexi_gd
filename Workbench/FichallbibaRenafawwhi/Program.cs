﻿// See https://aka.ms/new-console-template for more information

using System.Globalization;

using Icu;

Icu.Wrapper.Init();

var text = "asd fx, aasa “说话大学生上课”\nasd sadf";
// https://unicode.org/reports/tr14/
// Unicode Line Breaking Algorithm (UAX #14)
var boundaries = Icu.BreakIterator.GetBoundaries(BreakIterator.UBreakIteratorType.LINE, Locale.GetLocaleForLCID(CultureInfo.CurrentCulture.LCID), text);
foreach (Boundary boundary in boundaries)
{
    var subText = text.AsSpan().Slice(boundary.Start, boundary.End - boundary.Start);
    Console.WriteLine(subText.ToString());
}

// Will output "NFC form of XA\u0308bc is XÄbc"
Console.WriteLine($"NFC form of XA\\u0308bc is {Icu.Normalizer.Normalize("XA\u0308bc",
    Icu.Normalizer.UNormalizationMode.UNORM_NFC)}");
Icu.Wrapper.Cleanup();
