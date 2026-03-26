namespace AsitLib.Tests
{
    public class AlwaysReturnTestGlobalOptionHandler : GlobalOption
    {
        public AlwaysReturnTestGlobalOptionHandler() : base(OptionInfo.FromType(typeof(string), ["ret-test", "t"], implicitValue: "TEST"), "desc.")
        {

        }

        protected override object? OnReturned(CommandContext context, object? optionValue, object? returned)
        {
            return optionValue;
        }
    }
}
