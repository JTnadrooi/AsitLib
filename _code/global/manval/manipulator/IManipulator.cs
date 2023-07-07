using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AsitLib
{
    public interface IUniManipulator : IDisposable
    {
        /// <summary>
        /// a
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        [return: NotNull]
        public object Maniputate(object? input, string? arg);
    }
    public interface IUniManipulator<in TIn, out TOut> : IUniManipulator
    {
        /// <summary>
        /// b
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        [return: NotNull]
        public TOut Maniputate(TIn input, string? arg);
    }
}
