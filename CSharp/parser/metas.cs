using ByondLang.Variable;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
namespace ByondLang{
    class Metas{

        private static VarFunction GenericConcat = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){

                State state = new State();
                scope.callstack.Push(new DoLater(delegate{
                    if(!(state.returns[0] is VarString)){
                        returnTarget[returnID] = Var.nil;return;
                    }
                    if(!(state.returns[1] is VarString)){
                        returnTarget[returnID] = Var.nil;return;
                    }
                    returnTarget[returnID] = ((VarString)state.returns[0]).data + ((VarString)state.returns[1]).data;
                }));
                if(arguments.number_vars[0] is VarString){
                    state.returns[0] = arguments.number_vars[0];
                }else{
                    arguments.number_vars[0].ToString(scope, state.returns, 0);
                }
                if(arguments.number_vars[1] is VarString){
                    state.returns[1] = arguments.number_vars[1];
                }else{
                    arguments.number_vars[1].ToString(scope, state.returns, 1);
                }
            });
        
        private static VarFunction ReturnOne = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            returnTarget[returnID] = 1;
        });
        private static VarFunction ReturnZero = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            returnTarget[returnID] = 0;
        });

        private static VarFunction And = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            State state = new State();
            scope.callstack.Push(new DoLater(delegate{
                if(0.0d != (double)(VarNumber)state.returns[0]){
                    returnTarget[returnID] = arguments.number_vars[1];
                }else{
                    returnTarget[returnID] = arguments.number_vars[0];
                }
            }));
            arguments.number_vars[0].ToBool(scope, state.returns, 0);
        });

        private static VarFunction Or = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            State state = new State();
            scope.callstack.Push(new DoLater(delegate{
                if(0.0d == (double)(VarNumber)state.returns[0]){
                    returnTarget[returnID] = arguments.number_vars[1];
                }else{
                    returnTarget[returnID] = arguments.number_vars[0];
                }
            }));
            arguments.number_vars[0].ToBool(scope, state.returns, 0);
        });

        public static VarList Number(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> number = outp.string_vars;

            number["_add[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data + right.data;
            });
            number["_sub[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data - right.data;
            });
            number["_mult[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data * right.data;
            });
            number["_intdiv[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = (int)(left.data / right.data);
            });
            number["_div[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data / right.data;
            });
            number["_pow[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = Math.Pow(left.data,right.data);
            });
            number["_mod[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data % right.data;
            });
            number["_bitor[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)|((int)right);
            });
            number["_bitand[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)&((int)right);
            });
            number["_bitxor[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)^((int)right);
            });
            number["_bitshiftl[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)<<((int)right);
            });
            number["_bitshiftr[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)>>((int)right);
            });
            number["_concat"] = GenericConcat;
            number["_le[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data <= right.data ? 1 : 0;
            });
            number["_lt[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data < right.data ? 1 : 0;
            });
            number["_ge[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data >= right.data ? 1 : 0;
            });
            number["_gt[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data > right.data ? 1 : 0;
            });
            number["_eq[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data == right.data ? 1 : 0;
            });
            number["_ne[number]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                VarNumber right = (VarNumber)arguments.number_vars[1];
                returnTarget[returnID] = left.data != right.data ? 1 : 0;
            });
            number["_eq"] = ReturnZero;
            number["_ne"] = ReturnOne;

            number["_not"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                returnTarget[returnID] = left.data==0?1:0;
            });
            number["_unm"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                returnTarget[returnID] = -left.data;
            });
            number["_len"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                returnTarget[returnID] = left.data;
            });
            number["_bitnot"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarNumber left = (VarNumber)arguments.number_vars[0];
                returnTarget[returnID] = ~(int)left.data;
            });

            number["_and"] = And;
            number["_or"] = Or;

            number["_tostring"]  = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new VarString(""+(double)(VarNumber) arguments.number_vars[0]);
            });

            number["_tonumber"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[0];
            });
            return outp;
        }

        public static VarList String(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> str = outp.string_vars;
            str["_add"] = GenericConcat;
            str["_concat"] = GenericConcat;
            str["_index"] = globals.string_vars["string"];
            str["_eq[string]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarString left = (VarString)arguments.number_vars[0];
                VarString right = (VarString)arguments.number_vars[1];
                returnTarget[returnID] = left.data == right.data ? 1 : 0;
            });
            str["_ne[string]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarString left = (VarString)arguments.number_vars[0];
                VarString right = (VarString)arguments.number_vars[1];
                returnTarget[returnID] = left.data != right.data ? 1 : 0;
            });
            str["_eq"] = ReturnZero;
            str["_ne"] = ReturnOne;

            str["_len"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarString left = (VarString)arguments.number_vars[0];
                returnTarget[returnID] = left.data.Length;
            });

            str["_and"] = And;
            str["_or"] = Or;

            str["_tostring"]  = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[0];
            });
            str["_tonumber"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                double d = 0.0d;
                Double.TryParse((string)(VarString)arguments.number_vars[0], out d);
                returnTarget[returnID] = d;
            });
            return outp;
        }

        public static VarList Function(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> function = outp.string_vars;
            function["_concat"] = GenericConcat;
            function["_eq[function]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarFunction left = (VarFunction)arguments.number_vars[0];
                VarFunction right = (VarFunction)arguments.number_vars[1];
                returnTarget[returnID] = left == right ? 1 : 0;
            });
            function["_ne[function]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarFunction left = (VarFunction)arguments.number_vars[0];
                VarFunction right = (VarFunction)arguments.number_vars[1];
                returnTarget[returnID] = left != right ? 1 : 0;
            });
            function["_eq"] = ReturnZero;
            function["_ne"] = ReturnOne;

            function["_and"] = And;
            function["_or"] = Or;

            function["_tostring"]  = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = ((VarFunction)arguments.number_vars[0]).FunctionText;
            });
            return outp;
        }

        public static VarList List(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> list = outp.string_vars;
            list["_add[0]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList ls = (VarList)arguments.number_vars[0];
                int max_key = 0;
                while(ls.number_vars.ContainsKey(max_key))
                    max_key++;
                ls.number_vars[max_key] = arguments.number_vars[1];
            });
            list["_add[1]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList ls = (VarList)arguments.number_vars[1];
                int max_key = 0;
                while(ls.number_vars.ContainsKey(max_key))
                    max_key++;
                ls.number_vars[max_key] = arguments.number_vars[0];
            });
            list["_concat"] = GenericConcat;
            list["_eq[list]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList left = (VarList)arguments.number_vars[0];
                VarList right = (VarList)arguments.number_vars[1];
                returnTarget[returnID] = left == right ? 1 : 0;
            });
            list["_ne[list]"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList left = (VarList)arguments.number_vars[0];
                VarList right = (VarList)arguments.number_vars[1];
                returnTarget[returnID] = left != right ? 1 : 0;
            });
            list["_eq"] = ReturnZero;
            list["_ne"] = ReturnOne;
            list["_index"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                globals.string_vars["table"].Get(scope, returnTarget, returnID, arguments.number_vars[1], true, false);
            });

            list["_len"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList left = (VarList)arguments.number_vars[0];
                int i=0;
                while(left.number_vars.ContainsKey(i))
                    i++;
                returnTarget[returnID] = i;
            });

            list["_tonumber"] = list["_len"];

            list["_and"] = And;
            list["_or"] = Or;

            list["_tostring"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList ths = (VarList)arguments.number_vars[0];
                string outs = "{";
                string joiner = "";
                int maxCounted = 0;
                for(int i=0; ths.number_vars.ContainsKey(i); i++){
                    Var v = ths.number_vars[i];
                    
                    outs += joiner;
                    joiner = ", ";
                    
                    if (v is VarNumber){
                        outs += (double)(VarNumber)v;
                    }else if(v is VarString){
                        outs += (string)(VarString)v;
                    }else{
                        outs += v.type;
                    }

                    maxCounted = i;
                }
                foreach(KeyValuePair<double, Var> kv in ths.number_vars){
                    if(kv.Key%1!=0 || kv.Key < 0 || kv.Key > maxCounted){
                        Var v = kv.Value;
                        
                        outs += joiner + kv.Key + "=";
                        joiner = ", ";
                        
                        if (v is VarNumber){
                            outs += (double)(VarNumber)v;
                        }else if(v is VarString){
                            outs += (string)(VarString)v;
                        }else{
                            outs += v.type;
                        }
                    }
                }
                foreach(KeyValuePair<string, Var> kv in ths.string_vars){
                    Var v = kv.Value;
                    
                    outs += joiner + "\"" + Regex.Escape(kv.Key) + "\"=";
                    joiner = ", ";
                    
                    if (v is VarNumber){
                        outs += (double)(VarNumber)v;
                    }else if(v is VarString){
                        outs += (string)(VarString)v;
                    }else{
                        outs += v.type;
                    }
                }
                outs += "}";
                returnTarget[returnID] = outs;
            });
            return outp;
        }

        public static VarList Event(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> evnt = outp.string_vars;
            evnt["_call"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarEvent ths = (VarEvent)arguments.number_vars[0];
                int i = 0;
                State state = new State();
                state.returns[0] = Var.nil;
                DoLater toDo = null;
                toDo = new DoLater(delegate{
                    if(i >= ths.callbacks.Count){
                        return;
                    }
                    scope.callstack.Push(toDo);
                    VarList newArgs = new VarList();
                    foreach(KeyValuePair<Var,Var> kv in arguments){
                        if(kv.Key is VarNumber){
                            double k = (double)(VarNumber)kv.Key;
                            if(k>=0 && k%1 == 0){
                                if(k>0){
                                    newArgs.number_vars[k-1] = kv.Value;
                                }
                            }else{
                                newArgs.number_vars[k] = kv.Value;
                            }
                        }else if(kv.Key is VarString){
                            newArgs.string_vars[(string)(VarString)kv.Key] = kv.Value;
                        }else{
                            newArgs.other_vars[kv.Key] = kv.Value;
                        }
                    }
                    
                    ths.callbacks[i].Call(scope, state.returns, 0, newArgs);
                    i++;
                });
                scope.callstack.Push(toDo);
            });

            evnt["_and"] = And;
            evnt["_or"] = Or;

            evnt["_tostring"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = "<Event>";
            });
            return outp;
        }

        public static VarList Nil(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> nil = outp.string_vars;
            nil["_eq"] = ReturnZero;
            nil["_ne"] = ReturnOne;
            nil["_eq[nil]"] = ReturnOne;
            nil["_ne[nil]"] = ReturnZero;
            nil["_not"] = ReturnOne;

            nil["_and"] = And;
            nil["_or"] = Or;
            nil["_concat"] = GenericConcat;

            return outp;
        }
    }
}