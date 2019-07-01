/* PatternMatcher - (C) 2019 Premysl Fara 
 
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
    using System.Collections.Generic;


    /// <summary>
    /// Matches a string against defined positive patterns and negative patterns.
    /// Patterns can contain *, ? and # wildcards. Wildcards are used, if the UseWildCards property is true.
    /// Negative patterns are patterns starting with the '!' character.
    /// </summary>
    public class PatternMatcher
    {
        /// <summary>
        /// Defines an object, that can dynamically update a pattern to match the current state.
        /// </summary>
        public interface IPatternResolver
        {
            /// <summary>
            /// Resolves/overrides/updates the pattern to match the current user state.
            /// </summary>
            /// <param name="pattern">A pattern.</param>
            /// <returns>Updated pattern.</returns>
            string ResolvePattern(string pattern);
        }


        #region ctor

        public PatternMatcher()
        {
            UseWildCards = false;
            MatchAll = true;
        }

        #endregion


        #region public

        /// <summary>
        /// If true, patterns can contain wildcards (*, #, ?).
        /// </summary>
        public bool UseWildCards { get; set; }

        /// <summary>
        /// If true (the default), tested string must match against all positive patterns.
        /// </summary>
        public bool MatchAll { get; set; }

        /// <summary>
        /// The list of positive patterns.
        /// </summary>
        public List<string> PositivePatterns { get; } = new List<string>();

        /// <summary>
        /// The list of negative patterns.
        /// </summary>
        public List<string> NegativePatterns { get; } = new List<string>();


        /// <summary>
        /// Adds a pattern to the list of positive patterns.
        /// </summary>
        /// <param name="p">A pattern.</param>
        public void AddPositivePattern(string p)
        {
            if (string.IsNullOrEmpty(p) || PositivePatterns.Contains(p)) return;

            PositivePatterns.Add(p);
        }

        /// <summary>
        /// Adds a pattern to the list of negative patterns.
        /// </summary>
        /// <param name="p">A pattern.</param>
        public void AddNegativePattern(string p)
        {
            if (string.IsNullOrEmpty(p) || NegativePatterns.Contains(p)) return;

            NegativePatterns.Add(p);
        }

        /// <summary>
        /// Checks if a string matches agains patterns.
        /// No defined patterns = always a match.
        /// </summary>
        /// <param name="s">A string to match against defined patterns.</param>
        /// <param name="patternResolver">An optional user pattern resolver.</param>
        /// <returns>True, if the string matches.</returns>
        public bool Match(string s, IPatternResolver patternResolver = null)
        {
            // The match status.
            bool matched;

            // Match against positive patterns first if any.
            if (PositivePatterns.Count > 0)
            {
                if (UseWildCards)
                {
                    if (MatchAll)
                    {
                        matched = true;
                        foreach (var pattern in PositivePatterns)
                        {
                            var p = (patternResolver == null)
                                ? pattern
                                : patternResolver.ResolvePattern(pattern);

                            if (TextMatcher.WildCompare(s, p) == false)
                            {
                                matched = false;

                                break;
                            }
                        }
                    }
                    else
                    {
                        matched = false;
                        foreach (var pattern in PositivePatterns)
                        {
                            var p = (patternResolver == null)
                                ? pattern
                                : patternResolver.ResolvePattern(pattern);

                            if (TextMatcher.WildCompare(s, p))
                            {
                                matched = true;

                                break;
                            }
                        }
                    }
                }
                else
                {
                    matched = false;
                    foreach (var pattern in PositivePatterns)
                    {
                        var p = (patternResolver == null)
                            ? pattern
                            : patternResolver.ResolvePattern(pattern);

                        if (s.Contains(p))
                        {
                            matched = true;

                            if (MatchAll == false)
                            {
                                break;
                            }
                        }
                        else
                        {
                            matched = false;

                            if (MatchAll)
                            {
                                break;
                            }
                        }
                    }

                    //matched = MatchAll
                    //    ? PositivePatterns.All(s.Contains)
                    //    : PositivePatterns.Any(s.Contains);
                }
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
            if (NegativePatterns.Count > 0)
            {
                // A fileName should not contain any negative pattern.
                if (UseWildCards)
                {
                    foreach (var pattern in NegativePatterns)
                    {
                        var p = (patternResolver == null)
                            ? pattern
                            : patternResolver.ResolvePattern(pattern);

                        if (TextMatcher.WildCompare(s, p))
                        {
                            matched = false;

                            break;
                        }
                    }
                }
                else
                {
                    foreach (var pattern in NegativePatterns)
                    {
                        var p = (patternResolver == null)
                            ? pattern
                            : patternResolver.ResolvePattern(pattern);

                        if (s.Contains(p))
                        {
                            matched = false;

                            break;
                        }
                    }
                }
            }

            return matched;
        }
        
        /// <summary>
        /// Removes all patterns from this instance.
        /// </summary>
        public void Clear()
        {
            PositivePatterns.Clear();
            NegativePatterns.Clear();
        }

        #endregion
    }
}
