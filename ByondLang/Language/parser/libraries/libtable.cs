using ByondLang.Language.Variable;
using System.Collections.Generic;

namespace ByondLang.Language
{
    public static class LibTable{
        public static VarList Generate(VarList globals){
            VarList table_VAR = new VarList();
            Dictionary<string, Var> table = table_VAR.string_vars;
            table["set_meta"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                arguments.number_vars[0].meta = arguments.number_vars[1];
                returnTarget[returnID] = arguments.number_vars[0];
            });
            table["get_meta"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = arguments.number_vars[0].meta;
            });
            return table_VAR;
        }
    }
}