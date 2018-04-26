using ByondLang.Variable;
using System.Collections.Generic;

namespace ByondLang{
    class GlobalGenerator{
        public static VarList globals;
        public static VarList Generate(){
            globals = new VarList();
            globals.string_vars["string"] = new VarList();
            globals.string_vars["table"] = Table();
            globals.string_vars["print"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
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
                            if(state.returns[(int)arg.Key] is String){
                                Output += joiner;
                                Output += ((String)state.returns[(int)arg.Key]).data;
                                joiner = "\t";
                            }
                        }
                    }
                    scope.program.terminal.Write(Output + "\r\n");
                    returnTarget[returnID] = Output;
                });
                scope.callstack.Push(looping_func);
            });
            globals.string_vars["write"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
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
                    scope.program.terminal.Write(Output);
                    returnTarget[returnID] = Output;
                });
                scope.callstack.Push(looping_func);
            });
            globals.string_vars["event"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new Event();
            });

            globals.string_vars["term"] = Term();

            return globals;
        }

        public static VarList Table(){
            VarList table_VAR = new VarList();
            Dictionary<string, Var> table = table_VAR.string_vars;
            table["set_meta"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                arguments.number_vars[0].meta = arguments.number_vars[1];
                returnTarget[returnID] = arguments.number_vars[0];
            });
            table["get_meta"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[0].meta;
            });
            return table_VAR;
        }

        public static VarList Term(){
            VarList term_VAR = new VarList();
            Dictionary<string, Var> term = term_VAR.string_vars;

            term["set_foreground"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars.ContainsKey(1) && arguments.number_vars.ContainsKey(2)){
                    var red = arguments.number_vars[0];
                    var green = arguments.number_vars[1];
                    var blue = arguments.number_vars[2];
                    if(red is Number && green is Number && blue is Number){
                        scope.program.terminal.foreground = new Color((float)((Number)red).data, (float)((Number)green).data, (float)((Number)blue).data);
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["set_background"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars.ContainsKey(1) && arguments.number_vars.ContainsKey(2)){
                    var red = arguments.number_vars[0];
                    var green = arguments.number_vars[1];
                    var blue = arguments.number_vars[2];
                    if(red is Number && green is Number && blue is Number){
                        scope.program.terminal.background = new Color((float)((Number)red).data, (float)((Number)green).data, (float)((Number)blue).data);
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["set_cursor"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars.ContainsKey(1)){
                    var x = arguments.number_vars[0];
                    var y = arguments.number_vars[1];
                    if(x is Number && y is Number){
                        scope.program.terminal.cursor_x = (int)(Number)x;
                        scope.program.terminal.cursor_y = (int)(Number)y;
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["set_cursor_x"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0)){
                    var x = arguments.number_vars[0];
                    if(x is Number){
                        scope.program.terminal.cursor_x = (int)(Number)x;
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["set_cursor_y"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0)){
                    var y = arguments.number_vars[0];
                    if(y is Number){
                        scope.program.terminal.cursor_y = (int)(Number)y;
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["get_cursor"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarList(scope.program.terminal.cursor_x, scope.program.terminal.cursor_y);
            });
            term["get_cursor_x"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.cursor_x;
            });
            term["get_cursor_y"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.cursor_y;
            });
            term["clear"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                scope.program.terminal.Clear();
                returnTarget[returnID] = Var.nil;
            });
            term["write"] = globals.string_vars["write"];
            term["get_size"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarList(scope.program.terminal.cursor_x, scope.program.terminal.cursor_y);
            });
            term["get_width"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.width;
            });
            term["get_height"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.height;
            });

            return term_VAR;
        }
    }
}