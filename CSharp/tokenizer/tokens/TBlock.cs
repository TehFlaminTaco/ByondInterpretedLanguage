using ByondLang.Variable;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ByondLang.Tokenizer{
    public class TBlock : TExpression{
        private static Regex regexLeftBrace = new Regex(@"\{");
        private static Regex regexRightBrace = new Regex(@"\}");
        
        List<TExpression> tExpressions = new List<TExpression>();
        public static new TBlock Claim(StringClaimer claimer){
            Claim c = claimer.Claim(regexLeftBrace);
            if(!c.success)
                return null;
            TBlock nBlock = new TBlock();

            TExpression lastExp;
            while(!((lastExp = TExpression.Claim(claimer)) is null))
                nBlock.tExpressions.Add(lastExp);
            
            if(!claimer.Claim(regexRightBrace).success){
                c.Fail();
                return null;
            }
            
            return nBlock;
        }

        public override void Parse(Scope scope, CallTarget target, System.Action<Var> callback){

        }
    }
}