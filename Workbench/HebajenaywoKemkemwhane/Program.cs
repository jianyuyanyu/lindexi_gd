// See https://aka.ms/new-console-template for more information

Span<string> span = 
[
"DotNetCampus.MediaConverter.Tool.linux-arm64",
"DotNetCampus.MediaConverter.Tool.linux-x64",
"DotNetCampus.MediaConverter.Tool.win-arm64",
"DotNetCampus.MediaConverter.Tool.win-x64",
"DotNetCampus.MediaConverter.Tool.win-x86",
"DotNetCampus.MediaConverter.SkiaWmfRenderer",
];

foreach (var text in span)
{
    Console.WriteLine($"| {text} | [![](https://img.shields.io/nuget/v/{text}.svg)](https://www.nuget.org/packages/{text}) |");
}

Console.WriteLine("Hello, World!");
