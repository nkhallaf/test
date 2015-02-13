using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NER.BL
{
    public class FixedNE
    {

        public static List<FixedNE> GetAll() { 
        
        return (new DAL.NEREntities()).Words_tagged.Select(x=> new FixedNE{tag = x.tag,word = x.word}).ToList();
        
        }


        public string tag { get; set; }

        public string word { get; set; }
    }
}
