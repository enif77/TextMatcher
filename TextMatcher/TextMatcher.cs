/* TextMatcher - (C) 2016 - 2019 Premysl Fara 
 
TextMatcher and newer are available under the zlib license:

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
 
 */

namespace TextMatcher
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// The TextMatcher matches a text agains various patterns.
    /// </summary>
    public class TextMatcher
    {
        // PUBLIC ==============================================================

        /// <summary>
        /// Matches a text against a given pattern.
        /// Supported patterns: "regexp:pattern" - a regular expression,
        /// "regexpi:pattern" - a case insensitive regular expression,
        /// "exact:pattern" - a raw string comparison pattern,
        /// "glob:pattern" or "pattern" - a glob/wildcard pattern.
        /// </summary>
        /// <param name="text">A text.</param>
        /// <param name="pattern">A pattern.</param>
        /// <returns>True, if the text matches the pattern, false otherwise.</returns>
        public static bool MatchText(string text, string pattern)
        {
            // An empty pattern equals to an empty string only.
            if (string.IsNullOrEmpty(pattern))
            {
                return string.IsNullOrEmpty(text);
            }

            // If text is null, convert it to an empty string.
            if (text == null) text = string.Empty;

            if (pattern.StartsWith("regexp:"))
            {
                return Regex.IsMatch(text, pattern.Substring(7));
            }

            if (pattern.StartsWith("regexpi:"))
            {
                return Regex.IsMatch(text, pattern.Substring(8), RegexOptions.IgnoreCase);
            }

            if (pattern.StartsWith("exact:"))
            {
                return text.Equals(pattern.Substring(6));
            }

            if (pattern.StartsWith("glob:"))
            {
                return WildCompare(text, pattern.Substring(5));
            }

            // The default is a glob match
            return WildCompare(text, pattern);
        }

        /// <summary>
        /// Compares a text to a wildcard pattern and decides, if the text matches the pattern.
        /// Special chars/wildcards: '*' - zero or more of any character, 
        /// '?' a single any character, '#' a single digit character.
        /// </summary>
        /// <param name="text">A text.</param>
        /// <param name="pattern">A pattern.</param>
        /// <returns>True, if the text matches the pattern, false otherwise.</returns>
        public static bool WildCompare(string text, string pattern)
        {
            // An empty pattern equals to an empty string only.
            if (string.IsNullOrEmpty(pattern))
            {
                return string.IsNullOrEmpty(text);
            }

            // If s is null, convert it to an empty string.
            if (text == null) text = string.Empty;

            var textIndex = 0;
            var patternIndex = 0;

            while (textIndex < text.Length && patternIndex < pattern.Length)
            {
                var patternChar = pattern[patternIndex];

                if (patternChar == '*')
                {
                    break;
                }

                if (patternChar == '#')
                {
                    // A digit expected.
                    if (char.IsDigit(text[textIndex]) == false) return false;
                }
                else if (patternChar != '?' && (patternChar != text[textIndex]))
                {
                    // A two same chars expected.
                    return false;
                }

                textIndex++;
                patternIndex++;
            }

            // If we have reached the end of the pattern without finding a * wildcard,
            // the match must fail if the string is longer or shorter than the pattern.
            if (patternIndex == pattern.Length)
            {
                return text.Length == pattern.Length;
            }

            var position = 0;
            var mark = 0;

            while (textIndex < text.Length)
            {
                if (patternIndex < pattern.Length)
                {
                    var patternChar = pattern[patternIndex];

                    if (patternChar == '*')
                    {
                        if (++patternIndex >= pattern.Length)
                        {
                            // The '*' at the end of the pattern matches the rest of the text.
                            return true;
                        }

                        mark = patternIndex;
                        position = textIndex + 1;

                        continue;
                    }

                    if (patternChar == '#' && char.IsDigit(text[textIndex]))
                    {
                        patternIndex++;
                        textIndex++;

                        continue;
                    }

                    if (patternChar == '?' || (patternChar == text[textIndex]))
                    {
                        patternIndex++;
                        textIndex++;

                        continue;
                    }
                }

                patternIndex = mark;
                textIndex = position++;
            }

            // Eat all remaining '*'.
            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return patternIndex >= pattern.Length;
        }
    }
}
