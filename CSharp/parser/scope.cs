using System.Collections.Generic;
using ByondLang.Variable;
using ByondLang.Tokenizer;

namespace ByondLang{

    struct SubTarget{
        public int dataID;
        public int itemID;

        public SubTarget(int dataID, int itemID){
            this.dataID = dataID;
            this.itemID = itemID;
        }
    }

    class State{
        public Dictionary<int, Var> returns = new Dictionary<int, Var>();
    }

    class CallTarget{
        public Token target;
        public Dictionary<int, Var> returnTarget = new Dictionary<int, Var>(); // We implciitely initilize this, to prevent errors if we don't actually use this. Laziness.
        public int returnTargetID = -1;

        public State state = null;

        public VarList variables;

        public CallTarget(Token target, VarList variables){
            this.target = target;
            this.variables = variables;
        }

        public CallTarget(Dictionary<int, Var> returnTarget, int returnTargetID, Token target, VarList variables) : this(target, variables){
            this.returnTarget = returnTarget;
            this.returnTargetID = returnTargetID;
        }
    }

    class Scope{

        public Parser parser;

        public Stack<CallTarget> callstack = new Stack<CallTarget>();

        public Token code;

        public void ExecuteNextEntry(){
            if(callstack.Count>0){
                CallTarget toRun = callstack.Pop();
                Token target = toRun.target;
                parser.parse(target, toRun, toRun.state);

            }
        }

        public VarList listFromParent(VarList parent){
            VarList createdList = new VarList();
            VarList meta;
            createdList.meta = meta = new VarList();

            meta["_index"] = parent;

            return createdList;
        }

    }
}