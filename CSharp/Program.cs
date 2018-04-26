using ByondLang.Variable;
using ByondLang.Tokenizer;

namespace ByondLang{
    class Program{
        public Scope scope;
        public Parser parser;
        public Terminal terminal;

        public Program(string CodeToExecute, string ComputerRef){
            scope = new Scope(GlobalGenerator.Generate());
            parser = new Parser();
            terminal = new Terminal(ComputerRef);
            scope.parser = parser;
            parser.scope = scope;
            scope.program = this;
            scope.code = TokenCompiler.MatchToken(CodeToExecute);
            scope.callstack.Push(new CallTarget(scope.code, scope.globals));
        }
    }
}