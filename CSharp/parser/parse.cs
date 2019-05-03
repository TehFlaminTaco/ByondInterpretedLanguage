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
        public Stack<KeyValuePair<string, Var>> returnsStack = new Stack<KeyValuePair<string, Var>>();


        public static Regex escape_strings = new Regex(@"^(?:""|'|\[(=*)\[)(.*?)(?:""|'|]\1])$");

        public Var ReturnValue(CallTarget target, Var val){
            return target.returnTarget[target.returnTargetID] = val;
        }

        public Var ReturnValue(CallTarget target){
            return ReturnValue(target, Var.nil);
        }

        public void parse(CallTarget target){
            parse(target.target, target, target.state);
        }

        public Var ntnet_sanitize(Var value){
            if(!(value is VarList || value is VarString || value is VarNumber))
                return Var.nil;
            if(value is VarList){
                VarList newList = new VarList();
                foreach(KeyValuePair<string, Var> kv in value.string_vars)
                    newList.string_vars[kv.Key] = ntnet_sanitize(kv.Value);
                foreach(KeyValuePair<double, Var> kv in value.number_vars)
                    newList.number_vars[kv.Key] = ntnet_sanitize(kv.Value);
                foreach(KeyValuePair<Var, Var> kv in value.other_vars){
                    Var key = ntnet_sanitize(kv.Key);
                    if(key != Var.nil)
                        newList.other_vars[kv.Key] = ntnet_sanitize(kv.Value);
                }
                return newList;
            }
            return value;
        }

        public void Math(Dictionary<int, Var> returnTarget, int returnID, Var left, Var right, string op){
            string[] left_names = {"_"+op+"[0:"+right.type+"]", "_"+op+"["+right.type+"]", "_"+op+"[0]", "_"+op};
            string[] right_names = {"_"+op+"[1:"+left.type+"]", "_"+op+"["+left.type+"]", "_"+op+"[1]", "_"+op};
            Var metafunc = Var.nil;
            int i = 0;
            while(metafunc == Var.nil && i < left_names.Length + right_names.Length){
                if(i<left_names.Length){
                    metafunc = left.GetMeta(scope, left_names[i]);
                }else{
                    metafunc = right.GetMeta(scope, right_names[i - left_names.Length]);
                }
                i++;
            }
            if(!(metafunc is VarFunction)){
                returnTarget[returnID] = metafunc;
                return;
            }
            metafunc.Call(scope, returnTarget, returnID, left, right);
        }

        public void Math(Dictionary<int, Var> returnTarget, int returnID, Var left, string op){
            string[] left_names = {"_"+op+"[0]", "_"+op};
            Var metafunc = Var.nil;
            int i = 0;
            while(metafunc == Var.nil && i < left_names.Length){
                metafunc = left.GetMeta(scope, left_names[i]);
                i++;
            }
            if(!(metafunc is VarFunction)){
                returnTarget[returnID] = metafunc;
                return;
            }
            metafunc.Call(scope, returnTarget, returnID, left);
        }

        public void parse(Token token, CallTarget target, State state){
            switch(token.name){
                case "program":
                    if(state == null){
                        state = target.state = new State();
                    }
                    for(int i=0; i<token[0].items.Count; i++){
                        Token t = token[0][i][0][0];
                        if(!state.returns.ContainsKey(i)){
                            scope.callstack.Push(target);
                            parse(new CallTarget(state.returns, i, t, target.variables));
                            return;
                        }
                    }
                    target.returnTarget[target.returnTargetID] = state.returns[token[0].items.Count-1];
                    break;
                case "paranexp":
                    parse(new CallTarget(target.returnTarget, target.returnTargetID, token[1][0], target.variables));
                    break;
                case "expression":
                    if(token[0].name == "var"){
                        if(state == null){
                            target.state = state = new State();
                        }
                        if(!state.returns.ContainsKey(0)){
                            scope.callstack.Push(target);
                            parse(new CallTarget(state.returns, 0, token.data[0].items[0], target.variables));
                        }else{
                            Var varData = state.returns[0];
                            varData.string_vars["target"].Get(scope, target.returnTarget, target.returnTargetID, varData.string_vars["index"], ((VarNumber)varData.string_vars["islocal"]).data==1, ((VarNumber)varData.string_vars["iscurry"]).data==1);
                        }
                    }else{
                        parse(new CallTarget(target.returnTarget, target.returnTargetID, token[0][0], target.variables));
                        return;
                    }
                    
                    break;
                case "return":
                    string type = token[0][0].text;
                    if(token[1].items.Count>0){
                        if(state == null){
                            state = target.state = new State();
                        }
                        if(!state.returns.ContainsKey(0)){
                            scope.callstack.Push(target);
                            parse(new CallTarget(state.returns, 0, token[1][0], target.variables));
                        }else{
                            returnsStack.Push(new KeyValuePair<string, Var>(type, state.returns[0]));
                        }
                    }else{
                        returnsStack.Push(new KeyValuePair<string, Var>(type, Var.nil));
                    }
                    break;
                case "constant":
                    parse(new CallTarget(target.returnTarget, target.returnTargetID, token[0][0], target.variables));
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
                        parse(new CallTarget(state.returns, 0, token[0][0][0][0], target.variables));
                        break;
                    }
                    if(state.returns.ContainsKey(1)){
                        value = state.returns[1];
                    }else{
                        scope.callstack.Push(target); // Re-add this to the callstack, we still need more information.
                        parse(new CallTarget(state.returns, 1, token[3][0], target.variables));
                        break;
                    }
                    if(token[1].items.Count > 0){
                        if(!state.returns.ContainsKey(2)){
                            scope.callstack.Push(target);
                            modifyTarget.string_vars["target"].Get(scope, state.returns, 2, modifyTarget.string_vars["index"], ((VarNumber)modifyTarget.string_vars["islocal"]).data==1, false);
                        }else if(!state.returns.ContainsKey(3)){
                            scope.callstack.Push(target);
                            Math(state.returns, 3, state.returns[2], value, token[1][0][0].name);
                        }else{
                            modifyTarget.string_vars["target"].Set(scope, target.returnTarget, target.returnTargetID, modifyTarget.string_vars["index"], state.returns[3], ((VarNumber)modifyTarget.string_vars["islocal"]).data==1);
                        }
                    }else{
                        modifyTarget.string_vars["target"].Set(scope, target.returnTarget, target.returnTargetID, modifyTarget.string_vars["index"], value, ((VarNumber)modifyTarget.string_vars["islocal"]).data==1);
                    }
                    break;
                case "var":
                    if(token.data[0].name == "local"){
                        VarList vardata = new VarList();
                        vardata.string_vars["target"] = target.variables;
                        vardata.string_vars["index"] = token.data[1].items[0].text;
                        vardata.string_vars["islocal"] = token.data[0].items.Count;
                        vardata.string_vars["iscurry"] = 0;
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
                            parse(new CallTarget(state.returns, 0, token[0][0], target.variables));
                            break;
                        }
                        if(state.returns.ContainsKey(0)){
                            table_target = state.returns[0];
                        }else{
                            scope.callstack.Push(target);
                            parse(new CallTarget(state.returns, 0, token[0][0], target.variables));
                            break;
                        }
                        if(state.returns.ContainsKey(1)){
                            index = state.returns[1];
                        }else{
                            scope.callstack.Push(target);
                            parse(new CallTarget(state.returns, 1, token[1][0], target.variables));
                            break;
                        }
                        VarList vardata = new VarList();
                        vardata.string_vars["target"] = table_target;
                        vardata.string_vars["index"] = index;
                        vardata.string_vars["islocal"] = 0;
                        vardata.string_vars["iscurry"] = token[1][0][0].name == "::" ? 1 : 0;
                        ReturnValue(target, vardata);
                    }
                    break;
                case "index":
                    if(token[0].name == "\\[")
                        parse(new CallTarget(target.returnTarget, target.returnTargetID, token[1][0], target.variables));
                    else
                        ReturnValue(target, token[1][0].text);
                    break;
                case "ternary":
                    if(state == null){
                        state = target.state = new State();
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[0][0], target.variables));
                        return;
                    }else if(!state.returns.ContainsKey(1)){
                        scope.callstack.Push(target);
                        state.returns[0].ToBool(scope, state.returns, 1);
                        return;
                    }else{
                        if(state.returns[1] is VarNumber && 0.0d != (double)(VarNumber)state.returns[1]){
                            parse(new CallTarget(target.returnTarget, target.returnTargetID, token[2][0], target.variables));
                            return;
                        }else{
                            ReturnValue(target, state.returns[0]);
                            if(token[3].items.Count > 0){
                                parse(new CallTarget(target.returnTarget, target.returnTargetID, token[3][0][1][0], target.variables));
                                return;
                            }
                            return;
                        }
                    }
                case "eval":
                    if(state == null){
                        state = target.state = new State();
                        target.returnTarget[target.returnTargetID] = Var.nil;
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[1][0], target.variables));
                        return;
                    }else{
                        if(state.returns[0] is Variable.VarString){
                            string string_to_eval = (string)(Variable.VarString)state.returns[0];
                            Token parsed = Tokenizer.TokenCompiler.MatchToken(string_to_eval);
                            if(parsed == null){
                                target.returnTarget[target.returnTargetID] = Var.nil;
                            }else{
                                target.returnTarget[target.returnTargetID] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                                    scope.callstack.Push(new CallTarget(returnTarget, returnID, parsed, target.variables));
                                });
                            }
                        }
                        return;
                    }
                case "numberconstant":
                    ReturnValue(target, double.Parse(token.text));
                    break;
                case "stringconstant":
                    if(token[0].name == "templatestring"){
                        Token tstring = token[0][0];
                        string builtstring = "";
                        if(state == null)
                            target.state = state = new State();
                        if(tstring[1].items.Count>0)
                            builtstring += Regex.Unescape(tstring[1][0].text);
                        for(int i=0; i < tstring[2].items.Count; i++){
                            if(!state.returns.ContainsKey(i)){
                                scope.callstack.Push(target);
                                parse(new CallTarget(state.returns, i, tstring[2][i][0][0][1][0], target.variables));
                                return;
                            }else if(!(state.returns[i] is Variable.VarString)){
                                scope.callstack.Push(target);
                                state.returns[i].ToString(scope, state.returns, i);
                                return;
                            }else{
                                builtstring += ((Variable.VarString)state.returns[i]).data;
                            }
                            if(tstring[2][i][1].items.Count>0){
                                builtstring += Regex.Unescape(tstring[2][i][1][0].text);
                            }
                        }
                        ReturnValue(target,builtstring);
                    }else{
                        ReturnValue(target,Regex.Unescape(escape_strings.Replace(token[0][0].text, "$2")));
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
                            if(this_fill[0].name == "expression"){
                                parse(new CallTarget(state.returns, i*2, this_fill[0][0], target.variables)); // This is just an expression. Eg. {1, 2, 3}
                            }else if(this_fill[0].name == "var"){
                                parse(new CallTarget(state.returns, i*2, this_fill[2][0], target.variables));
                            }else{
                                parse(new CallTarget(state.returns, i*2, this_fill[2][0], target.variables));
                                parse(new CallTarget(state.returns, i*2+1, this_fill[0][0], target.variables));
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
                case "arithmatic":
                    if(state == null){
                        state = target.state = new State();
                    }
                    string op = token[1][0][0].name;
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[0][0], target.variables));
                        break;
                    }
                    if(op == "and"){
                        if(!state.returns.ContainsKey(1)){
                            scope.callstack.Push(target);
                            state.returns[0].ToBool(scope, state.returns, 1);
                        }else{
                            if(state.returns[1] is VarNumber && 0 != (double)(VarNumber)state.returns[1]){
                                parse(new CallTarget(target.returnTarget, target.returnTargetID, token[2][0], target.variables));
                            }else{
                                target.returnTarget[target.returnTargetID] = state.returns[0];
                            }
                        }
                        return;
                    }
                    if(op == "or"){
                        if(!state.returns.ContainsKey(1)){
                            scope.callstack.Push(target);
                            state.returns[0].ToBool(scope, state.returns, 1);
                        }else{
                            if(state.returns[1] is VarNumber && 0 == (double)(VarNumber)state.returns[1]){
                                parse(new CallTarget(target.returnTarget, target.returnTargetID, token[2][0], target.variables));
                            }else{
                                target.returnTarget[target.returnTargetID] = state.returns[0];
                            }
                        }
                        return;
                    }
                    if(!state.returns.ContainsKey(1)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 1, token[2][0], target.variables));
                        break;
                    }
                    Math(target.returnTarget, target.returnTargetID, state.returns[0], state.returns[1], op);
                    break;
                case "unaryarithmatic":
                    if(state == null){
                        state = target.state = new State();
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[1][0], target.variables));
                        break;
                    }
                    Math(target.returnTarget, target.returnTargetID, state.returns[0], token[0][0][0].name);
                    break;
                case "deop":
                    switch(token[1].name){
                        case "operator":
                            target.returnTarget[target.returnTargetID] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                                returnTarget[returnID] = Var.nil;
                                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars.ContainsKey(1)){
                                    Math(returnTarget, returnID, arguments.number_vars[0], arguments.number_vars[1], token[1][0][0].name);
                                }
                            });
                            break;
                        case "unoperator":
                            target.returnTarget[target.returnTargetID] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                                returnTarget[returnID] = Var.nil;
                                if(arguments.number_vars.ContainsKey(0)){
                                    Math(returnTarget, returnID, arguments.number_vars[0], token[1][0][0].name);
                                }
                            });
                            break;
                        case "expression":
                            target.returnTarget[target.returnTargetID] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                                parse(new CallTarget(returnTarget, returnID, token[1][0], target.variables));
                            });
                            break;
                    }
                    ((VarFunction)target.returnTarget[target.returnTargetID]).FunctionText = token.text;
                    break;
                case "is":
                    if(token[0][0][0].name == "var"){
                        if(state == null){
                            state = target.state = new State();
                        }
                        if(!state.returns.ContainsKey(0)){
                            scope.callstack.Push(target);
                            parse(new CallTarget(state.returns, 0, token[0][0][0][0], target.variables));
                            return;
                        }else{
                            if(token[2].name == "expression"){
                                if(!state.returns.ContainsKey(1)){
                                    scope.callstack.Push(target);
                                    parse(new CallTarget(state.returns, 1, token[2][0], target.variables));
                                    return;
                                }
                            }
                            Var p;
                            while((p=state.returns[0].string_vars["target"].GetMeta(scope, "_parent"))!=Var.nil && !state.returns[0].string_vars["target"].HasVariable(state.returns[0].string_vars["index"])){
                                state.returns[0].string_vars["target"] = p;
                            }
                            if(state.returns[0].string_vars["target"].handler == null){
                                state.returns[0].string_vars["target"].handler = new VarList();
                            }
                            state.returns[0].string_vars["target"].handler.Get(scope, state.returns, 3, state.returns[0].string_vars["index"], true, false);
                            if(state.returns[3] == Var.nil){
                                state.returns[0].string_vars["target"].handler.Set(scope, state.returns, 3, state.returns[0].string_vars["index"], new VarEvent(), true);
                            }
                            VarEvent sub_event = new VarEvent();
                            target.returnTarget[target.returnTargetID] = sub_event;
                            ((VarEvent)state.returns[3]).Hook(new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                                State sub_state = new State();
                                returnTarget[returnID] = Var.nil;
                                if(token[2].name == "expression"){
                                    scope.callstack.Push(new DoLater(delegate{
                                        if(sub_state.returns[0] is VarNumber && 0d!=(double)(VarNumber)sub_state.returns[0]){
                                            sub_event.Call(scope, returnTarget, returnID, arguments.number_vars[0]);
                                        }
                                    }));
                                    Math(sub_state.returns, 0, arguments.number_vars[0], state.returns[1], "eq");
                                }else{
                                    sub_event.Call(scope, returnTarget, returnID, arguments.number_vars[0]);
                                }
                            }));
                        }
                    }else{
                        target.returnTarget[target.returnTargetID] = Var.nil;
                        return;
                    }
                    break;
                case "whenblock":
                    if(state == null){
                        state = target.state = new State();
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[1][0], target.variables));
                        return;
                    }else{
                        if(!(state.returns[0] is VarEvent)){
                            target.returnTarget[target.returnTargetID] = Var.nil;
                            return;
                        }
                        VarEvent evnt = (VarEvent) state.returns[0];
                        evnt.Hook(new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                            VarList newScopeThing = new VarList();
                            newScopeThing.meta = new VarList();
                            newScopeThing.meta.string_vars["_index"] = new VarFunction(delegate(Scope Nscope, Dictionary<int, Var> NreturnTarget, int NreturnID, VarList Narguments){
                                State mState = new State();
                                scope.callstack.Push(new DoLater(delegate{
                                    if(mState.returns[0] == Var.nil)
                                        NreturnTarget[NreturnID] = mState.returns[1];
                                    else
                                        NreturnTarget[NreturnID] = mState.returns[0];
                                }));
                                arguments.Get(Nscope, mState.returns, 0, Narguments.number_vars[1], false, false);
                                target.variables.Get(Nscope, mState.returns, 1, Narguments.number_vars[1], false, false);
                            });
                            newScopeThing.meta.string_vars["_newindex"] = new VarFunction(delegate(Scope Nscope, Dictionary<int, Var> NreturnTarget, int NreturnID, VarList Narguments){
                                target.variables.Set(Nscope, NreturnTarget, NreturnID, Narguments.number_vars[1], Narguments.number_vars[2], false);
                            });
                            scope.callstack.Push(new CallTarget(returnTarget, returnID, token[2][0], newScopeThing));
                        }));
                        target.returnTarget[target.returnTargetID] = evnt;
                        return;
                    }
                case "withblock":
                    if(state == null){
                        state = target.state = new State();
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[1][0], target.variables));
                        return;
                    }else{
                        VarList newScopeThing = new VarList();
                        newScopeThing.meta = new VarList();
                        newScopeThing.meta.string_vars["_index"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                            State mState = new State();
                            scope.callstack.Push(new DoLater(delegate{
                                if(mState.returns[0] == Var.nil)
                                    returnTarget[returnID] = mState.returns[1];
                                else
                                    returnTarget[returnID] = mState.returns[0];
                            }));
                            state.returns[0].Get(scope, mState.returns, 0, arguments.number_vars[1], false, false);
                            target.variables.Get(scope, mState.returns, 1, arguments.number_vars[1], false, false);
                        });
                        newScopeThing.meta.string_vars["_newindex"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                            state.returns[0].Set(scope, returnTarget, returnID, arguments.number_vars[1], arguments.number_vars[2], false);
                        });
                        parse(new CallTarget(target.returnTarget, target.returnTargetID, token[2][0], newScopeThing));
                        return;
                    }
                case "switchblock":
                    if(state == null){
                        state = target.state = new State();
                        state.returns[-1] = scope.listFromParent(target.variables);
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[1][0], (VarList)state.returns[-1]));
                        return;
                    }else{
                        List<Token> cases = token[2][0][0].items;
                        if(token[2][0][0].name != "case"){
                            cases = token[2][0][1].items;
                        }
                        State substate = new State();
                        int i = 0;
                        bool passed_ever = false;
                        DoLater toDo = null;
                        toDo = new DoLater(delegate{
                            if(returnsStack.Count > 0){
                                KeyValuePair<string, Var> returns = returnsStack.Pop();
                                target.returnTarget[target.returnTargetID] = returns.Value;
                                if(returns.Key != "break")
                                    returnsStack.Push(returns);
                                return;
                            }
                            if(i >= cases.Count){
                                return;
                            }
                            Token this_case = cases[i];
                            if(!passed_ever){
                                if(this_case[1].name == "default"){
                                    passed_ever = true;
                                }else{
                                    if(!substate.returns.ContainsKey(0)){
                                        scope.callstack.Push(toDo);
                                        parse(new CallTarget(substate.returns, 0, this_case[2][0], (VarList)state.returns[-1]));
                                        return;
                                    }else if(!substate.returns.ContainsKey(1)){
                                        scope.callstack.Push(toDo);
                                        Math(substate.returns, 1, state.returns[0], substate.returns[0], "eq");
                                        return;
                                    }else{
                                        if(substate.returns[1] is VarNumber && 0.0f != (double)(VarNumber)substate.returns[1]){
                                            passed_ever = true;
                                        }else{
                                            substate.returns.Remove(0);
                                            substate.returns.Remove(1);
                                            i++;
                                            scope.callstack.Push(toDo);
                                            return;
                                        }
                                    }
                                }
                            }

                            State prgmState = new State();
                            int c = 0;
                            List<Token> prgms = this_case[2].items;
                            if(this_case[1].name != "default"){
                                prgms = this_case[3].items;
                            }
                            DoLater prgmLoop = null;
                            prgmLoop = new DoLater(delegate{
                                if(returnsStack.Count > 0){
                                    KeyValuePair<string, Var> returns = returnsStack.Pop();
                                    target.returnTarget[target.returnTargetID] = returns.Value;
                                    returnsStack.Push(returns);
                                    return;
                                }
                                if(c >= prgms.Count){
                                    return;
                                }
                                scope.callstack.Push(prgmLoop);
                                parse(new CallTarget(target.returnTarget, target.returnTargetID, prgms[c][0][0], (VarList)state.returns[-1]));
                                c++;
                            });
                            scope.callstack.Push(toDo);
                            scope.callstack.Push(prgmLoop);
                            i++;
                        });
                        scope.callstack.Push(toDo);
                    }
                    break;
                case "exporblock":
                    if(token[0].name == "expression"){
                        parse(new CallTarget(target.returnTarget, target.returnTargetID, token[0][0], target.variables));
                    }else{
                        if(state == null){
                            state = target.state = new State();
                            state.returns[-1] = scope.listFromParent(target.variables);
                        }
                        if(returnsStack.Count > 0){
                            target.returnTarget[target.returnTargetID] = returnsStack.Peek().Value;
                            return;
                        }
                        List<Token> block = token[0][0][1].items;
                        for(int i=0; i < block.Count; i++){
                            if(!state.returns.ContainsKey(i)){
                                scope.callstack.Push(target);
                                scope.callstack.Push(new CallTarget(state.returns, i, block[i][0][0], (VarList)state.returns[-1]));
                                return;
                            }
                        }
                        target.returnTarget[target.returnTargetID] = block.Count>0?state.returns[block.Count-1]:Var.nil;
                    }
                    break;
                case "ifblock":
                    target.returnTarget[target.returnTargetID] = Var.nil;
                    if(state == null){
                        state = target.state = new State();
                        state.returns[-1] = scope.listFromParent(target.variables);
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[1][0], (VarList)state.returns[-1]));
                    }else if(!(state.returns[0] is VarNumber)){
                        scope.callstack.Push(target);
                        state.returns[0].ToBool(scope, state.returns, 0);
                    }else{
                        if(((VarNumber)state.returns[0]).data > 0){
                            parse(new CallTarget(target.returnTarget, target.returnTargetID, token[2][0], (VarList)state.returns[-1]));
                        }else{
                            if(token[3].items.Count > 0){
                                parse(new CallTarget(target.returnTarget, target.returnTargetID, token[3][0][1][0], (VarList)state.returns[-1]));
                            }
                        }
                    }
                    break;
                case "crementor":
                    if(state == null){
                        state = target.state = new State();
                    }
                    if(token[0].name == "expression"){
                        if(token[0][0][0].name != "var"){
                            target.returnTarget[target.returnTargetID] = Var.nil;
                            return;
                        }
                        if(!state.returns.ContainsKey(0)){
                            scope.callstack.Push(target);
                            scope.callstack.Push(new CallTarget(state.returns, 0, token[0][0][0][0], target.variables));
                        }else if(!state.returns.ContainsKey(1)){
                            scope.callstack.Push(target);
                            state.returns[0].string_vars["target"].Get(scope, state.returns, 1, state.returns[0].string_vars["index"], ((VarNumber)state.returns[0].string_vars["islocal"]).data==1, false);
                        }else if(!state.returns.ContainsKey(2)){
                            scope.callstack.Push(target);
                            target.returnTarget[target.returnTargetID] = state.returns[1];
                            Math(state.returns, 2, state.returns[1], 1, token[1][0].text == "++" ? "add" : "sub");
                        }else{
                            state.returns[0].string_vars["target"].Set(scope, state.returns, 1, state.returns[0].string_vars["index"], state.returns[2], ((VarNumber)state.returns[0].string_vars["islocal"]).data==1);
                        }
                        return;
                    }else{
                        if(token[1][0][0].name != "var"){
                            target.returnTarget[target.returnTargetID] = Var.nil;
                            return;
                        }
                        if(!state.returns.ContainsKey(0)){
                            scope.callstack.Push(target);
                            scope.callstack.Push(new CallTarget(state.returns, 0, token[1][0][0][0], target.variables));
                        }else if(!state.returns.ContainsKey(1)){
                            scope.callstack.Push(target);
                            state.returns[0].string_vars["target"].Get(scope, state.returns, 1, state.returns[0].string_vars["index"], ((VarNumber)state.returns[0].string_vars["islocal"]).data==1, false);
                        }else if(!state.returns.ContainsKey(2)){
                            scope.callstack.Push(target);
                            Math(state.returns, 2, state.returns[1], 1, token[0][0].text == "++" ? "add" : "sub");
                        }else{
                            state.returns[0].string_vars["target"].Set(scope, target.returnTarget, target.returnTargetID, state.returns[0].string_vars["index"], state.returns[2], ((VarNumber)state.returns[0].string_vars["islocal"]).data==1);
                        }
                        return;
                    }
                case "whileblock":
                    if(returnsStack.Count > 0){
                        KeyValuePair<string, Var> returns = returnsStack.Pop();
                        target.returnTarget[target.returnTargetID] = returns.Value;
                        if(returns.Key != "break")
                            returnsStack.Push(returns);
                        return;
                    }
                    if(state == null){
                        state = target.state = new State();
                        state.returns[-1] = scope.listFromParent(target.variables);
                        state.returns[-2] = Var.nil;
                    }
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[1][0], (VarList)state.returns[-1]));
                        return;
                    }else if(!state.returns.ContainsKey(1)){
                        scope.callstack.Push(target);
                        state.returns[0].ToBool(scope, state.returns, 1);
                        return;
                    }else{
                        Var out_bool = state.returns[1];
                        state.returns.Remove(0);
                        state.returns.Remove(1);
                        if(out_bool is VarNumber){
                            if(((VarNumber)out_bool).data > 0){
                                scope.callstack.Push(target);
                                parse(new CallTarget(state.returns, -2, token[2][0], (VarList)state.returns[-1]));

                            }else{
                                target.returnTarget[target.returnTargetID] = state.returns[-2];
                            }
                        }else{
                            target.returnTarget[target.returnTargetID] = Var.nil;
                        }
                    }
                    break;
                case "forloop":
                    if(returnsStack.Count > 0){
                        KeyValuePair<string, Var> returns = returnsStack.Pop();
                        target.returnTarget[target.returnTargetID] = returns.Value;
                        if(returns.Key != "break")
                            returnsStack.Push(returns);
                        return;
                    }
                    if(state == null){
                        target.returnTarget[target.returnTargetID] = Var.nil;
                        state = target.state = new State();
                        state.returns[-1] = scope.listFromParent(target.variables);
                        state.returns[-2] = Var.nil;
                        if(token[1].name != "var" && token[3].items.Count > 0){
                            scope.callstack.Push(target);
                            parse(new CallTarget(token[2][0][0][0], (VarList)state.returns[-1]));
                            return;
                        }
                    }
                    if(token[1].name == "var"){
                        if(token[1].items.Count > 0){
                            if(!state.returns.ContainsKey(8)){ // 8 is infact arbitrary.
                                scope.callstack.Push(target);
                                scope.callstack.Push(new CallTarget(state.returns, 8, token[1][0], (VarList)state.returns[-1]));
                                return;
                            }
                        }
                        if(!state.returns.ContainsKey(0)){
                            scope.callstack.Push(target);
                            scope.callstack.Push(new CallTarget(state.returns, 0, token[3][0], (VarList)state.returns[-1]));
                            return;
                        }else{
                            Var to_iterate = state.returns[0];
                            if(to_iterate is VarNumber){
                                VarNumber n = new VarNumber(0);
                                DoLater toDo = null;
                                toDo = new DoLater(delegate{
                                    if(returnsStack.Count > 0){
                                        KeyValuePair<string, Var> returns = returnsStack.Pop();
                                        target.returnTarget[target.returnTargetID] = returns.Value;
                                        if(returns.Key != "break")
                                            returnsStack.Push(returns);
                                        return;
                                    }
                                    if(n.data <= ((VarNumber)to_iterate).data){
                                        scope.callstack.Push(toDo);
                                        scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[4][0], (VarList)state.returns[-1]));
                                        if(token[1].items.Count > 0){
                                            VarList vardata = (VarList)state.returns[8];
                                            vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], n.data, ((VarNumber)vardata.string_vars["islocal"]).data==1);
                                        }
                                    }
                                    n.data++;
                                });
                                scope.callstack.Push(toDo);
                            }else if(to_iterate is Variable.VarString){
                                int i = 0;
                                DoLater toDo = null;
                                toDo = new DoLater(delegate{
                                    if(returnsStack.Count > 0){
                                        KeyValuePair<string, Var> returns = returnsStack.Pop();
                                        target.returnTarget[target.returnTargetID] = returns.Value;
                                        if(returns.Key != "break")
                                            returnsStack.Push(returns);
                                        return;
                                    }
                                    if(i < ((Variable.VarString)to_iterate).data.Length){
                                        scope.callstack.Push(toDo);
                                        scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[4][0], (VarList)state.returns[-1]));
                                        if(token[1].items.Count > 0){
                                            VarList vardata = (VarList)state.returns[8];
                                            vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], ""+((Variable.VarString)to_iterate).data[i], ((VarNumber)vardata.string_vars["islocal"]).data==1);
                                        }
                                    }
                                    i++;
                                });
                                scope.callstack.Push(toDo);
                            }else if(to_iterate is VarFunction){
                                State sub_state = new State();
                                DoLater toDo = null;
                                target.returnTarget[target.returnTargetID] = Var.nil;
                                toDo = new DoLater(delegate{
                                    if(sub_state.returns.ContainsKey(0)){
                                        if(sub_state.returns[0] != Var.nil){
                                            scope.callstack.Push(toDo);
                                            scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[4][0], (VarList)state.returns[-1]));
                                            if(token[1].items.Count > 0){
                                                VarList vardata = (VarList)state.returns[8];
                                                vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], sub_state.returns[0], ((VarNumber)vardata.string_vars["islocal"]).data==1);
                                            }
                                        }
                                        sub_state.returns.Remove(0);
                                    }else{
                                        scope.callstack.Push(toDo);
                                        to_iterate.Call(scope, sub_state.returns, 0);
                                    }
                                });
                                scope.callstack.Push(toDo);
                            
                            }else{
                                VarEnumerator enm = to_iterate.GetEnumerator();
                                DoLater toDo = null;
                                toDo = new DoLater(delegate{
                                    if(returnsStack.Count > 0){
                                        KeyValuePair<string, Var> returns = returnsStack.Pop();
                                        target.returnTarget[target.returnTargetID] = returns.Value;
                                        if(returns.Key != "break")
                                            returnsStack.Push(returns);
                                        return;
                                    }
                                    if(enm.MoveNext()){
                                        scope.callstack.Push(toDo);
                                        scope.callstack.Push(new CallTarget(target.returnTarget, target.returnTargetID, token[4][0], (VarList)state.returns[-1]));
                                        if(token[1].items.Count > 0){
                                            VarList vardata = (VarList)state.returns[8];
                                            vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], enm.Current.Key, ((VarNumber)vardata.string_vars["islocal"]).data==1);
                                        }
                                    }
                                });
                                scope.callstack.Push(toDo);
                            }
                        }
                    }else{
                        if(token[2].items.Count >= 2 && !state.returns.ContainsKey(0)){
                            scope.callstack.Push(target);
                            parse(new CallTarget(state.returns, 0, token[2][1][0][0], (VarList)state.returns[-1]));
                            return;
                        }else if(token[2].items.Count >= 2 && !state.returns.ContainsKey(1)){
                            scope.callstack.Push(target);
                            state.returns[0].ToBool(scope, state.returns, 1);
                            return;
                        }else{
                            Var out_bool = new VarNumber(1);
                            if(token[2].items.Count >= 2){
                                out_bool = state.returns[1];
                                state.returns.Remove(0);
                                state.returns.Remove(1);
                            }
                            if(out_bool is VarNumber){
                                if(((VarNumber)out_bool).data > 0){
                                    scope.callstack.Push(target);
                                    if(token[2].items.Count >= 3)
                                        scope.callstack.Push(new CallTarget(token[2][2][0][0], (VarList)state.returns[-1]));
                                    parse(new CallTarget(state.returns, -2, token[4][0], (VarList)state.returns[-1]));

                                }else{
                                    target.returnTarget[target.returnTargetID] = state.returns[-2];
                                }
                            }else{
                                target.returnTarget[target.returnTargetID] = Var.nil;
                            }
                        }
                    }

                    break;
                case "arg_list":
                    if(state == null){
                        state = target.state = new State();
                    }
                    for(int i=0; i < token[1].items.Count; i++){
                        if(!state.returns.ContainsKey(i*2)){
                            if(token[1][i].data.Count == 2){
                                scope.callstack.Push(target);
                                state.returns[i*2] = token[1][i][0][0].text;
                                state.returns[i*2+1] = Var.nil;
                            }else{
                                scope.callstack.Push(target);
                                state.returns[i*2] = token[1][i][0][0].text;
                                parse(new CallTarget(state.returns, i*2+1, token[1][i][2][0], target.variables));
                            }
                            return;
                        }
                    }
                    VarList returnd = new VarList();
                    returnd.string_vars["splat_to"] = Var.nil;
                    if(token[2].items.Count > 0){
                        returnd.string_vars["splat_to"] = token[2][0][1][0].text;
                    }
                    returnd.string_vars["var_names"] = new VarList();
                    returnd.string_vars["defaults"] = new VarList();
                    returnd.string_vars["var_count"] = 0;
                    for(int i=0; i < token[1].items.Count; i++){
                        returnd.string_vars["var_names"].number_vars[i] = state.returns[i*2];
                        returnd.string_vars["defaults"].number_vars[i] = state.returns[i*2+1];
                        returnd.string_vars["var_count"] = i+1;
                    }
                    target.returnTarget[target.returnTargetID] = returnd;
                    break;
                case "function_builder":
                    if(state==null){
                        state = target.state = new State();
                    }
                    if(token[0].name == "function"){ // Chunky Funky
                        if(token[2].name == "arg_list"){
                            if(!state.returns.ContainsKey(0)){
                                scope.callstack.Push(target);
                                parse(new CallTarget(state.returns, 0, token[2][0], target.variables));
                                return;
                            }
                        }else{
                            if(!state.returns.ContainsKey(0)){
                                scope.callstack.Push(target);
                                VarList fakelist = new VarList();
                                fakelist.string_vars["splat_to"] = Var.nil;
                                fakelist.string_vars["var_names"] = new VarList();
                                fakelist.string_vars["defaults"] = new VarList();
                                fakelist.string_vars["var_count"] = 1;
                                fakelist.string_vars["var_names"].number_vars[0] = token[1][0].text;
                                fakelist.string_vars["defaults"].number_vars[0] = Var.nil;
                                state.returns[0] = fakelist;
                                return;
                            }
                        }
                    }else{  // Lightweight Function
                        if(token[0].name == "arg_list"){
                            if(!state.returns.ContainsKey(0)){
                                scope.callstack.Push(target);
                                parse(new CallTarget(state.returns, 0, token[0][0], target.variables));
                                return;
                            }
                        }else{
                            if(!state.returns.ContainsKey(0)){
                                scope.callstack.Push(target);
                                VarList fakelist = new VarList();
                                fakelist.string_vars["splat_to"] = Var.nil;
                                fakelist.string_vars["var_names"] = new VarList();
                                fakelist.string_vars["defaults"] = new VarList();
                                fakelist.string_vars["var_count"] = 1;
                                fakelist.string_vars["var_names"].number_vars[0] = token[0][0].text;
                                fakelist.string_vars["defaults"].number_vars[0] = Var.nil;
                                state.returns[0] = fakelist;
                                return;
                            }
                        }
                    }
                    Token toRun = null;
                    if(token[0].name == "function"){
                        if(token[2].name == "arg_list"){
                            toRun = token[3][0];
                        }else{
                            toRun = token[2][0];
                        }
                    }else{
                        toRun = token[2][0];
                    }
                    VarFunction outp = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                        Dictionary<string, Var> arg_data = state.returns[0].string_vars;
                        VarList fakeScope = new VarList();
                        fakeScope.meta = new VarList();
                        fakeScope.meta.string_vars["_newindex"] = fakeScope.meta.string_vars["_index"] = target.variables;
                        for(int i=0; i < (int)(VarNumber)arg_data["var_count"]; i++){
                            fakeScope.string_vars[(string)(Variable.VarString)arg_data["var_names"].number_vars[i]] = arg_data["defaults"].number_vars[i];
                        }
                        if(arg_data["splat_to"] != Var.nil){
                            fakeScope.string_vars[(string)(Variable.VarString)arg_data["splat_to"]] = new VarList();
                        }
                        foreach(KeyValuePair<Var, Var> kv in arguments){
                            if(kv.Key is VarNumber){
                                VarNumber k = (VarNumber)kv.Key;
                                if(k.data >= 0 && k.data%1 == 0){
                                    if(k.data >= ((VarNumber)arg_data["var_count"]).data && arg_data["splat_to"] != Var.nil){
                                        fakeScope.string_vars[(string)(Variable.VarString)arg_data["splat_to"]].number_vars[k.data - ((VarNumber)arg_data["var_count"]).data] = kv.Value;
                                        continue;
                                    }else if(k.data < ((VarNumber)arg_data["var_count"]).data){
                                        fakeScope.string_vars[(string)(Variable.VarString)arg_data["var_names"].number_vars[k.data]] = kv.Value;
                                        continue;
                                    }
                                }
                                fakeScope.number_vars[k.data] = kv.Value;
                            }else if(kv.Key is Variable.VarString){
                                if(arg_data["splat_to"] == Var.nil || arg_data["var_names"].number_vars.ContainsValue((string)(Variable.VarString)kv.Key))
                                    fakeScope.string_vars[(string)(Variable.VarString)kv.Key] = kv.Value;
                                else
                                    fakeScope.string_vars[(string)(Variable.VarString)arg_data["splat_to"]].string_vars[(string)(Variable.VarString)kv.Key] = kv.Value;
                            }else{
                                fakeScope.other_vars[kv.Key] = kv.Value;
                            }
                        }
                        scope.callstack.Push(new DoLater(delegate{
                            if(returnsStack.Count > 0){
                                KeyValuePair<string, Var> returns = returnsStack.Pop();
                                returnTarget[returnID] = returns.Value;
                                if(returns.Key != "return")
                                    returnsStack.Push(returns);
                                return;
                            }
                        }));
                        parse(new CallTarget(returnTarget, returnID, toRun, fakeScope));
                    });
                    if(token[0].name == "function" && token[2].name == "arg_list" && token[1].items.Count > 0){
                        scope.callstack.Push(new DoLater(delegate{
                            Var vardata = state.returns[-2];
                            vardata.string_vars["target"].Set(scope, state.returns, 90, vardata.string_vars["index"], outp, ((VarNumber)vardata.string_vars["islocal"]).data==1);
                        }));
                        parse(new CallTarget(state.returns, -2, token[1][0], target.variables));
                    }
                    outp.FunctionText = token.text;
                    target.returnTarget[target.returnTargetID] = outp;
                    break;
                case "call":
                    if(state == null){
                        target.state = state = new State();
                    }
                    Var toCall = null;
                    if(!state.returns.ContainsKey(0)){
                        scope.callstack.Push(target);
                        parse(new CallTarget(state.returns, 0, token[0][0], target.variables));
                        break;
                    }else{
                        toCall = state.returns[0];
                    }
                    switch(token[1].name){
                        case "splat_call":
                            if(!state.returns.ContainsKey(1)){
                                scope.callstack.Push(target);
                                parse(new CallTarget(state.returns, 1, token[1][0][1][0], target.variables));
                            }else{
                                if(state.returns[1] is VarList)
                                    toCall.Call(scope, target.returnTarget, target.returnTargetID, (VarList)state.returns[1]);
                                else
                                    toCall.Call(scope, target.returnTarget, target.returnTargetID, state.returns[1]);
                            }
                            return;
                        case "stringconstant": case "deop":
                            if(!state.returns.ContainsKey(1)){
                                scope.callstack.Push(target);
                                parse(new CallTarget(state.returns, 1, token[1][0], target.variables));
                                return;
                            }else{
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
                                            if(v.Key is VarNumber)
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