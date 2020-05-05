using ByondLang.Language.Variable;
using System.Collections.Generic;

namespace ByondLang.Language
{
    public static class LibMath{
        public static VarList Generate(VarList globals){
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
    }
}