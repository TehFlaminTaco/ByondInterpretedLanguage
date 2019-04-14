using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Web;
using System;

namespace ByondLang{

    class GetRequest{
        Dictionary<string, string> results = new Dictionary<string, string>();
        public GetRequest(String decoded){
            String[] arguments = decoded.Split("&");
            foreach(String s in arguments){
                String[] keyvalue = s.Split("=");
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
        TcpListener server;
        public static Dictionary<int, Program> programs = new Dictionary<int, Program>();

        public int NextFreeProgram(){
            int i=0;
            while(programs.ContainsKey(i))
                i++;
            return i;
        }

        public Listener(int port){
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);

            server.Start();
            Console.WriteLine("Running successfully on port {0}", port);

            Regex getRequest = new Regex("GET\\s+(.*?)\\s+");

            Byte[] buffer = new Byte[256];
            string data = "";

            while(true){
                Console.WriteLine("Awaiting Connection...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");
                
                data = "";

                NetworkStream stream = client.GetStream();

                string response = @"HTTP/1.0 200 OK
Server: NTSL2Daemon 1.0
Content-Type: text/html

";

                int i = 0;
                while((i = stream.Read(buffer, 0, buffer.Length))!=0){
                    data += System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                    Match mtch = getRequest.Match(data);
                    if(mtch.Success){
                        Console.WriteLine(mtch.Groups[1].Value);
                        GetRequest req = new GetRequest(mtch.Groups[1].Value.Substring(1));
                        if(req["action"]!=null){
                            switch(req["action"]){
                                case "clear":
                                    programs.Clear();
                                    response += "1";
                                    break;
                                case "new_program":
                                    int programID;
                                    response += programID = NextFreeProgram();
                                    try{programs[programID]=new Program(req["code"]==null?"":req["code"],req["ref"]==null?"":req["ref"]);}catch(Exception){}
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
                        stream.Write(System.Text.Encoding.ASCII.GetBytes(response), 0, response.Length);
                        break;
                    }
                }

                client.Close();
                Console.WriteLine("And finished!");

            }
        }
    }
}