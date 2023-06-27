using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using AsitLib;
using AsitLib.StringContructor;
using AsitLib.StringContructor.Defaults;
using AsitLib.IO;
using AsitLib.SpellScript;
using AsitLib.Collections;
using AsitLib.Debug;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Collections;
#nullable enable

namespace AsitLib.Collections
{
    //public class ArrayReferenceEnumerator<TIn, TOut> : IEnumerator<TOut>
    //{
    //    private readonly IEnumerator baseEnumerator;
    //    public ArrayReference<TIn, TOut> Source { get; }
    //    public ArrayReferenceEnumerator(ArrayReference<TIn, TOut> source)
    //    {
    //        Source = source;
    //        baseEnumerator = source.source.GetEnumerator();
    //    }
    //    public TOut Current => Source.Manipulator.Invoke((TIn)baseEnumerator.Current!);
    //    object? IEnumerator.Current => Current;

    //    public void Dispose() { }
    //    public bool MoveNext() => baseEnumerator.MoveNext();
    //    public void Reset() => baseEnumerator.Reset();
    //}
    //public class ArrayReference<TIn, TOut> : IEnumerable<TOut>, IReadOnlyCollection<TOut>
    //{
    //    internal TIn[] source;
    //    public Func<TIn, TOut> Manipulator { get; set; }
    //    public ArrayReference(TIn[] source, Func<TIn, TOut> manipulator)
    //    {
    //        Manipulator = manipulator;
    //        this.source = source;
    //    }

    //    public int Count => throw new NotImplementedException();

    //    public IEnumerator<TOut> GetEnumerator() => new ArrayReferenceEnumerator<TIn, TOut>(this);
    //    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    //}
}
