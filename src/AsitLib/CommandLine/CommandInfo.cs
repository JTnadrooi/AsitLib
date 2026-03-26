using AsitLib.Diagnostics;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;

namespace AsitLib.CommandLine
{
    public class DelegateCommandInfo : MethodCommandInfo
    {
        public Delegate Delegate { get; }

        public DelegateCommandInfo(ReadOnlySpan<string> ids, string description, Delegate @delegate)
            : base(ids, description, @delegate.Method)
        {
            Target = @delegate.Target;
            Delegate = @delegate;
        }
    }

    public class MethodCommandInfo : CommandInfo
    {
        public MethodInfo MethodInfo { get; }
        public object? Target { get; init; }
        public bool IsVoid => MethodInfo.ReturnType == typeof(void);

        public MethodCommandInfo(ReadOnlySpan<string> ids, string description, MethodInfo methodInfo)
            : base(ids, description)
        {
            MethodInfo = methodInfo;

            if (methodInfo.ReturnType == typeof(DBNull)) throw new ArgumentException("Source MethodInfo cannot return use type DBNull, use void instead. Use object if the method may return void, or a return value.", nameof(methodInfo));
        }

        public static MethodCommandInfo FromProvider(ReadOnlySpan<string> ids, string description, MethodInfo methodInfo, CommandProvider provider)
            => new MethodCommandInfo(ids, description, methodInfo)
            {
                Target = provider,
            };

        public static MethodCommandInfo FromMethod(MethodInfo methodInfo, object? target = null, bool autoProvider = true)
            => FromMethodImpl(methodInfo, autoProvider ? target as CommandProvider : null, target);
        public static MethodCommandInfo FromMethod(MethodInfo methodInfo, CommandProvider provider)
            => FromMethodImpl(methodInfo, provider, provider);
        public static MethodCommandInfo FromMethod(MethodInfo methodInfo, CommandProvider provider, object? target)
            => FromMethodImpl(methodInfo, provider, target);

        private static MethodCommandInfo FromMethodImpl(MethodInfo methodInfo, CommandProvider? provider, object? target)
        {
            CommandAttribute? attribute = methodInfo.GetCustomAttribute<CommandAttribute>();

            if (attribute is null) throw new ArgumentException("Source method does not have a CommandAttribute derived attribute.", nameof(methodInfo));

            string cmdId;
            switch (provider)
            {
                case CommandGroup g:
                    if (g.NameOfMainMethod == methodInfo.Name) cmdId = g.Name;
                    else cmdId = $"{provider.Name} {(attribute.Id ?? ParseHelpers.GetSignature(methodInfo))}";
                    break;
                default:
                    cmdId = attribute.Id ?? ParseHelpers.GetSignature(methodInfo);
                    break;
            }

            List<string> commandIds = new List<string>([cmdId]);
            commandIds.AddRange(attribute.Aliases);

            return new MethodCommandInfo(commandIds.ToArray(), attribute.Description, methodInfo)
            {
                Provider = provider,
                Target = target,
            };
        }

        public override OptionInfo[] GetOptions() => MethodInfo.GetParameters().Select(p => new OptionInfo(p)).ToArray();
        public override object? Invoke(object?[] parameters)
        {
            object? result = MethodInfo.Invoke(Target, parameters); // so always runs.

            return IsVoid ? DBNull.Value : result;
        }
    }

    /// <summary>
    /// Represents info for a specific command.
    /// </summary>
    public abstract class CommandInfo
    {
        /// <summary>
        /// Gets a <see cref="HashSet{T}"/> containing all strings this command belongs to. This includes generic flags, aliasses, etc.
        /// </summary>
        public ImmutableArray<string> Ids { get; }

        /// <summary>
        /// Gets all groups this command is in.
        /// </summary>
        public ImmutableArray<string> Groups { get; }

        /// <summary>
        /// Gets if the command represented by this instance has aliases.
        /// <code>Ids.Length > 1</code>
        /// </summary>
        public bool HasAliases => Ids.Length > 1;

        /// <summary>
        /// Gets the main id. This will be the first entry from the <see cref="Ids"/> array.
        /// </summary>
        public string Id => Ids[0];

        /// <summary>
        /// Gets the group this command belongs to or <see langword="null"/> if the command is ungrouped.
        /// </summary>
        public string? Group { get; }

        public CommandProvider? Provider { get; init; }

        /// <summary>
        /// <inheritdoc cref="CommandAttribute.Description" path="/summary"/>
        /// </summary>
        public string Description { get; }

        public bool IsEnabled { get; set; }

        public CommandInfo(ReadOnlySpan<string> ids, string description)
        {
            ArgumentNullException.ThrowIfNull(description);

            if (ids.Length == 0) throw new ArgumentException("Command must have at least one id.");

            HashSet<string> seenIds = new HashSet<string>();
            HashSet<string> seenGroups = new HashSet<string>();
            bool validGroup = true;

            foreach (string id in ids)
            {
                ThrowHelpers.ThrowIfInvalidCommandId(id);

                if (!seenIds.Add(id)) throw new ArgumentException("Duplicate command id's are invalid.");

                CommandIdentifier commandIdInfo = new CommandIdentifier(id);

                if (commandIdInfo.GetGroups().Length > 0)
                {
                    if (commandIdInfo.GetGroups().Length > 0)
                        for (int i = 0; i < commandIdInfo.GetGroups().Length; i++)
                            seenGroups.Add(commandIdInfo.GetGroups()[i]);
                }
                else validGroup = false;
            }

            if (validGroup && seenGroups.Count == 1)
            {
                Group = seenGroups.Single();
            }
            else
                Group = null;

            Groups = seenGroups.ToImmutableArray();

            Ids = ids.ToImmutableArray();
            Description = description;
            IsEnabled = true;
        }

        public abstract object? Invoke(object?[] parameters);

        /// <summary>
        /// When overridden, returns the options supported by this command.
        /// </summary>
        /// <returns>
        /// An array of <see cref="OptionInfo"/> instances describing the available options.
        /// The order of the options determines their positional mapping for arguments
        /// that use <see cref="ArgumentTarget.Index"/>.
        /// </returns>
        public abstract OptionInfo[] GetOptions();

        public virtual string GetHelpString()
        {
            StringBuilder sb = new StringBuilder(Id).Append(" ");
            OptionInfo[] options = GetOptions();

            if (HasAliases) sb.Append($"[{Ids.Skip(1).ToJoinedString(", ")}] ");

            if (options.Length != 0)
            {
                string optionsString = GetOptions().Select(p =>
                    $"{p.OptionType.Name.ToLower()}:{p.Id!.ToLower()}{(p.HasDefaultValue ? $"(default_value:{p.DefaultValue?.ToString() ?? StringHelpers.NULL_STRING}) " : " ")}")
                    .ToJoinedString();

                sb.Append(optionsString);
            }

            sb.Append($"# {Description}");

            return sb.ToString();
        }

        public override string ToString() => $"{{Id: {Id}, Desc: {Description}}}";
    }
}


