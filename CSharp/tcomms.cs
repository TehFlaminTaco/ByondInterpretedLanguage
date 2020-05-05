using System.Runtime.Serialization;
namespace ByondLang{
    [DataContract]
    public struct Signal{
        [DataMember]
        public string content;
        [DataMember]
        public string freq;
        [DataMember]
        public string source;
        [DataMember]
        public string job;
        [DataMember]
        public string pass;
        [DataMember]
        public string reference;
        [DataMember]
        public string verb;
        [DataMember]
        public string language;
        public Signal(string content, string freq, string source, string job, string pass, string reference, string verb, string language = "Ceti Basic"){
            this.content = content;
            this.freq = freq;
            this.source = source;
            this.job = job;
            this.pass = pass;
            this.reference = reference;
            this.verb = verb;
            this.language = language;
        }
        public Signal(string content, string freq, string source, string job, string pass, string verb, string language = "Ceti Basic"){
            this.content = content;
            this.freq = freq;
            this.source = source;
            this.job = job;
            this.pass = pass;
            this.reference = "";
            this.verb = verb;
            this.language = language;
        }
    }
}