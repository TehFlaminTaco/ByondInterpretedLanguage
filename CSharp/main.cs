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

            int port = 1945;

            if(args.Length > 0)
                Int32.TryParse(args[0], out port);

            new Listener(port);
        }
    }
}
