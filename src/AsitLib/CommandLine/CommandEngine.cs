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

        private readonly Dictionary<string, FlagHandler> _flagHandlers;
        private readonly Dictionary<string, CommandProvider> _providers;
        private readonly Dictionary<string, CommandInfo> _commands;
        private readonly Dictionary<string, CommandInfo> _uniqueCommands;

        public ReadOnlyDictionary<string, CommandProvider> Providers { get; }
        public ReadOnlyDictionary<string, CommandInfo> Commands { get; }
        public ReadOnlyDictionary<string, CommandInfo> UniqueCommands { get; }
        public ReadOnlyDictionary<string, FlagHandler> FlagHandlers { get; }

        public CommandEngine()
        {
            _providers = new Dictionary<string, CommandProvider>();
            _commands = new Dictionary<string, CommandInfo>();
            _uniqueCommands = new Dictionary<string, CommandInfo>();
            _flagHandlers = new Dictionary<string, FlagHandler>();

            Providers = _providers.AsReadOnly();
            Commands = _commands.AsReadOnly();
            UniqueCommands = _uniqueCommands.AsReadOnly();
            FlagHandlers = _flagHandlers.AsReadOnly();
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

        public CommandEngine RegisterFlagHandler(FlagHandler flagHandler)
        {
            _flagHandlers.Add(flagHandler.LongFormId, flagHandler);
            return this;
        }

        public void Execute(string args) => Execute(Split(args));
        public void Execute(string[] args)
        {
            string? ret = ExecuteAndCapture(args);
            if (ret != null) Console.WriteLine(ret);
        }

        public string? ExecuteAndCapture(string args) => ExecuteAndCapture(Split(args));
        public string? ExecuteAndCapture(string[] args)
        {
            ArgumentsInfo argsInfo = Parse(args);
            if (Commands.TryGetValue(argsInfo.CommandId, out CommandInfo? commandInfo))
            {
                FlagContext context = new FlagContext(argsInfo);
                object?[] conformed = Conform(ref argsInfo, commandInfo.GetParameters());
                FlagHandler[] pendingFlags = ExtractFlags(ref argsInfo, _flagHandlers.Values.ToArray());

                if (argsInfo.Arguments.Count > 0) throw new CommandException($"Duplicate or unresolved argument targets found; [{argsInfo.Arguments.ToJoinedString(", ")}].");

                foreach (FlagHandler flagHandler in pendingFlags) flagHandler.PreCommand(context);
                object? returned = commandInfo.Invoke(conformed);
                foreach (FlagHandler flagHandler in pendingFlags)
                {
                    returned = flagHandler.OnReturned(context, returned);
                    flagHandler.PostCommand(context);
                }

                return returned?.ToString();
            }
            else throw new InvalidOperationException($"Command with ID '{argsInfo.CommandId}' not found.");
        }

        public T? ExecuteAndCapture<T>(string args)
            => (T?)Convert(ExecuteAndCapture(Split(args)) ?? throw new InvalidOperationException("Cannot capture return type from void-returning commands."), typeof(T));
        public T? ExecuteAndCapture<T>(string[] args)
            => (T?)Convert(ExecuteAndCapture(args) ?? throw new InvalidOperationException("Cannot capture return type from void-returning commands."), typeof(T));

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
