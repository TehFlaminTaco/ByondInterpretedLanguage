using System;
using System.Web;

namespace ByondLang{
    class Terminal{
        public int cursor_x = 0;
        public int cursor_y = 0;
        public int width = 64;
        public int height = 20;
        string computer_ref;
        TerminalChar[][] char_array;

        public Color background = new Color(0,0,0);
        public Color foreground = new Color(255,255,255);

        public Terminal(int width, int height, string computer_ref){
            this.computer_ref = computer_ref;
            this.width = width;
            this.height = height;
            Clear();
        }

        public void Clear(){
            cursor_x = 0;
            cursor_y = 0;
            char_array = new TerminalChar[height][];
            for(int y = 0; y < height; y++){
                char_array[y] = new TerminalChar[width];
                for(int x = 0; x < width; x++){
                    char_array[y][x] = new TerminalChar(' ', background, foreground, "");
                }
            }
        }

        public string Stringify(){
            string outp = "";
            string joiner = "";
            for(int y=0; y < height; y++){
                outp += joiner;
                joiner = "<br>";
                for(int x=0; x < width; x++){
                    TerminalChar toDraw = char_array[y][x];
                    string to_Write = "";
                    if(toDraw.topic != "" && !(x > 0 && toDraw.topic == char_array[y][x-1].topic))
                        to_Write += String.Format("<a style='text-decoration:none;' href='?src={0};PRG_topic={1}'>", computer_ref, HttpUtility.UrlEncode(toDraw.topic));
                    if(!(x > 0 && toDraw.Like(char_array[y][x-1])))
                        to_Write += String.Format("<span style=\"color:{0};background-color:{1}\">", toDraw.foreground.toHTML(), toDraw.background.toHTML());
                    to_Write += encode(toDraw.text);
                    if(!(x < width-1 && toDraw.Like(char_array[y][x+1])))
                        to_Write += "</span>";
                    if(toDraw.topic != "" && !(x < width-1 && toDraw.topic == char_array[y][x+1].topic))
                        to_Write += "</a>";
                    outp += to_Write;
                }
            }
            return outp;
        }

        public string encode(char c){
            string outp = HttpUtility.HtmlEncode(""+c);
            if(outp == " ")
                outp = "&nbsp;";
            return outp;
        }

        public Terminal(string computer_ref) : this(64, 20, computer_ref){}

        public void MoveRight(){
            cursor_x++;
            if(cursor_x >= width){
                cursor_x = 0;
                MoveDown();
            }
        }

        public void MoveDown(){
            cursor_y++;
            if(cursor_y >= height){
                cursor_y--;
                for(int i=0; i < height-1; i++){
                    char_array[i] = char_array[i+1];
                }
                char_array[height-1] = new TerminalChar[width];
                for(int x = 0; x < width; x++){
                    char_array[height-1][x] = new TerminalChar(' ', background, foreground, "");
                }
            }
        }

        public void Write(String str){
            foreach(char c in str){
                if(c == '\r'){
                    cursor_x = 0;
                }else if(c == '\n'){
                    MoveDown();
                }else if(c=='\t'){
                    while(cursor_x%4>0)
                        MoveRight();
                }else{
                    char_array[cursor_y][cursor_x] = new TerminalChar(c, background, foreground, "");
                    MoveRight();
                }
            }
        }

        public void SetTopic(int x, int y, string topic){
            if(y >= 0 && x >= 0 && x < width && y < height){
                char_array[y][x].topic = topic;
            }
        }

        public void SetTopic(int x, int y, int w, int h, string topic){
            for(int X=0; X<w; X++){
                for(int Y=0; Y<h; Y++){
                    SetTopic(x+X, y+Y, topic);
                }
            }
        }
    }

    class TerminalChar{
        public char text = ' ';
        public Color background = new Color(0,0,0);
        public Color foreground = new Color(255,255,255);
        public string topic = "";


        public TerminalChar(){
        }

        public TerminalChar(char text, Color background, Color foreground, string topic) : base(){
            this.text = text;
            this.background = background.Copy();
            this.foreground = foreground.Copy();
            this.topic = topic;
        }

        public bool Like(TerminalChar other){
            return background.r == other.background.r &&
                   background.g == other.background.g &&
                   background.b == other.background.b &&
                   foreground.r == other.foreground.r &&
                   foreground.g == other.foreground.g &&
                   foreground.b == other.foreground.b;
        }
    }

    class Color{
        public float r = 0;
        public float g = 0;
        public float b = 0;

        public Color(){

        }

        public Color(float r, float g, float b){
            this.r = r;
            this.g = g;
            this.b = b;
            Validate();
        }

        public Color(int r, int g, int b){
            this.r = (r/255.0f);
            this.g = (g/255.0f);
            this.b = (b/255.0f);
            Validate();
        }

        public void Validate(){
            r = Math.Clamp(r, 0.0f, 1.0f);
            g = Math.Clamp(g, 0.0f, 1.0f);
            b = Math.Clamp(b, 0.0f, 1.0f);
        }

        public Color Copy(){
            return new Color(r, g, b);
        }

        public string toHTML(){
            int R = (int)(r * 255);
            int G = (int)(g * 255);
            int B = (int)(b * 255);
            return "#" + String.Format("{0:X}",R).PadLeft(2, '0') + String.Format("{0:X}",G).PadLeft(2, '0') + String.Format("{0:X}",B).PadLeft(2, '0');
        }
    }
}