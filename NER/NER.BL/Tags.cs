using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NER.BL
{
    public class Tags
    {
        public static List<Tags> GetAllTags()
        {

            var context = new DAL.NEREntities();
            return context.TaggingTables.Select(x => new Tags { ID = x.ID, Order = x.Order,Parent =  x.Parent,statues=  x.statues,Tag=  x.Tag,TriggerWords =  x.
                TriggerWords }).ToList();


        }




        public int ID { get; set; }

        public int? Order { get; set; }

        public int? Parent { get; set; }

        public bool? statues { get; set; }

        public string Tag { get; set; }

        public System.Data.Objects.DataClasses.EntityCollection<DAL.TriggerWord> TriggerWords { get; set; }
    }
}
