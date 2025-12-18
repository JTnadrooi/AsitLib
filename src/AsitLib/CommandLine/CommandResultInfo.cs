using System.IO;

namespace AsitLib.CommandLine
{
    public sealed class CommandResultInfo
    {
        private object? _rawValue; // can be DBNull.Value.

        public object? Value
        {
            get
            {
                ThrowIfVoid();
                return _rawValue;
            }
        }

        public CommandEngine Engine { get; }

        public bool IsVoid => _rawValue is DBNull;

        internal CommandResultInfo(CommandEngine engine, object? rawValue)
        {
            _rawValue = rawValue;
            Engine = engine;
        }

        internal void ThrowIfVoid()
        {
            if (IsVoid) throw new InvalidOperationException("Command returned void.");
        }

        public T? GetValue<T>() => (T?)Value;

        public string ToOutputString()
        {
            ThrowIfVoid();

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

        public void WriteToConsole() => WriteTo(Console.Out);

        public void WriteTo(TextWriter to)
        {
            if (IsVoid) return;

            to.WriteLine(Value);
            to.Flush();
        }

        public override string ToString()
        {
            return $"{{Value (as non-output ready): {(IsVoid ? "__void__" : (Value?.ToString() ?? StringHelpers.NULL_STRING))}}}";
        }
    }
}
