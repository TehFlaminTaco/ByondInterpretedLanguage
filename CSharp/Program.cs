using ByondLang.Variable;
using ByondLang.Tokenizer;

namespace ByondLang{
    class Program{
        public Scope scope;
        public Parser parser;
        public VarList globals;
        public string output;

        public Program(string CodeToExecute){
            scope = new Scope();
            parser = new Parser();
            globals = scope.listFromParent(GlobalGenerator.Generate());
            scope.parser = parser;
            parser.scope = scope;
            scope.program = this;

            scope.code = TokenCompiler.LocationiseTokens(TokenCompiler.MatchToken(CodeToExecute));
            scope.callstack.Push(new CallTarget(scope.code, globals));
        }
    }
}