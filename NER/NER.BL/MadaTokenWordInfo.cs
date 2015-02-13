using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NER.BL
{
    public class WordInfoItem
    {
        public string word { get; set; }
        public string diac { get; set; }
        public string lemma { get; set; }
        public string bw { get; set; }
        public string pos { get; set; }
        public string prc3 { get; set; }
        public string prc2 { get; set; }
        public string prc1 { get; set; }
        public string prc0 { get; set; }
        public string asp { get; set; }
        public string per { get; set; }
        public string vox { get; set; }
        public string mod { get; set; }
        public string gen { get; set; }
        public string num { get; set; }
        public string stt { get; set; }
        public string cas { get; set; }
        public string enc0 { get; set; }
        public string source { get; set; }
        public string stem { get; set; }
        public int ID { get; set; }
        public string gloss { get; set; }
    }
}
