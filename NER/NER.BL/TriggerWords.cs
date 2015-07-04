using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NER.BL
{
    public class TriggerWords
    {
        public static List<TriggerWords> GetTriggerWords()
        {
            var dc = new DAL.NEREntities();
            return dc.TriggerWords.OrderBy(x => x.TaggingTable.Order).Select(x => new TriggerWords
            {
                TriggerWord = x.Word,
                Tag = x.TaggingTable.NETag,
                Color = x.TaggingTable.Color,
                Tooltip = x.TaggingTable.TooltipInfo
            }).ToList();
        }
        public string TriggerWord { get; set; }
        public string Tag { get; set; }
        public string Color { get; set; }
        public string Tooltip { get; set; }
    }
}
