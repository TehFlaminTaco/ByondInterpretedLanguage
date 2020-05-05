using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

namespace ByondLang.Language.Tokenizer{
    public class InvalidTokenException : System.Exception
    {
        public InvalidTokenException() { }
        public InvalidTokenException(string message) : base(message) { }
        public InvalidTokenException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidTokenException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    class TokenPatterns{ // I'd document these, but I don't remember what they do.
        public static Regex SPEC_CHARS = new Regex("[\\s|]");
        public static Regex MATCH_STRING = new Regex("(['\"`])(([^\\\\]*?|\\\\.)*?)\\1");
        public static Regex MATCH_UPTO = new Regex("^(((['\"`])(([^\\\\]*?|\\\\.)*?)\\3|.)*?)("+SPEC_CHARS+"|$)");
        public static Regex MODIF_MATCH = new Regex("^(.*?)((\\*|\\+|:|\\?|\\{\\s*\\d*\\s*(?:,\\s*\\d*\\s*)?\\})*)$");
        public static Regex MODIF_SECT_MATCH = new Regex("([*+?:]|\\{\\s*\\d*\\s*(?:,\\s*\\d*\\s*)?\\})");
        public static Regex MODIF_N_MATCH = new Regex("\\{\\s*(\\d*)\\s*(?:,\\s*(\\d*)\\s*)?\\}");
        public static Regex REGEX_STRING = new Regex("(['\"`])((.|\\s)*)\\1");
    }

    enum TokenType{
        UNINITIALIZED, REGEX, TOKEN
    }

    class TokenMatch{
        public TokenType type = TokenType.UNINITIALIZED;
        public string data = "";
        public bool isPrefix = false;
        public int[] count = {1,1};
        public Regex regex = null;

        public TokenMatch(string definition){
            Match token_parts = TokenPatterns.MODIF_MATCH.Match(definition);
            MatchCollection modifiers = TokenPatterns.MODIF_SECT_MATCH.Matches(token_parts.Groups[2].Value); // AAAAAAAAAAAHH!!
            string token_string = token_parts.Groups[1].Value;
            
            data = token_string;

            foreach(Match modifier in modifiers){
                string modifier_text = modifier.Value;
                switch(modifier_text[0]){
                    case '?':
                        count[0] = 0;
                        count[1] = 1;
                        break;
                    case '*':
                        count[0] = 0;
                        count[1] = -1;
                        break;
                    case '+':
                        count[0] = 1;
                        count[1] = -1;
                        break;
                    case ':':
                        isPrefix = true;
                        break;
                    case '{':
                        Match counts = TokenPatterns.MODIF_N_MATCH.Match(modifier_text);
                        int countLower = 0;
                        int countHigher = 0;
                        Int32.TryParse(counts.Groups[1].Value, out countLower);
                        Int32.TryParse(counts.Groups[2].Value, out countHigher);
                        count = new int[]{countLower, countHigher};
                        break;
                }
            }

            if(token_string[0] == '\'' || token_string[0] == '"' || token_string[0] == '`'){
                type = TokenType.REGEX;
                data = TokenPatterns.REGEX_STRING.Match(token_string).Groups[2].Value; // AAAAAAAAAHHH!
                regex = new Regex('^' + data);
            }else{
                type = TokenType.TOKEN;
                if(!TokenCompiler.RawTokens.ContainsKey(data)){
                    throw new InvalidTokenException("Missing token: "+data);
                }
            }
        }
    }

    struct TokenHolder{
        public List<List<TokenMatch>> options;
        public bool isForcedWhitespace;
        public bool forceWhitespaceToggle;
    }

    class TokenCompiler{
        public static Dictionary<string, string> RawTokens = new Dictionary<string, string>();
        public static Dictionary<string, TokenHolder> Tokens = new Dictionary<string, TokenHolder>();
        public static Dictionary<string, Dictionary<string, TokenHolder>> prefixes = new Dictionary<string, Dictionary<string, TokenHolder>>(); //AAAAAAAAAAAAHHH!!!

        private static Regex token_line_string = new Regex("(.*?):(.*)");

        public static void GetTokens(){
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("ByondLang.Language.resource.tokens.txt");
            StreamReader reader = new StreamReader(stream);
            string line;
            while((line = reader.ReadLine())!= null){
                Match token_data = token_line_string.Match(line);
                if(token_data.Success){
                    String name = token_data.Groups[1].Value;
                    String data = token_data.Groups[2].Value;
                    RawTokens[name] = data;
                }
            }
        }

        public static void CompileTokens(){
            whitespace_stack = new Stack<bool>(); // Shutup, it's a relyable place
            whitespace_stack.Push(true);

            if(!RawTokens.ContainsKey("program")){
                throw new Exception("Tokenizers must define 'program'");
            }

            foreach(KeyValuePair<string, string> token_data in RawTokens){
                string tkn = token_data.Value;

                var token_holder = new TokenHolder();
                var options = token_holder.options = new List<List<TokenMatch>>();
                var option = new List<TokenMatch>();

                if(tkn[0]=='@'){
                    token_holder.forceWhitespaceToggle = true;
                    token_holder.isForcedWhitespace = tkn[1] == '1';
                    tkn = tkn.Substring(2);
                }

                while(tkn.Length > 0){
                    tkn = Regex.Replace(tkn, "^\\s+", "");
                    Match sub_token = TokenPatterns.MATCH_UPTO.Match(tkn);
                    String spec = sub_token.Groups[6].Value;
                    String sub_token_text = sub_token.Groups[1].Value;
                    tkn = TokenPatterns.MATCH_UPTO.Replace(tkn, "");
                    if(sub_token_text.Length > 0){
                        TokenMatch compiled = new TokenMatch(sub_token_text);
                        if(compiled.isPrefix){
                            if(!prefixes.ContainsKey(compiled.data)){
                                prefixes[compiled.data] = new Dictionary<string, TokenHolder>();
                            }
                            prefixes[compiled.data][token_data.Key] = token_holder;
                        }
                        option.Add(compiled);
                    }
                    if(spec.Length>0 && spec[0] == '|'){
                        if(option.Count > 0){
                            options.Add(option);
                        }
                        option = new List<TokenMatch>();
                    }
                }
                if(option.Count > 0){
                    options.Add(option);
                }
                Tokens[token_data.Key] = token_holder;
            }
        }


