using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

namespace Tokenizer{
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
        UNINITIALIZED, REGEX, TOKEN, FORCE_WS
    }

    class TokenMatch{
        public TokenType type = TokenType.UNINITIALIZED;
        public string data = "";
        public bool isPrefix = false;
        public int[] count = {1,1};

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
            }else{
                type = TokenType.TOKEN;
                if(!TokenCompiler.RawTokens.ContainsKey(data)){
                    throw new InvalidTokenException("Missing token: "+data);
                }
            }
        }
    }

    class TokenCompiler{
        public static Dictionary<string, string> RawTokens = new Dictionary<string, string>();
        public static Dictionary<string, List<List<TokenMatch>>> Tokens = new Dictionary<string, List<List<TokenMatch>>>();
        public static Dictionary<string, Dictionary<string, List<List<TokenMatch>>>> prefixes = new Dictionary<string, Dictionary<string, List<List<TokenMatch>>>>(); //AAAAAAAAAAAAHHH!!!

        private static Regex token_line_string = new Regex("(.*?):(.*)");

        public static void GetTokens(){
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("CSharp.resource.tokens.txt");
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
            if(!RawTokens.ContainsKey("program")){
                throw new Exception("Tokenizers must define 'program'");
            }

            foreach(KeyValuePair<string, string> token_data in RawTokens){
                string tkn = token_data.Value;

                var options = new List<List<TokenMatch>>();
                var option = new List<TokenMatch>();

                if(tkn[0]=='@'){
                    var wsToken = new TokenMatch(tkn[1]=='1'?"'1'":"'0'");
                    wsToken.type = TokenType.FORCE_WS;
                    wsToken.count[0] = 0;
                    wsToken.count[1] = 0;
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
                                prefixes[compiled.data] = new Dictionary<string, List<List<TokenMatch>>>();
                            }
                            prefixes[compiled.data][token_data.Key] = options;
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
                Tokens[token_data.Key] = options;
            }
        }
    }

}