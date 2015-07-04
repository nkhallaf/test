using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NER.BL.Entites;
using System.Data.SqlClient;
using System.Configuration;

namespace NER.BL
{
    public static class TagRetreival
    {
        public static List<WordTag> GetAllDataForTagging()
        {
            var dc = new DAL.NEREntities();
            return dc.Words_tagged.Select(x => new WordTag
            {
                Word = x.word,
                Tag = x.TaggingTable.NETag,
                Color = x.TaggingTable.Color
                
            }).ToList();


        }


    }
}
