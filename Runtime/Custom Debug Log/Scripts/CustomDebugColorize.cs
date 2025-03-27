using UnityEngine;

namespace CoreToolkit.Runtime.Custom_Debug_Log.Scripts
{
    public class CustomDebugColorize 
    {
        // Color Example

        public static readonly CustomDebugColorize Red = new CustomDebugColorize(Color.red);
        public static readonly CustomDebugColorize Yellow = new CustomDebugColorize(Color.yellow);
        public static readonly CustomDebugColorize Green = new CustomDebugColorize(Color.green);
        public static CustomDebugColorize Blue = new CustomDebugColorize(Color.blue);
        public static CustomDebugColorize Cyan = new CustomDebugColorize(Color.cyan);
        public static CustomDebugColorize Magenta = new CustomDebugColorize(Color.magenta);

        // Hex Example

        public static CustomDebugColorize Orange = new CustomDebugColorize("#FFA500");
        public static CustomDebugColorize Olive  = new CustomDebugColorize("#808000");
        public static CustomDebugColorize Purple  = new CustomDebugColorize("#800080");
        public static CustomDebugColorize DarkRed  = new CustomDebugColorize("#8B0000");
        public static CustomDebugColorize DarkGreen  = new CustomDebugColorize("#006400");
        public static CustomDebugColorize DarkOrange  = new CustomDebugColorize("#FF8C00");
        public static CustomDebugColorize Gold  = new CustomDebugColorize("#FFD700");
        public static CustomDebugColorize RedNice  = new CustomDebugColorize("#f54242");

        private readonly string _prefix;

        private const string Suffix = "</color>";

        // Convert Color to HtmlString
        private CustomDebugColorize(Color color){
            _prefix = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
        }
        // Use Hex Color
        private CustomDebugColorize(string hexColor)
        {
            _prefix = $"<color={hexColor}>";
        }

        public static string operator %(string text, CustomDebugColorize color){
            return color._prefix + text + Suffix;
        }
        
        /*Debug.Log("Hello Green" % Colorize.Green);
        Debug.Log("Hello Bold Green" % Colorize.Green % FontFormat.Bold);

        Debug.Log("Hello Orange" % Colorize.Orange);
        Debug.Log("Hello Italic Orange" % Colorize.Orange % FontFormat.Italic);

        Debug.Log(
        "Failure " % Colorize.Orange % FontFormat.Bold +
        "doesn't mean the " +            
        "game is over " % Colorize.Red % FontFormat.Bold +
        "It means try again with " +
        "experience!" % Colorize.Green % FontFormat.Bold
        );*/

    }
}
