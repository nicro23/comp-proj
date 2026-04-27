using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerScanner
{
    public static class TokenTypes
    {
        public const string Keyword = "KEYWORD";
        public const string Identifier = "IDENTIFIER";
        public const string Variable = "VARIABLE";
        public const string Number = "NUMBER";
        public const string Operator = "OPERATOR";
        public const string Symbol = "SYMBOL";
        public const string Error = "ERROR";
    }

}
