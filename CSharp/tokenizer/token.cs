using System.Collections.Generic;

namespace Tokenizer{
    struct Token{
        public string name;
        public List<TokenItem> data;
    }

    struct TokenItem{
        public string name;
        public List<Token> items;
    }
}