        public static Token MatchToken(string haystack){
            return MatchToken("program", haystack, 0, null);
        }

        public static Stack<bool> whitespace_stack = new Stack<bool>();
        public static Token MatchToken(string token_name, string haystack, int recurse_depth, Token as_prefix){ // If you say you understand this you're a lier.
            TokenHolder token = Tokens[token_name];
            if(token.forceWhitespaceToggle){
                whitespace_stack.Push(token.isForcedWhitespace);
            }else{
                whitespace_stack.Push(whitespace_stack.Peek());
            }
            recurse_depth++;
            if(recurse_depth>300){
                throw new Exception("Stack overflow. Program is too deep, man!");
            }

            foreach(List<TokenMatch> option in token.options){
                string option_str = haystack;
                List<TokenItem> match = new List<TokenItem>();
                bool needtousepref = as_prefix!=null;
                bool success = true;
                foreach(TokenMatch to_match in option){
                    int max_matches = to_match.count[1];
                    List<Token> this_match = new List<Token>();
                    while(max_matches != 0){
                        if(whitespace_stack.Peek()){
                            option_str = Regex.Replace(option_str, "^(\\$\\*([^*]|\\*[^$])*\\*\\$|\\$[^*\\r\\n].*|\\s)*", "");
                        }
                        if(to_match.isPrefix){
                            if(as_prefix!=null){
                                this_match.Add(as_prefix);
                                needtousepref = false;
                            }
                        }else if(to_match.type == TokenType.TOKEN){
                            Token subMatch = MatchToken(to_match.data, option_str, recurse_depth, null);
                            if(subMatch!=null){
                                option_str = subMatch.remainder;
                                this_match.Add(subMatch);
                            }else{
                                break;
                            }
                        }else if(to_match.type == TokenType.REGEX){
                            Match str_match = to_match.regex.Match(option_str);
                            if(str_match.Success){
                                option_str = to_match.regex.Replace(option_str, "");
                                Token string_token = new Token();
                                string_token.text = string_token.name = str_match.Groups[0].Value;
                                string_token.remainder = option_str;
                                string_token.isOnlyString = true;
                                this_match.Add(string_token);
                            }else{
                                break;
                            }
                        }
                        max_matches--;
                    }
                    if(this_match.Count < to_match.count[0] || (to_match.count[1]!=-1 && this_match.Count > to_match.count[1])){
                        success = false;
                        break;
                    }
                    TokenItem token_item = new TokenItem();
                    token_item.name = to_match.data;
                    token_item.items = this_match;
                    match.Add(token_item);
                }
                if(needtousepref){
                    success = false;
                }
                if(success){
                    Token this_tokn = new Token();
                    this_tokn.data = match;
                    this_tokn.name = token_name;
                    this_tokn.remainder = option_str;
                    this_tokn.text = haystack.Substring(0, haystack.Length - option_str.Length);
                    if(prefixes.ContainsKey(token_name)){
                        Dictionary<string, TokenHolder> prfxs = prefixes[token_name];
                        while(true){
                            Token shortest = this_tokn;
                            int shortlen = option_str.Length;
                            foreach(KeyValuePair<string, TokenHolder> kv in prfxs){
                                Token contestant = MatchToken(kv.Key, option_str, recurse_depth, this_tokn);
                                if(contestant!=null && contestant.remainder.Length < shortlen){
                                    shortlen = contestant.remainder.Length;
                                    contestant.text = haystack.Substring(0, haystack.Length - contestant.remainder.Length);
                                    shortest = new Token();
                                    shortest.name = token_name;
                                    shortest.remainder = contestant.remainder;
                                    shortest.text = contestant.text;
                                    shortest.data = new List<TokenItem>();
                                    TokenItem sub_token_item = new TokenItem();
                                    sub_token_item.name = kv.Key;
                                    sub_token_item.items = new List<Token>();
                                    sub_token_item.items.Add(contestant);
                                    shortest.data.Add(sub_token_item);
                                }
                            }
                            option_str = shortest.remainder;
                            if(shortest == this_tokn){
                                whitespace_stack.Pop();
                                return shortest;
                            }
                            this_tokn = shortest;
                        }
                    }
                    whitespace_stack.Pop();
                    return this_tokn;
                }
            }
            whitespace_stack.Pop();
            return null;
        }

        public static Token LocationiseTokens(Token token, List<SubTarget> curLocation){
            if(!token.isOnlyString){
                token.location = new List<SubTarget>(curLocation);
                for(int d=0; d < token.data.Count; d++){
                    for(int i=0; i < token.data[d].items.Count; i++){
                        List<SubTarget> newLocation = new List<SubTarget>(curLocation);
                        newLocation.Add(new SubTarget(d, i));
                        LocationiseTokens(token.data[d].items[i], newLocation);
                    }
                }
            }
            return token;
        }

        public static Token LocationiseTokens(Token token){
            return LocationiseTokens(token, new List<SubTarget>());
        }
    }

}