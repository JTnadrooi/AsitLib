using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsitLib.IO
{
    public interface IAsitFile : IDisposable
    {
        public Encoding Encoding { get; }
        public string FileType { get; }
        public int Version { get; } //Maybe custom formatter would be nice.. BUT I SHALL STAY FOCUSSED!
        public string CoreExtension { get; }
        public object[]? Other {  get; }
        public FileInfo FileInfo { get; }
    }
}
