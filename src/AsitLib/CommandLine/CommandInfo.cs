using AsitLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AsitLib.CommandLine
{
    public class DelegateCommandInfo : MethodCommandInfo
    {
        public Delegate Delegate { get; }

        public DelegateCommandInfo(string[] ids, string description, Delegate @delegate, bool isGenericFlag = false) : base(ids, description, @delegate.Method, @delegate.Target, isGenericFlag: isGenericFlag)
        {
            Delegate = @delegate;
        }
    }

    public class MethodCommandInfo : CommandInfo
    {
        public MethodInfo MethodInfo { get; }
        public object? Target { get; }
        public bool IsVoid => MethodInfo.ReturnType == typeof(void);

        public MethodCommandInfo(string[] ids, string description, MethodInfo methodInfo, CommandProvider provider, bool isGenericFlag = false)
            : this(ids, description, methodInfo, provider, isGenericFlag: isGenericFlag, provider: provider) { }
        public MethodCommandInfo(string[] ids, string description, MethodInfo methodInfo, object? target = null, bool isGenericFlag = false, CommandProvider? provider = null)
            : base(ids, description, isGenericFlag: isGenericFlag, provider: provider)
        {
            MethodInfo = methodInfo;
            Target = target;

            if (methodInfo.ReturnType == typeof(DBNull)) throw new ArgumentException("Source MethodInfo cannot return use type DBNull, use void instead. Use object if the method may return void, or a return value.", nameof(methodInfo));
        }

        public override OptionInfo[] GetOptions() => MethodInfo.GetParameters().Select(p => p.ToOptionInfo()).ToArray();
        public override object? Invoke(object?[] parameters)
        {
            object? result = MethodInfo.Invoke(Target, parameters); // so always runs.

            return IsVoid ? DBNull.Value : result;
        }
    }

    public class CommandGroupCommandInfo : MethodCommandInfo
    {
        /// <summary>
        /// Gets a value indicating if this command is the main command. Main commands inherit their <see cref="CommandInfo.Id"/> from the source <see cref="CommandGroup.Name"/>.
        /// </summary>
        public bool IsMain { get; }

        public CommandGroup SourceGroup => (CommandGroup)base.Provider!;

        [Obsolete($"Use the {nameof(SourceGroup)} property instead.")]
        public new object? Provider => base.Provider;

        [Obsolete($"Use the {nameof(SourceGroup)} property instead.")]
        public new object? Target => base.Target;

        public CommandGroupCommandInfo(string[] ids, CommandAttribute attribute, MethodInfo methodInfo, CommandGroup sourceGroup)
            : base(ids, attribute.Description, methodInfo, sourceGroup, attribute.IsGenericFlag)
        {
            IsMain = sourceGroup.NameOfMainMethod == methodInfo.Name;
        }
    }

    /// <summary>
    /// Represents info for a specific command. See <see cref="CommandAttribute"/>.
    /// </summary>
    public abstract class CommandInfo : IValidatable
    {
        /// <summary>
        /// Gets a <see cref="HashSet{T}"/> containing all command ids. Includes <see cref="Id"/>.
        /// </summary>
        public ReadOnlyCollection<string> Ids { get; }
        public bool HasAliases => Ids.Count > 1;
        public string Id => Ids[0];

        public string? Group { get; }

        public bool HasGroup => Group is not null;

        public CommandProvider? Provider { get; }

        public bool HasProvider => Provider is not null;
        /// <summary>
        /// <inheritdoc cref="CommandAttribute.Description" path="/summary"/>
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets if the command can be used as a generic flag. 
        /// If <see langword="true"/>, the command can be invoked by prepending any of the <see cref="Ids"/> with a single dash for <see cref="Ids"/> consisting of a single character, or with two dashes for longer <see cref="Ids"/>.
        /// </summary>
        public bool IsGenericFlag { get; }

        public bool IsEnabled { get; set; }

        public CommandInfo(string[] ids, string description, bool isGenericFlag = false, CommandProvider? provider = null)
        {
            if (ids.Length == 0) throw new InvalidOperationException("Command must have at least one id.");

            string? group = null;
            string mainId = ids[0];
            List<string> additionalIds = new List<string>();

            foreach (string id in ids)
            {
                ParseHelpers.ThrowIfInvalidName(id, true, "Command Id");

                switch (id.Count(c => c == ' '))
                {
                    case 0: break;
                    case 1:
                        string idGroup = id.Split(' ')[0];
                        group ??= idGroup;
                        if (group != idGroup) throw new InvalidOperationException($"Command aliasses for command '{mainId}' use differing groups.");
                        break;
                    case > 1:
                        if (id.Contains("  ")) throw new InvalidOperationException($"Command cannot have double spaces; '{id}'.");
                        else throw new InvalidOperationException($"Command nested subgroups are invalid.");
                }
                if (isGenericFlag) additionalIds.Add(ParseHelpers.GetGenericFlagSignature(id));
            }

            Ids = ids.Concat(additionalIds).ToArray().AsReadOnly();
            Description = description;
            IsGenericFlag = isGenericFlag;
            Group = group;
            Provider = provider;
            IsEnabled = true;
        }

        public abstract object? Invoke(object?[] parameters);
        public abstract OptionInfo[] GetOptions();

        public virtual InvalidReason[] GetInvalidReasons()
        {
            List<InvalidReason> invalidReasons = new List<InvalidReason>();

            if (IsGenericFlag && GetOptions().Count(o => !o.HasDefaultValue) > 0) invalidReasons.Add("Generic flags are not supported for commands with required options.");

            return invalidReasons.ToArray();
        }

        public virtual string GetHelpString()
        {
            StringBuilder sb = new StringBuilder(Id).Append(" ");
            OptionInfo[] options = GetOptions();

            if (HasAliases) sb.Append($"[{Ids.Skip(1).ToJoinedString(", ")}] ");

            if (options.Length != 0)
            {
                string optionsString = GetOptions().Select(p =>
                    $"{p.OptionType.Name.ToLower()}:{p.Name!.ToLower()}{(p.HasDefaultValue ? $"(default_value:{p.DefaultValue?.ToString() ?? StringHelpers.NULL_STRING}) " : " ")}")
                    .ToJoinedString();

                sb.Append(optionsString);
            }

            sb.Append($"# {Description}");

            return sb.ToString();
        }

        public bool IsMainCommandEligible() => GetOptions().Length == 0;

        public override string ToString() => $"{{Id: {Id}, Desc: {Description}}}";
    }
}
