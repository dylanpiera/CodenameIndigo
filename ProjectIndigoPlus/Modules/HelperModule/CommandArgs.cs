using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectIndigoPlus.Modules.HelperModule
{
    public static class CommandArgs
    {
        public static Dictionary<ArgType, string> ReadArgs(string[] args)
        {
            Dictionary<ArgType, string> output = new Dictionary<ArgType, string>();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case string s when s.StartsWith("--f"):
                        if (!string.IsNullOrEmpty(args.ElementAt(i + 1)))
                        {
                            i++;
                        }
                            output.Add(ArgType.Force, "true");
                        continue;
                }
            }
            return output;
        }

        public static Dictionary<string, string> ReadArgsString(string[] args)
        {
            return null;
        }
    }

    public enum ArgType
    {
        Force,
    }
}
