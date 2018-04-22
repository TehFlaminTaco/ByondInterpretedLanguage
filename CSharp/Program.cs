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

            Token result = TokenCompiler.LocationiseTokens(TokenCompiler.MatchToken(@"print('Hello, World!')"));

            Scope scope = new Scope();
            Parser parser = new Parser();
            scope.parser = parser;
            parser.scope = scope;

            scope.code = result;
            Variable.VarList varscope = scope.listFromParent(new Variable.VarList());  // TODO: Implement global table to replace this.
            scope.callstack.Push(new CallTarget(result, varscope));

            while(scope.callstack.Count>0)
                scope.ExecuteNextEntry();

            Console.WriteLine("Done");
        }
    }
}
