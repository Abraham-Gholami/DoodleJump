using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace UGS
{
    public static class UsernameValidator
    {
        private const int MinLength = 3;
        private const int MaxLength = 50;

        // Comprehensive profanity patterns with character substitution support
        private static readonly string[] ProfanityPatterns =
        {
            // Common profanities with flexible character matching
            @"f+[u\*_\-\.]?[u\*_\-\.]*c+k+",
            @"s+h+[i1!]+t+",
            @"[a4@]+s+s+",
            @"[a4@]+s+s+h+[o0]+l+[e3]+",
            @"b+[i1!]+t+c+h+",
            @"d+[a4@]+m+n+",
            @"h+[e3]+l+l+",
            @"c+r+[a4@]+p+",
            @"p+[i1!]+s+s+",
            @"[c<\(]+[u\*]+n+t+",
            @"d+[i1!]+c+k+",
            @"c+[o0]+c+k+",
            @"b+[a4@]+s+t+[a4@]+r+d+",
            @"s+[e3]+x+",
            @"p+[o0]+r+n+",
            @"n+[i1!]+g+",
            @"f+[a4@]+g+",
            @"s+l+[u\*]+t+",
            @"w+h+[o0]+r+[e3]+",
            @"r+[e3]+t+[a4@]+r+d+",
            @"s+t+[u\*]+p+[i1!]+d+",
            @"[i1!]+d+[i1!]+[o0]+t+",
            @"m+[o0]+r+[o0]+n+",
            @"d+[u\*]+m+b+",
            @"n+[o0]{2,}b+",
            
            // Gaming/Internet specific slurs
            @"k+[i1!]+l+l+\s*y+[o0]+[u\*]+r+\s*s+[e3]+l+f+",
            @"g+[e3]+t+\s*c+[a4@]+n+c+[e3]+r+",
            @"n+[e3]+c+k+\s*y+[o0]+[u\*]+r+\s*s+[e3]+l+f+",
            
            // Racial slurs (abbreviated patterns)
            @"n+[i1!]+g+[e3a4@]+r+",
            @"s+p+[i1!]+c+",
            @"c+h+[i1!]+n+k+",
            
            // Homophobic slurs
            @"f+[a4@]+g+g+[o0]*t+",
            @"d+y+k+[e3]+",
            @"q+[u\*]+[e3]+[e3]+r+",
            
            // Nazi/hate symbols
            @"n+[a4@]+z+[i1!]+",
            @"h+[i1!]+t+l+[e3]+r+",
            @"[8]+[8]+",  // 88 = HH = Heil Hitler
            @"[1]+[4]+[8]+[8]+", // 1488
            
            // Sexual content
            @"p+[e3]+n+[i1!]+s+",
            @"v+[a4@]+g+[i1!]+n+[a4@]+",
            @"b+[o0]{2,}b+s*",
            @"t+[i1!]+t+s*",
            @"a+n+[a4@]+l+",
            @"[o0]+r+g+[a4@]+s+m+",
            @"m+[a4@]+s+t+[u\*]+r+b+",
            @"r+[a4@]+p+[e3]+",
            
            // Drug references
            @"w+[e3]+[e3]+d+",
            @"p+[o0]+t+h+[e3]+[a4@]+d+",
            @"c+[o0]+k+[e3]+h+[e3]+[a4@]+d+",
            
            // Violence
            @"k+[i1!]+l+l+",
            @"m+[u\*]+r+d+[e3]+r+",
            @"d+[i1!]+[e3]+",
            @"d+[e3]+[a4@]+t+h+",
            
            // Admin/mod impersonation attempts
            @"[a4@]+d+m+[i1!]+n+",
            @"m+[o0]+d+[e3]+r+[a4@]+t+[o0]+r+",
            @"[o0]+f+f+[i1!]+c+[i1!]+[a4@]+l+",
            @"s+t+[a4@]+f+f+",
            @"s+[u\*]+p+p+[o0]+r+t+"
        };

        private static readonly Regex[] ProfanityRegexList;
        private static readonly Regex AllowedCharactersRegex = new Regex(@"^[a-zA-Z0-9._@-]+$");

        // Character substitution map for normalization
        private static readonly Dictionary<char, char> SubstitutionMap = new Dictionary<char, char>
        {
            {'4', 'a'}, {'@', 'a'}, {'/', 'a'},
            {'8', 'b'},
            {'(', 'c'}, {'<', 'c'}, {'{', 'c'},
            {'3', 'e'},
            {'6', 'g'}, {'9', 'g'},
            {'#', 'h'},
            {'1', 'i'}, {'!', 'i'}, {'|', 'i'},
            {'0', 'o'},
            {'5', 's'}, {'$', 's'},
            {'7', 't'}, {'+', 't'},
            {'_', 'u'},
            {'2', 'z'}
        };

        static UsernameValidator()
        {
            ProfanityRegexList = new Regex[ProfanityPatterns.Length];
            for (int i = 0; i < ProfanityPatterns.Length; i++)
            {
                ProfanityRegexList[i] = new Regex(ProfanityPatterns[i], RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
        }

        public static ValidationResult ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ValidationResult(false, "Username cannot be empty.");
            }

            username = username.Trim();

            if (username.Length < MinLength)
            {
                return new ValidationResult(false, $"Username must be at least {MinLength} characters.");
            }

            if (username.Length > MaxLength)
            {
                return new ValidationResult(false, $"Username must be no more than {MaxLength} characters.");
            }

            if (username.Contains(" "))
            {
                return new ValidationResult(false, "Username cannot contain spaces.");
            }

            if (!AllowedCharactersRegex.IsMatch(username))
            {
                return new ValidationResult(false, "Username can only contain letters, numbers, and these symbols: . - @ _");
            }

            // Check profanity with multiple strategies
            if (ContainsProfanity(username))
            {
                return new ValidationResult(false, "Username contains inappropriate language.");
            }

            // Additional check: normalized version (convert leet speak to normal letters)
            string normalizedUsername = NormalizeUsername(username);
            if (ContainsProfanity(normalizedUsername))
            {
                return new ValidationResult(false, "Username contains inappropriate language.");
            }

            // Check for repeating patterns that might hide profanity
            string collapsedUsername = CollapseRepeatingCharacters(username);
            if (ContainsProfanity(collapsedUsername))
            {
                return new ValidationResult(false, "Username contains inappropriate language.");
            }

            // Check without special characters (removes ., -, _, @)
            string strippedUsername = Regex.Replace(username, @"[._@\-]", "");
            if (ContainsProfanity(strippedUsername))
            {
                return new ValidationResult(false, "Username contains inappropriate language.");
            }

            return new ValidationResult(true, "Valid username.");
        }

        private static bool ContainsProfanity(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (var regex in ProfanityRegexList)
            {
                if (regex.IsMatch(text))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Normalizes username by converting leet speak substitutions to regular letters
        /// </summary>
        private static string NormalizeUsername(string username)
        {
            var normalized = new StringBuilder(username.Length);
            foreach (char c in username.ToLower())
            {
                if (SubstitutionMap.TryGetValue(c, out char replacement))
                {
                    normalized.Append(replacement);
                }
                else
                {
                    normalized.Append(c);
                }
            }
            return normalized.ToString();
        }

        /// <summary>
        /// Collapses repeating characters (e.g., "fuuuuck" becomes "fuck")
        /// </summary>
        private static string CollapseRepeatingCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var collapsed = new StringBuilder();
            char lastChar = '\0';
            int repeatCount = 0;

            foreach (char c in text.ToLower())
            {
                if (c == lastChar)
                {
                    repeatCount++;
                    // Keep at most 2 repeating characters
                    if (repeatCount <= 2)
                    {
                        collapsed.Append(c);
                    }
                }
                else
                {
                    collapsed.Append(c);
                    lastChar = c;
                    repeatCount = 1;
                }
            }

            return collapsed.ToString();
        }
    }
}