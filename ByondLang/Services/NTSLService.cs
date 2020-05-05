using ByondLang.Language;
using ByondLang.Language.Variable;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ByondLang.Services
{
    public class NTSLService
    {
        private Dictionary<int, Language.Program> programs = new Dictionary<int, Language.Program>();
        private Queue<Tuple<string, Language.Variable.Var>> subspace_messages = new Queue<Tuple<string, Language.Variable.Var>>();
        private int lastId = 1;


        public NTSLService()
        {
        }

        internal void Reset()
        {
            lastId = 1;
            programs.Clear();
            subspace_messages.Clear();
        }

        private int GenerateNewId()
        {
            return lastId++;
        }

        internal int NewProgram(string code, string computerRef)
        {
            var id = GenerateNewId();
            var program = new Language.Program(code, computerRef);
            programs[id] = program;
            return id;
        }

        internal void Execute(int id, int cycles)
        {
            if (!programs.ContainsKey(id))
                throw new ArgumentException("Provided ID is not found.");
            try
            {
                programs[id].scope.ExecuteNextN(cycles);
            }
            catch
            {
                programs.Remove(id);
                throw new Exception("NTSL execution error, program disposed.");
            }
        }

        internal string GetTerminalBuffer(int id)
        {
            if (!programs.ContainsKey(id))
                throw new ArgumentException("Provided ID is not found.");
            return programs[id].terminal.Stringify();
        }

        internal void ProcessMessage(int id, string signal_ref, NameValueCollection signal)
        {
            if (!programs.ContainsKey(id))
                throw new ArgumentException("Provided ID is not found.");
            var program = programs[id];
            program.scope.callstack.Push(new DoLater(delegate {
                VarList args = new VarList();
                args.number_vars[0] = args.string_vars["content"] = signal["content"];
                args.number_vars[1] = args.string_vars["source"] = signal["source"];
                args.number_vars[2] = args.string_vars["job"] = signal["job"];
                args.number_vars[3] = args.string_vars["freq"] = signal["freq"];
                args.number_vars[4] = args.string_vars["pass"] = signal["pass"];
                args.number_vars[5] = args.string_vars["ref"] = signal_ref;
                args.number_vars[6] = args.string_vars["language"] = signal["language"];
                args.number_vars[7] = args.string_vars["verb"] = signal["verb"];
                program.scope.globals.meta.string_vars["_parent"].string_vars["tcomm"].string_vars["onmessage"].Call(program.scope, new Dictionary<int, Var>(), 0, args);
            }));
        }

        internal string GetSubspaceMessageToSend()
        {
            if (subspace_messages.Count == 0)
                return "0";
            var buffer = "";
            while (subspace_messages.Count > 0)
            {
                Tuple<string, Var> message = subspace_messages.Dequeue();
                string varType = "none";
                string convertedData = "0";
                Var contents = message.Item2;
                if (contents is VarString)
                {
                    varType = "text";
                    convertedData = (contents as VarString).data;
                }
                else if (contents is VarNumber)
                {
                    varType = "num";
                    convertedData = (contents as VarNumber).data.ToString();
                }
                else if (contents is VarList l && l.privateVariables.ContainsKey("subspace_ref"))
                {
                    varType = "ref";
                    convertedData = ((contents as VarList).privateVariables["subspace_ref"] as VarString).data;
                }
                buffer += "channel=" + HttpUtility.UrlEncode(message.Item1) + "&type=" + varType + "&data=" + HttpUtility.UrlEncode(convertedData) + "\n";
            }
            return buffer;
        }

        internal void HandleTopic(int id, string topic)
        {
            if (!programs.ContainsKey(id))
                throw new ArgumentException("Provided ID is not found.");

            var program = programs[id];
            program.scope.callstack.Clear();
            program.scope.callstack.Push(new DoLater(delegate {
                VarList args = new VarList();
                args.string_vars["topic"] = args.number_vars[0] = topic;
                program.scope.globals.meta.string_vars["_parent"].string_vars["term"].string_vars["topic"].Call(program.scope, new Dictionary<int, Var>(), 0, args);
            }));
                
        }

        internal void SubspaceReceive(string channel, string type, string data)
        {
            
            string hook = (string)(VarString)channel;
            foreach (KeyValuePair<int, Language.Program> kv in programs)
            {
                var their_net = kv.Value.scope.globals.meta.string_vars["_parent"].string_vars["net"].string_vars;
                if (their_net["connections"].string_vars.ContainsKey(hook))
                {
                    VarList args = new VarList();
                    Var bc_dat = Var.nil;
                    if (type == "text")
                        bc_dat = new VarString(data);
                    else if (type == "num")
                        bc_dat = new VarNumber(double.Parse(data));
                    else
                    {
                        bc_dat = new VarList();
                        (bc_dat as VarList).privateVariables["subspace_ref"] = data;
                    }
                    args.number_vars[0] = bc_dat;
                    args.string_vars["message"] = bc_dat;
                    their_net["connections"].string_vars[hook].Call(kv.Value.scope, new Dictionary<int, Var>(), 0, args);
                }
            }
        }

        internal Signal? GetSignal(int id)
        {
            if (!programs.ContainsKey(id))
                throw new ArgumentException("Provided ID is not found.");

            var program = programs[id];
            if (program.signals.Count > 0)
                return program.signals.Dequeue();
            return null;
        }

        internal void Remove(int id)
        {
            if (!programs.ContainsKey(id))
                throw new ArgumentException("Provided ID is not found.");
            programs.Remove(id);
        }
    }
}
