using AsitLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public MethodCommandInfo(string[] ids, string description, MethodInfo methodInfo, object? target = null, bool isGenericFlag = false)
            : base(ids, description, isGenericFlag: isGenericFlag)
        {
            MethodInfo = methodInfo;
            Target = target;
        }

        public override OptionInfo[] GetOptions() => MethodInfo.GetParameters().Select(p => p.ToOptionInfo()).ToArray();
        public override object? Invoke(object?[] parameters) => MethodInfo.Invoke(Target, parameters);
    }

    public class ProviderCommandInfo : MethodCommandInfo
    {
        /// <summary>
        /// Gets a value indicating if this command is the main command. Main commands inherit their <see cref="Id"/> from the source <see cref="CommandProvider.FullNamespace"/>.
        /// </summary>
        public bool IsMain { get; }

        public CommandProvider Provider => (CommandProvider)base.Target!;

        [Obsolete($"Use the {nameof(Provider)} property instead.")]
        public new object? Target => base.Target;

        public ProviderCommandInfo(string[] ids, CommandAttribute attribute, MethodInfo methodInfo, CommandProvider provider) : base(ids, attribute.Description, methodInfo, provider, attribute.IsGenericFlag)
        {
            IsMain = attribute.IsMain;
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

        /// <summary>
        /// <inheritdoc cref="CommandAttribute.Description" path="/summary"/>
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets if the command can be used as a generic flag. 
        /// If <see langword="true"/>, the command can be invoked by prepending any of the <see cref="Ids"/> with a single dash for <see cref="Ids"/> consisting of a single character, or with two dashes for longer <see cref="Ids"/>.
        /// </summary>
        public bool IsGenericFlag { get; }

        public bool IsEnabled { get; }

        public CommandInfo(string[] ids, string description, bool isGenericFlag = false, bool isEnabled = true)
        {
            if (ids.Length == 0) throw new InvalidOperationException("Command must have at least one id.");

            Ids = ids.AsReadOnly();
            Description = description;
            IsGenericFlag = isGenericFlag;

            IsEnabled = isEnabled;
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
                    $"{p.Type.Name.ToLower()}:{p.Name!.ToLower()}{(p.HasDefaultValue ? $"(default_value:{p.DefaultValue?.ToString() ?? StringHelpers.NULL_STRING}) " : " ")}")
                    .ToJoinedString();

                sb.Append(optionsString);
            }

            sb.Append($"# {Description}");

            return sb.ToString();
        }

        public override string ToString() => $"{{Id: {Id}, Desc: {Description}}}";
    }
}
