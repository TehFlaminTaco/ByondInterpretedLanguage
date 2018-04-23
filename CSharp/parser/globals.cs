using ByondLang.Variable;
using System.Collections.Generic;

namespace ByondLang{
    class GlobalGenerator{
        public static VarList Generate(){
            VarList globals = new VarList();
            globals.string_vars["string"] = new VarList();
            globals.string_vars["table"] = new VarList();
            globals.string_vars["print"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                State state = new State();
                DoLater looping_func = null;
                looping_func = new DoLater(delegate{
                    foreach(KeyValuePair<double,Var> arg in arguments.number_vars){
                        if(arg.Key%1 == 0){
                            if(!state.returns.ContainsKey((int)arg.Key)){
                                arg.Value.ToString(scope, state.returns, (int)arg.Key);
                                scope.callstack.Push(looping_func);
                                return;
                            }
                        }
                    }
                    string Output = "";
                    string joiner = "";
                    foreach(KeyValuePair<double,Var> arg in arguments.number_vars){
                        if(arg.Key%1 == 0){
                            if(state.returns[(int)arg.Key] is String){
                                Output += joiner;
                                Output += ((String)state.returns[(int)arg.Key]).data;
                                joiner = "\t";
                            }
                        }
                    }
                    scope.program.output += Output + "\r\n";
                    returnTarget[returnID] = Output;
                });
                scope.callstack.Push(looping_func);
            });
            return globals;
        }
    }
}