using ByondLang.Tokenizer;
using ByondLang.States;
using ByondLang.Variable;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

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

        public static Regex escape_strings = new Regex(@"^(?:""|'|\[(=*)\[)(.*?)(?:""|'|]\1])$");

        public Var ReturnValue(CallTarget target, Var val){
            return target.returnTarget[target.returnTargetID] = val;
        }

        public Var ReturnValue(CallTarget target){
            return ReturnValue(target, Var.nil);
        }

        public void parse(Token token, CallTarget target, State state){
            switch(token.name){
                case "program":
                    bool first = true;
                    for(int i=token.data[0].items.Count-1; i>=0; i--){
                        Token t = token.data[0].items[i].data[0].items[0];
                        if(first){
                            scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, t, target.variables));
                            first = false;
                        }else
                            scope.callstack.Push(new CallTarget(t, target.variables));
                    }
                    break;
                case "expression":
                    if(state == null){
                        target.state = state = new State();
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        CallTarget subCall = new CallTarget(state.returns, 0, token.data[0].items[0], target.variables);
                        scope.callstack.Push(subCall);
                    }else{
                        if(token[0].name == "var"){
                            Var varData = state.returns[0];
                            varData.string_vars["target"].Get(scope, target.returnTarget, target.returnTargetID, varData.string_vars["index"]);
                        }else{
                            ReturnValue(target, state.returns[0]);
                        }
                    }
                    
                    break;
                case "constant":
                    scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token.data[0].items[0], target.variables));
                    break;
                case "assignment":
                    if(state == null){
                        target.state = state = new State();
                    }
                    Var modifyTarget;
                    Var value;
                    if(state.returns.ContainsKey(0)){
                        modifyTarget = state.returns[0];
                    }else{
                        scope.callstack.Push(target); // Re-add this to the callstack, we still need more information.
                        scope.callstack.Push(new CallTarget(state.returns, 0, token[0][0][0][0], target.variables));
                        break;
                    }
                    if(state.returns.ContainsKey(1)){
                        value = state.returns[1];
                    }else{
                        scope.callstack.Push(target); // Re-add this to the callstack, we still need more information.
                        scope.callstack.Push(new CallTarget(state.returns, 1, token.data[3].items[0], target.variables));
                        break;
                    }
                    modifyTarget.string_vars["target"].Set(scope, target.returnTarget, target.returnTargetID, modifyTarget.string_vars["index"], value);
                    break;
                case "var":
                    if(token.data[0].name == "local"){
                        VarList vardata = new VarList();
                        vardata.string_vars["target"] = target.variables;
                        vardata.string_vars["index"] = token.data[1].items[0].text;
                        ReturnValue(target, vardata);
                    }else{
                        if(state == null){
                            target.state = state = new State();
                        }
                        Var table_target = null;
                        Var index = null;
                        if(state.returns.ContainsKey(0)){
                            table_target = state.returns[0];
                        }else{
                            scope.callstack.Push(target);
                            scope.callstack.Push(new CallTarget(state.returns, 0, token[0][0], target.variables));
                            break;
                        }
                        if(state.returns.ContainsKey(0)){
                            table_target = state.returns[0];
                        }else{
                            scope.callstack.Push(target);
                            scope.callstack.Push(new CallTarget(state.returns, 0, token[0][0], target.variables));
                            break;
                        }
                        if(state.returns.ContainsKey(1)){
                            index = state.returns[1];
                        }else{
                            scope.callstack.Push(target);
                            scope.callstack.Push(new CallTarget(state.returns, 1, token[1][0], target.variables));
                            break;
                        }
                        VarList vardata = new VarList();
                        vardata.string_vars["target"] = table_target;
                        vardata.string_vars["index"] = index;
                        ReturnValue(target, vardata);
                    }
                    break;
                case "index":
                    if(token[0].name == "\\["){
                        scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[1][0], target.variables));
                    }else if(token[0].name == "\\."){
                        ReturnValue(target, token[1][0].text);
                    }else{
                        throw new Exception("Currying not yet implemented, but neither are functions so who gives a fuck."); // TODO: Implement currying.
                    }
                    break;
                case "numberconstant":
                    ReturnValue(target, double.Parse(token.text));
                    break;
                case "stringconstant":
                    if(token[0].name == "templatestring"){
                        // TODO:
                    }else{
                        target.returnTarget[target.returnTargetID] = Regex.Unescape(escape_strings.Replace(token[0][0].text, "$2"));
                    }
                    break;
                case "tableconstant":
                    if(state == null){
                        target.state = state = new State();
                    }
                    TokenItem fills = token.data[1];
                    bool hasAll = true;
                    for(var i=0; i < fills.items.Count; i++){
                        if(!state.returns.ContainsKey(i*2)){
                            hasAll = false;
                            scope.callstack.Push(target);
                            Token this_fill = fills[i];
                            if(this_fill[0].name != "constant"){
                                scope.callstack.Push(new CallTarget(state.returns, i*2, this_fill[0][0], target.variables)); // This is just an expression. Eg. {1, 2, 3}
                            }else{
                                scope.callstack.Push(new CallTarget(state.returns, i*2, this_fill[2][0], target.variables)); // This is either a var or a constant. Eg. {a = 1, "hello" = 2}
                                scope.callstack.Push(new CallTarget(state.returns, i*2+1, this_fill[0][0], target.variables));
                            }
                            break;
                        }
                    }
                    if(hasAll){
                        VarList newList = new VarList();
                        int currentIndex = 0;
                        for(var i=0; i < fills.items.Count; i++){
                            Token this_fill = fills[i];
                            if(this_fill[0].name == "expression"){
                                newList.number_vars[currentIndex++] = state.returns[i*2];
                            }else if(this_fill[0].name == "constant"){
                                newList.Set(scope, new Dictionary<int, Var>(), 0, state.returns[i*2+1], state.returns[i*2]);
                            }else{
                                newList.string_vars[this_fill[0][0].text] = state.returns[i*2];
                            }
                        }
                        ReturnValue(target, newList);
                    }
                    break;
                case "call":
                    if(state == null){
                        target.state = state = new State();
                    }
                    Var toCall = null;
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        scope.callstack.Push(new CallTarget(state.returns, 0, token[0][0], target.variables));
                        break;
                    }else{
                        toCall = state.returns[0];
                    }
                    switch(token[1].name){
                        case "splat_call":
                            if(!state.returns.ContainsKey(1)){
                                scope.callstack.Push(target);
                                scope.callstack.Push(new CallTarget(state.returns, 1, token[1][0][1][0], target.variables));
                            }else{
                                if(state.returns[1] is VarList)
                                    toCall.Call(scope, target.returnTarget, target.returnTargetID, (VarList)state.returns[1]);
                                else
                                    toCall.Call(scope, target.returnTarget, target.returnTargetID, state.returns[1]);
                            }
                            return;
                        case "\\(":
                            TokenItem args = token[2];
                            for(int i=0; i < args.items.Count; i++){
                                if(!state.returns.ContainsKey(i*2+1)){
                                    scope.callstack.Push(target);
                                    switch(args[i][0].name){
                                        case "expression":
                                            scope.callstack.Push(new CallTarget(state.returns, i*2+1, args[i][0][0], target.variables));
                                            break;
                                        case "splat_call":
                                            scope.callstack.Push(new CallTarget(state.returns, i*2+1, args[i][0][0][1][0], target.variables));
                                            break;
                                        case "constant":
                                            scope.callstack.Push(new CallTarget(state.returns, i*2+1, args[i][0][0], target.variables));
                                            scope.callstack.Push(new CallTarget(state.returns, i*2+2, args[i][2][0], target.variables));
                                            break;
                                        case "var":
                                            scope.callstack.Push(new CallTarget(state.returns, i*2+1, args[i][0][0], target.variables));
                                            break;
                                    }
                                    return;
                                }
                            }
                            VarList callArguments = new VarList();
                            scope.callstack.Push(new DoLater(delegate{
                                toCall.Call(scope, target.returnTarget, target.returnTargetID, callArguments);
                            }));
                            int currentIndex = 0;
                            for(int i=0; i < args.items.Count; i++){
                                switch(args[i][0].name){
                                    case "expression":
                                        callArguments.number_vars[currentIndex++] = state.returns[i*2+1];
                                        break;
                                    case "splat_call":
                                        foreach(KeyValuePair<Var, Var> v in state.returns[i*2+1]){
                                            if(v.Key is Number)
                                                callArguments.number_vars[currentIndex++] = v.Value;
                                            else
                                                callArguments.Set(scope, new Dictionary<int, Var>(), 0, v.Key, v.Value, true);
                                        }
                                        break;
                                    case "constant":
                                        callArguments.Set(scope, new Dictionary<int, Var>(), 0, state.returns[i*2+1], state.returns[i*2+2], true);
                                        break;
                                    case "var":
                                        callArguments.Set(scope, new Dictionary<int, Var>(), 0, args[i][0][0].text, state.returns[i*2+1], true);
                                        break;
                                }
                            }
                            return;
                    }
                    ReturnValue(target);
                    break;
                default:
                    throw new UnknownTokenException(token.name);
            }
        }
    }
}