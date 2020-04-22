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

            globals.string_vars["table"] = Table();
            globals.string_vars["term"] = Term();
            globals.string_vars["string"] = String();
            globals.string_vars["math"] = Math();
            globals.string_vars["tcomm"] = TComm();
            globals.string_vars["net"] = Net();

            globals.string_vars["tostring"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                arguments.number_vars[0].ToString(scope, returnTarget, returnID);
            });

            globals.string_vars["tonumber"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                arguments.number_vars[0].ToNumber(scope, returnTarget, returnID);
            });

            return globals;
        }

        public static VarList TComm(){
            VarList tcomm_VAR = new VarList();
            Dictionary<string, Var> tcomm = tcomm_VAR.string_vars;
            tcomm["onmessage"] = new VarEvent();
            tcomm["broadcast"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Signal newSignal = new Signal("*beep*", "1459", "Telecomms Broadcaster", "Machine", "1", "says");
                if(arguments.string_vars.ContainsKey("content") && arguments.string_vars["content"] is VarString){
                    newSignal.content = (string)(VarString)arguments.string_vars["content"];
                }else if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    newSignal.content = (string)(VarString)arguments.number_vars[0];
                }

                if(arguments.string_vars.ContainsKey("source") && arguments.string_vars["source"] is VarString){
                    newSignal.source = (string)(VarString)arguments.string_vars["source"];
                }else if(arguments.number_vars.ContainsKey(2) && arguments.number_vars[1] is VarString){
                    newSignal.source = (string)(VarString)arguments.number_vars[1];
                }

                if(arguments.string_vars.ContainsKey("job") && arguments.string_vars["job"] is VarString){
                    newSignal.job = (string)(VarString)arguments.string_vars["job"];
                }else if(arguments.number_vars.ContainsKey(3) && arguments.number_vars[2] is VarString){
                    newSignal.job = (string)(VarString)arguments.number_vars[2];
                }

                if(arguments.string_vars.ContainsKey("freq") && arguments.string_vars["freq"] is VarString){
                    newSignal.freq = (string)(VarString)arguments.string_vars["freq"];
                }else if(arguments.number_vars.ContainsKey(3) && arguments.number_vars[3] is VarString){
                    newSignal.freq = (string)(VarString)arguments.number_vars[3];
                }

                if(arguments.string_vars.ContainsKey("pass") && arguments.string_vars["pass"] is VarString){
                    newSignal.pass = (string)(VarString)arguments.string_vars["pass"];
                }else if(arguments.number_vars.ContainsKey(4) && arguments.number_vars[4] is VarString){
                    newSignal.pass = (string)(VarString)arguments.number_vars[4];
                }

                if(arguments.string_vars.ContainsKey("ref") && arguments.string_vars["ref"] is VarString){
                    newSignal.reference = (string)(VarString)arguments.string_vars["ref"];
                }else if(arguments.number_vars.ContainsKey(5) && arguments.number_vars[5] is VarString){
                    newSignal.reference = (string)(VarString)arguments.number_vars[5];
                }

                if(arguments.string_vars.ContainsKey("verb") && arguments.string_vars["verb"] is VarString){
                    newSignal.verb = (string)(VarString)arguments.string_vars["verb"];
                }else if(arguments.number_vars.ContainsKey(6) && arguments.number_vars[6] is VarString){
                    newSignal.verb = (string)(VarString)arguments.number_vars[6];
                }

                if(arguments.string_vars.ContainsKey("language") && arguments.string_vars["language"] is VarString){
                    newSignal.language = (string)(VarString)arguments.string_vars["verb"];
                }else if(arguments.number_vars.ContainsKey(7) && arguments.number_vars[7] is VarString){
                    newSignal.language = (string)(VarString)arguments.number_vars[7];
                }



                scope.program.signals.Enqueue(newSignal);
            });
            return tcomm_VAR;
        }

        public static VarList Math(){
            VarList math_VAR = new VarList();
            Dictionary<string, Var> math = math_VAR.string_vars;
            math["sin"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = System.Math.Sin((double)(VarNumber)arguments.number_vars[0]);
            });
            math["cos"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = System.Math.Cos((double)(VarNumber)arguments.number_vars[0]);
            });
            math["tan"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = System.Math.Tan((double)(VarNumber)arguments.number_vars[0]);
            });
            math["asin"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = System.Math.Asin((double)(VarNumber)arguments.number_vars[0]);
            });
            math["acos"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = System.Math.Acos((double)(VarNumber)arguments.number_vars[0]);
            });
            math["atan"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(1))
                    returnTarget[returnID] = System.Math.Atan2((double)(VarNumber)arguments.number_vars[0],(double)(VarNumber)arguments.number_vars[1]);
                else
                    returnTarget[returnID] = System.Math.Atan((double)(VarNumber)arguments.number_vars[0]);
            });
            math["floor"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(1)){
                    double e = System.Math.Pow(10,(double)(VarNumber)arguments.number_vars[1]);
                    returnTarget[returnID] = System.Math.Floor(((double)(VarNumber)arguments.number_vars[0])*e)/e;
                }else
                    returnTarget[returnID] = System.Math.Floor((double)(VarNumber)arguments.number_vars[0]);
            });
            math["ceil"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(1)){
                    double e = System.Math.Pow(10,(double)(VarNumber)arguments.number_vars[1]);
                    returnTarget[returnID] = System.Math.Ceiling(((double)(VarNumber)arguments.number_vars[0])*e)/e;
                }else
                    returnTarget[returnID] = System.Math.Ceiling((double)(VarNumber)arguments.number_vars[0]);
            });
            math["round"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(1)){
                    double e = System.Math.Pow(10,(double)(VarNumber)arguments.number_vars[1]);
                    returnTarget[returnID] = System.Math.Round(((double)(VarNumber)arguments.number_vars[0])*e)/e;
                }else
                    returnTarget[returnID] = System.Math.Round((double)(VarNumber)arguments.number_vars[0]);
            });
            math["max"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                State state = new State();
                scope.callstack.Push(new DoLater(delegate{
                    if(state.returns[0] is VarNumber && 0==(double)(VarNumber)state.returns[0]){
                        returnTarget[returnID] = arguments.number_vars[0];
                    }else{
                        returnTarget[returnID] = arguments.number_vars[1];
                    }
                }));
                scope.parser.Math(state.returns, 0, arguments.number_vars[0], arguments.number_vars[1], "lt");
            });
            math["min"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                State state = new State();
                scope.callstack.Push(new DoLater(delegate{
                    if(state.returns[0] is VarNumber && 0!=(double)(VarNumber)state.returns[0]){
                        returnTarget[returnID] = arguments.number_vars[0];
                    }else{
                        returnTarget[returnID] = arguments.number_vars[1];
                    }
                }));
                scope.parser.Math(state.returns, 0, arguments.number_vars[0], arguments.number_vars[1], "lt");
            });
            math["clamp"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = System.Math.Clamp((double)(VarNumber)arguments.number_vars[0], (double)(VarNumber)arguments.number_vars[1], (double)(VarNumber)arguments.number_vars[2]);
            });
            System.Random rng = new System.Random();
            math["random"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                double v = rng.NextDouble();
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarNumber){
                    double a = (double)(VarNumber)arguments.number_vars[0];
                    if(arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarNumber){
                        double b = (double)(VarNumber)arguments.number_vars[1];
                        returnTarget[returnID] = System.Math.Round(a + v * (b-a));
                    }else{
                        returnTarget[returnID] = System.Math.Round(v * a);
                    }
                }else{
                    returnTarget[returnID] = v;
                }
            });
            math["abs"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = System.Math.Abs((double)(VarNumber)arguments.number_vars[0]);
            });
            math["deg"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = (double)(VarNumber)arguments.number_vars[0]/System.Math.PI*180;
            });
            math["rad"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = (double)(VarNumber)arguments.number_vars[0]/180*System.Math.PI;
            });
            math["pi"] = System.Math.PI;
            
            return math_VAR;
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
            str["sub"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(2))
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).Substring((int)(VarNumber)arguments.number_vars[1], (int)(VarNumber)arguments.number_vars[2]);
                else
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).Substring((int)(VarNumber)arguments.number_vars[1]);
            });

            str["match"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        Match m = matcher.Match(haystack);
                        if(m.Success){
                            if(m.Groups.Count == 1){
                                returnTarget[returnID] = m.Groups[0].Value;
                            }else{
                                VarList outList = new VarList();
                                for(int i=0; i < m.Groups.Count; i++){
                                    outList.string_vars[m.Groups[i].Name] = outList.number_vars[i] = m.Groups[i].Value;
                                }
                                returnTarget[returnID] = outList;
                            }
                        }
                    }catch{}
                }
            });

            str["gmatch"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        MatchCollection M = matcher.Matches(haystack);
                        int i = 0;
                        returnTarget[returnID] = new VarFunction(delegate(Scope nscope, Dictionary<int, Var> nreturnTarget, int nreturnID, VarList narguments){
                            if(i < M.Count){
                                Match m = M[i];
                                if(m.Groups.Count == 1){
                                    nreturnTarget[nreturnID] = m.Groups[0].Value;
                                }else{
                                    VarList outList = new VarList();
                                    for(int c=0; c < m.Groups.Count; c++){
                                        outList.string_vars[m.Groups[c].Name] = outList.number_vars[c] = m.Groups[c].Value;
                                    }
                                    nreturnTarget[nreturnID] = outList;
                                }
                            }else{
                                nreturnTarget[nreturnID] = Var.nil;
                            }
                            i++;
                        });
                    }catch{}
                }
            });

            str["gsub"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString && arguments.number_vars.ContainsKey(2) && arguments.number_vars[2] is VarString){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        returnTarget[returnID] = matcher.Replace(haystack, (string)(VarString)arguments.number_vars[2]);
                    }catch{}
                }else if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString && arguments.number_vars.ContainsKey(2) && arguments.number_vars[2] is VarFunction){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    VarFunction replace_func = (VarFunction)arguments.number_vars[2];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        MatchCollection M = matcher.Matches(haystack);
                        string built = "";
                        int lastIndex = 0;
                        int i = 0;
                        DoLater loopfnc = null;
                        State s_state = new State();
                        loopfnc = new DoLater(delegate{
                            if(i < M.Count){
                                scope.callstack.Push(loopfnc);
                                Match m = M[i];
                                if(i > 0){
                                    if(s_state.returns[0] is VarString){
                                        built += (string)(VarString)s_state.returns[0];
                                    }
                                }
                                built += haystack.Substring(lastIndex, m.Index - lastIndex);
                                lastIndex = m.Index + m.Groups[0].Length;
                                if(m.Groups.Count == 1){
                                    replace_func.Call(scope, s_state.returns, 0, m.Groups[0].Value);
                                }else{
                                    VarList outList = new VarList();
                                    for(int c=0; c < m.Groups.Count; c++){
                                        outList.string_vars[m.Groups[c].Name] = outList.number_vars[c] = m.Groups[c].Value;
                                    }
                                    replace_func.Call(scope, s_state.returns, 0, outList);
                                }
                                i++;
                            }else{
                                if(i > 0){
                                    if(s_state.returns[0] is VarString){
                                        built += (string)(VarString)s_state.returns[0];
                                    }
                                }
                                built += haystack.Substring(lastIndex, haystack.Length - lastIndex);
                                returnTarget[returnID] = built;
                            }
                        });
                        scope.callstack.Push(loopfnc);
                    }catch{}
                }
            });            

            str["upper"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).ToUpper();
                }
            });
            str["lower"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).ToLower();
                }
            });
            str["reverse"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    char[] arr =  ((string)(VarString)arguments.number_vars[0]).ToCharArray();
                    System.Array.Reverse(arr);
                    returnTarget[returnID] = new string(arr);
                }
            });

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

        public static VarList Net(){
            VarList net_VAR = new VarList();
            Dictionary<string, Var> net = net_VAR.string_vars;
            net["connections"] = new VarList();

            net["subscribe"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                string hook = (string)(VarString)arguments.number_vars[0];
                VarEvent toHook;
                if(net["connections"].string_vars.ContainsKey(hook)){
                    toHook = (VarEvent)net["connections"].string_vars[hook];
                }else{
                    net["connections"].string_vars[hook] = toHook = new VarEvent();
                }
                returnTarget[returnID] = toHook;
            });

            net["message"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                string hook = (string)(VarString)arguments.number_vars[0];
                Listener.subspace_messages.Enqueue(new System.Tuple<string, Var>(hook, arguments.number_vars.ContainsKey(1)?arguments.number_vars[1]:Var.nil));
                foreach(KeyValuePair<int, Program> kv in Listener.programs){
                    var their_net = kv.Value.scope.globals.meta.string_vars["_parent"].string_vars["net"].string_vars;
                    if(their_net["connections"].string_vars.ContainsKey(hook)){
                        Variable.VarList args = new Variable.VarList();
                        if(arguments.number_vars.ContainsKey(1)){
                            args.number_vars[0] = arguments.number_vars[1];
                            args.string_vars["message"] = arguments.number_vars[1];
                        }
                        their_net["connections"].string_vars[hook].Call(kv.Value.scope, new Dictionary<int, Variable.Var>(), 0, args);
                    }
                }
                returnTarget[returnID] = arguments.number_vars.ContainsKey(1) ? arguments.number_vars[1] : Var.nil;
            });

            return net_VAR;
        }
    }
}