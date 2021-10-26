using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace A2LEngine
{
    static class Helpers
    {
        public static IEnumerable<string> GetFirstInstanceTextBetween(string source, string leftWord, string rightWord, string customRegex = "")
        {
            var strFilt = String.Format(@"(?<={0})(.*?)(?={1})", leftWord, rightWord);
            if (customRegex != "")
            {
                strFilt = customRegex;
            }
            var matches = Regex.Matches(source, strFilt,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match item in matches)
            {
                yield return item.Value;
            }
        }

        public static string GetStringAfter(string source, string leftWord)
        {          
            var strFilt = String.Format(@"(?<={0})(.*?)(?=\n)", leftWord);
            var matches = Regex.Match(source, strFilt,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return matches.Value.Trim();

        }


        public static string Cleanup(this string value)
        {
            return value.Trim().Replace("\"", string.Empty);
        }

        public static T ToEnum<T>(this string value, bool ignoreCase = true)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
    }
}
