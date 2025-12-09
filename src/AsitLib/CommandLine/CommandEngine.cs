using AsitLib.Diagnostics;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public sealed class CommandEngine
    {
        public ReadOnlyDictionary<string, CommandProvider> Providers { get; }
        public ReadOnlyDictionary<string, CommandInfo> Commands { get; }
        public ReadOnlyDictionary<string, CommandInfo> UniqueCommands { get; }
        public ReadOnlyDictionary<string, GlobalOption> GlobalOptions { get; }
        public ReadOnlyDictionary<string, ActionHook> Hooks { get; }
        public IReadOnlyCollection<string> Groups { get; }

        public string NewLine { get; set; }
        public string KeyValueSeperator { get; set; }

        private readonly Dictionary<string, List<string>> _srcMap;
        private readonly Dictionary<string, List<string>> _groupMap;
        private readonly Dictionary<string, GlobalOption> _globalOptions;
        private readonly Dictionary<string, CommandProvider> _providers;
        private readonly Dictionary<string, CommandInfo> _commands;
        private readonly Dictionary<string, CommandInfo> _uniqueCommands;
        private readonly Dictionary<string, ActionHook> _hooks;

        public CommandEngine()
        {
            _providers = new Dictionary<string, CommandProvider>();
            _commands = new Dictionary<string, CommandInfo>();
            _uniqueCommands = new Dictionary<string, CommandInfo>();
            _globalOptions = new Dictionary<string, GlobalOption>();
            _hooks = new Dictionary<string, ActionHook>();
            _groupMap = new Dictionary<string, List<string>>();

            Providers = _providers.AsReadOnly();
            Commands = _commands.AsReadOnly();
            UniqueCommands = _uniqueCommands.AsReadOnly();
            GlobalOptions = _globalOptions.AsReadOnly();
            Hooks = _hooks.AsReadOnly();
            Groups = _groupMap.Keys;

            NewLine = "\n";
            KeyValueSeperator = "=";

            _srcMap = new Dictionary<string, List<string>>();

            AddCommand((string? commandId = null) =>
            {
                StringBuilder sb = new StringBuilder();
                if (commandId is not null) return Commands[commandId].GetHelpString();
                else
                    foreach (CommandInfo cmd in GetEnabledCommands().OrderBy(c => c.Id)) sb.Append(cmd.GetHelpString()).Append(NewLine);

                sb.Length -= NewLine.Length;

                return sb.ToString();
            }, "help", "Prints help.", ["?", "h"], isGenericFlag: true);

            AddGlobalOption(new HelpGlobalOption());
        }

        #region ADD_REMOVE

        public CommandEngine AddCommand(MethodInfo method, string description, string[]? aliases = null, bool isGenericFlag = false)
            => AddCommand(new MethodCommandInfo((aliases ?? Enumerable.Empty<string>()).Prepend(ParseHelpers.GetSignature(method)).ToArray(), description, method, isGenericFlag: isGenericFlag));
        public CommandEngine AddCommand(Delegate @delegate, string id, string description, string[]? aliases = null, bool isGenericFlag = false)
            => AddCommand(new DelegateCommandInfo((aliases ?? Enumerable.Empty<string>()).Prepend(id).ToArray(), description, @delegate, isGenericFlag: isGenericFlag));
        public CommandEngine AddCommand(CommandInfo info)
        {
            info.ThrowIfInvalid();

            foreach (string id in info.Ids)
            {
                if (_groupMap.ContainsKey(id) && !info.IsMainCommandEligible()) throw new InvalidOperationException($"Command id '{id}' is not valid as main command for group with same name.");
            }

            if (info.HasGroup)
            {
                if (_commands.TryGetValue(info.Group!, out CommandInfo? mainCommandInfo) && !mainCommandInfo.IsMainCommandEligible())
                    throw new InvalidOperationException($"Group cannot be added as command has already been added that cannot be a main command for group '{info.Group!}'.");
            }

            foreach (string id in info.Ids)
                if (!_commands.TryAdd(id, info)) throw new InvalidOperationException($"Command with duplicate key '{id}' found.");

            _uniqueCommands.Add(info.Id, info);

            if (info.HasProvider)
            {
                CommandProvider provider = info.Provider!;

                if (_providers.ContainsKey(provider.Name) && _providers[provider.Name].GetType() != provider.GetType())
                    throw new InvalidOperationException("Command is provided by a provider with a name equal to a different-type already registered provider.");

                _providers.TryAdd(provider.Name, provider);
                _srcMap.TryAdd(provider.Name, new List<string>());

                _srcMap[provider.Name].Add(info.Id);
            }

            if (info.HasGroup)
            {
                _groupMap.TryAdd(info.Group!, new List<string>());
                _groupMap[info.Group!].Add(info.Id);
            }

            return this;
        }

        public CommandEngine RemoveCommand(string id)
        {
            if (!_uniqueCommands.ContainsKey(id)) throw new KeyNotFoundException($"Command with MAIN ID '{id}' was not found.");

            CommandInfo info = _uniqueCommands[id];

            foreach (string aliasId in _uniqueCommands[id].Ids)
            {
                if (info.IsGenericFlag) _commands.Remove(ParseHelpers.GetGenericFlagSignature(aliasId));
                _commands.Remove(aliasId);
            }

            if (info.HasProvider)
            {
                CommandProvider provider = info.Provider!;

                _srcMap[provider.Name].Remove(info.Id);

                if (_srcMap.Count == 0)
                {
                    _srcMap.Remove(provider.Name);
                    _providers.Remove(provider.Name);
                }
            }

            if (info.HasGroup)
            {
                _groupMap[info.Group!].Remove(info.Id);
                if (_groupMap.Count == 0) _groupMap.Remove(info.Group!);
            }

            _uniqueCommands.Remove(id);

            return this;
        }

        public CommandEngine AddProvider(CommandProvider provider)
        {
            CommandInfo[] commands = provider.GetCommands();

            if (commands.Length == 0) throw new InvalidOperationException("Cannot add empty command provider, GetCommands() returned an empty array.");

            foreach (CommandInfo command in commands)
                AddCommand(command);

            return this;
        }

        public CommandEngine RemoveProvider(string name)
        {
            foreach (string id in _srcMap[name])
                RemoveCommand(id);
            _srcMap.Remove(name);
            return this;
        }

        public CommandEngine AddGlobalOption(GlobalOption flagHandler)
        {
            _globalOptions.Add(flagHandler.LongFormId, flagHandler);
            return this;
        }

        public CommandEngine RemoveGlobalOption(string id)
        {
            if (!_globalOptions.Remove(id)) throw new KeyNotFoundException($"FlagHandler with Id '{id}' was not found.");
            return this;
        }

        public CommandEngine AddHook(ActionHook hook)
        {
            _hooks.Add(hook.Id, hook);
            return this;
        }

        public CommandEngine RemoveHook(string id)
        {
            if (!_hooks.Remove(id)) throw new KeyNotFoundException($"ActionHook with Id '{id}' was not found.");
            return this;
        }

        #endregion

        public ArgumentsInfo Parse(string[] args)
        {
            if (args.Length == 0) throw new ArgumentException("No command provided.", nameof(args));

            List<string> currentValues = new List<string>();
            List<Argument> outArgs = new List<Argument>();
            string? currentName = null;
            int position = 0;
            bool noMoreParams = false;
            string token = string.Empty;
            string commandId = args[0];
            bool callsGenericFlag = commandId.StartsWith('-');

            void PushArgument()
            {
                outArgs.Add(new Argument(new ArgumentTarget(currentName), currentValues.ToArray()));
                currentValues.Clear();
            }

            for (int i = 1; i < args.Length; i++)
            {
                token = args[i];

                if (!noMoreParams && token == "--")
                {
                    noMoreParams = true;
                    continue;
                }

                if (!noMoreParams && token.StartsWith("-"))
                {
                    if (currentName is not null)
                    {
                        PushArgument();
                    }

                    currentName = token;
                }
                else
                {
                    if (currentName is null) outArgs.Add(new Argument(new ArgumentTarget(position++), [token]));
                    else currentValues.Add(token);
                }
            }

            if (currentName is not null) PushArgument();

            ArgumentsInfo toret = new ArgumentsInfo(args[0], outArgs.ToArray(), callsGenericFlag);

            if (_groupMap.ContainsKey(commandId) && outArgs.Count > 0)
            {
                toret = Parse(ArrayHelpers.Combine(args[0] + " " + args[1], args[2..]));
            }

            return toret;
        }

        public CommandInfo[] GetProviderCommands(string name)
        {
            List<CommandInfo> providerCommands = new List<CommandInfo>();

            foreach (string id in _srcMap[name])
                if (_uniqueCommands.TryGetValue(id, out CommandInfo? commandInfo))
                    providerCommands.Add((CommandInfo)commandInfo);

            return providerCommands.ToArray();
        }

        public CommandResultInfo Execute(string args) => Execute(ParseHelpers.Split(args));
        public CommandResultInfo Execute(string[] args)
        {
            ArgumentsInfo argsInfo = Parse(args);
            if (Commands.TryGetValue(argsInfo.CommandId, out CommandInfo? commandInfo))
            {
                if (!commandInfo.IsEnabled) throw new InvalidOperationException("Cannot call disabled command.");

                CommandContext context = new CommandContext(this, argsInfo, true, commandInfo);
                object?[] conformed = ParseHelpers.Conform(ref argsInfo, commandInfo.GetOptions(), context);
                GlobalOption[] toRunGlobalOptions = ParseHelpers.ExtractFlags(ref argsInfo, _globalOptions.Values.ToArray());
                List<ActionHook> toRunHooks = new List<ActionHook>(toRunGlobalOptions);
                toRunHooks.AddRange(_hooks.Values);

                if (argsInfo.Arguments.Count > 0) throw new CommandException($"Duplicate or unresolved argument targets found; [{argsInfo.Arguments.ToJoinedString(", ")}].");

                foreach (ActionHook hook in toRunHooks) hook.PreCommand(context);

                object? returned = context.HasFlag(ExecutingContextFlags.PreventCommand) ? DBNull.Value : commandInfo.Invoke(conformed);
                context.PreCommand = false;

                object?[] contextResults = context.RunAll();

                if (contextResults.Length == 1) returned = contextResults[0];
                else if (contextResults.Length > 1) throw new CommandException("Multiple context-action/function returns is invalid.");

                if (!context.HasFlag(ExecutingContextFlags.PreventFlags))
                    foreach (ActionHook hook in toRunHooks)
                    {
                        returned = hook.OnReturned(context, returned);
                        hook.PostCommand(context);
                    }

                return new CommandResultInfo(this, returned);
            }
            else if (argsInfo.CallsGenericFlag)
            {
                if (Commands.ContainsKey(argsInfo.CommandId.TrimStart('-'))) throw new InvalidOperationException($"Command with id '{argsInfo.SanitizedCommandId}' cannot be used as generic flag.");
                throw new InvalidOperationException($"Generic flag with ID '{argsInfo.CommandId}' not found.");
            }
            else throw new InvalidOperationException($"Command with ID '{argsInfo.CommandId}' not found.");
        }

        /// <summary>
        /// Gets the instance of a <see cref="CommandProvider"/> of the specified <typeparamref name="TProvider"/> type.
        /// </summary>
        /// <typeparam name="TProvider">The <see cref="CommandProvider"/> type.</typeparam>
        /// <returns>The instance of a <see cref="CommandProvider"/> of the specified <typeparamref name="TProvider"/> type.</returns>
        public TProvider GetProvider<TProvider>() where TProvider : CommandProvider
            => (TProvider)Providers.Values.First(p => p.GetType() == typeof(TProvider));

        public CommandEngine Populate(
            Assembly? assembly = null,
            Func<Type, bool>? predicate = null,
            Func<Type, CommandProvider>? activator = null)
        {
            activator ??= t => (CommandProvider)Activator.CreateInstance(t)!;

            IEnumerable<Type> types = (assembly ?? Assembly.GetCallingAssembly()).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CommandProvider)));

            if (predicate is not null) types = types.Where(t => predicate.Invoke(t));

            foreach (Type type in types) AddProvider(activator.Invoke(type));

            return this;
        }

        public IEnumerable<CommandInfo> GetEnabledCommands() => UniqueCommands.Values.Where(c => c.IsEnabled);
    }
}
