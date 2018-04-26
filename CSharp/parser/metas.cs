using ByondLang.Variable;
using System.Collections.Generic;
using System;
namespace ByondLang{
    class Metas{

        private static Function GenericConcat = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){

                State state = new State();
                scope.callstack.Push(new DoLater(delegate{
                    if(!(state.returns[0] is Variable.String)){
                        returnTarget[returnID] = Var.nil;return;
                    }
                    if(!(state.returns[1] is Variable.String)){
                        returnTarget[returnID] = Var.nil;return;
                    }
                    returnTarget[returnID] = ((Variable.String)state.returns[0]).data + ((Variable.String)state.returns[1]).data;
                }));
                if(arguments.number_vars[0] is Variable.String){
                    state.returns[0] = arguments.number_vars[0];
                }else{
                    arguments.number_vars[0].ToString(scope, state.returns, 0);
                }
                if(arguments.number_vars[1] is Variable.String){
                    state.returns[1] = arguments.number_vars[1];
                }else{
                    arguments.number_vars[1].ToString(scope, state.returns, 1);
                }
            });
        
        private static Function ReturnOne = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            returnTarget[returnID] = 1;
        });
        private static Function ReturnZero = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            returnTarget[returnID] = 0;
        });

        private static Function And = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            State state = new State();
            scope.callstack.Push(new DoLater(delegate{
                if(0.0d != (double)(Number)state.returns[0]){
                    returnTarget[returnID] = arguments.number_vars[1];
                }else{
                    returnTarget[returnID] = arguments.number_vars[0];
                }
            }));
            arguments.number_vars[0].ToBool(scope, state.returns, 0);
        });

        private static Function Or = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            State state = new State();
            scope.callstack.Push(new DoLater(delegate{
                if(0.0d == (double)(Number)state.returns[0]){
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

            number["_add[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data + right.data;
            });
            number["_sub[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data - right.data;
            });
            number["_mult[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data * right.data;
            });
            number["_intdiv[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = (int)(left.data / right.data);
            });
            number["_div[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data / right.data;
            });
            number["_pow[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = Math.Pow(left.data,right.data);
            });
            number["_mod[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data % right.data;
            });
            number["bitor[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)|((int)right);
            });
            number["bitand[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)&((int)right);
            });
            number["bitxor[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)^((int)right);
            });
            number["bitshiftl[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)<<((int)right);
            });
            number["bitshiftr[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = ((int)left)>>((int)right);
            });
            number["_concat"] = GenericConcat;
            number["_le[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data <= right.data ? 1 : 0;
            });
            number["_lt[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data < right.data ? 1 : 0;
            });
            number["_ge[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data >= right.data ? 1 : 0;
            });
            number["_gt[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data > right.data ? 1 : 0;
            });
            number["_eq[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data == right.data ? 1 : 0;
            });
            number["_ne[number]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                Number right = (Number)arguments.number_vars[1];
                returnTarget[returnID] = left.data != right.data ? 1 : 0;
            });
            number["_eq"] = ReturnZero;
            number["_ne"] = ReturnOne;

            number["_not"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                returnTarget[returnID] = left.data==0?1:0;
            });
            number["_unm"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                returnTarget[returnID] = -left.data;
            });
            number["_len"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                returnTarget[returnID] = left.data;
            });
            number["_bitnot"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Number left = (Number)arguments.number_vars[0];
                returnTarget[returnID] = ~(int)left.data;
            });

            number["_and"] = And;
            number["_or"] = Or;

            number["_tostring"]  = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = new Variable.String(""+(double)(Number) arguments.number_vars[0]);
            });
            return outp;
        }

        public static VarList String(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> str = outp.string_vars;
            str["_add"] = GenericConcat;
            str["_concat"] = GenericConcat;
            str["_index"] = globals.string_vars["string"];
            str["_eq[string]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Variable.String left = (Variable.String)arguments.number_vars[0];
                Variable.String right = (Variable.String)arguments.number_vars[1];
                returnTarget[returnID] = left.data == right.data ? 1 : 0;
            });
            str["_ne[string]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Variable.String left = (Variable.String)arguments.number_vars[0];
                Variable.String right = (Variable.String)arguments.number_vars[1];
                returnTarget[returnID] = left.data != right.data ? 1 : 0;
            });
            str["_eq"] = ReturnZero;
            str["_ne"] = ReturnOne;

            str["_len"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Variable.String left = (Variable.String)arguments.number_vars[0];
                returnTarget[returnID] = left.data.Length;
            });

            str["_and"] = And;
            str["_or"] = Or;

            str["_tostring"]  = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[0];
            });
            return outp;
        }

        public static VarList Function(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> function = outp.string_vars;
            function["_concat"] = GenericConcat;
            function["_eq[function]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Variable.Function left = (Variable.Function)arguments.number_vars[0];
                Variable.Function right = (Variable.Function)arguments.number_vars[1];
                returnTarget[returnID] = left == right ? 1 : 0;
            });
            function["_ne[function]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Function left = (Function)arguments.number_vars[0];
                Function right = (Function)arguments.number_vars[1];
                returnTarget[returnID] = left != right ? 1 : 0;
            });
            function["_eq"] = ReturnZero;
            function["_ne"] = ReturnOne;

            function["_and"] = And;
            function["_or"] = Or;

            function["_tostring"]  = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = ((Function)arguments.number_vars[0]).FunctionText;
            });
            return outp;
        }

        public static VarList List(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> list = outp.string_vars;
            list["_add[0]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList ls = (VarList)arguments.number_vars[0];
                int max_key = 0;
                while(ls.number_vars.ContainsKey(max_key))
                    max_key++;
                ls.number_vars[max_key] = arguments.number_vars[1];
            });
            list["_add[1]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList ls = (VarList)arguments.number_vars[1];
                int max_key = 0;
                while(ls.number_vars.ContainsKey(max_key))
                    max_key++;
                ls.number_vars[max_key] = arguments.number_vars[0];
            });
            list["_concat"] = GenericConcat;
            list["_eq[list]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList left = (VarList)arguments.number_vars[0];
                VarList right = (VarList)arguments.number_vars[1];
                returnTarget[returnID] = left == right ? 1 : 0;
            });
            list["_ne[list]"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList left = (VarList)arguments.number_vars[0];
                VarList right = (VarList)arguments.number_vars[1];
                returnTarget[returnID] = left != right ? 1 : 0;
            });
            list["_eq"] = ReturnZero;
            list["_ne"] = ReturnOne;
            list["_index"] = globals.string_vars["table"];

            list["_len"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                VarList left = (VarList)arguments.number_vars[0];
                int i=0;
                while(left.number_vars.ContainsKey(i))
                    i++;
                returnTarget[returnID] = i;
            });

            list["_and"] = And;
            list["_or"] = Or;

            list["_tostring"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                
            });
            return outp;
        }

        public static VarList Event(VarList globals){
            VarList outp = new VarList();
            Dictionary<string, Var> evnt = outp.string_vars;
            evnt["_call"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Event ths = (Event)arguments.number_vars[0];
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
                    for(int c=1; arguments.number_vars.ContainsKey(c); c++){
                        newArgs.number_vars[c-1] = arguments.number_vars[c];
                    }
                    ths.callbacks[i].Call(scope, state.returns, 0, newArgs);
                    i++;
                });
                scope.callstack.Push(toDo);
            });

            evnt["_and"] = And;
            evnt["_or"] = Or;

            evnt["_tostring"] = new Function(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = "<Event>";
            });
            return outp;
        }
    }
}