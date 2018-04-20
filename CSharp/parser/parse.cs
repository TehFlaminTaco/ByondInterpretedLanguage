using ByondLang.Tokenizer;

namespace ByondLang{

    [System.Serializable]
    public class UnknownTokenException : System.Exception
    {
        public UnknownTokenException() { }
        public UnknownTokenException(string message) : base("Unknown Token Type: "+message) { }
        public UnknownTokenException(string message, System.Exception inner) : base("Unknown Token Type: "+message, inner) { }
        protected UnknownTokenException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    class Parser{
        public Scope scope;

        public void parse(Token token, CallTarget target, State state){
            switch(token.name){
                case "program":
                    bool first = true;
                    for(int i=token.data[0].items.Count-1; i>=0; i--){
                        Token t = token.data[0].items[i].data[0].items[0];
                        if(first){
                            scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, t.location));
                            first = false;
                        }else
                            scope.callstack.Push(new CallTarget(t.location));
                    }
                    break;
                case "expression":
                    scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token.data[0].items[0].location));
                    break;
                case "assignment":

                default:
                    throw new UnknownTokenException(token.name);
            }
        }
    }
}