using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static AsitLib.SpellScript.SSpell;
#nullable enable

namespace AsitLib.SpellScript
{
    /// <summary>
    /// A collection of return arguments for the <see cref="ISpellInterpeter.Run(SpellRunArgs)"/> method.
    /// </summary>
    public readonly struct SpellReturnArgs
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
        /// Create a new <see cref="SpellReturnArgs"/> with set return value.
        /// </summary>
        /// <param name="returnValue">The returned value. Seealso: <see cref="ReturnValue"/>.</param>
        public SpellReturnArgs(object returnValue) : this(true, returnValue, 0) { }
        /// <summary>
        /// Create a new <see cref="SpellReturnArgs"/> with a set <see cref="HasExecuted"/> parameter.
        /// </summary>
        /// <param name="hasExecuted"></param>
        public SpellReturnArgs(bool hasExecuted) : this(hasExecuted, null, 0) { }
        /// <summary>
        /// Create a new <see cref="SpellReturnArgs"/> with set <see cref="ExitCode"/>.
        /// </summary>
        /// <param name="exitCode"></param>
        public SpellReturnArgs(int exitCode) : this(true, null, exitCode) { }
        private SpellReturnArgs(bool hasExecuted, object? returnValue, int exitCode = 0)
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
        public static implicit operator SpellReturnArgs(int i) => new SpellReturnArgs(i);
        public static implicit operator SpellReturnArgs(bool b) => new SpellReturnArgs(b);
        /// <summary>
        /// A empty <see cref="SpellReturnArgs"/>.
        /// </summary>
        public static SpellReturnArgs Empty => new SpellReturnArgs(false, null, -1);
        /// <summary>
        /// A <see cref="SpellReturnArgs"/> with the returned object set to <see langword="null"/> and <see cref="ExitCode"/> set to 0. (Succes)
        /// </summary>
        public static SpellReturnArgs NullSucces => new SpellReturnArgs(true, null, 0);
        /// <summary>
        /// A <see cref="SpellReturnArgs"/> with the returned object set to <see langword="null"/> and <see cref="ExitCode"/> set to 1. (Failure)
        /// </summary>
        public static SpellReturnArgs NullFailure => new SpellReturnArgs(true, null, 1);
        /// <summary>
        /// "User-defined conversions to values of a base class are not allowed! You do not need such an operator."
        /// </summary>
        /// <param name="o">The object to create a new <see cref="SpellRunArgs"/> object from.</param>
        /// <returns>A new <see cref="SpellRunArgs"/> created from the object with set <see cref="ReturnValue"/>.</returns>
        public static SpellReturnArgs GetFromObject(object o) => new SpellReturnArgs(o);
    }
}
