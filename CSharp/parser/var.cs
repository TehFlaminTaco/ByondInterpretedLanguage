using System.Collections.Generic;

namespace ByondLang.Variable{
    class Var{

        public static Var nil = new Var();

        Dictionary<Var, Var> vars = new Dictionary<Var, Var>();
    }

    class Number : Var{
        double data = 0;
        public Number(double data){
            this.data = data;
        }

        public Number(){

        }
    }

    class String : Var{
        string data = "";
        public String(string data){
            this.data = data;
        }

        public String(){

        }
    }

    class Function : Var{
        public Var Call(List<Var> arguments){
            return Var.nil;
        }
    }
}