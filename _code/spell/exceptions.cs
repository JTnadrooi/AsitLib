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
    [Serializable]
    public class InvalidCommandException : SpellScriptException
    {
        public InvalidCommandException() { }
        public InvalidCommandException(string message) : base(message) { }
        public InvalidCommandException(string message, Exception inner) : base(message, inner) { }
        protected InvalidCommandException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class SpellScriptException : Exception
    {
        public SpellScriptException() { }
        public SpellScriptException(SpellCommand command) : this("command source:" + command) { }
        public SpellScriptException(string message, SpellCommand command) : this(message + " -- command source:" + command) { }
        public SpellScriptException(string message) : base(message) { }
        public SpellScriptException(string message, Exception inner) : base(message, inner) { }
        protected SpellScriptException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class SpellScriptMemoryException : SpellScriptException
    {
        public SpellScriptMemoryException() { }
        public SpellScriptMemoryException(string message) : base(message) { }
        public SpellScriptMemoryException(string message, Exception inner) : base(message, inner) { }
        protected SpellScriptMemoryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
