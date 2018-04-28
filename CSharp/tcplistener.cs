using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using ByondLang.Override;
using System;

namespace ByondLang{

    class GetRequest{
        Dictionary<string, string> results = new Dictionary<string, string>();
        public GetRequest(String decoded){
            String[] arguments = decoded.Split('&');
            foreach(String s in arguments){
                String[] keyvalue = s.Split('=');
                if(keyvalue.Length >= 2)
                    results[HttpUtility.UrlDecode(keyvalue[0])] = HttpUtility.UrlDecode(keyvalue[1]);
            }
        }

        public string this[string key]{
            get{
                return results.ContainsKey(key)?results[key]:null;
            }
        }
    }
    
    class Listener{
        Dictionary<int, Program> programs = new Dictionary<int, Program>();

        public int NextFreeProgram(){
            int i=0;
            while(programs.ContainsKey(i))
                i++;
            return i;
        }

        public Listener(){}


        public string Interact(string input){
            Console.WriteLine(input);
            GetRequest req = new GetRequest(input);
            string response = "";
            if(req["action"]!=null){
                switch(req["action"]){
                    case "clear":
                        programs.Clear();
                        break;
                    case "new_program":
                        int programID;
                        response += programID = NextFreeProgram();
                        programs[programID]=new Program(req["code"]==null?"":req["code"],req["ref"]==null?"":req["ref"]);
                        break;
                    case "execute":
                        int id = programs.Count;
                        int cycles = 0;
                        if(req["id"]!=null)
                            int.TryParse(req["id"], out id);
                        if(req["cycles"]!=null)
                            int.TryParse(req["cycles"], out cycles);
                        if(programs.ContainsKey(id)){
                            try{
                                programs[id].scope.ExecuteNextN(cycles);
                                response += "1";
                            }catch{
                                programs.Remove(id);
                                response += "0";
                            }
                        }else{
                            response += "0";
                        }
                        break;
                    case "get_buffered":
                        id = programs.Count;
                        if(req["id"]!=null)
                            int.TryParse(req["id"], out id);
                        if(programs.ContainsKey(id)){
                            response += programs[id].terminal.Stringify();
                        }else{
                            response += "Error: Not Found!";
                        }
                        break;
                    case "remove":
                        id = programs.Count;
                        if(req["id"]!=null)
                            int.TryParse(req["id"], out id);
                        if(programs.ContainsKey(id)){
                            programs.Remove(id);
                            response += "1";
                        }else{
                            response += "0";
                        }
                        break;
                    case "message":
                        id = programs.Count;
                        if(req["id"]!=null)
                            int.TryParse(req["id"], out id);
                        
                        if(req["signal"]!=null && req["sig_ref"]!=null && programs.ContainsKey(id)){
                            Program prg = programs[id];
                            //prg.scope.callstack.Clear();
                            prg.scope.callstack.Push(new DoLater(delegate{
                                Variable.VarList args = new Variable.VarList();
                                GetRequest signal = new GetRequest(req["signal"]);
                                args.number_vars[0] = args.string_vars["content"]   = signal["content"];
                                args.number_vars[1] = args.string_vars["source"]    = signal["source"];
                                args.number_vars[2] = args.string_vars["job"]       = signal["job"];
                                args.number_vars[3] = args.string_vars["freq"]      = signal["freq"];
                                args.number_vars[4] = args.string_vars["pass"]      = signal["pass"];
                                args.number_vars[5] = args.string_vars["ref"]       = req["sig_ref"];
                                args.number_vars[6] = args.string_vars["language"]  = signal["language"];
                                args.number_vars[7] = args.string_vars["verb"]      = signal["verb"];
                                prg.scope.globals.meta.string_vars["_parent"].string_vars["tcomm"].string_vars["onmessage"].Call(prg.scope, new Dictionary<int, Variable.Var>(), 0, args);
                            }));
                            response += "1";
                        }else{
                            response += "0";
                        }
                        break;
                    case "get_signals":
                        id = programs.Count;
                        if(req["id"]!=null)
                            int.TryParse(req["id"], out id);
                        if(programs.ContainsKey(id)){
                            Program prg = programs[id];
                            if(prg.signals.Count > 0){
                                Signal sig = prg.signals.Dequeue();
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Signal));
                                MemoryStream strm = new MemoryStream();
                                ser.WriteObject(strm, sig);
                                response += System.Text.Encoding.UTF8.GetString(strm.ToArray());
                            }else{
                                response += "0";
                            }
                        }else{
                            response += "0";
                        }
                        break;
                    case "topic":
                        id = programs.Count;
                        if(req["id"]!=null)
                            int.TryParse(req["id"], out id);
                        if(programs.ContainsKey(id)){
                            Program prg = programs[id];
                            prg.scope.callstack.Clear();
                            prg.scope.callstack.Push(new DoLater(delegate{
                                Variable.VarList args = new Variable.VarList();
                                args.string_vars["topic"] = args.number_vars[0] = req["topic"]==null?"":req["topic"];
                                prg.scope.globals.meta.string_vars["_parent"].string_vars["term"].string_vars["topic"].Call(prg.scope, new Dictionary<int, Variable.Var>(), 0, args);
                            }));
                            response += "1";
                        }else{
                            response += "0";
                        }
                        break;
                    case "pause":
                        break;
                }
            }
            return response;
        }
    }
}