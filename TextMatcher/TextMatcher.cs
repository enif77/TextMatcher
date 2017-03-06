/* TextMatcher 1.1 - (C) 2016 - 2017 Premysl Fara 
 
TextMatcher 1.0 and newer are available under the zlib license:

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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The TextMatcher matches a text agains various patterns.
    /// </summary>
    public static class TextMatcher
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
            if (String.IsNullOrEmpty(pattern))
            {
                return String.IsNullOrEmpty(text);
            }

            // If text is null, convert it to an empty string.
            if (text == null) text = String.Empty;

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
            if (String.IsNullOrEmpty(pattern))
            {
                return String.IsNullOrEmpty(text);
            }

            // If s is null, convert it to an empty string.
            if (text == null) text = String.Empty;

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
                    if (Char.IsDigit(text[textIndex]) == false) return false;
                }
                else if (patternChar != '?' && (patternChar != text[textIndex]))
                {
                    // Two the same chars expected.
                    return false;
                }

                textIndex++;
                patternIndex++;
            }

            // If we have reached the end of the pattern without finding a * wildcard,
            // the match must fail if the string is longer or shorter than the pattern
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

                    if (patternChar == '#' && Char.IsDigit(text[textIndex]))
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


        /// <summary>
        /// Checks if a string matches agains patterns.
        /// No defined patterns or no pattern config profided = always a match.
        /// </summary>
        /// <param name="patternConfig">A PatternConfig instance.</param>
        /// <param name="s">A string to match against defined patterns.</param>
        /// <returns>True, if the string matches.</returns>
        public static bool MatchPattern(PatternConfig patternConfig, string s)
        {
            // Nothing to match...
            if (patternConfig == null)
            {
                return true;
            }

            // The match status.
            bool matched;

            // Match against positive patterns first if any.
            if (patternConfig.PositivePatterns.Count > 0)
            {
                matched = patternConfig.MatchAll
                    ? patternConfig.PositivePatterns.All(s.Contains)
                    : patternConfig.PositivePatterns.Any(s.Contains);
            }
            else
            {
                // No positive matches means a match.
                matched = true;
            }

            // Positive patterns are saying not matched,
            // so we can ignore negative patterns.
            if (matched == false) return false;

            // Match against negative patterns if any.
            if (patternConfig.NegativePatterns.Count > 0)
            {
                // A fileName should not contain any negative pattern.
                //matched = !_negativePatterns.Any(fileName.Contains);
                foreach (var pattern in patternConfig.NegativePatterns)
                {
                    if (s.Contains(pattern))
                    {
                        matched = false;

                        break;
                    }
                }
            }

            return matched;
        }


        /// <summary>
        /// Defines patterns and other pattern-match related settings.
        /// </summary>
        public class PatternConfig
        {
            #region ctor

            /// <summary>
            /// Constructor.
            /// </summary>
            public PatternConfig()
            {
                MatchAll = true;

                PositivePatterns = new List<string>();
                NegativePatterns = new List<string>();
            }

            #endregion


            #region public

            /// <summary>
            /// If true (the default), tested string must match against all positive patterns.
            /// </summary>
            public bool MatchAll { get; set; }

            /// <summary>
            /// The list of positive patterns.
            /// </summary>
            public List<string> PositivePatterns
            {
                get; private set;
            }

            /// <summary>
            /// The list of negative patterns.
            /// </summary>
            public List<string> NegativePatterns
            {
                get; private set;
            }


            /// <summary>
            /// Adds a pattern to the list of positive patterns.
            /// Ignores empty patterns.
            /// </summary>
            /// <param name="p">A pattern.</param>
            public void AddPositivePattern(string p)
            {
                if (String.IsNullOrEmpty(p)) return;

                PositivePatterns.Add(p);
            }

            /// <summary>
            /// Adds a pattern to the list of negative patterns.
            /// Ignores empty patterns.
            /// </summary>
            /// <param name="p">A pattern.</param>
            public void AddNegativePattern(string p)
            {
                if (String.IsNullOrEmpty(p)) return;

                NegativePatterns.Add(p);
            }
            
            #endregion
        }
    }
}
