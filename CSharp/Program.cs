using System;
using ByondLang.Tokenizer;

namespace ByondLang.CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            TokenCompiler.GetTokens();
            TokenCompiler.CompileTokens();

            Token result = TokenCompiler.LocationiseTokens(TokenCompiler.MatchToken(@"x=3; print(x+3)"));

            Scope scope = new Scope();
            Parser parser = new Parser();
            scope.parser = parser;
            parser.scope = scope;

            scope.code = result;
            scope.callstack.Push(new CallTarget());

            while(scope.callstack.Count>0)
                scope.ExecuteNextEntry();

            Console.WriteLine("Done");
        }
    }
}
