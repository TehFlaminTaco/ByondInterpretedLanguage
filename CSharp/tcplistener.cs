using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web;
using System;

namespace ByondLang{

    class GetRequest{
        Dictionary<string, string> results = new Dictionary<string, string>();
        public GetRequest(String encoded){
            String decoded = encoded.Substring(1);
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
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 1945);
        Dictionary<int, Program> programs = new Dictionary<int, Program>();

        public int NextFreeProgram(){
            int i=0;
            while(programs.ContainsKey(i))
                i++;
            return i;
        }

        public Listener(){
            server.Start();

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
Server: ByondLangServer 1.0
Content-Type: text/html

";

                int i = 0;
                while((i = stream.Read(buffer, 0, buffer.Length))!=0){
                    data += System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                    Match mtch = getRequest.Match(data);
                    if(mtch.Success){
                        Console.WriteLine(mtch.Groups[1].Value);
                        GetRequest req = new GetRequest(mtch.Groups[1].Value);
                        if(req["action"]!=null){
                            switch(req["action"]){
                                case "clear":
                                    programs.Clear();
                                    break;
                                case "new_program":
                                    int programID;
                                    response += programID = NextFreeProgram();
                                    programs[programID]=new Program(req["code"]==null?"":req["code"]);
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