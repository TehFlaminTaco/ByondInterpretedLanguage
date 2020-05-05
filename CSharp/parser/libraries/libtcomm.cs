using ByondLang.Variable;
using System.Collections.Generic;

namespace ByondLang{
    public static class LibTComm{
        public static VarList Generate(VarList globals){
            VarList tcomm_VAR = new VarList();
            Dictionary<string, Var> tcomm = tcomm_VAR.string_vars;
            tcomm["onmessage"] = new VarEvent();
            tcomm["broadcast"] = new VarFunction(delegate(Scope scope, VarList arguments, System.Action<Var> callback){
                Signal newSignal = new Signal("*beep*", "1459", "Telecomms Broadcaster", "Machine", "1", "says");
                if(arguments.string_vars.ContainsKey("content") && arguments.string_vars["content"] is VarString){
                    newSignal.content = (string)(VarString)arguments.string_vars["content"];
                }else if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    newSignal.content = (string)(VarString)arguments.number_vars[0];
                }

                if(arguments.string_vars.ContainsKey("source") && arguments.string_vars["source"] is VarString){
                    newSignal.source = (string)(VarString)arguments.string_vars["source"];
                }else if(arguments.number_vars.ContainsKey(2) && arguments.number_vars[1] is VarString){
                    newSignal.source = (string)(VarString)arguments.number_vars[1];
                }

                if(arguments.string_vars.ContainsKey("job") && arguments.string_vars["job"] is VarString){
                    newSignal.job = (string)(VarString)arguments.string_vars["job"];
                }else if(arguments.number_vars.ContainsKey(3) && arguments.number_vars[2] is VarString){
                    newSignal.job = (string)(VarString)arguments.number_vars[2];
                }

                if(arguments.string_vars.ContainsKey("freq") && arguments.string_vars["freq"] is VarString){
                    newSignal.freq = (string)(VarString)arguments.string_vars["freq"];
                }else if(arguments.number_vars.ContainsKey(3) && arguments.number_vars[3] is VarString){
                    newSignal.freq = (string)(VarString)arguments.number_vars[3];
                }

                if(arguments.string_vars.ContainsKey("pass") && arguments.string_vars["pass"] is VarString){
                    newSignal.pass = (string)(VarString)arguments.string_vars["pass"];
                }else if(arguments.number_vars.ContainsKey(4) && arguments.number_vars[4] is VarString){
                    newSignal.pass = (string)(VarString)arguments.number_vars[4];
                }

                if(arguments.string_vars.ContainsKey("ref") && arguments.string_vars["ref"] is VarString){
                    newSignal.reference = (string)(VarString)arguments.string_vars["ref"];
                }else if(arguments.number_vars.ContainsKey(5) && arguments.number_vars[5] is VarString){
                    newSignal.reference = (string)(VarString)arguments.number_vars[5];
                }

                if(arguments.string_vars.ContainsKey("verb") && arguments.string_vars["verb"] is VarString){
                    newSignal.verb = (string)(VarString)arguments.string_vars["verb"];
                }else if(arguments.number_vars.ContainsKey(6) && arguments.number_vars[6] is VarString){
                    newSignal.verb = (string)(VarString)arguments.number_vars[6];
                }

                if(arguments.string_vars.ContainsKey("language") && arguments.string_vars["language"] is VarString){
                    newSignal.language = (string)(VarString)arguments.string_vars["verb"];
                }else if(arguments.number_vars.ContainsKey(7) && arguments.number_vars[7] is VarString){
                    newSignal.language = (string)(VarString)arguments.number_vars[7];
                }



                scope.program.signals.Enqueue(newSignal);
            });
            return tcomm_VAR;
        }
    }
}