using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static AsitLib.SpellScript.SpellUtils;
#nullable enable

namespace AsitLib.SpellScript
{
    /// <summary>
    /// A collection of return arguments for the <see cref="ISpellInterpeter.Run(SpellRunArgs)"/> method.
    /// </summary>
    public readonly struct ReturnArgs
    {
        /// <summary>
        /// The returned value, <see langword="null"/> when there is no value returned.
        /// </summary>
        public object? ReturnValue { get; }
        /// <summary>
        /// A <see cref="bool"/> value indicating if there is a value returned.
        /// </summary>
        public bool HasReturned => ReturnValue != null;
        /// <summary>
        /// A <see cref="bool"/> value indicating if there has been a operation.
        /// </summary>
        public bool HasExecuted { get; }
        /// <summary>
        /// The exitcode of the <see cref="ISpellInterpeter"/> object's action.<br/>
        /// <i>Negative: succes, Zero: succes, Positive: failure.</i>
        /// </summary>
        public int ExitCode { get; }
        /// <summary>
        /// Create a new <see cref="ReturnArgs"/> with set return value.
        /// </summary>
        /// <param name="returnValue">The returned value. Seealso: <see cref="ReturnValue"/>.</param>
        public ReturnArgs(object returnValue) : this(true, returnValue, 0) { }
        /// <summary>
        /// Create a new <see cref="ReturnArgs"/> with a set <see cref="HasExecuted"/> parameter.
        /// </summary>
        /// <param name="hasExecuted"></param>
        public ReturnArgs(bool hasExecuted) : this(hasExecuted, null, 0) { }
        /// <summary>
        /// Create a new <see cref="ReturnArgs"/> with set <see cref="ExitCode"/>.
        /// </summary>
        /// <param name="exitCode"></param>
        public ReturnArgs(int exitCode) : this(true, null, exitCode) { }
        private ReturnArgs(bool hasExecuted, object? returnValue, int exitCode = 0)
        {
            ReturnValue = returnValue;
            HasExecuted = hasExecuted;
            ExitCode = exitCode;
        }
        /// <summary>
        /// Get the <see cref="ReturnValue"/> pre-casted to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast the <see cref="ReturnValue"/> to.</typeparam>
        /// <returns>The <see cref="ReturnValue"/> pre-casted to type <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetReturnValue<T>() => ReturnValue is T t ? t : throw new InvalidOperationException("Invalid cast when getting return args.");
        public static implicit operator ReturnArgs(int i) => new ReturnArgs(i);
        public static implicit operator ReturnArgs(bool b) => new ReturnArgs(b);
        /// <summary>
        /// A empty <see cref="ReturnArgs"/>.
        /// </summary>
        public static ReturnArgs Empty => new ReturnArgs(false, null, -1);
        /// <summary>
        /// A <see cref="ReturnArgs"/> with the returned object set to <see langword="null"/> and <see cref="ExitCode"/> set to 0. (Succes)
        /// </summary>
        public static ReturnArgs NullSucces => new ReturnArgs(true, null, 0);
        /// <summary>
        /// A <see cref="ReturnArgs"/> with the returned object set to <see langword="null"/> and <see cref="ExitCode"/> set to 1. (Failure)
        /// </summary>
        public static ReturnArgs NullFailure => new ReturnArgs(true, null, 1);
        /// <summary>
        /// Get a new <see cref="ReturnArgs"/> struct with <paramref name="o"/> as set <see cref="ReturnValue"/>.
        /// </summary>
        /// <param name="o">The object to create a new <see cref="SpellRunArgs"/> object from.</param>
        /// <returns>A new <see cref="SpellRunArgs"/> created from the object with set <see cref="ReturnValue"/>.</returns>
        public static ReturnArgs GetFromObject(object o) => new ReturnArgs(o);
    }
}
