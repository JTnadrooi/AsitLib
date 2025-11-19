using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class FunctionCommandInfo : CommandInfo
    {
        public Func<object?> Function { get; }

        public FunctionCommandInfo(string[] ids, string description, Func<object?> func) : base(ids, description)
        {
            Function = func;
        }

        public override ParameterInfo[] GetParameters() => Array.Empty<ParameterInfo>();
        public override object? Invoke(object?[]? parameters) => Function.Invoke();
    }

    public class MethodCommandInfo : CommandInfo
    {
        /// <summary>
        /// Gets the <see cref="System.Reflection.MethodInfo"/> this <see cref="CommandInfo"/> is created from.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        public object? Object { get; }

        public MethodCommandInfo(string[] ids, string description, MethodInfo methodInfo, object? @object = null) : base(ids, description)
        {
            MethodInfo = methodInfo;
            Object = @object;
        }

        public override ParameterInfo[] GetParameters() => MethodInfo.GetParameters();
        public override object? Invoke(object?[]? parameters) => MethodInfo.Invoke(Object, parameters);
    }

    public class ProviderCommandInfo : MethodCommandInfo
    {
        /// <summary>
        /// Gets a value indicating if this command is the main command. Main commands inherit their <see cref="Id"/> from the source <see cref="CommandProvider.FullNamespace"/>.
        /// </summary>
        public bool IsMain { get; }

        public CommandProvider Provider => (CommandProvider)Object!;

        public ProviderCommandInfo(string[] ids, CommandAttribute attribute, MethodInfo methodInfo, CommandProvider provider) : base(ids, attribute.Description, methodInfo, provider)
        {
            IsMain = attribute.IsMain;
        }
    }

    /// <summary>
    /// Represents info for a specific command. See <see cref="CommandAttribute"/>.
    /// </summary>
    public abstract class CommandInfo
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

        public CommandInfo(string[] ids, string description)
        {
            Ids = ids.AsReadOnly();
            Description = description;
        }

        public abstract object? Invoke(object?[]? parameters);
        public abstract ParameterInfo[] GetParameters();

        //public override string ToString() => $"{{Id: {Id}, Method: {MethodInfo}, Provider: {Provider?.ToString()}}}";
    }
}
