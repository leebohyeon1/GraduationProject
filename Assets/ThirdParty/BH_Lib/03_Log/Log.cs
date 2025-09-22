
using UnityEngine;

namespace BH_Lib.Log
{

    public static class Log
    {
        public static bool LogOn = true;

        public static void PrintColor(Color color, params object[] args)
        {
            if (!LogOn)
            {
                return;
            }
#if (DEBUG_MODE)

            string message = string.Join(string.Empty, args);
            Debug.Log("<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + message + "</color>");
            #endif
        }

        public static void Print(params object[] args)
        {
            if (!LogOn)
            {
                return;
            }
#if (DEBUG_MODE)
            Debug.Log(string.Join(string.Empty, args));
            #endif
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
