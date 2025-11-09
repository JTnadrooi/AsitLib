using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public class DefaultInfoFactory : ICommandInfoFactory<CommandAttribute, ProviderCommandInfo>
        {
            public DefaultInfoFactory() { }
            public ProviderCommandInfo Convert(CommandAttribute attribute, CommandProvider provider, MethodInfo methodInfo)
                => new ProviderCommandInfo(CommandHelpers.CreateCommandId(attribute, provider, methodInfo).ToSingleArray().Concat(attribute.Aliases).ToArray(), attribute.Description, methodInfo, provider);
        }

        public static ICommandInfoFactory<CommandAttribute, CommandInfo> Default { get; } = new DefaultInfoFactory();
    }

    public class CommandEngine : IDisposable
    {
        public const string MAIN_COMMAND_ID = "_M";

        private bool _disposedValue;

        //private readonly FrozenDictionary<string, flagHandler>? flagHandlers;
        private Dictionary<string, CommandProvider> _providers;
        private Dictionary<string, CommandInfo> _commands;
        private Dictionary<string, CommandInfo> _uniqueCommands;

        public ReadOnlyDictionary<string, CommandProvider> Providers { get; }
        public ReadOnlyDictionary<string, CommandInfo> Commands { get; }
        public ReadOnlyDictionary<string, CommandInfo> UniqueCommands { get; }
        //public FrozenDictionary<string, FlagHandler> FlagHandlers => flagHandlers ?? throw CommandEngine.GetNotInitializedException();

        public CommandEngine()
        {
            _providers = new Dictionary<string, CommandProvider>();
            _commands = new Dictionary<string, CommandInfo>();
            _uniqueCommands = new Dictionary<string, CommandInfo>();

            Providers = _providers.AsReadOnly();
            Commands = _commands.AsReadOnly();
            UniqueCommands = _uniqueCommands.AsReadOnly();
        }

        public CommandEngine RegisterCommand(MethodInfo method, string description, string[]? aliases = null)
            => RegisterCommand(new MethodCommandInfo((aliases ?? Enumerable.Empty<string>()).Prepend(ParseHelpers.ParseSignature(method)).ToArray(), description, method));
        public CommandEngine RegisterCommand(Func<object?> func, string id, string description, string[]? aliases = null)
            => RegisterCommand(new FunctionCommandInfo((aliases ?? Enumerable.Empty<string>()).Prepend(id).ToArray(), description, func));
        public CommandEngine RegisterCommand(CommandInfo info)
        {
            _uniqueCommands.Add(info.Id, info);
            foreach (string id in info.Ids) _commands.Add(id, info);
            return this;
        }

        public CommandEngine RegisterProvider<TAttribute, TCommandInfo>(CommandProvider provider, ICommandInfoFactory<TAttribute, TCommandInfo> infoFactory) where TAttribute : CommandAttribute where TCommandInfo : CommandInfo
        {
            MethodInfo[] commandMethods = provider.GetType().GetMethods();
            foreach (MethodInfo methodInfo in commandMethods)
                if (methodInfo.GetCustomAttribute<TAttribute>() is TAttribute attribute)
                    RegisterCommand(infoFactory.Convert(attribute, provider, methodInfo));
            return this;
        }

        //public CommandEngine<TCommandInfo> RegisterFlagHandler(CommandProvider provider)
        //{
        //    return this;
        //}

        public void Execute(string args) => Execute(Split(args));
        public void Execute(string[] args)
        {
            string? ret = ExecuteAndCapture(args);
            if (ret != null) Console.WriteLine(ret);
        }

        public string? ExecuteAndCapture(string args) => ExecuteAndCapture(Split(args));
        public string? ExecuteAndCapture(string[] args)
        {
            ArgumentsInfo argsinfo = Parse(args);
            if (Commands.TryGetValue(argsinfo.CommandId, out CommandInfo? commandInfo))
            {
                object?[] conformed = Conform(argsinfo, commandInfo.GetParameters());
                return commandInfo.Invoke(conformed)?.ToString()!;

                //Console.WriteLine(Commands.ToJoinedString(", "));
                //Console.WriteLine(argsinfo.ToDisplayString());
                //Console.WriteLine(conformed.ToJoinedString(", "));
            }
            else throw new InvalidOperationException($"Command with ID '{argsinfo.CommandId}' not found.");
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
