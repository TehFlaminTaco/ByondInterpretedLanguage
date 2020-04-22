using System.Collections.Generic;
using System.Collections;
using ByondLang;
using System;

namespace ByondLang.Variable{
    class Var : IEnumerable{

        bool ReadOnly = false;

        public static Var nil = new Var();

        
        public Dictionary<double, Var> number_vars = new Dictionary<double, Var>();
        public Dictionary<string, Var> string_vars = new Dictionary<string, Var>();
        public Dictionary<Var, Var> other_vars = new Dictionary<Var, Var>();


        IEnumerator IEnumerable.GetEnumerator(){
            return (IEnumerator) GetEnumerator();
        }

        public VarEnumerator GetEnumerator(){
            return new VarEnumerator(this);
        }

        public Var meta = nil;

        public VarList handler;

        public string type = "nil";

        public Var FromDefaultMeta(Scope scope, string key){
            if(scope.metas.ContainsKey(type)){
                if(scope.metas[type].string_vars.ContainsKey(key)){
                    return scope.metas[type].string_vars[key];
                }
            }
            return nil;
        }

        public Var GetMeta(Scope scope, string key){
            if(meta == null)
                return FromDefaultMeta(scope, key);
            if(meta == nil)
                return FromDefaultMeta(scope, key);
            if(!meta.string_vars.ContainsKey(key))
                return FromDefaultMeta(scope, key);
            return meta.string_vars[key];
        }

        public bool HasVariable(Var key){
            if(key is VarNumber){
                return number_vars.ContainsKey((double)(VarNumber)key);
            }
            if(key is VarString){
                return string_vars.ContainsKey((string)(VarString)key);
            }
            return other_vars.ContainsKey(key);
        }

        public void Get(Scope scope, Dictionary<int, Var> returnTarget, int returnID, Var key, bool force, bool curry){
            if(curry){
                scope.callstack.Push(new DoLater(delegate{
                    Var val = returnTarget[returnID];
                    returnTarget[returnID] = new VarFunction(delegate(Scope sub_scope, Dictionary<int, Var> sub_returnTarget, int sub_returnID, VarList arguments){
                        int max_arg = 0;
                        while(arguments.number_vars.ContainsKey(max_arg))
                            max_arg++;
                        for(int i=max_arg; i>0; i--)
                            arguments.number_vars[i] = arguments.number_vars[i-1];
                        arguments.number_vars[0] = this;
                        val.Call(scope, sub_returnTarget, sub_returnID, arguments);
                    });
                }));
            }
            if(this == nil){
                returnTarget[returnID] = nil;
                return;
            }

            if(key is VarNumber){
                double k = ((VarNumber)key).data;
                if(number_vars.ContainsKey(k)){
                    returnTarget[returnID] = number_vars[k];
                    return;
                }
            }else if(key is VarString){
                string k = ((VarString)key).data;
                if(string_vars.ContainsKey(k)){
                    returnTarget[returnID] = string_vars[k];
                    return;
                }
            }else{
                if(other_vars.ContainsKey(key)){
                    returnTarget[returnID] = other_vars[key];
                    return;
                }
            }

            if(!force){
                Var parent = GetMeta(scope, "_parent");
                if(parent != nil){
                    scope.callstack.Push(new DoLater(delegate{
                        parent.Get(scope, returnTarget, returnID, key);
                    }));
                    return;
                }
                Var index = GetMeta(scope, "_index");
                if(index != nil){
                    scope.callstack.Push(new DoLater(delegate{
                        if(index is VarList)
                            index.Get(scope, returnTarget, returnID, key);
                        else
                            index.Call(scope, returnTarget, returnID, this, key);
                    }));
                    return;
                }
            }

            returnTarget[returnID] = nil;
            return;
        }

