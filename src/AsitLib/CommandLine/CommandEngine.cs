using AsitLib.Diagnostics;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using static AsitLib.CommandLine.ParseHelpers;
using static System.Net.WebRequestMethods;

namespace AsitLib.CommandLine
{
    public class CommandEngine : IDisposable
    {
        private bool _disposedValue;

        private readonly Dictionary<string, GlobalOptionHandler> _flagHandlers;
        private readonly Dictionary<string, CommandProvider> _providers;
        private readonly Dictionary<string, CommandInfo> _commands;
        private readonly Dictionary<string, CommandInfo> _uniqueCommands;

        public ReadOnlyDictionary<string, CommandProvider> Providers { get; }
        public ReadOnlyDictionary<string, CommandInfo> Commands { get; }
        public ReadOnlyDictionary<string, CommandInfo> UniqueCommands { get; }
        public ReadOnlyDictionary<string, GlobalOptionHandler> FlagHandlers { get; }

        public string NewLine { get; set; }
        public string KeyValueSeperator { get; set; }
        public string? NullString { get; set; }

        private readonly Dictionary<string, List<string>> _srcMap;

        public CommandEngine()
        {
            _providers = new Dictionary<string, CommandProvider>();
            _commands = new Dictionary<string, CommandInfo>();
            _uniqueCommands = new Dictionary<string, CommandInfo>();
            _flagHandlers = new Dictionary<string, GlobalOptionHandler>();

            Providers = _providers.AsReadOnly();
            Commands = _commands.AsReadOnly();
            UniqueCommands = _uniqueCommands.AsReadOnly();
            FlagHandlers = _flagHandlers.AsReadOnly();

            NewLine = "\n";
            KeyValueSeperator = "=";
            NullString = StringHelpers.NULL_STRING;

            _srcMap = new Dictionary<string, List<string>>();

            AddCommand((string? commandId = null) =>
            {
                StringBuilder sb = new StringBuilder();
                void WriteCommand(CommandInfo cmd) => sb.Append($"{cmd.Id}{(cmd.HasAliases ? $"[{cmd.Ids.Skip(1).ToJoinedString(", ")}]" : string.Empty)} {cmd.GetOptions()
                    .Select(p => $"{p.Type.Name.ToLower()}:{p.Name!.ToLower()}{(p.HasDefaultValue ? ($"(default_value:{p.DefaultValue?.ToString() ?? StringHelpers.NULL_STRING}) ") : " ")}").ToJoinedString("")}" +
                    $"# {cmd.Description}" + NewLine);
                //void WriteProvider(CommandProvider provider) => sb.AppendLine($"{provider.Namespace} # {Commands.Values.Where(cmd => (cmd is ProviderCommandInfo pci) && pci.Provider == provider).Count()} commands.");

                if (commandId is not null) WriteCommand(Commands[commandId]);
                else
                    foreach (CommandInfo cmd in UniqueCommands.Values.OrderBy(c => c.Id)) WriteCommand(cmd);

                sb.Length--;

                return sb.ToString();
            }, "help", "Print help.", ["?", "h"], isGenericFlag: true);

            AddGlobalOption(new HelpGlobalOptionHandler());
        }

        public CommandEngine AddCommand(MethodInfo method, string description, string[]? aliases = null, bool isGenericFlag = false)
            => AddCommand(new MethodCommandInfo((aliases ?? Enumerable.Empty<string>()).Prepend(ParseHelpers.GetSignature(method)).ToArray(), description, method, isGenericFlag: isGenericFlag));
        public CommandEngine AddCommand(Delegate @delegate, string id, string description, string[]? aliases = null, bool isGenericFlag = false)
            => AddCommand(new DelegateCommandInfo((aliases ?? Enumerable.Empty<string>()).Prepend(id).ToArray(), description, @delegate, isGenericFlag: isGenericFlag));
        public CommandEngine AddCommand(CommandInfo info)
        {
            info.ThrowIfInvalid();

            List<string> additionalIds = new List<string>();
            OptionInfo[] parameters = info.GetOptions();

            foreach (string id in info.Ids)
            {
                if (id.StartsWith('-')) throw new InvalidOperationException($"Invalid command id '{id}'; invalid first character.");
                if (info.IsGenericFlag) additionalIds.Add(ParseHelpers.GetGenericFlagSignature(id));
            }

            foreach (string id in info.Ids.Concat(additionalIds))
            {
                if (!_commands.TryAdd(id, info)) throw new InvalidOperationException($"Command with duplicate key '{id}' found.");
            }

            _uniqueCommands.Add(info.Id, info);
            if (info is ProviderCommandInfo pCInfo) _srcMap[pCInfo.Provider.Namespace].Add(info.Id);

            return this;
        }

        public CommandEngine RemoveCommand(string id)
        {
            if (!_uniqueCommands.ContainsKey(id)) throw new KeyNotFoundException($"Command with MAIN ID '{id}' was not found.");

            bool isGenericFlag = _uniqueCommands[id].IsGenericFlag;

            foreach (string aliasId in _uniqueCommands[id].Ids)
            {
                if (isGenericFlag) _commands.Remove(ParseHelpers.GetGenericFlagSignature(aliasId));
                _commands.Remove(aliasId);
            }

            _uniqueCommands.Remove(id);

            return this;
        }

