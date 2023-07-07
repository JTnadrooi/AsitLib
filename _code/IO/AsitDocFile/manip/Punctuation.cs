using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AsitLib.IO
{
    public class IntManipulator : IUniManipulator<int, string>
    {
        public void Dispose() { }
        [return: NotNull]
        public string Maniputate(int input, string? arg)
        {
            switch (arg)
            {
                case "punc":
                    string toreturn = input < 0 ? "-" : string.Empty;
                    for (input = System.Math.Abs(input); input != 1 && input != 0; input -= 2) toreturn += ':';
                    return input == 1 ? (toreturn + '.') : toreturn;
                default: throw new ArgumentException();
            }
        }
        [return: NotNull]
        public object Maniputate(object? input, string? arg) => Maniputate(((int?)input) ?? 0, arg);
    }
}
