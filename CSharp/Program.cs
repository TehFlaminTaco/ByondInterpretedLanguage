using ByondLang.Variable;
using ByondLang.Tokenizer;
using System.Collections.Generic;

namespace ByondLang{
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
            //scope.code = TokenCompiler.MatchToken(CodeToExecute);

            StringClaimer claimer = new StringClaimer(CodeToExecute);
            //while(!((latestEXP = TExpression.Claim(claimer)) is null)){ // Keep claiming expressions untill this is impossible.
            //    scope.callstack.Push(new CallTarget(scope.code, scope.globals));
            //}
            System.Action<Var> nextStep = null;
            nextStep = var => {
                TExpression nextToken = TStatement.Claim(claimer);
                if(nextToken != null){
                    scope.callstack.Push(()=>nextToken.Parse(scope, nextStep));
                }
            };
            nextStep(null);
        }
    }
}