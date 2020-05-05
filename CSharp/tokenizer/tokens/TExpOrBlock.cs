using ByondLang.Variable;
using System.Text.RegularExpressions;

namespace ByondLang.Tokenizer{
    public abstract class TExpOrBlock : Token{
        public static TExpression Claim(StringClaimer claimer){
            return  TBlock.Claim(claimer) as TExpression ??
                    TExpression.Claim(claimer) as TExpression;
        }
    }
}