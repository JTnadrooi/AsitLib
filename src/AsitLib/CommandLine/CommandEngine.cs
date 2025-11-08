using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using static AsitLib.CommandLine.ParseHelper;

namespace AsitLib.CommandLine
{
    public static class CommandEngine
    {
        public const string MAIN_COMMAND_ID = "_M";

        internal static InvalidOperationException GetNotInitializedException() => new InvalidOperationException("CommandEngine is not yet initialized.");
    }

    public class CommandEngine<TCommandInfo> : IDisposable where TCommandInfo : CommandInfo
    {
        private bool disposedValue;

        private readonly FrozenDictionary<string, FlagHandler>? flagHandlers;
        private readonly FrozenDictionary<string, CommandProvider>? providers;
        private readonly FrozenDictionary<string, TCommandInfo>? commands;

        public FrozenDictionary<string, TCommandInfo> Commands => commands ?? throw CommandEngine.GetNotInitializedException();
        public FrozenDictionary<string, CommandProvider> Providers => providers ?? throw CommandEngine.GetNotInitializedException();
        public FrozenDictionary<string, FlagHandler> FlagHandlers => flagHandlers ?? throw CommandEngine.GetNotInitializedException();

        public CommandEngine()
        {

        }

        public CommandEngine<TCommandInfo> RegisterAttribute<TAttribute>(Func<TAttribute, MethodInfo, CommandProvider, CommandInfo> infoFactory) where TAttribute : Attribute
        {
            return this;
        }

        public CommandEngine<TCommandInfo> RegisterProvider(CommandProvider provider)
        {
            return this;
        }

        public CommandEngine<TCommandInfo> RegisterFlagHandler(CommandProvider provider)
        {
            return this;
        }

        public CommandEngine<TCommandInfo> Initialize()
        {
            return this;
        }

        public void Execute(string args) => Execute(Split(args));
        public void Execute(string[] args)
        {
            Console.WriteLine(args.ToJoinedString(", "));
            Console.WriteLine(Parse(args).ToDisplayString());
            Console.WriteLine("--" + "noCamelCase".ToKebabCase());
        }

        /// <summary>
        /// Gets the instance of a <see cref="CommandProvider"/> of the specified <typeparamref name="TProvider"/> type.
        /// </summary>
        /// <typeparam name="TProvider">The <see cref="CommandProvider"/> type.</typeparam>
        /// <returns>The instance of a <see cref="CommandProvider"/> of the specified <typeparamref name="TProvider"/> type.</returns>
        public TProvider GetProvider<TProvider>() where TProvider : CommandProvider
            => (TProvider)Providers.Values.First(p => p.GetType() == typeof(TProvider));

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
