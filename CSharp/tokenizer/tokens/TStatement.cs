using ByondLang.Variable;
using System.Text.RegularExpressions;

namespace ByondLang.Tokenizer{
    public static class TStatement{
        private static Regex regexSemiColon = new Regex("@;");
        public static TExpression Claim(StringClaimer claimer){
            TExpression chunk = TExpression.Claim(claimer);
            claimer.Claim(regexSemiColon);
            return chunk;
        }
    }
}