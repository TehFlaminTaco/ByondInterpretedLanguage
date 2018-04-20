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

            Token result = TokenCompiler.MatchToken(@"print('Hello, World!')");
            Console.WriteLine("Done");
        }
    }
}
