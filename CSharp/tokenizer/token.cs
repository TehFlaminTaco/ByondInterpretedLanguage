using System.Collections.Generic;

namespace ByondLang.Tokenizer{
    class Token{
        public bool isOnlyString = false;
        public string name = "";
        public List<TokenItem> data;
        public string remainder = "";
        public string text = "";

        public List<SubTarget> location;
    }

    struct TokenItem{
        public string name;
        public List<Token> items;
    }
}