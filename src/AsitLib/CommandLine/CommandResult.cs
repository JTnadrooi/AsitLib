using System.IO;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents the result of a command execution.
    /// </summary>
    public sealed class CommandResult
    {
        private object? _rawValue; // can be DBNull.Value.

        /// <summary>
        /// Gets the result value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the result represents a void return.
        /// </exception>
        public object? Value
        {
            get
            {
                if (IsVoid) throw new InvalidOperationException("Cannot get value from void return.");

                return _rawValue;
            }
        }

        public CommandEngine Engine { get; }

        /// <summary>
        /// Gets a value indicating whether the command returned no value.
        /// </summary>
        public bool IsVoid => _rawValue is DBNull;

        /// <summary>
        /// Gets the raw result value, which may be <see cref="DBNull.Value"/> for void results.
        /// </summary>
        public object? RawValue => _rawValue; // can be dbnull.

        internal CommandResult(CommandEngine engine, object? rawValue)
        {
            _rawValue = rawValue;
            Engine = engine;
        }

        /// <summary>
        /// Converts the result to a string suitable for console output.
        /// </summary>
        /// <returns>A formatted string representation of the result.</returns>
        public string ToOutputString()
        {
            if (IsVoid) throw new InvalidOperationException("Cannot get output string from void return.");

            if (Value is null) return StringHelpers.NULL_STRING;

            StringBuilder sb = new StringBuilder();
            switch (Value)
            {
                case IDictionary dict:
                    foreach (DictionaryEntry e in dict)
                    {
                        string keyStr = e.Key.ToString()!;
                        string valueStr = e.Value?.ToString() ?? StringHelpers.NULL_STRING;

                        static bool CheckIfValid(string s) => !s.Contains('\n') || !s.Contains('=');

                        if (!CheckIfValid(keyStr)) throw new InvalidOperationException("Dictionary key has newline or equals sign (=), this is invalid and will conflict with parsing.");
                        if (!CheckIfValid(valueStr)) throw new InvalidOperationException("Dictionary value has newline or equals sign (=), this is invalid and will conflict with parsing.");

                        sb.Append(keyStr).Append(Engine.KeyValueSeperator).Append(valueStr).Append(Engine.NewLine);
                    }
                    sb.Length -= Engine.NewLine.Length;
                    break;
                case string s: goto default; // because string implements IEnumerable<char>.
                case IEnumerable values:
                    foreach (object v in values)
                    {
                        string s = v.ToString()!;
                        if (s.Contains('\n')) throw new InvalidOperationException("IEnumerable value has newline, this is invalid and will conflict with parsing.");
                        sb.Append(s).Append(Engine.NewLine);
                    }
                    sb.Length -= Engine.NewLine.Length;
                    break;
                default:
                    sb.Append(Value.ToString());
                    break;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Writes the result to <see cref="Console.Out"/>.
        /// </summary>
        public void WriteToConsole() => WriteTo(Console.Out);

        /// <summary>
        /// Writes the result to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="target">The target writer.</param>
        public void WriteTo(TextWriter target)
        {
            if (IsVoid) return;

            target.WriteLine(ToOutputString());
            target.Flush();
        }

        /// <summary>
        /// Returns a non-output-formatted string representation of the result.
        /// </summary>
        public override string ToString()
        {
            return $"{{Value (as non-output ready): {(IsVoid ? "__void__" : (Value?.ToString() ?? StringHelpers.NULL_STRING))}}}";
        }
    }
}
