using System.Collections.Generic;
using ByondLang;

namespace ByondLang.Variable{
    class Var{

        public static Var nil = new Var();

        
        Dictionary<double, Var> number_vars = new Dictionary<double, Var>();
        Dictionary<string, Var> string_vars = new Dictionary<string, Var>();
        Dictionary<Var, Var> other_vars = new Dictionary<Var, Var>();


        public Var meta = nil;

        public string type = "nil";

        public Var this[Var key]{
            get{
                if(key is Number){
                    double k = ((Number)key).data;
                    if(number_vars.ContainsKey(k))
                        return number_vars[k];
                    else
                        return nil;
                }
                if(key is String){
                    string k = ((String)key).data;
                    if(string_vars.ContainsKey(k))
                        return string_vars[k];
                    else
                        return nil;
                }
                if(other_vars.ContainsKey(key))
                    return other_vars[key];
                else
                    return nil;
            }
            set{
                if(key is Number){
                    number_vars[((Number)key).data] = value;
                    return;
                }
                if(key is String){
                    string_vars[((String)key).data] = value;
                    return;
                }
                other_vars[key] = value;
            }
        }

        public Var this[double key]{
            get{
                if(number_vars.ContainsKey(key))
                    return number_vars[key];
                else
                    return nil;
            }
            set{
                number_vars[key] = value;
            }
        }

        public Var this[string key]{
            get{
                if(string_vars.ContainsKey(key))
                    return string_vars[key];
                else
                    return nil;
            }
            set{
                string_vars[key] = value;
            }
        }

        public static implicit operator Var(double d){
            return new Number(d);
        }
        public static implicit operator Var(string s){
            return new String(s);
        }
    }

    class Number : Var{
        public double data = 0;
        public Number(double data) : this(){
            this.data = data;
            type = "number";
        }

        public Number(){
            meta = Meta.Number;
        }

        public static explicit operator double(Number var){
            return var.data;
        }
    }

    class String : Var{
        public string data = "";
        public String(string data) : this(){
            this.data = data;
            type = "string";
        }

        public String(){
            meta = Meta.String;
        }

        public static explicit operator string(String var){
            return var.data;
        }
    }

    class Function : Var{

        public Function(){
            meta = Meta.Function;
            type = "function";
        }

        public Var Call(List<Var> arguments){
            return Var.nil;
        }
    }

    class VarList : Var{

        public VarList(){
            meta = Meta.List;
            type = "list";
        }
    }
}