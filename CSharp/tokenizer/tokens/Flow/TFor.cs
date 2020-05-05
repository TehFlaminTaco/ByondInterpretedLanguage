using ByondLang.Variable;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ByondLang.Tokenizer{
    public class TFor : TExpression{
        private static Regex regexFor = new Regex(@"for");
        private static Regex regexLeftParan = new Regex(@"\(");
        private static Regex regexRightParan = new Regex(@"\)");


        TExpression init;
        TExpression cond;
        TExpression incr;
        TExpression body;
        public static new TExpression Claim(StringClaimer claimer){
            Claim c = claimer.Claim(regexFor);
            if(!c.success)
                return null;
            claimer.Claim(regexLeftParan);
            TExpression init = TStatement.Claim(claimer);
            TExpression cond = TStatement.Claim(claimer);
            TExpression incr = TStatement.Claim(claimer);
            claimer.Claim(regexRightParan);

            TExpression body = TExpOrBlock.Claim(claimer);
            if(body is null){
                c.Fail();
                return null;
            }
            TFor nFor = new TFor();
            nFor.init = init;
            nFor.cond = cond;
            nFor.incr = incr;
            nFor.body = body;
            return nFor;
        }

        public override void Parse(Scope scope, CallTarget target, System.Action<Var> callback){
            System.Action loopStep = null;
            Var lastVar = Var.nil;
            loopStep = () => {
                System.Action<Var> condParsed = condition => {
                    if(scope.returnsStack.Count > 0){
                        KeyValuePair<ReturnType, Var> returns = scope.returnsStack.Pop();
                        scope.callstack.Push(()=>callback(returns.Value));
                        if(returns.Key != ReturnType.Break)
                            scope.returnsStack.Push(returns);
                        return;
                    }
                    if(condition is VarNumber n && n.data == 1){ // Only step if truthy.
                        scope.callstack.Push(()=>body.Parse(scope, target, bodyResult => {
                            lastVar = bodyResult;
                            scope.callstack.Push(loopStep);
                            if(scope.returnsStack.Count == 0 && incr != null)
                                incr.Parse(scope, target, v=>{});
                        }));
                    }else{
                        callback(lastVar);
                    }
                };
                if(cond is null){
                    condParsed(1);
                }else{
                    cond.Parse(scope, target, v=>v.ToBool(scope, condParsed));
                }
            };
            if(init is null)
                scope.callstack.Push(loopStep);
            else
                init.Parse(scope, target, v=>scope.callstack.Push(loopStep));
        }
    }
    public class TForIn : TExpression{
        private static Regex regexFor = new Regex(@"for");
        private static Regex regexIn = new Regex(@"in");

        TVariable iterV;
        TExpression iterOver;
        TExpression body;
        public static new TForIn Claim(StringClaimer claimer){
            Claim c = claimer.Claim(regexFor);
            if(!c.success)
                return null;
            TVariable v = TVariable.Claim(claimer); // For some ungodly reason optional.
            if(!claimer.Claim(regexIn).success){
                c.Fail();
                return null;
            }
            TExpression toIterate = TExpression.Claim(claimer);
            if(toIterate is null){
                c.Fail();
                return null;
            }
            TExpression body = TExpOrBlock.Claim(claimer);
            if(body is null){
                c.Fail();
                return null;
            }

            TForIn nFor = new TForIn();
            nFor.iterV = v;
            nFor.iterOver = toIterate;
            nFor.body = body;
            return nFor;
        }

        public override void Parse(Scope scope, CallTarget target, System.Action<Var> callback){
            iterOver.Parse(scope, target, to_iterate => {
                Var lastVar = Var.nil;
                if(to_iterate is VarNumber numb){
                    VarNumber n = new VarNumber(0);
                    System.Action toDo = null;
                    toDo = ()=>{
                        if(scope.returnsStack.Count > 0){
                            KeyValuePair<ReturnType, Var> returns = scope.returnsStack.Pop();
                            scope.callstack.Push(()=>callback(returns.Value));
                            if(returns.Key != ReturnType.Break)
                                scope.returnsStack.Push(returns);
                            return;
                        }
                        if(n.data <= numb.data){
                            System.Action postSet = ()=>{
                                scope.callstack.Push(toDo);
                                body.Parse(scope, target, v=>lastVar = v);
                            };
                            if(iterV is null)
                                postSet();
                            else
                                iterV.Set(scope, target, n, v=>postSet());
                        }
                        n.data++;
                    };
                    scope.callstack.Push(toDo);
                }else if(to_iterate is Variable.VarString){
                    int i = 0;
                    DoLater toDo = null;
                    toDo = new DoLater(delegate{
                        if(returnsStack.Count > 0){
                            KeyValuePair<string, Var> returns = returnsStack.Pop();
                            target.returnTarget[target.returnTargetID] = returns.Value;
                            if(returns.Key != "break")
                                returnsStack.Push(returns);
                            return;
                        }
                        if(i < ((Variable.VarString)to_iterate).data.Length){
                            scope.callstack.Push(toDo);
                            scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[4][0], (VarList)state.returns[-1]));
                            if(token[1].items.Count > 0){
                                VarList vardata = (VarList)state.returns[8];
                                vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], ""+((Variable.VarString)to_iterate).data[i], ((VarNumber)vardata.string_vars["islocal"]).data==1);
                            }
                        }
                        i++;
                    });
                    scope.callstack.Push(toDo);
                }else if(to_iterate is VarFunction){
                    State sub_state = new State();
                    DoLater toDo = null;
                    target.returnTarget[target.returnTargetID] = Var.nil;
                    toDo = new DoLater(delegate{
                        if(sub_state.returns.ContainsKey(0)){
                            if(sub_state.returns[0] != Var.nil){
                                scope.callstack.Push(toDo);
                                scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[4][0], (VarList)state.returns[-1]));
                                if(token[1].items.Count > 0){
                                    VarList vardata = (VarList)state.returns[8];
                                    vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], sub_state.returns[0], ((VarNumber)vardata.string_vars["islocal"]).data==1);
                                }
                            }
                            sub_state.returns.Remove(0);
                        }else{
                            scope.callstack.Push(toDo);
                            to_iterate.Call(scope, sub_state.returns, 0);
                        }
                    });
                    scope.callstack.Push(toDo);
                
                }else{
                    VarEnumerator enm = to_iterate.GetEnumerator();
                    DoLater toDo = null;
                    toDo = new DoLater(delegate{
                        if(returnsStack.Count > 0){
                            KeyValuePair<string, Var> returns = returnsStack.Pop();
                            target.returnTarget[target.returnTargetID] = returns.Value;
                            if(returns.Key != "break")
                                returnsStack.Push(returns);
                            return;
                        }
                        if(enm.MoveNext()){
                            scope.callstack.Push(toDo);
                            scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[4][0], (VarList)state.returns[-1]));
                            if(token[1].items.Count > 0){
                                VarList vardata = (VarList)state.returns[8];
                                vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], enm.Current.Key, ((VarNumber)vardata.string_vars["islocal"]).data==1);
                            }
                        }
                    });
                    scope.callstack.Push(toDo);
                }
            });
        }
    }
}