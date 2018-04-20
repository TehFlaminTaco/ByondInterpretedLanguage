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

    }

    class CallTarget{
        List<SubTarget> target = new List<SubTarget>();
        public Dictionary<int, Var> returnTarget;
        public int returnTargetID = -1;

        public State state;

        public CallTarget(List<SubTarget> target){
            this.target = target;
        }

        public CallTarget(params SubTarget[] targets){
            foreach(SubTarget t in targets){
                target.Add(t);
            }
        }

        public CallTarget(Dictionary<int, Var> returnTarget, int returnTargetID, List<SubTarget> target) : this(target){
            this.returnTarget = returnTarget;
            this.returnTargetID = returnTargetID;
        }

        public CallTarget(Dictionary<int, Var> returnTarget, int returnTargetID, params SubTarget[] targets) : this(targets){
            this.returnTarget = returnTarget;
            this.returnTargetID = returnTargetID;
        }

        public Token GetToken(Token root){
            for(int i = target.Count - 1; i>=0; i--){
                SubTarget sub_target = target[i];
                root = root.data[sub_target.dataID].items[sub_target.itemID];
            }
            return root;
        }
    }

    class Scope{

        public Parser parser;

        public Stack<CallTarget> callstack = new Stack<CallTarget>();

        public Token code;

        public void ExecuteNextEntry(){
            if(callstack.Count>0){
                CallTarget toRun = callstack.Pop();
                Token target = toRun.GetToken(code);
                parser.parse(target, toRun, toRun.state);

            }
        }

    }
}