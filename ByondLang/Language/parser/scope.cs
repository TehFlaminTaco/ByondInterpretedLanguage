using System.Collections.Generic;
using ByondLang.Language.Variable;
using ByondLang.Language.Tokenizer;

namespace ByondLang.Language
{

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

    public interface AsyncCallable{
        void Run(Parser parser);
    }

    public class CallTarget : AsyncCallable{
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

        public void Run(Parser parser){
            parser.parse(this.target, this, this.state);
        }
    }

    public delegate void AsyncCall(Parser parser);

    public class DoLater : AsyncCallable{
        AsyncCall todo;
        public DoLater(AsyncCall func){
            todo = func;
        }
        public void Run(Parser parser){
            todo(parser);
        }
    }

    public class Scope{

        public Parser parser;

        public Stack<AsyncCallable> callstack = new Stack<AsyncCallable>();

        public Token code;

        public Program program;

        public Dictionary<string, VarList> metas = new Dictionary<string, VarList>();

        public VarList globals;


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
                AsyncCallable toRun = callstack.Pop();
                toRun.Run(parser);

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