        public void Set(Scope scope, Dictionary<int, Var> returnTarget, int returnID, Var key, Var value, bool force){
            if(ReadOnly){
                returnTarget[returnID] = nil;
                return;
            }
            if(this == nil){
                returnTarget[returnID] = nil;
                return;
            }

            if(!force){
                Var parent = GetMeta(scope, "_parent");
                if(!HasVariable(key) && parent != nil){
                    parent.Set(scope, returnTarget, returnID, key, value, false);
                    return;
                }
                Var newindex = GetMeta(scope, "_newindex");
                if(!HasVariable(key) && newindex != nil){
                    scope.callstack.Push(new DoLater(delegate{
                        if(newindex is VarList)
                            newindex.Set(scope, returnTarget, returnID, key, value);
                        else
                            newindex.Call(scope, returnTarget, returnID, this, key, value);
                    }));
                    return;
                }
            }

            if(key is VarNumber){
                if(value == nil)
                    number_vars.Remove(((VarNumber)key).data);
                else
                    number_vars[((VarNumber)key).data] = value;
            }else if(key is VarString){
                if(value == nil)
                    string_vars.Remove(((VarString)key).data);
                else
                    string_vars[((VarString)key).data] = value;
            }else if(value == nil)
                other_vars.Remove(key);
            else
                other_vars[key] = value;


            if(handler != null){
                State handlerState = new State();
                handler.Get(scope, handlerState.returns, 0, key, true, false);
                if(handlerState.returns[0] != nil){
                    scope.callstack.Push(new DoLater(delegate{
                        handlerState.returns[0].Call(scope, new Dictionary<int, Var>(), 0, value);
                    }));   
                }
            }
            
            Get(scope, returnTarget, returnID, key);
            return;
        }

        public void Get(Scope scope, Dictionary<int, Var> returnTarget, int returnID, Var key){
            Get(scope, returnTarget, returnID, key, false, false);
        }

        public void Set(Scope scope, Dictionary<int, Var> returnTarget, int returnID, Var key, Var value){
            Set(scope, returnTarget, returnID, key, value, false);
        }

        public virtual void Call(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            returnTarget[returnID] = this;
            var _call = GetMeta(scope, "_call");
            if(_call!=nil){
                scope.callstack.Push(new DoLater(delegate{
                    int max_arg = 0;
                    while(arguments.number_vars.ContainsKey(max_arg))
                        max_arg++;
                    for(int i = max_arg; i>0; i--){
                        arguments.number_vars[i] = arguments.number_vars[i-1];
                    }
                    arguments.number_vars[0] = this;
                    _call.Call(scope, returnTarget, returnID, arguments);
                }));
                return;
            }
        }

        public void Call(Scope scope, Dictionary<int, Var> returnTarget, int returnID, params Var[] arguments){
            Call(scope, returnTarget, returnID, new VarList(arguments));
        }

        public virtual void ToString(Scope scope, Dictionary<int, Var> returnTarget, int returnID){
            Var _toString = GetMeta(scope, "_tostring");
            if(_toString!=nil){
                if(_toString is VarString)
                    returnTarget[returnID] = _toString;
                else
                    scope.callstack.Push(new DoLater(delegate{
                        _toString.Call(scope, returnTarget, returnID, this);
                    }));

                return;
            }
            returnTarget[returnID] = new VarString("nil");
        }

        public virtual void ToBool(Scope scope, Dictionary<int, Var> returnTarget, int returnID){
            Var _toBool = GetMeta(scope, "_tobool");
            if(_toBool!=nil){
                if(_toBool is VarNumber)
                    returnTarget[returnID] = _toBool;
                else
                    scope.callstack.Push(new DoLater(delegate{
                        _toBool.Call(scope, returnTarget, returnID, this);
                    }));

                return;
            }
            returnTarget[returnID] = new VarNumber(0);
        }

        public virtual void ToNumber(Scope scope, Dictionary<int, Var> returnTarget, int returnID){
            Var _tonumber = GetMeta(scope, "_tonumber");
            if(_tonumber!=nil){
                if(_tonumber is VarNumber)
                    returnTarget[returnID] = _tonumber;
                else
                    scope.callstack.Push(new DoLater(delegate{
                        _tonumber.Call(scope, returnTarget, returnID, this);
                    }));

                return;
            }
            returnTarget[returnID] = new VarNumber(0);
        }

        public static implicit operator Var(double d){
            return new VarNumber(d);
        }
        public static implicit operator Var(string s){
            return new VarString(s);
        }
    }

    class VarEnumerator : IEnumerator{
        int state = 0;
        Var target;

        IEnumerator<KeyValuePair<double,Var>> number_vars;
        IEnumerator<KeyValuePair<string,Var>> string_vars;
        IEnumerator<KeyValuePair<Var,Var>> other_vars;

        public VarEnumerator(Var target){
            this.target = target;
            number_vars = target.number_vars.GetEnumerator();
            string_vars = target.string_vars.GetEnumerator();
            other_vars = target.other_vars.GetEnumerator();
        }

        public void Reset(){
            number_vars.Reset();
            string_vars.Reset();
            other_vars.Reset();
            state = 0;
        }

        object IEnumerator.Current{
            get{
                return Current;
            }
        }

