using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using static AsitLib.CommandLine.ParseHelpers;

namespace AsitLib.CommandLine
{
    public interface ICommandInfoFactory<in TAttribute, out TCommandInfo> where TAttribute : CommandAttribute where TCommandInfo : CommandInfo
    {
        public TCommandInfo Convert(TAttribute attribute, CommandProvider provider, MethodInfo methodInfo);
    }

    public static class CommandInfoFactory
    {
        public class DefaultInfoFactory : ICommandInfoFactory<CommandAttribute, CommandInfo>
        {
            public DefaultInfoFactory() { }
            public CommandInfo Convert(CommandAttribute attribute, CommandProvider provider, MethodInfo methodInfo)
                => new CommandInfo(provider.Namespace + "-" + ParseHelpers.ParseSignature(methodInfo.Name), attribute.Description, methodInfo, provider);
        }

        public static ICommandInfoFactory<CommandAttribute, CommandInfo> Default { get; } = new DefaultInfoFactory();
    }

    public static class CommandEngine
    {
        public const string MAIN_COMMAND_ID = "_M";

        internal static InvalidOperationException GetNotInitializedException() => new InvalidOperationException("CommandEngine is not yet initialized.");
    }

    public class CommandEngine<TAttribute, TCommandInfo> : IDisposable where TAttribute : CommandAttribute where TCommandInfo : CommandInfo
    {
        private bool _disposedValue;
        private bool _initialized;

        //private readonly FrozenDictionary<string, flagHandler>? flagHandlers;
        private FrozenDictionary<string, CommandProvider>? _providers;
        private FrozenDictionary<string, TCommandInfo>? _commands;
        private ICommandInfoFactory<TAttribute, TCommandInfo> _infoFactory;

        public FrozenDictionary<string, TCommandInfo> Commands => _commands ?? throw CommandEngine.GetNotInitializedException();
        public FrozenDictionary<string, CommandProvider> Providers => _providers ?? throw CommandEngine.GetNotInitializedException();
        //public FrozenDictionary<string, FlagHandler> FlagHandlers => flagHandlers ?? throw CommandEngine.GetNotInitializedException();

        private List<CommandProvider> _tempProviders;
        private List<TCommandInfo> _tempCommands;
        private HashSet<string> _addedProviders;


        public CommandEngine(ICommandInfoFactory<TAttribute, TCommandInfo> infoFactory)
        {
            _infoFactory = infoFactory;
            _tempProviders = new List<CommandProvider>();
            _addedProviders = new HashSet<string>();
            _tempCommands = new List<TCommandInfo>();
        }


        public CommandEngine<TAttribute, TCommandInfo> RegisterProvider(CommandProvider provider)
        {
            _tempProviders.Add(provider);
            _addedProviders.Add(provider.Namespace);
            return this;
        }

        //public CommandEngine<TCommandInfo> RegisterFlagHandler(CommandProvider provider)
        //{
        //    return this;
        //}

        public CommandEngine<TAttribute, TCommandInfo> Initialize()
        {
            foreach (CommandProvider provider in _tempProviders)
            {
                MethodInfo[] commandMethods = provider.GetType().GetMethods();
                foreach (MethodInfo methodInfo in commandMethods)
                    if (methodInfo.GetCustomAttribute<TAttribute>() is TAttribute attribute)
                    {
                        if (methodInfo.ReturnType != typeof(void)) throw new InvalidOperationException("Commands must have a void return type.");

                        string cmdId;
                        if (methodInfo.Name == "_M") cmdId = provider.Namespace;
                        else cmdId = (attribute.InheritNamespace ? (provider.Namespace + "-") : string.Empty) + (attribute.Id?.ToLower() ?? methodInfo.Name.ToLower());

                        TCommandInfo info = _infoFactory.Convert(attribute, provider, methodInfo);
                        _tempCommands.Add(info);
                    }
            }

            _providers = _tempProviders.ToDictionary(p => p.Namespace).ToFrozenDictionary();
            _commands = _tempCommands.ToDictionary(c => c.Id).ToFrozenDictionary();

            return this;
        }

        public void Execute(string args) => Execute(Split(args));
        public void Execute(string[] args)
        {
            Console.WriteLine(Commands.ToJoinedString());
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
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
