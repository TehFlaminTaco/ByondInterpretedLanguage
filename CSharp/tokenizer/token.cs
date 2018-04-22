using System.Collections.Generic;

namespace ByondLang.Tokenizer{
    class Token{
        public bool isOnlyString = false;
        public string name = "";
        public List<TokenItem> data;
        public string remainder = "";
        public string text = "";

        public List<SubTarget> location;

        public TokenItem this[int key]{
            get{
                return data[key];
            }
            set{
                data[key] = value;
            }
        }
    }

    struct TokenItem{
        public string name;
        public List<Token> items;

        public Token this[int key]{
            get{
                return items[key];
            }
            set{
                items[key] = value;
            }
        }
    }
}