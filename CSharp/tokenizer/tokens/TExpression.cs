using ByondLang.Variable;

namespace ByondLang.Tokenizer{
    public abstract class TExpression{
        public static TExpression Claim(StringClaimer claimer){
            TExpression curExp = LeftClaim(claimer);
            if(curExp == null)
                return null;
            
            // Allows chaining of expressions. This is how we produce things like...
            // (call())["foo"]
            TExpression rightExp;
            while(!((rightExp = RightClaim(curExp, claimer)) is null))
                curExp = rightExp;
            return curExp;
        }

        public static TExpression LeftClaim(StringClaimer claimer){
            return TWith.Claim(claimer)     as TExpression ??
                   TSwitch.Claim(claimer)   as TExpression ??
                   TReturn.Claim(claimer)   as TExpression ??
                   TForIn.Claim(claimer)    as TExpression ??
                   TFor.Claim(claimer)      as TExpression;
        }

        public static TExpression RightClaim(TExpression left, StringClaimer claimer){
            return TIndex.RightClaim(left, claimer) as TExpression;
        }

        public abstract void Parse(Scope scope, CallTarget target, System.Action<Var> callback);
    }
}