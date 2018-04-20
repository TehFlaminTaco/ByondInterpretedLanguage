using System.Text.RegularExpressions;
using System;

namespace Tokenizer{

    class TokenPatterns{ // I'd document these, but I don't remember what they do.
        public static Regex SPEC_CHARS = new Regex("[\\s|]");
        public static Regex MATCH_STRING = new Regex("(['\"`])(([^\\\\]*?|\\\\.)*?)\\1");
        public static Regex MATCH_UPTO = new Regex("(((['\"`])(([^\\\\]*?|\\\\.)*?)\\3|.)*?)("+SPEC_CHARS+"|$)");
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

        public TokenMatch(string definition){
            Match token_parts = TokenPatterns.MODIF_MATCH.Match(definition);
            MatchCollection modifiers = TokenPatterns.MODIF_SECT_MATCH.Matches(token_parts.Groups[2].Value); // AAAAAAAAAAAHH!!
            string token_string = token_parts.Groups[1].Value;
            
            data = token_string;

            foreach(Match modifier in modifiers){
                string modifier_text = modifier.Value;
                switch(modifier_text){
                    case "?":
                        count = new int[]{0,1};
                        break;
                    case "*":
                        count = new int[]{0,-1};
                        break;
                    case "+":
                        count = new int[]{1,-1};
                        break;
                    case ":":
                        isPrefix = true;
                        break;
                    case "{":
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
            }
        }
    }
}