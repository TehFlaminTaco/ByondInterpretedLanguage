using ByondLang.Variable;
using System.Collections.Generic;

namespace ByondLang{
    public static class LibNet{
        public static VarList Generate(VarList globals){
            VarList net_VAR = new VarList();
            Dictionary<string, Var> net = net_VAR.string_vars;
            net["connections"] = new VarList();

            net["subscribe"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                string hook = (string)(VarString)arguments.number_vars[0];
                VarEvent toHook;
                if(net["connections"].string_vars.ContainsKey(hook)){
                    toHook = (VarEvent)net["connections"].string_vars[hook];
                }else{
                    net["connections"].string_vars[hook] = toHook = new VarEvent();
                }
                callback(toHook);
            });

            net["message"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                string hook = (string)(VarString)arguments.number_vars[0];
                Listener.subspace_messages.Enqueue(new System.Tuple<string, Var>(hook, arguments.number_vars.ContainsKey(1)?arguments.number_vars[1]:Var.nil));
                foreach(KeyValuePair<int, Program> kv in Listener.programs){
                    var their_net = kv.Value.scope.globals.meta.string_vars["_parent"].string_vars["net"].string_vars;
                    if(their_net["connections"].string_vars.ContainsKey(hook)){
                        Variable.VarList args = new Variable.VarList();
                        if(arguments.number_vars.ContainsKey(1)){
                            args.number_vars[0] = arguments.number_vars[1];
                            args.string_vars["message"] = arguments.number_vars[1];
                        }
                        their_net["connections"].string_vars[hook].Call(kv.Value.scope, args, v=>{});
                    }
                }
                callback(arguments.number_vars.ContainsKey(1) ? arguments.number_vars[1] : Var.nil);
            });

            return net_VAR;
        }
    }
}