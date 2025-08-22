
using UnityEngine;

namespace BH_Lib.Log
{

    public static class Log
    {
        public static bool LogOn = true;
     
        public static void ColorLog(Color color, params object[] args)
        {
            if (!LogOn) 
            {
                return; 
            }

            string message = string.Join(string.Empty, args);
            Debug.Log("<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + message + "</color>");
        }

        public static void Print(params object[] args)
        {
            if (!LogOn)
            {
                return;
            }

            Debug.Log(string.Join(string.Empty, args));
        }

        public static void PrintErr(params object[] args)
        {
            if (!LogOn)
            {
                return;
            }

            Debug.LogError(string.Join(string.Empty, args));
        }

        public static void PrintWarning(params object[] args)
        {
            if (!LogOn)
            {
                return;
            }

            Debug.LogWarning(string.Join(string.Empty, args));
        }
        
    }
}
