using ByondLang.Variable;
using System.Collections.Generic;

namespace ByondLang{
    public static class LibMath{
        public static VarList Generate(VarList globals){
            VarList math_VAR = new VarList();
            Dictionary<string, Var> math = math_VAR.string_vars;
            math["sin"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback(System.Math.Sin((double)(VarNumber)arguments.number_vars[0]));
            });
            math["cos"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback(System.Math.Cos((double)(VarNumber)arguments.number_vars[0]));
            });
            math["tan"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback(System.Math.Tan((double)(VarNumber)arguments.number_vars[0]));
            });
            math["asin"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback(System.Math.Asin((double)(VarNumber)arguments.number_vars[0]));
            });
            math["acos"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback(System.Math.Acos((double)(VarNumber)arguments.number_vars[0]));
            });
            math["atan"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                if(arguments.number_vars.ContainsKey(1))
                    callback(System.Math.Atan2((double)(VarNumber)arguments.number_vars[0],(double)(VarNumber)arguments.number_vars[1]));
                else
                    callback(System.Math.Atan((double)(VarNumber)arguments.number_vars[0]));
            });
            math["floor"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                if(arguments.number_vars.ContainsKey(1)){
                    double e = System.Math.Pow(10,(double)(VarNumber)arguments.number_vars[1]);
                    callback(System.Math.Floor(((double)(VarNumber)arguments.number_vars[0])*e)/e);
                }else
                    callback(System.Math.Floor((double)(VarNumber)arguments.number_vars[0]));
            });
            math["ceil"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                if(arguments.number_vars.ContainsKey(1)){
                    double e = System.Math.Pow(10,(double)(VarNumber)arguments.number_vars[1]);
                    callback(System.Math.Ceiling(((double)(VarNumber)arguments.number_vars[0])*e)/e);
                }else
                    callback(System.Math.Ceiling((double)(VarNumber)arguments.number_vars[0]));
            });
            math["round"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                if(arguments.number_vars.ContainsKey(1)){
                    double e = System.Math.Pow(10,(double)(VarNumber)arguments.number_vars[1]);
                    callback(System.Math.Round(((double)(VarNumber)arguments.number_vars[0])*e)/e);
                }else
                    callback(System.Math.Round((double)(VarNumber)arguments.number_vars[0]));
            });
            math["max"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                scope.parser.Math(arguments.number_vars[0], arguments.number_vars[1], "lt", val => {
                    if(val is VarNumber n && n.data == 0) // Replace with isBool?
                        callback(arguments.number_vars[0]);
                    else
                        callback(arguments.number_vars[1]);
                });
            });
            math["min"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                scope.parser.Math(arguments.number_vars[0], arguments.number_vars[1], "lt", val => {
                    if(val is VarNumber n && n.data == 0) // Replace with isBool?
                        callback(arguments.number_vars[1]);
                    else
                        callback(arguments.number_vars[0]);
                });
            });
            math["clamp"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback(System.Math.Clamp((double)(VarNumber)arguments.number_vars[0], (double)(VarNumber)arguments.number_vars[1], (double)(VarNumber)arguments.number_vars[2]));
            });
            System.Random rng = new System.Random();
            math["random"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                double v = rng.NextDouble();
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarNumber){
                    double a = (double)(VarNumber)arguments.number_vars[0];
                    if(arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarNumber){
                        double b = (double)(VarNumber)arguments.number_vars[1];
                        callback(System.Math.Round(a + v * (b-a)));
                    }else{
                        callback(System.Math.Round(v * a));
                    }
                }else{
                    callback(v);
                }
            });
            math["abs"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback(System.Math.Abs((double)(VarNumber)arguments.number_vars[0]));
            });
            math["deg"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback((double)(VarNumber)arguments.number_vars[0]/System.Math.PI*180);
            });
            math["rad"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                callback((double)(VarNumber)arguments.number_vars[0]/180*System.Math.PI);
            });
            math["pi"] = System.Math.PI;
            
            return math_VAR;
        }
    }
}