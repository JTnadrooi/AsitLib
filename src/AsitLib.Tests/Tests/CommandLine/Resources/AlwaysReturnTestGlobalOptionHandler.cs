namespace AsitLib.Tests
{
    public class AlwaysReturnTestGlobalOptionHandler : GlobalOption
    {
        public AlwaysReturnTestGlobalOptionHandler() : base(OptionInfo.FromType(typeof(string), ["ret-test", "t"], implicitValue: "TEST"), "desc.")
        {

        }

        public override object? OnReturned(CommandContext context, object? returned)
        {
            return context.GetGlobalOptionValue<string>(this);
        }
    }
}
