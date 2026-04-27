using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerScanner
{
    public class Token
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public Token(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
