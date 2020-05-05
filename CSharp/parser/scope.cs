using System.Collections.Generic;
using ByondLang.Variable;
using ByondLang.Tokenizer;

namespace ByondLang{

    public class CallTarget{
        //public Dictionary<int, Var> returnTarget = new Dictionary<int, Var>(); // We implciitely initilize this, to prevent errors if we don't actually use this. Laziness.
        //public State state = null;
        public VarList variables;

        public CallTarget(VarList variables){
            this.variables = variables;
        }
    }

    public struct SubTarget{
        public int dataID;
        public int itemID;

        public SubTarget(int dataID, int itemID){
            this.dataID = dataID;
            this.itemID = itemID;
        }
    }

    public class State{
        public Dictionary<int, Var> returns = new Dictionary<int, Var>();
    }

    public enum ReturnType{
        Return,
        Break
    }

    public class Scope{

        public Parser parser;

        public Stack<System.Action> callstack = new Stack<System.Action>();

        public Token code;

        public Program program;

        public Dictionary<ReturnType, VarList> metas = new Dictionary<ReturnType, VarList>();

        public VarList globals;
        public Stack<KeyValuePair<ReturnType, Var>> returnsStack = new Stack<KeyValuePair<ReturnType, Var>>();


        public Scope(VarList globals){
            this.globals = listFromParent(globals);
            metas["string"] = Metas.String(globals);
            metas["number"] = Metas.Number(globals);
            metas["list"] = Metas.List(globals);
            metas["function"] = Metas.Function(globals);
            metas["event"] = Metas.Event(globals);
            metas["nil"] = Metas.Nil(globals);
        }


        public void ExecuteNextEntry(){
            if(callstack.Count>0){
                callstack.Pop()();
            }
        }

        public void ExecuteNextN(int amount){
            while(amount-->0){
                ExecuteNextEntry();
            }
        }

        public VarList listFromParent(VarList parent){
            VarList createdList = new VarList();
            VarList meta;
            createdList.meta = meta = new VarList();

            meta.string_vars["_parent"] = parent;

            return createdList;
        }

    }
}