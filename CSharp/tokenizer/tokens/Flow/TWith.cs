using ByondLang.Variable;
using System.Text.RegularExpressions;

namespace ByondLang.Tokenizer{
    public class TWith : TExpression{
        private static Regex regexWith = new Regex(@"with");

        TExpression tList = null;
        TExpression tBlock = null;

        public static new TWith Claim(StringClaimer claimer){
            Claim c = claimer.Claim(regexWith);
            if(!c.success)
                return null;
            
            TExpression list = TExpression.Claim(claimer);
            if(list == null){
                c.Fail();
                return null;
            }

            TExpression expOrBlock = TExpOrBlock.Claim(claimer);
            if(list == null){
                c.Fail();
                return null;
            }

            TWith nWith = new TWith();
            nWith.tList = list;
            nWith.tBlock = expOrBlock;
            return nWith;
        }

        public override void Parse(Scope scope, CallTarget target, System.Action<Var> callback){
            tList.Parse(scope, target, v => {
                if (v is VarList list){
                    VarList newScopeThing = new VarList();
                    newScopeThing.meta = new VarList();
                    newScopeThing.meta.string_vars["_index"] = new VarFunction(delegate(Scope subScope, VarList arguments, System.Action<Var> subCallback){
                        list.Get(scope, arguments.number_vars[1], false, false, listVar=>{
                            if(listVar != null){
                                subCallback(listVar);
                            }else{
                                target.variables.Get(scope, arguments.number_vars[1], false, false, subCallback);
                            }
                        });
                    });
                    newScopeThing.meta.string_vars["_newindex"] = new VarFunction(delegate(Scope subScope, VarList arguments, System.Action<Var> subCallback){
                        list.Set(scope, arguments.number_vars[1], arguments.number_vars[2], false, subCallback);
                    });
                }
            });
        }
    }
}