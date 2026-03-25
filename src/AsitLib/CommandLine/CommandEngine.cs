using AsitLib.Diagnostics;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

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
        public ICommandInfoFactory? DefaultInfoFactory { get; set; }

        private readonly Dictionary<string, List<string>> _providerMap;
        private readonly Dictionary<string, List<string>> _groupMap;
        private readonly Dictionary<string, GlobalOption> _globalOptions;
        private readonly Dictionary<string, CommandProvider> _providers;
        private readonly Dictionary<string, CommandInfo> _commands;
        private readonly Dictionary<string, CommandInfo> _uniqueCommands;
        private readonly Dictionary<string, ActionHook> _hooks;

        public CommandInfo this[string id] => Commands[id];

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

            DefaultInfoFactory = null;

            NewLine = "\n";
            KeyValueSeperator = "=";

            _providerMap = new Dictionary<string, List<string>>();

            AddCommand((string? commandId = null) =>
            {
                StringBuilder sb = new StringBuilder();
                if (commandId is not null) return Commands[commandId].GetHelpString();
                else
                    foreach (CommandInfo cmd in GetEnabledCommands().OrderBy(c => c.Id)) sb.Append(cmd.GetHelpString()).Append(NewLine);

                sb.Length -= NewLine.Length;

                return sb.ToString();
            }, "help", "Prints help.", ["?", "h"]);

            AddGlobalOption(new HelpGlobalOption());
        }

        #region ADD_REMOVE

        //public CommandEngine AddCommand(MethodInfo method, string description, string[]? aliases = null)
        //    => AddCommand(MethodCommandInfo);
        public CommandEngine AddCommand(Delegate @delegate, string id, string description, string[]? aliases = null)
            => AddCommand(new DelegateCommandInfo((aliases ?? Enumerable.Empty<string>()).Prepend(id).ToArray(), description, @delegate));
        public CommandEngine AddCommand(CommandInfo info)
        {
            foreach (string id in info.Ids)
                if (!_commands.TryAdd(id, info)) throw new InvalidOperationException($"Command with duplicate key '{id}' found.");

            _uniqueCommands.Add(info.Id, info);

            if (info.Provider is not null)
            {
                CommandProvider provider = info.Provider;

                if (_providers.ContainsKey(provider.Name) && _providers[provider.Name].GetType() != provider.GetType())
                    throw new InvalidOperationException("Command is provided by a provider with a name equal to a different-type already registered provider.");

                _providers.TryAdd(provider.Name, provider);
                _providerMap.TryAdd(provider.Name, new List<string>());

                _providerMap[provider.Name].Add(info.Id);
            }

            foreach (string group in info.Groups)
            {
                _groupMap.TryAdd(group!, new List<string>());
                _groupMap[group!].Add(info.Id);
            }

            return this;
        }

        public CommandEngine RemoveCommand(string id)
        {
            if (!_commands.ContainsKey(id)) throw new InvalidOperationException($"Command with Id '{id}' was not found.");

            CommandInfo commandToRemove = _commands[id];

            foreach (string commandId in commandToRemove.Ids)
            {
                _commands.Remove(commandId);
            }

            if (commandToRemove.Provider is not null)
            {
                CommandProvider provider = commandToRemove.Provider;

                _providerMap[provider.Name].Remove(commandToRemove.Id);

                if (_providerMap[provider.Name].Count == 0)
                {
                    _providerMap.Remove(provider.Name);
                    _providers.Remove(provider.Name);
                }
            }

            foreach (string group in commandToRemove.Groups)
            {
                _groupMap[group].Remove(commandToRemove.Id);
                if (_groupMap[group].Count == 0) _groupMap.Remove(group);
            }

            _uniqueCommands.Remove(commandToRemove.Id);

            return this;
        }

        public CommandEngine AddProvider(CommandProvider provider)
        {
            CommandInfo[] commands = provider.GetCommands(DefaultInfoFactory);

            if (commands.Length == 0) throw new InvalidOperationException("Cannot add empty command provider, GetCommands() returned an empty array.");

            foreach (CommandInfo command in commands)
                AddCommand(command);

            return this;
        }

        public CommandEngine RemoveProvider(string name)
        {
            foreach (string id in _providerMap[name])
                RemoveCommand(id);
            _providerMap.Remove(name);
            return this;
        }

        public CommandEngine AddGlobalOption(GlobalOption globalOption)
        {
            _globalOptions.Add(globalOption.Id, globalOption);
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

        //public CallInfo Parse(string args) => Parse(ParseHelpers.GetTokens(args));

        /// <summary>
        /// Parses a array of tokens to a <see cref="CallInfo"/> call. There is no guarantee the call arguments will be valid but the target command is guaranteed to exist and be enabled.
        /// See also; <see cref="ParseHelpers.GetTokens(string)"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="CommandException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public CallInfo Parse(string[] tokens)
        {
            if (tokens.Length == 0) throw new CommandException("No command provided.");
            if (_commands.Count == 0) throw new InvalidOperationException("Cannot parse for engine without commands.");

            string commandId = tokens[0];

            if (!_commands.ContainsKey(commandId))
            {
                if (_groupMap.ContainsKey(commandId) && tokens.Length > 1)
                {
                    return Parse(ArrayHelpers.Combine($"{tokens[0]} {tokens[1]}", tokens[2..]));
                }

                CallInfo tempCallInfo = new CallInfo(commandId, Array.Empty<Argument>());

                if (tempCallInfo.CallsGenericFlag)
                {
                    if (Commands.ContainsKey(tempCallInfo.CommandId.TrimStart('-')))
                        throw new InvalidOperationException($"Command with id '{tempCallInfo.SanitizedCommandId}' cannot be used as generic flag.");
                    else
                        throw new CommandNotFoundException($"Generic flag '{tempCallInfo.CommandId}' not found.", tempCallInfo.CommandId);
                }
                else
                    throw new CommandNotFoundException($"Command '{tempCallInfo.CommandId}' not found.", tempCallInfo.CommandId);
            }
            else if (!_commands[commandId].IsEnabled)
                throw new CommandException("Cannot call disabled command.");

            CommandInfo targetCommand = _commands[commandId];

            List<string> currentTokens = new List<string>();
            List<Argument> outArguments = new List<Argument>();
            string? currentOptionName = null;
            int argumentIndex = 0;
            bool acceptNamedParams = true;
            OptionInfo[] options = targetCommand.GetOptions();

            void PushPositionalArgument()
            {
                outArguments.Add(new Argument(new ArgumentTarget(argumentIndex), currentTokens.ToArray()));
                currentTokens.Clear();

                argumentIndex++;
            }

            void PushNamedArgument()
            {
                Debug.Assert(currentOptionName is not null);

                outArguments.Add(new Argument(new ArgumentTarget(currentOptionName!), currentTokens.ToArray()));
                currentTokens.Clear();
            }

            for (int tokenIndex = 1; tokenIndex < tokens.Length; tokenIndex++)
            {
                string token = tokens[tokenIndex];

                if (_groupMap.ContainsKey($"{tokens[0]} {tokens[1]}") || _commands.ContainsKey($"{tokens[0]} {tokens[1]}"))
                {
                    return Parse(ArrayHelpers.Combine($"{tokens[0]} {tokens[1]}", tokens[2..]));
                }

                if (acceptNamedParams && token == "--")
                {
                    acceptNamedParams = false;
                    continue;
                }

                if (acceptNamedParams && token.StartsWith("-"))
                {
                    if (currentOptionName is not null)
                    {
                        PushNamedArgument();
                    }
                    else if (currentTokens.Count > 0) PushPositionalArgument();

                    currentOptionName = token;
                }
                else
                {
                    if (currentOptionName is null)
                    {
                        currentTokens.Add(token);

                        if (!options[argumentIndex].OptionType.IsArray)
                        {
                            PushPositionalArgument();
                        }
                    }
                    else
                    {
                        currentTokens.Add(token);
                    }
                }
            }

            if (currentOptionName is not null)
            {
                PushNamedArgument();
            }
            else if (currentTokens.Count > 0) PushPositionalArgument();

            CallInfo result;

            if (_groupMap.ContainsKey(commandId) && outArguments.Count > 0 && outArguments[0].CanTargetSubcommand && Commands.ContainsKey($"{tokens[0]} {tokens[1]}"))
            {
                result = Parse(ArrayHelpers.Combine($"{tokens[0]} {tokens[1]}", tokens[2..]));
            }
            else
            {
                result = new CallInfo(tokens[0], outArguments.ToArray());
            }

            return result;
        }

        public CommandInfo[] GetProviderCommands(string name)
        {
            List<CommandInfo> providerCommands = new List<CommandInfo>();

            foreach (string id in _providerMap[name])
                if (_uniqueCommands.TryGetValue(id, out CommandInfo? commandInfo))
                    providerCommands.Add((CommandInfo)commandInfo);

            return providerCommands.ToArray();
        }

        public CommandResult Execute(string args) => Execute(ParseHelpers.GetTokens(args));
        public CommandResult Execute(string[] args)
        {
            CallInfo call = Parse(args);
            CommandInfo commandInfo = _commands[call.CommandId];

            CommandContext context = new CommandContext(this, call, true, commandInfo);
            object?[] conformed = ParseHelpers.Conform(ref call, commandInfo.GetOptions(), context);

            GlobalOption[] toRunGlobalOptions = ParseHelpers.ExtractGlobalOptions(ref call, _globalOptions.Values.ToArray());
            List<ActionHook> toRunHooks = new List<ActionHook>(toRunGlobalOptions.Concat(_hooks.Values));

            if (call.Arguments.Length > 0) throw new CommandArgumentException($"Duplicate or unresolved argument targets found; [{call.Arguments.ToJoinedString(", ")}].");

            foreach (ActionHook hook in toRunHooks) hook.PreCommand(context);

            object? returned = context.HasFlag(ExecutingContextFlags.PreventCommand) ? DBNull.Value : commandInfo.Invoke(conformed);
            context.PreCommand = false;

            object? contextResult = context.RunAll();

            if (contextResult is not DBNull)
            {
                returned = contextResult;
            }

            if (!context.HasFlag(ExecutingContextFlags.PreventFlags))
                foreach (ActionHook hook in toRunHooks)
                {
                    returned = hook.OnReturned(context, returned);
                    hook.PostCommand(context);
                }

            return new CommandResult(this, returned);
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
