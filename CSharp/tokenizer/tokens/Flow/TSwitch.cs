using ByondLang.Variable;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ByondLang.Tokenizer{
    public class TSwitch : TExpression{
        private static Regex regexSwitch = new Regex(@"switch");
        private static Regex regexLeftBrace = new Regex(@"\{");
        private static Regex regexRightBrace = new Regex(@"\}");

        TExpression tSubject;
        List<TCase> cases;

        public static new TSwitch Claim(StringClaimer claimer){
            Claim c = claimer.Claim(regexSwitch);

            if(!c.success)
                return null;
            
            TExpression subject = TExpression.Claim(claimer);
            if(subject is null){
                c.Fail();
                return null;
            }

            TSwitch nSwitch = new TSwitch();
            nSwitch.tSubject = subject;

            // Get matching cases.

            // Technically optional, but we don't want to try and claim a right brace if we don't use one.
            bool hasCurlyBrace = claimer.Claim(regexLeftBrace).success;

            nSwitch.cases = new List<TCase>();
            TCase nextCase;
            while(!((nextCase = TCase.Claim(claimer)) is null)){
                nSwitch.cases.Add(nextCase);
            }

            if(hasCurlyBrace){
                Claim rightBrace = claimer.Claim(regexRightBrace);
                if(!rightBrace.success){ // Imagine getting this far... just to fail...
                    c.Fail();
                    return null;
                }
            }
                

            return nSwitch;
        }

        public override void Parse(Scope scope, CallTarget target, System.Action<Var> callback){
            tSubject.Parse(scope, target, subject => {
                System.Action nextStep = null;
                int curCase = 0;
                Var lastVar = Var.nil;
                nextStep = () => {
                    // We're gettin' out of here.
                    if(scope.returnsStack.Count > 0){
                        KeyValuePair<ReturnType, Var> returns = scope.returnsStack.Pop();
                        scope.callstack.Push(()=>callback(returns.Value));
                        if(returns.Key != ReturnType.Break)
                            scope.returnsStack.Push(returns);
                        return;
                    }
                    if(cases.Count > curCase){
                        TCase tCase = cases[curCase++];
                        int curStatement = 0;

                        System.Action caseStep = null;
                        caseStep = () => {
                            // If we break, stop processing.
                            if(scope.returnsStack.Count > 0){
                                return;
                            }
                            TExpression tStatement = tCase.caseBody[curStatement++];
                            tStatement.Parse(scope, target, evalResult=>{
                                lastVar = evalResult;
                                scope.callstack.Push(caseStep);
                            });
                        };

                        if(tCase.isDefault){
                            scope.callstack.Push(caseStep);
                        }else{
                            tCase.caseHeader.Parse(scope, target, caseDef=>{
                                scope.parser.Math(subject, caseDef, "eq", isEqual => {
                                    if(isEqual is VarNumber n && n.data == 1)
                                        scope.callstack.Push(caseStep);
                                });
                            });
                        }
                    }else{
                        callback(lastVar);
                    }
                };
                scope.callstack.Push(nextStep);
            });
        }

        private class TCase{
            private static Regex regexColon = new Regex(@":");
            private static Regex regexDefault = new Regex(@"default");
            private static Regex regexCase = new Regex(@"case");
            
            public TExpression caseHeader = null;
            public List<TExpression> caseBody;
            public bool isDefault = false;

            public static TCase Claim(StringClaimer claimer){
                Claim c = claimer.Claim(regexColon);
                if(!c.success)
                    return null;

                TCase nCase = new TCase();
                Claim tryDefault = claimer.Claim(regexDefault);
                if(tryDefault.success)
                    nCase.isDefault = true;
                else{
                    claimer.Claim(regexCase); // Surprisingly optional.
                    TExpression header = TExpression.Claim(claimer);
                    if(header is null){
                        c.Fail();
                        return null;
                    }
                    nCase.caseHeader = header;   
                }
                nCase.caseBody = new List<TExpression>();

                TExpression nextExp;
                while(!((nextExp = TStatement.Claim(claimer)) is null)){
                    nCase.caseBody.Add(nextExp);
                }
                return nCase;
            }
        }
    }
}