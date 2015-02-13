﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NER.BL.Entites;

namespace NER.BL
{
    public static class Colorizing
    {

        public static string ColorizeTheText(List<string> theText, string fileName)
        {
            List<WordTag> wordTag = TagRetreival.GetAllDataForTagging(fileName);
            var triggerWords = TriggerWords.GetTriggerWords();
            for (int g = 0; g < theText.Count; g++)
            {
                for (int i = 0; i < wordTag.Count; i++)
                {
                    TaggingHelper.TagLine(theText, wordTag, i, g);
                }
                for (int i = 0; i < triggerWords.Count; i++)
                {
                    TaggingHelper.TagLineForTriggerWord(theText, triggerWords, i, g);
                }

            }
            return theText.Aggregate(string.Empty, (current, item) => current + (item + "<br/>"));
        }

        public static string GetColor(int p)
        {
            var dc = new DAL.NEREntities();
            var str = (from i in dc.TaggingTables where i.ID == p select i.Color).ToList();
            return str.Count == 1 ? str[0] : "";
        }



    }
}