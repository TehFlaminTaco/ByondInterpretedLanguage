using System;
using ByondLang.Tokenizer;

namespace ByondLang.CSharp
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            TokenCompiler.GetTokens();
            TokenCompiler.CompileTokens();

            new Listener();
        }
    }
}