        public bool MoveNext(){
            switch(state){
                case 0:
                    if(!number_vars.MoveNext()){
                        state++;
                        return MoveNext();
                    }
                    return true;
                case 1:
                    if(!string_vars.MoveNext()){
                        state++;
                        return MoveNext();
                    }
                    return true;
                case 2:
                    if(!other_vars.MoveNext()){
                        state++;
                        return MoveNext();
                    }
                    return true;
            }
            return false;
        }

        public KeyValuePair<Var,Var> Current{
            get{
                switch(state){
                    case 0:
                        return new KeyValuePair<Var, Var>(number_vars.Current.Key, number_vars.Current.Value);
                    case 1:
                        return new KeyValuePair<Var, Var>(string_vars.Current.Key, string_vars.Current.Value);
                    case 2:
                        return other_vars.Current;
                }
                throw new InvalidOperationException();
            }
        }
    }

    class VarNumber : Var{
        public double data = 0;
        public VarNumber(double data) : this(){
            this.data = data;
        }

        public VarNumber(){
            type = "number";
        }

        public static explicit operator double(VarNumber var){
            return var.data;
        }

        public override void ToBool(Scope scope, Dictionary<int, Var> returnTarget, int returnID){
            returnTarget[returnID] = new VarNumber(this.data>0?1:0);
        }
    }

    class VarString : Var{
        public string data = "";
        public VarString(string data) : this(){
            this.data = data;
        }

        public VarString(){
            type = "string";
        }

        public static explicit operator string(VarString var){
            return var.data;
        }

        public override void ToBool(Scope scope, Dictionary<int, Var> returnTarget, int returnID){
            Var _toBool = GetMeta(scope, "_tobool");
            if(_toBool!=nil){
                if(_toBool is VarNumber)
                    returnTarget[returnID] = _toBool;
                else
                    scope.callstack.Push(new DoLater(delegate{
                        _toBool.Call(scope, returnTarget, returnID, this);
                    }));

                return;
            }
            returnTarget[returnID] = new VarNumber(data.Length>0?1:0);
        }
    }

    delegate void VarFunc(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments);
    class VarFunction : Var{
        public VarFunc todo;
        public string FunctionText = "[internal function]";
        public VarFunction(){
            type = "function";
        }

        public VarFunction(VarFunc todo) : this(){
            this.todo = todo;
        }

        public override void Call(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
            returnTarget[returnID] = Var.nil;
            if(todo!=null)
                todo(scope, returnTarget, returnID, arguments);
            else
                returnTarget[returnID] = Var.nil;
        }

        public override void ToBool(Scope scope, Dictionary<int, Var> returnTarget, int returnID){
            Var _toBool = GetMeta(scope, "_tobool");
            if(_toBool!=nil){
                if(_toBool is VarNumber)
                    returnTarget[returnID] = _toBool;
                else
                    scope.callstack.Push(new DoLater(delegate{
                        _toBool.Call(scope, returnTarget, returnID, this);
                    }));

                return;
            }
            returnTarget[returnID] = new VarNumber(1);
        }
    }

    class VarEvent : Var{
        public List<Var> callbacks;

        public VarEvent() : base(){
            type = "event";
            callbacks = new List<Var>();
            string_vars["hook"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                returnTarget[returnID] = nil;
                if(arguments.number_vars.Count > 0){
                    Hook(arguments.number_vars[0]);
                    returnTarget[returnID] = arguments.number_vars[0];
                }
            });
            string_vars["call"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                Call(scope, returnTarget, returnID, arguments);
            });
        }

        public void Hook(Var callback){
            for(int i=0; i < callbacks.Count; i++){
                if(callbacks[i] == callback)
                    return;
            }
            callbacks.Add(callback);
        }
    }

    class VarList : Var{
        public Dictionary<string, Var> privateVariables = new Dictionary<string, Var>(); // Used internal to store things users shouldn't touch in lists.

        public VarList(){
            type = "list";
        }
        public VarList(params Var[] vars) : this(){
            if(vars!=null)
                for(int i=0; i < vars.Length; i++){
                    this.number_vars[i] = vars[i];
                }
        }

        public override void ToBool(Scope scope, Dictionary<int, Var> returnTarget, int returnID){
            Var _toBool = GetMeta(scope, "_tobool");
            if(_toBool!=nil){
                if(_toBool is VarNumber)
                    returnTarget[returnID] = _toBool;
                else
                    scope.callstack.Push(new DoLater(delegate{
                        _toBool.Call(scope, returnTarget, returnID, this);
                    }));

                return;
            }
            returnTarget[returnID] = new VarNumber((string_vars.Count + number_vars.Count + other_vars.Count)>0?1:0);
        }
    }
}