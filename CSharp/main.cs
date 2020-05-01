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

            string hostname = "+";

            int port = 1945;

            if(args.Length > 1){
                hostname = args[0];
                Int32.TryParse(args[1], out port);
            }

            new Listener(hostname, port);
        }
    }
}
