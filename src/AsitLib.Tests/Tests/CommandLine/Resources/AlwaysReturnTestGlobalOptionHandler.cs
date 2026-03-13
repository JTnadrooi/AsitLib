namespace AsitLib.Tests
{
    public class AlwaysReturnTestGlobalOptionHandler : GlobalOption
    {
        public AlwaysReturnTestGlobalOptionHandler() : base("ret-test", "desc", "t", OptionInfo.FromType(typeof(string), implicitValue: "TEST"))
        {

        }

        public override object? OnReturned(CommandContext context, object? returned)
        {
            if (context.TryGetGlobalOptionValue<string>(this, out string? value)) return value;
            else return returned;
        }
    }
}
