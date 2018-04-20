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
            
        }
    }
}