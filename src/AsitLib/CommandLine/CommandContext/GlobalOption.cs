namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents a global option listener.
    /// </summary>
    public abstract class GlobalOption : ActionHook
    {
        public string Description { get; }

        public OptionInfo Option { get; }

        protected GlobalOption(OptionInfo option, string description, string? id = null) : base(id ?? option.Id)
        {
            ArgumentNullException.ThrowIfNull(option);
            ArgumentNullException.ThrowIfNullOrEmpty(description);

            if ((option.PassingPolicies & OptionPassingPolicies.Named) == 0) throw new ArgumentException("PassingPolicies does not include OptionPassingPolicies.Named.", nameof(option));
            if (option.Id == OptionInfo.IdForUnnamedOptions) throw new ArgumentException("Missing Id.", nameof(option));

            Description = description;
            Option = option;
        }
    }

    [Flags]
    public enum ExecutingContextFlags
    {
        None = 0,
        PreventCommand = 1,
        PreventFlags = 2,
        FullStop = PreventCommand | PreventFlags,
    }
}
