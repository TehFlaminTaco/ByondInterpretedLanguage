using System;
using Tokenizer;

namespace CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            TokenCompiler.GetTokens();
            TokenCompiler.CompileTokens();
        }
    }
}
