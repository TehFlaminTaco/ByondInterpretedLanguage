using ByondLang.Language.Variable;
using ByondLang.Language.Tokenizer;
using System.Collections.Generic;

namespace ByondLang.Language {
    public class Program{
        public Scope scope;
        public Parser parser;
        public Terminal terminal;

        public Queue<Signal> signals = new Queue<Signal>();

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