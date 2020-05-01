using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Web;
using System.Linq;
using System;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Diagnostics;

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
        public static Queue<Tuple<string, Variable.Var>> subspace_messages = new Queue<Tuple<string, Variable.Var>>();
        public static Dictionary<int, Program> programs = new Dictionary<int, Program>();
        static Random idRNG = new Random();
        static HttpListener httpServer;
        static Listener singleton;

        public static int NextFreeProgram(){
            int i=idRNG.Next();
            while(programs.ContainsKey(i))
                i=idRNG.Next();
            return i;
        }

        public Listener(string hostname, int port){
            httpServer =  new HttpListener();
            singleton = this;

            httpServer.Prefixes.Add("http://" + hostname + ":" + port + "/");

            Console.WriteLine($"Starting server on http://{hostname}:{port}/");
            try{httpServer.Start();}catch(System.Net.HttpListenerException){
                Console.WriteLine("Unable to obtain permissions to start web server.");
                Environment.Exit(0);
            }

            Task listenTask = HandleMessages();
            listenTask.GetAwaiter().GetResult();

            httpServer.Close();
        }

        public static async Task HandleMessages(){
            while (true){
                Console.WriteLine("Listening...");
                HttpListenerContext ctx = await httpServer.GetContextAsync();
                HttpListenerRequest request = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                string response = @"";
                Console.WriteLine(request.Url.AbsolutePath);
                GetRequest req = new GetRequest(request.Url.AbsolutePath.Substring(1));
                if(req["action"]!=null){
                    switch(req["action"]){
                        case "clear":
                            programs.Clear();
                            subspace_messages.Clear();
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
                                    sig.content = sanitize_message(sig.content);
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
                        case "subspace_receive":
                            string hook = (string)(Variable.VarString)req["channel"];
                            foreach(KeyValuePair<int, Program> kv in Listener.programs){
                                var their_net = kv.Value.scope.globals.meta.string_vars["_parent"].string_vars["net"].string_vars;
                                if(their_net["connections"].string_vars.ContainsKey(hook)){
                                    Variable.VarList args = new Variable.VarList();
                                    Variable.Var bc_dat = Variable.Var.nil;
                                    if(req["type"] == "text")
                                        bc_dat = new Variable.VarString(req["data"]);
                                    else if(req["type"] == "num")
                                        bc_dat = new Variable.VarNumber(Double.Parse(req["data"]));
                                    else{
                                        bc_dat = new Variable.VarList();
                                        (bc_dat as Variable.VarList).privateVariables["subspace_ref"] = req["data"];
                                    }
                                    args.number_vars[0] = bc_dat;
                                    args.string_vars["message"] = bc_dat;
                                    their_net["connections"].string_vars[hook].Call(kv.Value.scope, new Dictionary<int, Variable.Var>(), 0, args);
                                }
                            }
                            break;
                        case "subspace_transmit":
                            if(subspace_messages.Count == 0)
                                response += "0";
                            else while(subspace_messages.Count>0){
                                Tuple<string, Variable.Var> message = subspace_messages.Dequeue();
                                string channel = message.Item1;
                                string varType = "none";
                                string convertedData = "0";
                                Variable.Var contents = message.Item2;
                                if(contents is Variable.VarString){
                                    varType = "text";
                                    convertedData = (contents as Variable.VarString).data;
                                }else if(contents is Variable.VarNumber){
                                    varType = "num";
                                    convertedData = (contents as Variable.VarNumber).data.ToString();
                                }else if(contents is Variable.VarList l && l.privateVariables.ContainsKey("subspace_ref")){
                                    varType = "ref";
                                    convertedData = ((contents as Variable.VarList).privateVariables["subspace_ref"] as Variable.VarString).data;
                                }
                                response += "channel=" + HttpUtility.UrlEncode(message.Item1) + "&type=" + varType + "&data=" + HttpUtility.UrlEncode(convertedData) + "\n";
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
                
                byte[] data = System.Text.Encoding.UTF8.GetBytes(response);
                resp.ContentType = "text/html";
                resp.ContentEncoding = System.Text.Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
                Console.WriteLine("And finished!");
            }
        }

        private static string sanitize_message(string content){
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            return sanitize_message_node(doc.DocumentNode);
        }

        private static string[] safetags = {"b","u","s","i"};
        private static string sanitize_message_node(HtmlNode node){
            if(node.NodeType == HtmlNodeType.Text){
                return HttpUtility.HtmlEncode(node.OuterHtml);
            }
            string s;
            if(safetags.Contains(node.Name)){ // TODO: Better whitelist system.
                s = $"<{node.Name}>";
                foreach(HtmlNode n in node.ChildNodes){
                    s += sanitize_message_node(n);
                }
                s += $"</{node.Name}>";
                return s;
            }
            s = HttpUtility.HtmlEncode(node.OuterHtml.Substring(0,node.InnerStartIndex - node.StreamPosition));
            foreach(HtmlNode n in node.ChildNodes){
                s += sanitize_message_node(n);
            }
            return s + HttpUtility.HtmlEncode(node.OuterHtml.Substring(node.InnerStartIndex - node.StreamPosition + node.InnerLength));
        }
    }
}