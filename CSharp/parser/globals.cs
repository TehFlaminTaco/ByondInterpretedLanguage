using ByondLang.Variable;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ByondLang{
    class GlobalGenerator{
        public static VarList globals;
        public static VarList Generate(){
            globals = new VarList();
            globals.string_vars["print"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                State state = new State();
                DoLater looping_func = null;
                looping_func = new DoLater(delegate{
                    foreach(KeyValuePair<double,Var> arg in arguments.number_vars){
                        if(arg.Key%1 == 0){
                            if(!state.returns.ContainsKey((int)arg.Key)){
                                scope.callstack.Push(looping_func);
                                arg.Value.ToString(scope, state.returns, (int)arg.Key);
                                return;
                            }
                        }
                    }
                    string Output = "";
                    string joiner = "";
                    foreach(KeyValuePair<double,Var> arg in arguments.number_vars){
                        if(arg.Key%1 == 0){
                            if(state.returns[(int)arg.Key] is VarString){
                                Output += joiner;
                                Output += ((VarString)state.returns[(int)arg.Key]).data;
                                joiner = "\t";
                            }
                        }
                    }
                    scope.program.terminal.Write(Output + "\r\n");
                    returnTarget[returnID] = Output;
                });
                scope.callstack.Push(looping_func);
            });
            globals.string_vars["write"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                State state = new State();
                DoLater looping_func = null;
                looping_func = new DoLater(delegate{
                    foreach(KeyValuePair<double,Var> arg in arguments.number_vars){
                        if(arg.Key%1 == 0){
                            if(!state.returns.ContainsKey((int)arg.Key)){
                                scope.callstack.Push(looping_func);
                                arg.Value.ToString(scope, state.returns, (int)arg.Key);
                                return;
                            }
                        }
                    }
                    string Output = "";
                    string joiner = "";
                    foreach(KeyValuePair<double,Var> arg in arguments.number_vars){
                        if(arg.Key%1 == 0){
                            if(state.returns[(int)arg.Key] is VarString){
                                Output += joiner;
                                Output += ((VarString)state.returns[(int)arg.Key]).data;
                                joiner = "\t";
                            }
                        }
                    }
                    scope.program.terminal.Write(Output);
                    returnTarget[returnID] = Output;
                });
                scope.callstack.Push(looping_func);
            });
            globals.string_vars["event"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarEvent();
            });
            globals.string_vars["type"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[0].type;
            });

            globals.string_vars["table"] = LibTable.Generate(globals);
            globals.string_vars["term"] = LibTerm.Generate(globals);
            globals.string_vars["string"] = LibString.Generate(globals);
            globals.string_vars["math"] = LibMath.Generate(globals);
            globals.string_vars["tcomm"] = LibTComm.Generate(globals);
            globals.string_vars["net"] = LibNet.Generate(globals);

            globals.string_vars["tostring"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                arguments.number_vars[0].ToString(scope, returnTarget, returnID);
            });

            globals.string_vars["tonumber"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                arguments.number_vars[0].ToNumber(scope, returnTarget, returnID);
            });

            return globals;
        }
    }
}