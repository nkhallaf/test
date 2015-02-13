using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NER.BL.Entites;

namespace NER.BL
{
    public static class TaggingHelper
    {
        public static bool CheckExist(List<RemoveTagLine> removeTagLinelist, List<WordTag> wordTag, int wordTagIndex, int lineIndex)
        {
            const bool notexist = true;

            var removetagIndexToRemove = 0;
            foreach (RemoveTagLine item in removeTagLinelist)
            {
                if (wordTag[wordTagIndex].Word == item.Word && lineIndex == item.LineNo)
                {

                    removeTagLinelist.Remove(removeTagLinelist[removetagIndexToRemove]);

                    return false;
                }
                removetagIndexToRemove++;
            }

            return notexist;
        }
        public static void TagLine(List<string> theText, List<WordTag> wordTag, int wordTagIndex, int textIndex)
        {

            var theWord = wordTag[wordTagIndex].Word;
            var theLine = theText[textIndex];


            string[] diacLineWords = theLine.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);



            string pureLine = TextEditing.RemoveDiacritics(theLine);

            if (pureLine.Contains(theWord))
            {

                var locations = GetLocationForGeneralWordTag(theWord, pureLine);

                for (var i = locations.Count - 1; i >= 0; i--)
                {
                    var loc = locations[i];
                    var count = theWord.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries).Length;

                    if (count == 1)
                    {

                        diacLineWords[loc] = "<font title='Tag-" + wordTag[wordTagIndex].Tag.ToUpper() + "' style='color:" + wordTag[wordTagIndex].Color + "'>" + diacLineWords[loc] + "</font>";
                    }
                    else
                    {

                        diacLineWords[loc] = "<font title='Tag-" + wordTag[wordTagIndex].Tag.ToUpper() + "' style='color:" + wordTag[wordTagIndex].Color + "'>" + diacLineWords[loc];

                        diacLineWords[loc + count - 1] += "</font>";

                    }
                }


                var line = string.Empty;

                for (var i = 0; i < diacLineWords.Length; i++)
                {
                    line += i != diacLineWords.Length - 1 ? diacLineWords[i] + " " : diacLineWords[i];
                }

                theText[textIndex] = line;

            }





        }
        internal static List<int> GetLocationForGeneralWordTag(string theWord, string pureLine)
        {
            var locations = new List<int>();


            string[] lineWords = pureLine.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);
            string[] tagWords = theWord.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);

            if (!tagWords.Any())
                return new List<int>();


            for (var i = 0; i < lineWords.Length; i++)
            {

                if (TextEditing.IsTheWord(tagWords[0], lineWords[i]))
                {
                    var startIndex = i;

                    if (lineWords.Length >= startIndex + tagWords.Length && lineWords[startIndex + tagWords.Length - 1].Contains(tagWords[tagWords.Length - 1]))
                    {
                        if (tagWords.Length > 3)
                        {
                            if (tagWords[1] == lineWords[i + 1] && tagWords[2] == lineWords[i + 2])
                            {
                                locations.Add(startIndex);
                            }
                        }
                        else
                        {
                            if (tagWords.Length > 2)
                            {
                                if (tagWords[1] == lineWords[i + 1])
                                {
                                    locations.Add(startIndex);
                                }
                            }
                            else
                            {
                                locations.Add(startIndex);
                            }
                        }
                    }
                }
            }



            return locations;

        }
        internal static void TagLineForTriggerWord(List<string> theText, List<TriggerWords> triggerWords, int wordTagIndex, int textIndex)
        {
            var theWord = triggerWords[wordTagIndex].TriggerWord;
            var theLine = theText[textIndex];


            string[] diacLineWords = theLine.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);



            string pureLine = TextEditing.RemoveDiacritics(theLine);

            if (pureLine.Contains(theWord))
            {
                //  locations of the trigger words
                var locations = GetLocationForGeneralWordTag(theWord, pureLine);

                for (var i = locations.Count - 1; i >= 0; i--)
                {
                    var loc = locations[i];

                    if (diacLineWords.Count()>loc)
                    {
                        loc++;
                        var count = theWord.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries).Length;

                        if (count == 1)
                        {

                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc] + "</font>";
                        }
                        else
                        {

                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];

                            diacLineWords[loc + count - 1] += "</font>";

                        }
                    }
                }


                var line = string.Empty;

                for (var i = 0; i < diacLineWords.Length; i++)
                {
                    line += i != diacLineWords.Length - 1 ? diacLineWords[i] + " " : diacLineWords[i];
                }

                theText[textIndex] = line;

            }




        }
    }
}
