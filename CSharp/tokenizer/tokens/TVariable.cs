using ByondLang.Variable;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ByondLang.Tokenizer{
    public class TVariable : TExpression{
        private static Regex regexLocal = new Regex(@"var|local");
        TIdentifier identifier;
        bool isLocal = false;

        public static new TVariable Claim(StringClaimer claimer){
            Claim c = claimer.failPoint();
            Claim isLocal = claimer.Claim(regexLocal);
            TIdentifier ident = TIdentifier.Claim(claimer);
            if(ident == null){
                c.Fail();
                return null;
            }
            TVariable nVariable = new TVariable();
            nVariable.identifier = ident;
            nVariable.isLocal = isLocal.success;
            return nVariable;

        }

        public virtual void Get(Scope scope, CallTarget target, System.Action<Var> callback){
            target.variables.Get(scope, identifier.name, isLocal, false, callback);
        }

        public virtual void Set(Scope scope, CallTarget target, Var value, System.Action<Var> callback){
            target.variables.Set(scope, identifier.name, value, isLocal, callback);
        }

        public override void Parse(Scope scope, CallTarget target, System.Action<Var> callback){
            Get(scope, target, callback);
        }
    }

    public class TIndex : TVariable {
        private static Regex regexCurry = new Regex(@"::");
        private static Regex regexDot = new Regex(@"\.");
        private static Regex regexSquareLeft = new Regex(@"\[");
        private static Regex regexSquareRight = new Regex(@"\]");

        bool isCurry = false;
        TExpression left;

        // Splitting this into two variables is saner and faster.
        TExpression indexExpr;
        TIdentifier indexIdent;

        public static new TIndex RightClaim(TExpression left, StringClaimer claimer){
            Claim c;
            c = claimer.Claim(regexSquareLeft);
            if(c.success){
                TExpression indexExpr = TExpression.Claim(claimer);
                if(indexExpr is null){
                    c.Fail();
                    return null;
                }
                if(!claimer.Claim(regexSquareRight).success){
                    c.Fail();
                    return null;
                }
                TIndex nIndex = new TIndex();
                nIndex.left = left;
                nIndex.indexExpr = indexExpr;
                return nIndex;
            }

            c = claimer.Claim(regexDot);
            if(c.success){
                TIdentifier indexIdent = TIdentifier.Claim(claimer);
                if(indexIdent is null){
                    c.Fail();
                    return null;
                }
                TIndex nIndex = new TIndex();
                nIndex.left = left;
                nIndex.indexIdent = indexIdent;
                return nIndex;
            }

            c = claimer.Claim(regexCurry);
            if(c.success){
                TIdentifier indexIdent = TIdentifier.Claim(claimer);
                if(indexIdent is null){
                    c.Fail();
                    return null;
                }
                TIndex nIndex = new TIndex();
                nIndex.left = left;
                nIndex.indexIdent = indexIdent;
                nIndex.isCurry = true;
                return nIndex;
            }
            return null;
        }

        public override void Get(Scope scope, CallTarget target, System.Action<Var> callback){
            left.Parse(scope, target, vLeft => {
                if(indexExpr is null){
                    vLeft.Get(scope, indexIdent.name, false, isCurry, callback);
                }else{
                    indexExpr.Parse(scope, target, indexVal => {
                        vLeft.Get(scope, indexVal, false, false, callback);
                    });
                }
            });
        }

        public override void Set(Scope scope, CallTarget target, Var value, System.Action<Var> callback){
            left.Parse(scope, target, vLeft => {

            });
        }
    }

    public class TIdentifier : Token{
        private static Regex regexIdentifier = new Regex(@"[a-zA-Z_]\w*");
        public string name;
        public static TIdentifier Claim(StringClaimer claimer){
            Claim c = claimer.Claim(regexIdentifier);
            if(!c.success)
                return null;
            TIdentifier nIdentifier = new TIdentifier();
            nIdentifier.name = c.GetText();
            return nIdentifier;
        }
    }
}