using DSharpPlus;
using Humanizer;
using System;

namespace ProjectIndigoPlus.Modules.HelperModule
{
    public class EmojiHelper
    {
        /// <summary>
        /// Only works from 1-9
        /// </summary>
        public static string EmojiStringFromNumber(int num)
        {
            switch (num)
            {
                case 1:
                    return ":one:";
                case 2:
                    return ":two:";
                case 3:
                    return ":three:";
                case 4:
                    return ":four:";
                case 5:
                    return ":five:";
                case 6:
                    return ":six:";
                case 7:
                    return ":seven:";
                case 8:
                    return ":eight:";
                case 9:
                    return ":nine:";
                }
            return "";
        }

        /// <summary>
        /// Turns a number in words into it's integer equivalent. ONLY WORKS FROM 1 - 8
        /// </summary>
        /// <param name="v"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool TryParseString(string v, out int i)
        {
            switch(v)
            {
                case "one":
                    i = 1;
                    return true;
                case "two":
                    i = 2;
                    return true;
                case "three":
                    i = 3;
                    return true;
                case "four":
                    i = 4;
                    return true;
                case "five":
                    i = 5;
                    return true;
                case "six":
                    i = 6;
                    return true;
                case "seven":
                    i = 7;
                    return true;
                case "eight":
                    i = 8;
                    return true;
            }
            i = 0;
            return false;
        }
    }
}
