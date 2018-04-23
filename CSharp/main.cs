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

            /*Program program = new Program(@"
local some_var = 1
local some_other = ""Biggo Memer""

print`This is a template string test.
    some_var: [some_var]
    some_other: [some_other]`
");

            int iterations = 0;

            while(program.scope.callstack.Count>0){
                program.scope.ExecuteNextEntry();
                iterations++;
            }

            Console.WriteLine("Done in {0} iterations.", iterations);*/

            new Listener();
        }
    }
}
