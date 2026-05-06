using System;
using System.IO;
using System.IO.Compression;

using OpenMcdf;

namespace GacahocearNairhembaykaka
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootStorage = OpenMcdf.RootStorage.OpenRead("oleObject1.bin");
            using CfbStream cfStream = rootStorage.OpenStream("Package");
            using var zipArchive = new ZipArchive(cfStream, ZipArchiveMode.Read);
            foreach (var zipArchiveEntry in zipArchive.Entries)
            {
                GC.KeepAlive(zipArchiveEntry);
            }
        }
    }
}
