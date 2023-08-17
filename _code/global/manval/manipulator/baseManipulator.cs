using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AsitLib;
using System.Diagnostics.CodeAnalysis;
using System.Collections;
using System.IO;
#nullable enable

namespace AsitLib
{
    public sealed class FormatterManipulator : IUniManipulator<string, string>
    {
        ICustomFormatter CustomFormatter { get; }
        public string? Format { get; }
        public IFormatProvider? FormatProvider { get; }
        public FormatterManipulator(ICustomFormatter customFormatter, string? format, IFormatProvider? formatProvider)
        {
            CustomFormatter = customFormatter;
            Format = format;
            FormatProvider = formatProvider;
        }
        [return: NotNull]
        public object Maniputate(object? input, string? arg) => Maniputate((string?)input, arg);
        [return: NotNull]
        public string Maniputate(string? input, string? arg)
            => CustomFormatter.Format(Format, input, FormatProvider);
        public void Dispose() { }
    }
    public sealed class DelegateManipulator<TIn, TOut> : IUniManipulator<TIn, TOut>
    {
        public Func<TIn, TOut> Function { get; }
        public DelegateManipulator(Func<TIn, TOut> function)
        {
            Function = function;
        }
        [return: NotNull]
        public object Maniputate(object? input, string? arg) => Maniputate((string?)input, arg);
        [return: NotNull]
        public TOut Maniputate(TIn input, string? arg)
            => Function.Invoke(input)!;
        public void Dispose() { }
    }
    public sealed class ObjManipulator : IUniManipulator<object, object>
    {
        public IUniManipulator Source { get; }
        public ObjManipulator(IUniManipulator source)
        {
            Source = source;
        }
        [return: NotNull]
        public object Maniputate(object? input, string? arg)
            => Source.Maniputate(input, arg);
        public void Dispose() { }
    }
    //public sealed class TextWriterManipulator : TextWriter, IUniManipulator<char, string>
    //{
    //    public override Encoding Encoding => throw new NotImplementedException();

    //    public string? Arg { get; }
    //    public IUniManipulator<char, string> Source { get; }

    //    public TextWriterManipulator(IUniManipulator<char, string> source, string? arg = null)
    //    {
    //        Source = source;
    //        Arg = arg;
    //    }
    //    [return: NotNull]
    //    public string Maniputate(char input, string? arg)
    //        => Source.Maniputate(input, arg);
    //    [return: NotNull]
    //    public object Maniputate(object? input, string? arg)
    //        => Source.Maniputate(input, arg);
    //    public override void Write(char value)
    //    {
    //        if (Source.Maniputate(value, Arg).Length == 1) Write(Source.Maniputate(value, Arg)[0]);
    //        else Write(Source.Maniputate(value, Arg));
    //        base.Write(value);
    //    }
    //}
}
