namespace PropertyValidator.Test.Helpers
{
    public static class Debug
    {
        public static void Log(string message, params string?[] args)
        {
            var formattedMessage = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine(formattedMessage);
        }
    }
}