        public CommandEngine AddProvider<TAttribute, TCommandInfo>(CommandProvider provider, ICommandInfoFactory<TAttribute, TCommandInfo> infoFactory) where TAttribute : CommandAttribute where TCommandInfo : CommandInfo
        {
            if (!_providers.TryAdd(provider.Namespace, provider)) throw new InvalidOperationException($"CommandProvider with duplicate namespace '{provider.Namespace}' found.");

            MethodInfo[] commandMethods = provider.GetType().GetMethods();
            _srcMap.Add(provider.Namespace, new List<string>());

            foreach (MethodInfo methodInfo in commandMethods)
                if (methodInfo.GetCustomAttribute<TAttribute>() is TAttribute attribute)
                    AddCommand(infoFactory.Convert(attribute, provider, methodInfo));

            return this;
        }

        public CommandEngine RemoveProvider(string @namespace)
        {
            if (!_providers.Remove(@namespace)) throw new KeyNotFoundException($"CommandProvider with Namespace '{@namespace}' was not found.");
            foreach (string id in _srcMap[@namespace])
                RemoveCommand(id);
            return this;
        }

        public CommandEngine AddGlobalOption(GlobalOptionHandler flagHandler)
        {
            _flagHandlers.Add(flagHandler.LongFormId, flagHandler);
            return this;
        }

        public CommandEngine RemoveGlobalOptionHandler(string id)
        {
            if (!_flagHandlers.Remove(id)) throw new KeyNotFoundException($"FlagHandler with Id '{id}' was not found.");
            return this;
        }

        public ProviderCommandInfo[] GetProviderCommands(string @namespace)
        {
            List<ProviderCommandInfo> providerCommands = new List<ProviderCommandInfo>();

            foreach (string id in _srcMap[@namespace])
                if (_uniqueCommands.TryGetValue(id, out CommandInfo? commandInfo))
                    providerCommands.Add((ProviderCommandInfo)commandInfo);

            return providerCommands.ToArray();
        }

        public string? Execute(string args) => Execute(Split(args));
        public string? Execute(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            object? ret = ExecuteAndCapture(args);
            switch (ret)
            {
                case null:
                    if (NullString is null) return null;
                    else sb.Append(NullString);
                    break;
                case IDictionary dict:
                    foreach (DictionaryEntry e in dict)
                    {
                        string keyStr = e.Key.ToString()!;
                        string valueStr = e.Value?.ToString() ?? StringHelpers.NULL_STRING;

                        static bool CheckIfValid(string s) => !s.Contains('\n') || !s.Contains('=');

                        if (!CheckIfValid(keyStr)) throw new InvalidOperationException("Dictionary key has newline or equals sign (=), this is invalid and will conflict with parsing.");
                        if (!CheckIfValid(valueStr)) throw new InvalidOperationException("Dictionary value has newline or equals sign (=), this is invalid and will conflict with parsing.");

                        sb.Append(keyStr).Append(KeyValueSeperator).Append(valueStr).Append(NewLine);
                    }
                    sb.Length--;
                    break;
                case string s: goto default; // because string implements IEnumerable<char>.
                case IEnumerable values:
                    foreach (object v in values)
                    {
                        string s = v.ToString()!;
                        if (s.Contains('\n')) throw new InvalidOperationException("IEnumerable value has newline, this is invalid and will conflict with parsing.");
                        sb.Append(s).Append(NewLine);
                    }
                    sb.Length--;
                    break;
                default:
                    sb.Append(ret.ToString());
                    break;
            }
            return sb.ToString();
        }

        public object? ExecuteAndCapture(string args) => ExecuteAndCapture(Split(args));
        public object? ExecuteAndCapture(string[] args)
        {
            ArgumentsInfo argsInfo = Parse(args);
            if (Commands.TryGetValue(argsInfo.CommandId, out CommandInfo? commandInfo))
            {
                CommandContext context = new CommandContext(this, argsInfo, true);
                object?[] conformed = Conform(ref argsInfo, commandInfo.GetOptions());
                GlobalOptionHandler[] pendingFlags = ExtractFlags(ref argsInfo, _flagHandlers.Values.ToArray());

                if (argsInfo.Arguments.Count > 0) throw new CommandException($"Duplicate or unresolved argument targets found; [{argsInfo.Arguments.ToJoinedString(", ")}].");

                foreach (GlobalOptionHandler flagHandler in pendingFlags) flagHandler.PreCommand(context);

                object? returned = context.HasFlag(ExecutingContextFlags.PreventCommand) ? null : commandInfo.Invoke(conformed);
                context.PreCommand = false;

                returned = context.RunAllActions() ?? returned;

                if (!context.HasFlag(ExecutingContextFlags.PreventFlags))
                    foreach (GlobalOptionHandler flagHandler in pendingFlags)
                    {
                        returned = flagHandler.OnReturned(context, returned);
                        flagHandler.PostCommand(context);
                    }

                return returned;
            }
            else if (argsInfo.CallsGenericFlag)
            {
                if (Commands.ContainsKey(argsInfo.CommandId.TrimStart('-'))) throw new InvalidOperationException($"Command with id '{argsInfo.SanitizedCommandId}' cannot be used as generic flag.");
                throw new InvalidOperationException($"Generic flag with ID '{argsInfo.CommandId}' not found.");
            }
            else throw new InvalidOperationException($"Command with ID '{argsInfo.CommandId}' not found.");
        }

        public T? ExecuteAndCapture<T>(string args)
            => (T?)ExecuteAndCapture(Split(args)) ?? throw new InvalidOperationException("Cannot capture return type from void-returning commands.");
        public T? ExecuteAndCapture<T>(string[] args)
            => (T?)ExecuteAndCapture(args) ?? throw new InvalidOperationException("Cannot capture return type from void-returning commands.");

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
