using ByondLang.Language;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace ByondLang.Services
{
    public class NTSLService
    {
        public NTSLService()
        {
        }

        internal void Reset()
        {
            //programs.Clear();
            //subspace_messages.Clear();
            throw new NotImplementedException();
        }

        internal int NewProgram(string code, string computerRef)
        {
            //int programID;
            //response += programID = NextFreeProgram();
            //try { programs[programID] = new Program(req["code"] == null ? "" : req["code"], req["ref"] == null ? "" : req["ref"]); } catch (Exception) { }
            throw new NotImplementedException();
        }

        internal void Execute(int id, int cycles)
        {
            //if (programs.ContainsKey(id))
            //{
            //    try
            //    {
            //        programs[id].scope.ExecuteNextN(cycles);
            //        response += "1";
            //    }
            //    catch
            //    {
            //        programs.Remove(id);
            //        response += "0";
            //    }
            //}
            //else
            //{
            //    response += "0";
            //}
            throw new NotImplementedException();
        }

        internal string GetTerminalBuffer(int id)
        {
            /*
            id = programs.Count;
            if (req["id"] != null)
                int.TryParse(req["id"], out id);
            if (programs.ContainsKey(id))
            {
                response += programs[id].terminal.Stringify();
            }
            else
            {
                response += "Error: Not Found!";
            }
            */
            throw new NotImplementedException();
        }

        internal void ProcessMessage(int id, string signal_ref, NameValueCollection signal)
        {
            /*
            Program prg = programs[id];
            //prg.scope.callstack.Clear();
            prg.scope.callstack.Push(new DoLater(delegate {
                Variable.VarList args = new Variable.VarList();
                GetRequest signal = new GetRequest(req["signal"]);
                args.number_vars[0] = args.string_vars["content"] = signal["content"];
                args.number_vars[1] = args.string_vars["source"] = signal["source"];
                args.number_vars[2] = args.string_vars["job"] = signal["job"];
                args.number_vars[3] = args.string_vars["freq"] = signal["freq"];
                args.number_vars[4] = args.string_vars["pass"] = signal["pass"];
                args.number_vars[5] = args.string_vars["ref"] = req["sig_ref"];
                args.number_vars[6] = args.string_vars["language"] = signal["language"];
                args.number_vars[7] = args.string_vars["verb"] = signal["verb"];
                prg.scope.globals.meta.string_vars["_parent"].string_vars["tcomm"].string_vars["onmessage"].Call(prg.scope, new Dictionary<int, Variable.Var>(), 0, args);
            }));*/
            throw new NotImplementedException();
        }

        internal string GetSubspaceMessageToSend()
        {
            /*
            if (subspace_messages.Count == 0)
                response += "0";
            else while (subspace_messages.Count > 0)
                {
                    Tuple<string, Variable.Var> message = subspace_messages.Dequeue();
                    string channel = message.Item1;
                    string varType = "none";
                    string convertedData = "0";
                    Variable.Var contents = message.Item2;
                    if (contents is Variable.VarString)
                    {
                        varType = "text";
                        convertedData = (contents as Variable.VarString).data;
                    }
                    else if (contents is Variable.VarNumber)
                    {
                        varType = "num";
                        convertedData = (contents as Variable.VarNumber).data.ToString();
                    }
                    else if (contents is Variable.VarList l && l.privateVariables.ContainsKey("subspace_ref"))
                    {
                        varType = "ref";
                        convertedData = ((contents as Variable.VarList).privateVariables["subspace_ref"] as Variable.VarString).data;
                    }
                    response += "channel=" + HttpUtility.UrlEncode(message.Item1) + "&type=" + varType + "&data=" + HttpUtility.UrlEncode(convertedData) + "\n";
                }
                */
            throw new NotImplementedException();
        }

        internal void HandleTopic(int id, string topic)
        {
            /*
            id = programs.Count;
            if (req["id"] != null)
                int.TryParse(req["id"], out id);
            if (programs.ContainsKey(id))
            {
                Program prg = programs[id];
                prg.scope.callstack.Clear();
                prg.scope.callstack.Push(new DoLater(delegate {
                    Variable.VarList args = new Variable.VarList();
                    args.string_vars["topic"] = args.number_vars[0] = req["topic"] == null ? "" : req["topic"];
                    prg.scope.globals.meta.string_vars["_parent"].string_vars["term"].string_vars["topic"].Call(prg.scope, new Dictionary<int, Variable.Var>(), 0, args);
                }));
                response += "1";
            }
            else
            {
                response += "0";
            }
            */
            throw new NotImplementedException();
        }

        internal void SubspaceReceive(int id, string channel, string type, string data)
        {
            /*
            string hook = (string)(Variable.VarString)req["channel"];
            foreach (KeyValuePair<int, Program> kv in Listener.programs)
            {
                var their_net = kv.Value.scope.globals.meta.string_vars["_parent"].string_vars["net"].string_vars;
                if (their_net["connections"].string_vars.ContainsKey(hook))
                {
                    Variable.VarList args = new Variable.VarList();
                    Variable.Var bc_dat = Variable.Var.nil;
                    if (req["type"] == "text")
                        bc_dat = new Variable.VarString(req["data"]);
                    else if (req["type"] == "num")
                        bc_dat = new Variable.VarNumber(Double.Parse(req["data"]));
                    else
                    {
                        bc_dat = new Variable.VarList();
                        (bc_dat as Variable.VarList).privateVariables["subspace_ref"] = req["data"];
                    }
                    args.number_vars[0] = bc_dat;
                    args.string_vars["message"] = bc_dat;
                    their_net["connections"].string_vars[hook].Call(kv.Value.scope, new Dictionary<int, Variable.Var>(), 0, args);
                }
            }
            */
            throw new NotImplementedException();
        }

        internal Signal[] GetSignals(int id)
        {
            /*
            Program prg = programs[id];
            if (prg.signals.Count > 0)
            {
                Signal sig = prg.signals.Dequeue();
                sig.content = sanitize_message(sig.content);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Signal));
                MemoryStream strm = new MemoryStream();
                ser.WriteObject(strm, sig);
                response += System.Text.Encoding.UTF8.GetString(strm.ToArray());
            }
            else
            {
                response += "0";
            }
            */
            throw new NotImplementedException();
        }

        internal void Remove(int id)
        {
            /*
            id = programs.Count;
            if (req["id"] != null)
                int.TryParse(req["id"], out id);
            if (programs.ContainsKey(id))
            {
                programs.Remove(id);
                response += "1";
            }
            else
            {
                response += "0";
            }
            */
            throw new NotImplementedException();
        }
    }
}
