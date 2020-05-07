using ByondLang.Language.Variable;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ByondLang.Language
{
    public static class LibString{
        public static VarList Generate(VarList globals){
            VarList string_VAR = new VarList();
            Dictionary<string, Var> str = string_VAR.string_vars;
            str["sub"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(2))
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).Substring((int)(VarNumber)arguments.number_vars[1], (int)(VarNumber)arguments.number_vars[2]);
                else
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).Substring((int)(VarNumber)arguments.number_vars[1]);
            });

            str["match"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        Match m = matcher.Match(haystack);
                        if(m.Success){
                            if(m.Groups.Count == 1){
                                returnTarget[returnID] = m.Groups[0].Value;
                            }else{
                                VarList outList = new VarList();
                                for(int i=0; i < m.Groups.Count; i++){
                                    outList.string_vars[m.Groups[i].Name] = outList.number_vars[i] = m.Groups[i].Value;
                                }
                                returnTarget[returnID] = outList;
                            }
                        }
                    }catch{}
                }
            });

            str["gmatch"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        MatchCollection M = matcher.Matches(haystack);
                        int i = 0;
                        returnTarget[returnID] = new VarFunction(delegate(Scope nscope, Dictionary<int, Var> nreturnTarget, int nreturnID, VarList narguments){
                            if(i < M.Count){
                                Match m = M[i];
                                if(m.Groups.Count == 1){
                                    nreturnTarget[nreturnID] = m.Groups[0].Value;
                                }else{
                                    VarList outList = new VarList();
                                    for(int c=0; c < m.Groups.Count; c++){
                                        outList.string_vars[m.Groups[c].Name] = outList.number_vars[c] = m.Groups[c].Value;
                                    }
                                    nreturnTarget[nreturnID] = outList;
                                }
                            }else{
                                nreturnTarget[nreturnID] = Var.nil;
                            }
                            i++;
                        });
                    }catch{}
                }
            });

            str["gsub"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString && arguments.number_vars.ContainsKey(2) && arguments.number_vars[2] is VarString){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        returnTarget[returnID] = matcher.Replace(haystack, (string)(VarString)arguments.number_vars[2]);
                    }catch{}
                }else if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString && arguments.number_vars.ContainsKey(1) && arguments.number_vars[1] is VarString && arguments.number_vars.ContainsKey(2) && arguments.number_vars[2] is VarFunction){
                    string haystack = (string)(VarString)arguments.number_vars[0];
                    string needle = (string)(VarString)arguments.number_vars[1];
                    VarFunction replace_func = (VarFunction)arguments.number_vars[2];
                    Regex matcher = new Regex(needle, RegexOptions.None, new System.TimeSpan(0, 0, 1));
                    try{
                        MatchCollection M = matcher.Matches(haystack);
                        string built = "";
                        int lastIndex = 0;
                        int i = 0;
                        DoLater loopfnc = null;
                        State s_state = new State();
                        loopfnc = new DoLater(delegate{
                            if(i < M.Count){
                                scope.callstack.Push(loopfnc);
                                Match m = M[i];
                                if(i > 0){
                                    if(s_state.returns[0] is VarString){
                                        built += (string)(VarString)s_state.returns[0];
                                    }
                                }
                                built += haystack.Substring(lastIndex, m.Index - lastIndex);
                                lastIndex = m.Index + m.Groups[0].Length;
                                if(m.Groups.Count == 1){
                                    replace_func.Call(scope, s_state.returns, 0, m.Groups[0].Value);
                                }else{
                                    VarList outList = new VarList();
                                    for(int c=0; c < m.Groups.Count; c++){
                                        outList.string_vars[m.Groups[c].Name] = outList.number_vars[c] = m.Groups[c].Value;
                                    }
                                    replace_func.Call(scope, s_state.returns, 0, outList);
                                }
                                i++;
                            }else{
                                if(i > 0){
                                    if(s_state.returns[0] is VarString){
                                        built += (string)(VarString)s_state.returns[0];
                                    }
                                }
                                built += haystack.Substring(lastIndex, haystack.Length - lastIndex);
                                returnTarget[returnID] = built;
                            }
                        });
                        scope.callstack.Push(loopfnc);
                    }catch{}
                }
            });            

            str["upper"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).ToUpper();
                }
            });
            str["lower"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    returnTarget[returnID] = ((string)(VarString)arguments.number_vars[0]).ToLower();
                }
            });
            str["reverse"] = new VarFunction(delegate(Scope scope, Dictionary<int, Var> returnTarget, int returnID, VarList arguments){
                if(arguments.number_vars.ContainsKey(0) && arguments.number_vars[0] is VarString){
                    char[] arr =  ((string)(VarString)arguments.number_vars[0]).ToCharArray();
                    System.Array.Reverse(arr);
                    returnTarget[returnID] = new string(arr);
                }
            });

            return string_VAR;
        }
    }
}