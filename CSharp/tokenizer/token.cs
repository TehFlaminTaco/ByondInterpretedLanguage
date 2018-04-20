using System.Collections.Generic;

namespace Tokenizer{
    class Token{
        public bool isOnlyString = false;
        public string name = "";
        public List<TokenItem> data;
        public string remainder = "";
        public string text = "";
    }

    struct TokenItem{
        public string name;
        public List<Token> items;
    }
}