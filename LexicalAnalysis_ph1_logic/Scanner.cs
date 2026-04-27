using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerScanner
{
    public class Scanner
    {
        private HashSet<string> keywords = new HashSet<string>
            {
                "for","while","if","do","return","break","continue","end",
            };

        private HashSet<string> identifiers = new HashSet<string>
            {
                "int","float","string","double","bool","char"
            };

        private HashSet<char> symbols = new HashSet<char>
            {
                '(', ')', '{', '}', ',', ';'
            };

        public List<Token> Scan(string code)
        {
            List<Token> tokens = new List<Token>();
            int i = 0;

            while (i < code.Length)
            {
                char current = code[i];

                // 1. Skip whitespace
                if (char.IsWhiteSpace(current))
                {
                    i++;
                    continue;
                }

                // 2. Identifier or Keyword
                if (char.IsLetter(current))
                {
                    string word = "";

                    while (i < code.Length && char.IsLetterOrDigit(code[i]))
                    {
                        word += code[i];
                        i++;
                    }

                    if (keywords.Contains(word))
                        tokens.Add(new Token(TokenTypes.Keyword, word));
                    else if (identifiers.Contains(word))
                        tokens.Add(new Token(TokenTypes.Identifier, word));
                    else
                        tokens.Add(new Token(TokenTypes.Variable, word));

                    continue;
                }

                // 3. Numbers
                if (char.IsDigit(current))
                {
                    string number = "";

                    while (i < code.Length && (char.IsDigit(code[i]) || code[i] == '.'))
                    {
                        number += code[i];
                        i++;
                    }
                    //If contains . -> float/double else -> int
                    tokens.Add(new Token(TokenTypes.Number, number));
                    continue;
                }

                // 4. Double operators (&&, ||)
                if (i + 1 < code.Length)
                {
                    string twoChar = $"{code[i]}{code[i + 1]}";

                    if (twoChar == "&&" || twoChar == "||")
                    {
                        tokens.Add(new Token(TokenTypes.Operator, twoChar));
                        i += 2;
                        continue;
                    }
                }

                // 5. Single operators
                if ("+-*/%<>=!".Contains(current))
                {
                    tokens.Add(new Token(TokenTypes.Operator, current.ToString()));
                    i++;
                    continue;
                }

                // 6. Symbols
                if (symbols.Contains(current))
                {
                    tokens.Add(new Token(TokenTypes.Symbol, current.ToString()));
                    i++;
                    continue;
                }

                // 7. Unknown (Error)
                tokens.Add(new Token(TokenTypes.Error, current.ToString()));
                i++;
            }

            return tokens;
        }

        //public int Line { get; set; }
    }
}
