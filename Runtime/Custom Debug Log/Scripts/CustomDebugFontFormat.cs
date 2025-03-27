namespace CoreToolkit.Runtime.Custom_Debug_Log.Scripts
{
    public class CustomDebugFontFormat 
    {
        private readonly string _prefix;

        private readonly string _suffix;

        public static readonly CustomDebugFontFormat Bold = new CustomDebugFontFormat("b");
        public static readonly CustomDebugFontFormat Italic = new CustomDebugFontFormat("i");
        private CustomDebugFontFormat(string format)
        {
            _prefix = $"<{format}>";
            _suffix = $"</{format}>";
        }

        public static string operator %(string text, CustomDebugFontFormat textFormat)
        {
            return textFormat._prefix + text + textFormat._suffix;
        }
    }
}
