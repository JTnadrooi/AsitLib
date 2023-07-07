using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AsitLib;
using System.Data.SqlTypes;
#nullable enable

namespace AsitLib
{
    public static class Manipulator<TIn, TOut>
    {
        public static IUniManipulator<TIn, TOut> Empty => new DelegateManipulator<TIn, TOut>((s) => (TOut)(object)s! ?? throw new Exception());
    }
    public static class Manipulator
    {
        public static IUniManipulator Empty => new DelegateManipulator<object, object>((s) => s);
        public static DelegateManipulator<TIn, TOut> AsManipulator<TIn, TOut>(this Func<TIn, TOut> function) => new DelegateManipulator<TIn, TOut>(function); //?????
        public static FormatterManipulator AsManipulator(this ICustomFormatter customFormatter, string? format, IFormatProvider? formatProvider) => new FormatterManipulator(customFormatter, format, formatProvider);
        public static IUniManipulator<object, object> AsGeneric(this IUniManipulator source) => new ObjManipulator(source);
        public static TOut Manipulate<TIn, TOut>(this IUniManipulator<TIn, TOut> manipulator, TIn input)
            => manipulator.Maniputate(input, null);
        public static object Manipulate(this IUniManipulator manipulator, object? input) => manipulator.Maniputate(input, null);
        public static bool TryManipulate(this IUniManipulator manipulator, object input, string? arg, out object output)
            => TryManipulate(manipulator.AsGeneric()!, input, arg, out output);
        public static bool TryManipulate<TIn, TOut>(this IUniManipulator<TIn, TOut> manipulator, TIn input, string? arg, out TOut output)
        {
            try
            {
                output = manipulator.Maniputate(input, arg);
                return true;
            }
            catch (Exception)
            {
                output = default!;
                return false;
            }
        }
    }
}
