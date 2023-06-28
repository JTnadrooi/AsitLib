using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AsitLib
{
    public interface IUniManipulator : IDisposable
    {
        [return: NotNull]
        public string Maniputate(string? input, object? arg);
    }
}
