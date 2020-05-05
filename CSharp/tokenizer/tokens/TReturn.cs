using ByondLang.Variable;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ByondLang.Tokenizer{
    public class TReturn : TExpression{
        private static Regex regexReturn = new Regex(@"return");
        private static Regex regexBreak = new Regex(@"break");
        TExpression action;
        ReturnType retType = ReturnType.Return;

        public static new TReturn Claim(StringClaimer claimer){
            if(claimer.Claim(regexReturn).success){
                TReturn nReturn = new TReturn();
                nReturn.action = TExpression.Claim(claimer); // Entirely optional.
                return nReturn;
            }
            
            if(claimer.Claim(regexBreak).success){
                TReturn nReturn = new TReturn();
                nReturn.retType = ReturnType.Break;
                nReturn.action = TExpression.Claim(claimer); // Entirely optional.
                return nReturn;
            }

            return null;
        }

        public override void Parse(Scope scope, CallTarget target, System.Action<Var> callback){
            if(action != null){
                action.Parse(scope, target, val => {
                    scope.returnsStack.Push(new KeyValuePair<ReturnType, Var>(retType, val));
                    callback(val);
                });
            }else{
                scope.returnsStack.Push(new KeyValuePair<ReturnType, Var>(retType, Var.nil));
                callback(Var.nil);
            }
        }
    }
}