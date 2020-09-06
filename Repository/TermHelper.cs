using Models;
using System;

namespace Helpers
{
    public static class TermHelper
    {
        public static string GetAction(string status)
        {
            var tokenStatus = (TokenStatusEnume)Enum.Parse(typeof(TokenStatusEnume), status);
            switch (tokenStatus)
            {
                case TokenStatusEnume.InQueue:
                    return "Ready to serve";
                case TokenStatusEnume.InCounter:
                    return "Complete";
                case TokenStatusEnume.Served:
                    return "Complete";
                default:
                    return string.Empty;
            }
        }

        public static string GetEnumeValue(string intVal, Type type)
        {
            var strVal = Enum.Parse(type, intVal);
            return strVal.ToString();
        }
    }
}
