using ByondLang.Variable;
using System.Collections.Generic;

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

            globals.string_vars["table"] = Table();
            globals.string_vars["term"] = Term();
            globals.string_vars["string"] = String();

            return globals;
        }

        public static VarList Table(){
            VarList table_VAR = new VarList();
            Dictionary<string, Var> table = table_VAR.string_vars;
            table["set_meta"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                arguments.number_vars[0].meta = arguments.number_vars[1];
                returnTarget[returnID] = arguments.number_vars[0];
            });
            table["get_meta"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[0].meta;
            });
            return table_VAR;
        }

        public static VarList String(){
            VarList string_VAR = new VarList();
            Dictionary<string, Var> str = string_VAR.string_vars;



            return string_VAR;
        }

        public static VarList Term(){
            VarList term_VAR = new VarList();
            Dictionary<string, Var> term = term_VAR.string_vars;

            term["topic"] = new VarEvent();

            term["set_foreground"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars.ContainsKey(1) && arguments.number_vars.ContainsKey(2)){
                    var red = arguments.number_vars[0];
                    var green = arguments.number_vars[1];
                    var blue = arguments.number_vars[2];
                    if(red is VarNumber && green is VarNumber && blue is VarNumber){
                        scope.program.terminal.foreground = new Color((float)((VarNumber)red).data, (float)((VarNumber)green).data, (float)((VarNumber)blue).data);
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["get_foreground"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarList(scope.program.terminal.foreground.r, scope.program.terminal.foreground.g, scope.program.terminal.foreground.b);
            });
            term["set_background"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars.ContainsKey(1) && arguments.number_vars.ContainsKey(2)){
                    var red = arguments.number_vars[0];
                    var green = arguments.number_vars[1];
                    var blue = arguments.number_vars[2];
                    if(red is VarNumber && green is VarNumber && blue is VarNumber){
                        scope.program.terminal.background = new Color((float)((VarNumber)red).data, (float)((VarNumber)green).data, (float)((VarNumber)blue).data);
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["get_background"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarList(scope.program.terminal.background.r, scope.program.terminal.background.g, scope.program.terminal.background.b);
            });
            term["set_cursor"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars.ContainsKey(1)){
                    var x = arguments.number_vars[0];
                    var y = arguments.number_vars[1];
                    if(x is VarNumber && y is VarNumber){
                        scope.program.terminal.cursor_x = (int)(VarNumber)x;
                        scope.program.terminal.cursor_y = (int)(VarNumber)y;
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["set_cursor_x"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0)){
                    var x = arguments.number_vars[0];
                    if(x is VarNumber){
                        scope.program.terminal.cursor_x = (int)(VarNumber)x;
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["set_cursor_y"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0)){
                    var y = arguments.number_vars[0];
                    if(y is VarNumber){
                        scope.program.terminal.cursor_y = (int)(VarNumber)y;
                    }
                }
                returnTarget[returnID] = Var.nil;
            });
            term["get_cursor"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarList(scope.program.terminal.cursor_x, scope.program.terminal.cursor_y);
            });
            term["get_cursor_x"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.cursor_x;
            });
            term["get_cursor_y"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.cursor_y;
            });
            term["clear"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                scope.program.terminal.Clear();
                returnTarget[returnID] = Var.nil;
            });
            term["write"] = globals.string_vars["write"];
            term["get_size"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarList(scope.program.terminal.cursor_x, scope.program.terminal.cursor_y);
            });
            term["get_width"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.width;
            });
            term["get_height"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = scope.program.terminal.height;
            });
            term["set_topic"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[4];
                scope.program.terminal.SetTopic((int)(double)(VarNumber)arguments.number_vars[0],
                                                (int)(double)(VarNumber)arguments.number_vars[1],
                                                (int)(double)(VarNumber)arguments.number_vars[2],
                                                (int)(double)(VarNumber)arguments.number_vars[3],
                                                (string)(VarString)arguments.number_vars[4]);
            });

            return term_VAR;
        }
    }
}