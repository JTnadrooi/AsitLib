namespace AsitLib.Tests
{
    public class DummyCommandInfo : CommandInfo
    {
        public OptionInfo[] Options { get; }

        public DummyCommandInfo(string id, string? description = null, OptionInfo[]? options = null)
            : this(new[] { id }, description, options) { }
        public DummyCommandInfo(string[] ids, string? description = null, OptionInfo[]? options = null)
            : base(ids, description ?? "No desc.")
        {
            Options = options ?? Array.Empty<OptionInfo>();
        }

        public override OptionInfo[] GetOptions() => Options;
        public override object? Invoke(object?[] parameters) => DBNull.Value;
    }
}
