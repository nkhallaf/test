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
        public static void TagLine(List<string> theText, List<WordTag> wordTag, int wordTagIndex, int textIndex, Status status)
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
                        if (status == Status.Tag)
                        {
                            diacLineWords[loc] = "<font title='Tag-" + wordTag[wordTagIndex].Tag.ToUpper() + "' style='color:" + wordTag[wordTagIndex].Color + "'>" + diacLineWords[loc] + "</font>";
                        }
                        else if (status == Status.Download)
                        {
                            diacLineWords[loc] = "<" + wordTag[wordTagIndex].Tag.ToUpper() + ">" + diacLineWords[loc] + @"</" + wordTag[wordTagIndex].Tag.ToUpper() + ">";

                        }
                    }
                    else
                    {
                        if (status == Status.Tag)
                        {
                            diacLineWords[loc] = "<font title='Tag-" + wordTag[wordTagIndex].Tag.ToUpper() + "' style='color:" + wordTag[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + count - 1] += "</font>";
                        }
                        else if (status == Status.Download)
                        {

                            diacLineWords[loc] = "<" + wordTag[wordTagIndex].Tag.ToUpper() + ">" + diacLineWords[loc];
                            diacLineWords[loc + count - 1] += (@"</" + wordTag[wordTagIndex].Tag.ToUpper() + ">");
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
        internal static void TagLineForTriggerWord(List<string> theText, List<TriggerWords> triggerWords, int wordTagIndex, int textIndex, Status status)
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
                    // get 5 words after that location 
                    var sentenceWordsCount = diacLineWords.Count();
                    var remainingWords = sentenceWordsCount - loc;
                    var sentence = string.Empty;
                    if (remainingWords > 5)
                    {
                        for (int Index = loc; Index < loc + 5; Index++)
                        {
                            sentence += (diacLineWords[Index] + " ");
                        }
                    }
                    else
                    {
                        for (int Index = loc; Index < sentenceWordsCount; Index++)
                        {
                            sentence += (diacLineWords[Index] + " ");
                        }

                    }

                    var words = BL.MadaMiraHandler.Analyse(sentence);
                    var triggerWordTag = triggerWords[wordTagIndex].Tag.ToUpper();
                    ////################ we need to start the tagging process after the last tagged word
                    //how to excelude puncutations???
                    //first Tagging through previous trigger words
                    //Second Tagging through Followed trigger words
                    //Third MadaMira "بال" analaysis for all words starts with "يال"
                    //OR using the enclitics Table 
                    //Fourth Tagging untagged NEs if it is exist in the text
                    //Fifth Tagging from Fixed table
                    // The title of the text needs to be analysed by MadaMira for searching for Prop-noun
                    ////لما يلاقيها خلاص ميبصش علي اللي بعدها
                    #region 6-Event
                    //occasion
                    ///Religious Festival
                    if (triggerWordTag == "neekr")
                    {
                        //tagged throught the fixed taggs
                    }
                    #region Event Game
                    ///Game tagging
                    if (triggerWordTag == "neekg".ToUpper())
                    {

                        if (words[0].pos == "noun_prop" && words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "digit")
                            {
                                ///اولمبياد برشلونة 1992

                                if (status == Status.Tag)
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<" + triggerWords[wordTagIndex].Tag.ToUpper() + ">" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += ("</" + triggerWords[wordTagIndex].Tag.ToUpper() + ">");
                                }
                            }
                            else if (words[2].pos != "digit")
                            {
                                //اولمبياد اثينا كلمتان NES
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                        else if (words[0].pos == "noun_prop" && words[1].pos == "digit")
                        {
                            // مونديال 2009 
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2 - 1] += "</font>";

                        }
                        else if (words[0].pos == "noun" && words[1].pos == "digit")
                        {
                            // دوري 2009
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2 - 1] += "</font>";

                        }
                        //بطولة العالم لالعاب القوى 

                        //بطولة المملكة للمسابقة الثقافية 
                        else if (words[0].pos == "noun" && words[1].pos == "noun")
                        {
                            if (words[2].pos == "noun")
                            {
                                if (words[2].prc1 == "li_prep" && words[3].pos == "noun")
                                {
                                    if (words[5].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }


                                }
                                else if (words[2].prc1 != "li_prep" && words[3].pos == "noun")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }

                                else if (words[2].prc1 == "li_prep" && words[3].pos == "adj")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                //بطولة كاس الملك //####### فيها مشكله عشان بطوله وكاس كل واحده منهم لوحدها Trigger words

                                else if (words[2].prc1 != "li_prep" && words[2].pos == "noun")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                ///بطولة اندية الدرجة الاولى
                                else if (words[3].pos == "adj_num" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";

                                }
                                //بطولة العالم للجري
                                else if (words[2].prc1 == "li_prep")
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }
                            /// دوري الدرجة الاولى
                            else if (words[2].pos == "adj_num" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";

                            }
                            else if (words[2].pos == "adj")
                            {
                                //دورة الالعاب العربية السابعة
                                if (words[3].pos == "adj_num")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                //// كاس الامم الاوروبية لكرة القدم
                                else if (words[3].pos == "noun" && words[3].prc1 == "li_prep")
                                {
                                    //لكرة القدم
                                    if (words[4].pos == "noun" && words[4].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else if (words[4].pos != "noun" | words[4].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            //دوري ابطال اوروبا 
                            // بطولة امم اوروبا 2008
                            else if (words[2].pos == "noun_prop" && words[2].prc0 == "0" && words[1].prc0 == "0")
                            {
                                if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }

                            else
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                        //المسابقة السباعية
                        else if (words[0].pos == "noun" && words[1].pos == "adj")
                        {

                            // العاب اولمبية صيفية 1980
                            //// noun+adj + adj +number
                            if (words[2].pos == "adj" && words[3].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 4 - 1] += "</font>";
                            }
                            // الدوري الممتاز للرجبي //// i have to access the first two letters of the word "لل"
                            else if (words[2].pos == "" && words[2].word == "للرجبي")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                                //البطولة العربية الاولى 1978
                            //noun+adj+num+digit
                            else if (words[2].pos == "adj_num")
                            {
                                if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }

                                ///Noun+adj+adj
                            else if (words[2].pos == "adj")
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            // البطولة العربية للناشئات في 
                            // البطولة الاولمبية لكرة القدم
                            else if (words[2].pos == "noun" && words[2].prc1 == "li_prep")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words[3].pos == "noun" && words[3].prc0 != "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }


                            ////معرفين بال
                            else if (words[0].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            ////ثلاث كلمات معرفين بال
                            else if (words[0].prc0 == "Al_det" && words[1].prc0 == "Al_det" && words[2].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                                   ///Noun + noun
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";

                            }
                        }

                        else if (words[0].pos == "noun" && words[1].pos == "noun_prop" && words[2].pos == "adj")
                        {

                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 3 - 1] += "</font>";
                        }
                        // دورة لوس انجلوس في
                        else if (words[0].pos == "noun" && words[1].pos == "noun_prop" && words[2].pos == "noun_prop")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 3 - 1] += "</font>";
                        }
                        //  دورة طوكيو في 
                        else if (words[0].pos == "noun" && words[1].pos == "noun_prop")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2 - 1] += "</font>";
                        }
                        // دورة استكهولم في
                        else if (words[0].pos == "noun" && words[1].pos == "")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2 - 1] += "</font>";
                        }
                        else if (words[0].pos == "noun_prop" && words[1].pos == "")
                        {
                            if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                        }
                    }
                    #endregion
                    #region Event Conference
                    ///Conference tagging
                    ///########ندوه بعنوان ""
                    if (triggerWordTag == "neekc".ToUpper())
                    {
                        //مؤتمر امستردام

                        if (words[0].pos == "noun" && words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            //ندوة ( ابو حيان التوحيدي )


                            else if (words[0].pos == "noun" && words[1].pos == "noun_prop")
                            {
                                if (words[2].pos == "noun" || words[2].pos == "noun_prop" || words[2].pos == "")
                                {
                                    if (words[3].pos == "noun" || words[3].pos == "noun_prop" || words[3].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }

                        ///موتمر 2001

                        else if (words[0].pos == "noun" && words[1].pos == "digit")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2 - 1] += "</font>";
                        }


                        else if (words[0].pos == "noun" && words[1].pos == "adj")
                        {
                            // المهرجان الدولي للاغنية بالقاهرة
                            if (words[2].pos == "noun" && words[2].prc1 == "li_prep")
                            {
                                if (words[3].pos == "noun_prop" && words[3].prc1 == "bi_prep")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                //الندوة التاسيسية لمركز الدراسات والبحوث العربية
                                else if (words[3].pos == "noun" && words[3].prc0 == "Al_det")
                                {
                                    if (words[4].pos == "noun" && words[4].prc2 == "wa_conj")
                                    {
                                        ///####والبحوث العربية 
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            //المؤتمر الهندسي السعودي السادس
                            else if (words[2].pos == "adj")
                            {
                                if (words[2].prc0 == "Al_det" && words[2].prc0 == "Al_det")
                                {
                                    if (words[3].pos == "adj_num" && words[3].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }

                                else
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            //القمة العربية
                            else if (words[0].prc0 == "Al_det" && words[1].prc0 == "Al_det" && words[0].num == "s" && words[1].num == "s")
                            {
                                if (words[2].bw == "fiy/PREP" || words[2].bw == "Ean/PREP")
                                {
                                    if (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[4].pos == "adj" && words[4].prc0 == "Al_det")
                                    {

                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }

                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                //الندوة الربيعية الاولى للجامعة الصيفية العربية الاوربية
                                else if (words[2].pos == "adj_num" && words[2].prc0 == "Al_det")
                                {
                                    if (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc0 == "Al_det")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                            }


                        //الندوة الربيعية الاولى للجامعة الصيفية العربية الاوربية
                            else if (words[2].pos == "adj_num" && words[2].prc0 == "Al_det")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep")
                                {
                                    if (words[4].pos == "adj" && words[4].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            else if (words[2].bw == "fiy/PREP" || words[2].bw == "Ean/PREP")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[4].pos == "adj" && words[4].prc0 == "Al_det")
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 5 - 1] += "</font>";
                                }
                                else
                                {

                                }
                            }
                        }

                           //مؤتمر احياء الاعياد الاولمبية القديمة
                        else if (words[0].pos == "noun" && words[1].pos == "noun")
                        {

                            if (words[2].pos == "noun" && words[2].num == "s" && words[1].num == "s")
                            {
                                if (words[3].pos == "adj" && words[3].num == "s" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det")
                                {
                                    if (words[4].pos == "adj" && words[4].num == "s" && words[4].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }

                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                //ندوة حول الرواية والقصة الخليجية والكويتية
                                else if (words[3].pos == "noun" && words[3].prc2 == "wa_conj")
                                {
                                    if (words[4].pos == "adj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else if (words[4].pos == "noun" && words[4].prc2 == "wa_conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else if (words[3].pos == "noun_prop" || words[3].pos == "")
                                {
                                    if (words[4].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";

                            }
                            //ندوة جامعة عدن
                            else if (words[2].pos == "noun_prop" || words[2].pos == "" || words[2].pos == "noun")
                            {
                                //ندوة رواق عوشة بنت حسين
                                if (words[3].pos == "noun_prop" || words[3].pos == "noun")
                                {
                                    if (words[4].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else if (words[4].pos != "noun_prop")
                                    {

                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }

                                //ندوة جامعة عدن
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            //ندوة " العرب في الاعلام الغربي "
                            //الندوة العلمية عن الادب العربي الحديث #### الحديث adj
                            else if (words[2].bw == "fiy/PREP" || words[2].bw == "Ean/PREP")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[4].pos == "adj" && words[4].prc0 == "Al_det")
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 5 - 1] += "</font>";
                                }
                                else
                                {

                                }

                            }


                            else
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                        }



                    }
                    #endregion
                    #region Event Incident
                    //Incident
                    if (triggerWordTag == "neei".ToUpper())
                    {
                        ///####معركة دي لا جونا دي لا خاندا في 
                        ///#####معركة سانور في 
                        ///<tokenized scheme="ATB">
                        ////<tok id="0" form0="س+"/>
                        /////<tok id="1" form0="انور"/>
                        ////// </tokenized>
                        #region
                        if (words[0].pos == "noun" && words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "adj_num")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "noun_prop")
                            {
                                if (words[2].gloss == "bin" || words[2].gloss == "Ibn" || words[2].gloss == "Abu")
                                {
                                    if (words[3].pos == "noun_prop")
                                    {
                                        if (words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";


                                        }
                                    }
                                    else if (words[3].pos == "")
                                    {
                                        if (words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";


                                        }
                                    }
                                    else if (words[3].pos != "noun_prop")
                                    {
                                        if (words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";


                                        }
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else if (words[3].pos == "noun_prop")
                                {

                                    if (words[4].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "prep" && words[2].gloss == "from")
                            {
                                if (words[3].pos == "noun" && words[3].gloss == "year")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }


                            }
                            else if (words[2].pos == "noun" && words[2].gloss == "year")
                            {
                                if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words[2].pos == "noun")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }


                        }
                        #endregion
                        #region
                        else if (words[0].pos == "noun" && words[1].pos == "adj")
                        {

                            if (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                            else if (words[2].pos == "adj_num" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "noun_prop" && words[2].prc1 == "li_prep")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "prep" && words[2].gloss == "on;above")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words[3].pos == "")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        #endregion
                        #region
                        else if (words[0].pos == "noun" && words[1].pos == "digit")
                        {
                            if (words[2].pos == "noun_prop")
                            {
                                if (words[3].pos == "noun" && words[3].gloss == "year")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }

                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                        #endregion
                        #region
                        else if (words[0].pos == "noun" && words[1].pos == "noun")
                        {
                            if (words[1].prc0 == "Al_det")
                            {
                                if (words[2].pos == "noun" && words[2].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words[2].pos == "")
                                {
                                    if (words[3].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "noun")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words[3].pos == "")
                                {
                                    if (words[4].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "")
                            {
                                if (words[3].pos == "")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }

                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        #endregion
                        else if (words[0].pos == "noun" && words[1].pos == "adj_num" || words[1].pos == "noun_num")
                        {
                            if (words[2].pos == "noun" && words[2].gloss == "year")
                            {
                                if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].gloss == "from" && words[2].pos == "prep")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                        else if (words[0].pos == "noun" && words[1].pos == "")
                        {
                            if (words[2].pos == "noun_prop")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            else if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "adj_num")
                            {
                                if (words[3].pos == "adj_num")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            else if (words[2].pos == "")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words[3].pos == "")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                    }
                    #endregion
                    #region Event Other
                    //Other
                    if (triggerWordTag == "neeo".ToUpper())
                    {

                        #region
                        if (words[0].pos == "noun" && words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "adj_num")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "noun_prop")
                            {
                                if (words[2].gloss == "bin" || words[2].gloss == "Ibn" || words[2].gloss == "Abu")
                                {
                                    if (words[3].pos == "noun_prop")
                                    {
                                        if (words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";


                                        }
                                    }
                                    else if (words[3].pos == "")
                                    {
                                        if (words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";


                                        }
                                    }
                                    else if (words[3].pos != "noun_prop")
                                    {
                                        if (words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";


                                        }
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else if (words[3].pos == "noun_prop")
                                {

                                    if (words[4].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "prep" && words[2].gloss == "from")
                            {
                                if (words[3].pos == "noun" && words[3].gloss == "year")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }


                            }
                            else if (words[2].pos == "noun" && words[2].gloss == "year")
                            {
                                if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words[2].pos == "noun" && words[2].prc1 == "li_prep")
                            {
                                if (words[3].pos == "adj" && words[3].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "noun")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }


                        }
                        #endregion
                        #region
                        else if (words[0].pos == "noun" && words[1].pos == "adj")
                        {

                            if (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                            else if (words[2].pos == "adj_num" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "noun_prop" && words[2].prc1 == "li_prep")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "prep" && words[2].gloss == "on;above")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words[3].pos == "")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "noun" && words[2].prc1 == "li_prep")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        #endregion
                        #region
                        else if (words[0].pos == "noun" && words[1].pos == "digit")
                        {
                            if (words[2].pos == "noun_prop")
                            {
                                if (words[3].pos == "noun" && words[3].gloss == "year")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }

                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                        #endregion
                        #region
                        else if (words[0].pos == "noun" && words[1].pos == "noun")
                        {
                            if (words[1].prc0 == "Al_det")
                            {
                                if (words[2].pos == "noun" && words[2].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words[2].pos == "")
                                {
                                    if (words[3].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words[2].pos == "noun")
                            {
                                if (words[3].pos == "noun" && words[3].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words[3].pos == "")
                                {
                                    if (words[4].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "")
                            {
                                if (words[3].pos == "")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }

                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        #endregion
                        else if (words[0].pos == "noun" && words[1].pos == "adj_num" || words[1].pos == "noun_num")
                        {
                            if (words[2].pos == "noun" && words[2].gloss == "year")
                            {
                                if (words[3].pos == "digit")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].gloss == "from" && words[2].pos == "prep")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                        else if (words[0].pos == "noun" && words[1].pos == "")
                        {
                            if (words[2].pos == "noun_prop")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            else if (words[2].pos == "digit")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words[2].pos == "adj_num")
                            {
                                if (words[3].pos == "adj_num")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            else if (words[2].pos == "")
                            {
                                if (words[3].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words[3].pos == "")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                    }
                    #endregion
                    #endregion

                    #region 2-Numex
                    //Percent and Ordinal Tagged through the Fixed tagging table

                    //Measurments Temperature                  
                    if (triggerWordTag == "neumt".ToUpper())
                    {
                        ///do nothing
                    }
                    //Measurments Speed
                    if (triggerWordTag == "neums".ToUpper())
                    {
                        ///do nothing
                    }
                    #region Space
                    //Measurments Space tagging
                    if (triggerWordTag == "neuma".ToUpper())
                    {

                        /////// لازم اعمل كود يشوف اللي قبلها رقم ولا لأ
                        ///// ازاي افرقها عن كيلو الوزن؟؟؟؟؟؟
                        if (words[1].gloss == "meter")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2 - 1] += "</font>";
                        }


                    }
                    #endregion
                    #region Weight
                    //Measurments Weight tagging
                    if (triggerWordTag == "neumw".ToUpper())
                    {
                        /////// لازم اعمل كود يشوف اللي قبلها رقم ولا لأ
                        if (words.Count == 2)
                        {
                            if (words[1].gloss == "gram")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }
                        }

                        else
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }

                    }
                    #endregion
                    //Measurments Volume tagging
                    if (triggerWordTag == "neumv".ToUpper())
                    {
                        ///do nothing
                    }
                    #region Extent
                    //Measurments Extent tagging
                    if (triggerWordTag == "neume".ToUpper())
                    {
                        ///عاوزاه يتاج اخر كلمه بس
                        ///وممكن احدد الكمات كمان اللي بعد الرقم 
                        if (words[1].pos == "digit")
                        {

                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2] += "</font>";
                        }

                    }
                    #endregion
                    //Measurments other tagging
                    if (triggerWordTag == "neumo".ToUpper())
                    {

                    }
                    #endregion

                    #region 4-Disease
                    //Disease tagging
                    if (triggerWordTag == "ned".ToUpper())
                    {
                        if (words[0].pos == "noun")
                        {
                            if (words[0].gloss == "disease;epidemic" || words[0].gloss == "disease;illness")
                            {
                                ///وباء السكري
                                if (words[1].pos == "noun")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            //التلقيح ضد الجدري
                            else if (words[0].gloss == "inoculation;pollination;impregnation" && words[1].gloss == "contrary;against;opposed;anti-;counter-")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";

                            }
                            //أمراض مستعصيه مثل الحمى والملاريا والجدري

                            else if (words[0].gloss == "diseases;illnesses")
                            {
                                if (words[1].gloss == "difficult;incurable" && words[1].pos == "adj")
                                {
                                    if (words[2].gloss == "like;such_as" && words[2].pos == "noun")
                                    {
                                        if (words[3].pos == "noun" || words[3].pos == "adj")
                                        {
                                            if (words[4].prc2 == "wa_conj")
                                            {


                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";


                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }


                                        }
                                        else
                                        { }
                                    }
                                    else { }
                                }
                                //أمراض  مثل الحمى والملاريا والجدري
                                else if (words[1].gloss == "like;such_as" && words[1].pos == "noun")
                                {
                                    if (words[2].pos == "noun" || words[2].pos == "adj")
                                    {
                                        if (words[3].prc2 == "wa_conj")
                                        {
                                            if (words[3].prc2 == "wa_conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }

                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    { }
                                }


                            }
                            ///السكري
                            else if (words[0].gloss != "disease;epidemic" || words[0].gloss != "disease;illness")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }


                        }
                        ///اصيب بالملاريا
                        ////انتهكته الحمي
                        /// i neeed to select the second word only

                        else if (words[0].pos == "verb")
                        {
                            if (words[0].per == "3" && words[1].pos == "noun" && words[1].prc0 == "Al_det")
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";

                            }
                            else if (words[0].per == "3" && words[1].pos == "noun" && words[1].prc1 == "bi_prep")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            ///توفي اثر ازمة قلبية
                            ///we still need to tag the last two words
                            else if (words[0].per == "3" && words[1].gloss == "right_after")
                            {
                                if (words[2].pos == "noun" || words[2].pos == "adj")
                                {
                                    if (words[3].pos == "noun" || words[3].pos == "adj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else { }

                            }
                            else
                            { }

                        }

                        else
                        { }
                    }
                    #endregion
                    #region 5-Natural Object
                    //Chemical Tagging
                    if (triggerWordTag == "nenc".ToUpper())
                    {

                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                        diacLineWords[loc + 1 - 1] += "</font>";

                    }
                    //Living thing
                    ///Animal Tagging
                    if (triggerWordTag == "nenla".ToUpper())
                    {
                        //Do nothing
                    }
                    ///Bird Tagging
                    if (triggerWordTag == "nenlb".ToUpper())
                    {
                        //Do nothing
                    }
                    ///Insect Tagging
                    if (triggerWordTag == "nenli".ToUpper())
                    {
                        if (words[0].pos == "noun")
                        {
                            if (words[1].pos == "noun" || words[1].pos == "")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";

                            }
                            else
                            { }

                        }
                        else
                        { }

                    }
                    ///Plant Tagging
                    if (triggerWordTag == "nenlp".ToUpper())
                    {
                        if (words[0].gloss == "agriculture;cultivation" && words[0].pos == "noun")
                        {
                            if (words[1].pos == "noun" && words[1].prc0 == "Al_det")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";

                            }
                            else
                            { }

                        }
                        else
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";

                        }
                    }
                    ///Micro-Organism Tagging
                    if (triggerWordTag == "nenlm".ToUpper())
                    {
                        //Do nothing
                    }
                    ///Other Tagging
                    if (triggerWordTag == "nenlo".ToUpper())
                    {
                        //Do nothing
                    }
                    #endregion
                    #region 7-Product
                    #region Clothing
                    //Clothing tagging
                    if (triggerWordTag == "nerc".ToUpper())
                    {
                        //do nothing
                    }
                    #endregion
                    #region Drug
                    //Drug tagging
                    if (triggerWordTag == "nerd".ToUpper())
                    {
                        //Do nothing
                    }
                    #endregion
                    #region Weapon
                    //Weapon tagging
                    if (triggerWordTag == "nerw".ToUpper())
                    {

                    }
                    #endregion
                    #region Vehicle
                    //Vehicle
                    ///Car tagging
                    if (triggerWordTag == "nervc".ToUpper())
                    {
                        if (words[1].pos == "" || words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                            {
                                if (words[3].pos == "" || words[3].pos == "noun_prop" || words[3].pos == "noun")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        else if (words[1].gloss == "type;kind;form" && words[1].pos == "noun")
                        {
                            if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                            {
                                if (words[3].pos == "" || words[3].pos == "noun_prop")
                                {
                                    if (words[4].pos == "" || words[3].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                        }

                    }
                    ///Train tagging
                    if (triggerWordTag == "nervt".ToUpper())
                    {
                        // do nothing
                    }
                    ///Aircraft tagging
                    if (triggerWordTag == "nerva".ToUpper())
                    {
                        if (words[1].pos == "" || words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                            {
                                if (words[3].pos == "" || words[3].pos == "noun_prop" || words[3].pos == "noun")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        else if (words[1].gloss == "type;kind;form" && words[1].pos == "noun")
                        {
                            if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                            {
                                if (words[3].pos == "" || words[3].pos == "noun_prop")
                                {
                                    if (words[4].pos == "" || words[3].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                        }
                        else if (words[1].gloss == "from" && words[1].pos == "prep")
                        {
                            if (words[2].pos == "noun")
                            {
                                if (words[2].gloss == "model;type;calibre" || words[2].gloss == "type;kind;form")
                                {
                                    if (words[3].pos == "" || words[3].pos == "noun_prop")
                                    {
                                        if (words[4].pos == "" || words[3].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                    }
                                    else { }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }

                    }
                    ///Ship Tagging
                    if (triggerWordTag == "nervs".ToUpper())
                    {
                        if (words[1].pos == "" || words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                            {
                                if (words[3].pos == "" || words[3].pos == "noun_prop" || words[3].pos == "noun")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        else if (words[1].gloss == "type;kind;form" && words[1].pos == "noun")
                        {
                            if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                            {
                                if (words[3].pos == "" || words[3].pos == "noun_prop")
                                {
                                    if (words[4].pos == "" || words[3].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                        }

                    }
                    ///Other tagging
                    if (triggerWordTag == "nervo".ToUpper())
                    {
                        // do nothing
                    }
                    #endregion
                    #region FoodAndDrinks
                    //Food And Drinks tagging
                    if (triggerWordTag == "nerf".ToUpper())
                    {
                        if (words[0].gloss == "eat;consume" || words[0].gloss == "drink")
                        {
                            if (words[1].pos == "" || words[1].pos == "noun_prop" || words[1].pos == "noun")
                            {
                                if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                                {
                                    if (words[3].pos == "" || words[3].pos == "noun_prop" || words[3].pos == "noun")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else { }

                        }
                        else { }

                    }
                    #endregion
                    #region Acadimic
                    //Acadimic tagging
                    //// عاوزه احط الاكاديمك قبل القران والحديث وكده################
                    if (triggerWordTag == "ners".ToUpper())
                    {
                        if (words[0].gloss == "study" || words[0].gloss == "sciences" || words[0].gloss == "substance;material" || words[0].gloss == "methods;approaches;programs;curricula" || words[0].gloss == "area;field;arena;context;opportunity" || words[0].gloss == "world" || words[0].gloss == "studying;checking;examining" || words[0].gloss == "science" || words[0].gloss == "be_known;be_found_out" || words[0].gloss == "know;find_out")
                        {
                            if (words[1].bw == ":/PUNC")
                            {
                                if (words[2].pos == "noun")
                                {
                                    if (words[3].pos == "noun" && words[3].prc2 == "wa_conj")
                                    {
                                        if (words[4].pos == "noun" && words[4].prc2 == "wa_conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {

                                }
                            }
                            else if (words[2].bw == ":/PUNC")
                            {
                                if (words[3].pos == "noun")
                                {
                                    if (words[4].pos == "noun" && words[4].prc2 == "wa_conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {

                                }
                            }
                            else if (words[3].bw == ":/PUNC")
                            {
                                if (words[4].pos == "noun")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 5 - 1] += "</font>";
                                }
                                else
                                {

                                }
                            }
                            else if (words[1].pos == "noun")
                            {
                                if (words[2].pos == "noun")
                                {
                                    if (words[3].pos == "noun" && words[3].prc2 == "wa_conj")
                                    {
                                        if (words[4].pos == "noun" && words[4].prc2 == "wa_conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else
                            { }


                        }
                        else if (words[0].gloss == "certificate;witness;testimony" || words[0].gloss == "doctor;Doctor;Dr." || words[0].gloss == "professor")
                        {
                            if (words[1].bw == "fiy/PREP")
                            {
                                if (words[2].pos == "noun")
                                {
                                    if (words[3].pos == "noun")
                                    {
                                        if (words[4].pos == "noun" && words[4].prc2 == "wa_conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {

                                }

                            }
                            else if (words[1].prc1 == "bi_prep" || words[1].prc1 == "li_prep" && words[1].pos == "noun")
                            {

                                if (words[2].pos == "noun")
                                {
                                    if (words[3].pos == "noun")
                                    {
                                        if (words[4].pos == "noun" && words[4].prc2 == "wa_conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                        }

                        else { }
                    }
                    #endregion
                    #region ART
                    //Art
                    #region Picture
                    ///Picture tagging
                    /// لوحة الفنان الامريكي() في عام () و المسمي "    "
                    ///  how to tag this
                    if (triggerWordTag == "nertp".ToUpper())
                    {

                    }
                    #endregion
                    #region Prog
                    ///Program tagging
                    if (triggerWordTag == "nerto".ToUpper())
                    {
                        if (words[1].pos == "" || words[1].pos == "noun_prop")
                        {
                            if (words[2].pos == "" || words[2].pos == "noun_prop" || words[2].pos == "noun")
                            {
                                if (words[3].pos == "" || words[3].pos == "noun_prop" || words[3].pos == "noun")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                        }
                        else if (words[2].pos == "prep")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 3 - 1] += "</font>";
                        }
                        else if (words[3].pos == "prep")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 4 - 1] += "</font>";
                        }
                        else
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 4 - 1] += "</font>";
                        }

                    }
                    #endregion
                    #region Movie
                    ///movie tagging
                    /// how too tag those??????????
                    ///فيلم سينمائي بعنوان
                    ///الافلام والمسلسلات نذكر منها:
                    ///فيلم وثائقي من انتاج ايراني بعنوان ()

                    if (triggerWordTag == "nertm".ToUpper())
                    {
                        /////فيلم : ام كلثوم 
                        if (words[0].gloss == "film;movie" || words[0].gloss == "serial;sequence;soap_opera" || words[0].gloss == "films;movies")
                        {
                            if (words[1].pos == "punc")
                            {
                                if (words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }

                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }

                                }
                                else
                                {
                                    if (words[1].pos == "prep" || words[1].pos == "conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }

                            }

                            else if (words[1].pos == "noun_prop" || words[1].pos == "noun" || words[1].pos == "" || words[1].pos == "conj" || words[1].pos == "prep" || words[1].pos == "digit")
                            {
                                if (words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words[1].pos == "adj")
                            {
                                if (words[2].pos == "punc")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else if (words[2].pos == "noun" && words[2].gloss == "address" && words[2].prc1 == "bi_prep")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";

                                        }

                                    }
                                    else
                                    {
                                        if (words[3].pos == "prep" || words[3].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else if (words[2].gloss == "like;such_as")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";

                                        }

                                    }
                                    else
                                    {
                                        if (words[3].pos == "prep" || words[3].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else
                                { }



                            }
                            else
                            {
                                if (words[1].pos == "prep" || words[1].pos == "conj")
                                {

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                            }


                        }

                        else
                        { }

                    }
                    #endregion
                    #region Show
                    ///Show tagging
                    ///مسرحية كوزي فان توتي في :
                    ///مسرحية اوبرا داي زوبرفلوت في :
                    ///مسرحيات المزمار السحري في :
                    ///مسرحيته اوديب ملكا في :
                    ///مسرحيته الفرس في :
                    ///مسرحية السيد جوفاني في :

                    if (triggerWordTag == "nerts".ToUpper())
                    {
                        if (words[0].gloss == "theatrical" || words[0].gloss == "plays_(theater)" || words[0].gloss == "play_(theater)")
                        {

                            if (words[1].pos == "punc")
                            {
                                if (words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 2 - 1] += "</font>";
                                            }

                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 1 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }

                                }
                                else
                                {
                                    if (words[1].pos == "prep" || words[1].pos == "conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }

                            }

                            else if (words[1].pos == "noun_prop" || words[1].pos == "noun" || words[1].pos == "" || words[1].pos == "conj" || words[1].pos == "prep" || words[1].pos == "digit")
                            {
                                if (words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit" || words[2].pos == "adj")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words[1].pos == "adj")
                            {
                                if (words[2].pos == "punc")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else if (words[2].pos == "noun" && words[2].gloss == "address" && words[2].prc1 == "bi_prep")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }

                                        }

                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else if (words[2].gloss == "like;such_as")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";

                                        }

                                    }
                                    else
                                    {
                                        if (words[3].pos == "prep" || words[3].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else
                                { }



                            }
                            else
                            {
                                if (words[1].pos == "prep" || words[1].pos == "conj")
                                {

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                            }





                        }
                    }
                    #endregion
                    #region Music
                    ///Music tagging
                    if (triggerWordTag == "nertu".ToUpper())
                    {
                        //Do Nothing
                    }
                    #endregion
                    #region Book
                    ///Book tagging
                    ///Go Back to Excel Sheet
                    if (triggerWordTag == "nertb".ToUpper())
                    {
                        if (words.Count == 5)
                        {
                            if (words[1].pos == "punc")
                        {
                            if (words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit")
                            {
                                if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj")
                                {
                                    if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                    {

                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        if (words[3].pos == "prep" || words[3].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }

                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                    }
                                }
                                else
                                {
                                    if (words[2].pos == "prep" || words[2].pos == "conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }

                            }
                            else
                            {
                                if (words[1].pos == "prep" || words[1].pos == "conj")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                        }

                        else if (words[1].pos == "noun_prop" || words[1].pos == "noun" || words[1].pos == "" || words[1].pos == "conj" || words[1].pos == "prep" || words[1].pos == "digit")
                        {
                            if (words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit" || words[2].pos == "adj")
                            {
                                if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj")
                                {
                                    if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                    {

                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        if (words[3].pos == "prep" || words[3].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                    }
                                }
                                else
                                {
                                    if (words[2].pos == "prep" || words[2].pos == "conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }

                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                            else if (words[1].pos == "adj")
                            {
                                if (words[2].pos == "punc")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else if (words[2].pos == "noun" && words[2].gloss == "address" && words[2].prc1 == "bi_prep")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }

                                        }

                                    }
                                    else
                                    {
                                        if (words[2].pos == "prep" || words[2].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else if (words[2].gloss == "like;such_as")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            if (words[3].pos == "prep" || words[3].pos == "conj")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";

                                        }

                                    }
                                    else
                                    {
                                        if (words[3].pos == "prep" || words[3].pos == "conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else
                                { }
                            }
                        }
                    }
                    #endregion
                    #endregion
                    #region  Printing
                    //Printing
                    ///NewsPaper tagging
                    ///صدرت
                    #region NewsPaper
                    if (triggerWordTag == "nerpn".ToUpper())
                    {
                        #region noun Before
                        if (words[0].pos == "noun" || words[0].pos == "noun_prop")
                        {
                            if (words[1].pos == "" || words[1].pos == "part_det" || words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop")
                            {

                                if (words[1].pos == "adj")
                                {
                                    if (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "noun_prop")
                                    {
                                        if (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "noun_prop")
                                        {
                                            if (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "noun_prop")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    if (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun_prop" || words[2].pos == "" || words[2].pos == "noun")
                                    {
                                        if (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun_prop" || words[3].pos == "" || words[3].pos == "noun")
                                        {
                                            if (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun_prop" || words[4].pos == "" || words[4].pos == "noun")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                            }
                            else if (words[1].pos == "noun_prop" || words[1].pos == "")
                            {
                                if (words[2].pos == "noun_prop" || words[2].pos == "" || words[2].pos == "adj" || words[2].pos == "part_det")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "" || words[3].pos == "adj" || words[3].pos == "part_det")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "" || words[4].pos == "adj" || words[4].pos == "part_det")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }


                            }
                            else
                            {
                                if (words[1].pos == "punc")
                                {
                                    if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "adj" || words[4].pos == "noun_prop" || words[4].pos == "")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else if (words[4].pos == "punc")
                                    {

                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";


                                    }
                                    else if (words[4].pos == "adj" || words[4].pos == "noun_prop" || words[4].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }



                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }

                        }

                        #endregion
                        else
                        { }
                    }
                    #endregion
                    #region Magazine
                    ///Magazine tagging
                    if (triggerWordTag == "nerpm".ToUpper())
                    {
                        if (words[0].pos == "noun" || words[0].pos == "noun_prop")
                        {
                            if (words[1].pos == "" || words[1].pos == "part_det" || words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop")
                            {

                                if (words[1].pos == "adj")
                                {
                                    if (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun" || words[2].pos == "" || words[2].pos == "noun_prop")
                                    {
                                        if (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun" || words[3].pos == "" || words[3].pos == "noun_prop")
                                        {
                                            if (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun" || words[4].pos == "" || words[4].pos == "noun_prop")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    if (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun_prop" || words[2].pos == "" || words[2].pos == "noun")
                                    {
                                        if (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun_prop" || words[3].pos == "" || words[3].pos == "noun")
                                        {
                                            if (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun_prop" || words[4].pos == "" || words[4].pos == "noun")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                            }
                            else if (words[1].pos == "noun_prop" || words[1].pos == "")
                            {
                                if (words[2].pos == "noun_prop" || words[2].pos == "" || words[2].pos == "adj" || words[2].pos == "part_det")
                                {
                                    if (words[3].pos == "noun_prop" || words[3].pos == "" || words[3].pos == "adj" || words[3].pos == "part_det")
                                    {
                                        if (words[4].pos == "noun_prop" || words[4].pos == "" || words[4].pos == "adj" || words[4].pos == "part_det")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }


                            }
                            else
                            {
                                if (words[1].pos == "punc")
                                {
                                    if (words[3].pos == "punc")
                                    {

                                        if (words[4].pos == "adj" || words[4].pos == "noun_prop" || words[4].pos == "")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else if (words[4].pos == "punc")
                                    {

                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";


                                    }
                                    else if (words[4].pos == "adj" || words[4].pos == "noun_prop" || words[4].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }



                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }

                        }


                        else
                        { }
                    }
                    #endregion
                    #endregion
                    #region Doctrine method
                    //Doctrine Method
                    #region religion
                    ///Religion tagging
                    if (triggerWordTag == "nermr".ToUpper())
                    {
                        //مذهب ديانه
                        if (words[0].gloss == "manner;path" || words[0].gloss == "religion;creed")
                        {
                            if (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else
                            { }

                        }
                        //مصحف والمصحف
                        else if (words[0].gloss == "volume")
                        {

                            if (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else
                            { }

                        }
                        // اعتنق واعتناقه
                        else if (words[0].gloss == "adoption;embracing" || words[0].gloss == "embrace;adopt")
                        {
                            if (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else
                            { }
                        }
                        else
                        {
                            if (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun")
                            {
                                if (words[2].pos == "adj" || words[2].pos == "noun" || words[2].pos == "prop_noun")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }
                        }
                    }
                    #endregion
                    #region Sport non#################
                    ///Sport tagging
                    if (triggerWordTag == "nerms".ToUpper())
                    {

                    }
                    #endregion
                    #region theory
                    ///theory tagging
                    if (triggerWordTag == "nermt".ToUpper())
                    {
                        if (words[0].gloss == "thought;thinking" || words[0].gloss == "theoretical;speculative" || words[0].gloss == "method;procedure" || words[0].gloss == "theory")
                        {
                            if (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun" || words[1].pos == "")
                            {
                                if (words[2].pos == "adj" || words[2].pos == "noun" || words[2].pos == "prop_noun" || words[2].pos == "")
                                {

                                    if (words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep" && words[3].pos == "noun" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                else
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";

                                }
                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }


                        }
                        else
                        { }
                    }
                    #endregion
                    ///Plan tagging
                    if (triggerWordTag == "nermp".ToUpper())
                    {
                        // do nothing
                    }
                    ///Culture tagging
                    if (triggerWordTag == "nermc".ToUpper())
                    {
                        // do nothing
                    }
                    #region Movement
                    ///movement tagging
                    if (triggerWordTag == "nermm".ToUpper())
                    {
                        if (words[0].gloss == "movement;activity;organization" && words[0].pos == "noun")
                        {
                            if (words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop" || words[1].pos == "")
                            {
                                if (words[2].pos == "noun" || words[2].pos == "adj" || words[2].pos == "noun_prop" || words[2].pos == "")
                                {
                                    if (words[2].pos == "noun" || words[2].pos == "adj")
                                    {
                                        if (words[2].prc0 == "Al_det" || words[2].prc0 == "Al_det")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words[0].pos == "")
                            {

                                if (words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop" || words[1].pos == "")
                                {
                                    if (words[2].pos == "noun" || words[2].pos == "adj" || words[2].pos == "noun_prop" || words[2].pos == "")
                                    {
                                        if (words[3].pos == "noun" || words[3].pos == "adj" || words[3].pos == "noun_prop" || words[3].pos == "")
                                        {
                                            if (words[3].pos == "noun" || words[3].pos == "adj" || words[3].pos == "noun_prop" || words[3].pos == "")
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                        }

                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }


                                }

                            }
                            else

                            { }

                        }
                        else
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }
                    }
                    #endregion
                    #endregion
                    #region Rule
                    //Rule
                    #region Law
                    ///Law tagging
                    if (triggerWordTag == "nerrl".ToUpper())
                    {
                        if (words[0].gloss == "law;statutes;regulations" || words[0].gloss == "constitution;statute" || words[0].gloss == "laws;regulations;rules;statutes")
                        {
                            if (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "noun_prop" || words[1].pos == "" || words[1].pos == "digit")
                            {
                                if (words[2].pos == "adj" || words[2].pos == "noun" || words[2].pos == "noun_prop" || words[2].pos == "" || words[2].pos == "digit")
                                {
                                    if (words[3].pos == "adj" || words[3].pos == "noun" || words[3].pos == "noun_prop" || words[3].pos == "" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "digit")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }


                            }
                            else
                            { }
                        }
                        ////البند 525 في القانون الجنايي
                        else if (words[0].gloss == "article;clause")
                        {
                            if (words[1].pos == "digit")
                            {
                                if (words[2].bw == "min/PREP" || words[2].bw == "fiy/PREP")
                                {
                                    if (words[3].pos == "noun")
                                    {
                                        if (words[4].pos == "adj" || words[4].pos == "noun" || words[4].pos == "noun_prop" || words[4].pos == "" || words[4].pos == "digit")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                    }

                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else
                            {

                            }

                        }
                        else
                        { }

                    }
                    #endregion
                    #region treaty
                    //اتفاق اوسلو  ????? treaty or Event
                    ///Treaty tagging
                    if (triggerWordTag == "nerrt".ToUpper())
                    {
                        if (words[0].gloss == "document;charter" || words[0].gloss == "treaty;accord" || words[0].gloss == "treaty;accord;pact" || words[0].gloss == "agreement;accord;treaty")
                        {
                            if (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "noun_prop" || words[1].pos == "" || words[1].pos == "digit")
                            {
                                if (words[2].pos == "adj" || words[2].pos == "noun" || words[2].pos == "noun_prop" || words[2].pos == "" || words[2].pos == "digit")
                                {
                                    if (words[3].pos == "adj" || words[3].pos == "noun" || words[3].pos == "noun_prop" || words[3].pos == "" || words[3].pos == "digit")
                                    {
                                        if (words[4].pos == "digit")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }


                            }
                            else
                            { }
                        }
                    }
                    #endregion
                    #endregion
                    #region langyage
                    //Language tagging lessaaaaaaaaa
                    if (triggerWordTag == "nerl".ToUpper())
                    {
                        /// اجاد اجادته
                        if (words[0].gloss == "be_proficient_at;do_well" && words[0].pos == "verb")
                        {
                            if (words[1].gloss == "language")
                            {
                                if (words[2].pos == "adj" || words[2].pos == "")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || words[3].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }


                                else
                                { }

                            }
                            else if (words[1].gloss == "languages")
                            {
                                if (words[2].pos == "adj" || words[2].pos == "")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || words[3].pos == "")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || words[4].pos == "")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else
                                {

                                }



                            }
                            else
                            { }


                        }
                        ///اللغة اللغات اللغتين
                        else if (words[0].gloss == "language" || words[0].gloss == "languages")
                        {
                            if (words[1].pos == "adj" || words[1].pos == "")
                            {

                                if (words[2].pos == "adj" && words[2].prc2 == "wa_conj" || words[2].pos == "")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || words[3].pos == "")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || words[4].pos == "")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else
                            { }

                        }
                        ////ترجمت الي
                        else if (words[0].gloss == "translate;interpret" || words[0].gloss == "translator;interpreter")
                        {
                            ///ترجم كذلك أو ترجم ايضا 
                            if (words[1].gloss == "that" || words[1].gloss == "also,_too,_as_well_(as),_along_with,_in_addition_(to)")
                            {
                                if (words[2].bw == "&lt;ilaY/PREP")
                                {
                                    //الي العربيه
                                    if (words[3].pos == "adj")
                                    {
                                        if (words[4].pos == "adj")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    ////الي اللغة العربيه

                                    else if (words[3].gloss == "language" || words[3].gloss == "languages")
                                    {
                                        if (words[4].pos == "adj" || words[4].pos == "")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }

                                        else
                                        {

                                        }

                                    }
                                    else
                                    {

                                    }
                                }


                            }


                            else if (words[1].bw == "&lt;ilaY/PREP")
                            {
                                //الي العربيه
                                if (words[2].pos == "adj")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc2 == "wa_conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                ///الي اللغة العربية
                                else if (words[2].gloss == "language" || words[2].gloss == "languages")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || words[3].pos == "")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || words[4].pos == "")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            //ترجمت من الي
                            else if (words[1].bw == "min/PREP")
                            {

                                if (words[2].pos == "adj")
                                {
                                    if (words[3].bw == "&lt;ilaY/PREP")
                                    {
                                        if (words[4].pos == "adj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }


                                    }

                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else if (words[2].gloss == "language" || words[2].gloss == "languages")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || words[3].pos == "")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || words[4].pos == "")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else
                                { }
                            }
                            ///ترجمها فلان الي


                            else if (words[2].bw == "&lt;ilaY/PREP")
                            {
                                if (words[3].pos == "adj")
                                {
                                    if (words[4].pos == "adj" && words[4].prc2 == "wa_conj")
                                    {

                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else
                            { }



                        }
                        ////يدرس ب
                        else if (words[0].gloss == "study;learn" || words[0].gloss == "speak;discuss" || words[0].gloss == "be_published;be_issued")
                        {
                            if (words[1].gloss == "language" || words[1].gloss == "languages")
                            {
                                if (words[2].pos == "adj" || words[2].pos == "")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || words[3].pos == "")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || words[4].pos == "")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                            }


                            else if (words[1].pos == "adj" && words[1].bw == "bi/PREP")
                            {
                                if (words[2].pos == "adj" && words[3].prc2 == "wa_conj")
                                {
                                    if (words[3].pos == "adj" && words[3].prc2 == "wa_conj")
                                    {
                                        if (words[4].pos == "adj" && words[4].prc2 == "wa_conj")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                            }
                            else
                            { }
                        }

                        else
                        { }

                    }
                    #endregion
                    #region Currency
                    //Currencey tagging
                    if (triggerWordTag == "neru".ToUpper())
                    {
                        //// i need ti check if the previous word is digit
                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                        diacLineWords[loc + 1 - 1] += "</font>";
                    }
                    #endregion

                    #region other
                    //Other tagging
                    if (triggerWordTag == "nero".ToUpper())
                    {
                        if (words[0].gloss == "canal;channel" || words[0].gloss == "screen" || words[0].gloss == "testing;experimenting;probing" || words[0].gloss == "canals;channels" || words[0].gloss == "network;system")
                        {
                            if (words[1].pos == "noun" && words[2].pos == "adj")
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                            else if (words[1].pos == "noun")
                            {
                                if (words[2].gloss == "like;such_as" || words[2].gloss == "punc")
                                {
                                    if (words[3].pos == "noun" || words[3].pos == "noun_prop" || words[3].pos == "abbrev")
                                    {

                                        if (words[4].pos == "noun" || words[4].pos == "noun_prop" || words[4].pos == "abbrev")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else if (words[3].pos == "punc")
                                    {
                                        if (words[4].pos == "noun" || words[4].pos == "noun_prop" || words[4].pos == "abbrev")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                    }
                                    else
                                    {
                                        if (words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                            else if (words[1].pos == "abbrev" || words[1].pos == "" || words[1].pos == "noun_prop")
                            {
                                if (words[2].pos == "abbrev" || words[2].pos == "" || words[2].pos == "noun_prop")
                                {
                                    if (words[3].pos == "abbrev" || words[3].pos == "" || words[3].pos == "noun_prop")
                                    {
                                        if (words[4].pos == "abbrev" || words[4].pos == "" || words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                            else if (words[1].pos == "noun" || words[1].pos == "noun_prop")
                            {
                                if (words[2].pos == "abbrev" || words[2].pos == "" || words[2].pos == "noun_prop")
                                {
                                    if (words[3].pos == "abbrev" || words[3].pos == "" || words[3].pos == "noun_prop")
                                    {
                                        if (words[4].pos == "abbrev" || words[4].pos == "" || words[4].pos == "noun_prop")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                        }
                        else
                        { }
                    }
                    #endregion
                    #endregion
                    #region 3-Color
                    //Color tagging
                    if (triggerWordTag == "nec".ToUpper())
                    {
                        ////how to tagging from the second word
                        if (words[0].gloss == "color;tint" && words[0].pos == "noun")
                        {
                            if (words[1].gloss == "yellow" || words[1].gloss == "red" || words[1].gloss == "blue" || words[1].gloss == "black" || words[1].gloss == "white")
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else
                            { }


                        }
                        else if (words[0].gloss == "yellow" || words[0].gloss == "red" || words[0].gloss == "blue" || words[0].gloss == "black" || words[0].gloss == "white")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }

                    }
                    #endregion
                    #region 8-Facility
                    //Archaeological place tagging
                    if (triggerWordTag == "nefa".ToUpper())
                    {

                    }
                    //GOE
                    /// Port tagging
                    if (triggerWordTag == "nefgp".ToUpper())
                    {

                    }
                    /// Airport tagging
                    if (triggerWordTag == "nefga".ToUpper())
                    {

                    }
                    /// Station tagging
                    if (triggerWordTag == "nefgs".ToUpper())
                    {
                        // Do nothing
                    }
                    /// Worship Place tagging
                    if (triggerWordTag == "nefgw".ToUpper())
                    {

                    }
                    /// Theater and Cinema tagging
                    if (triggerWordTag == "nefgt".ToUpper())
                    {

                    }
                    /// Zoo tagging
                    if (triggerWordTag == "nefgz".ToUpper())
                    {
                        // Do nothing
                    }
                    /// Museum tagging
                    if (triggerWordTag == "nefgm".ToUpper())
                    {

                    }
                    /// Sports Facility tagging
                    if (triggerWordTag == "nefgf".ToUpper())
                    {

                    }
                    /// Park tagging
                    if (triggerWordTag == "nefgk".ToUpper())
                    {

                    }
                    /// Public institution tagging
                    if (triggerWordTag == "nefgi".ToUpper())
                    {

                    }
                    /// School tagging
                    if (triggerWordTag == "nefgh".ToUpper())
                    {

                    }
                    //Line
                    /// Canal tagging
                    if (triggerWordTag == "neflc".ToUpper())
                    {

                    }
                    /// Tunnel tagging
                    if (triggerWordTag == "neflt".ToUpper())
                    {
                        // Do nothing
                    }
                    /// Bridge tagging
                    if (triggerWordTag == "neflb".ToUpper())
                    {

                    }
                    //Other tagging

                    if (triggerWordTag == "nefo".ToUpper())
                    {

                    }
                    #endregion
                    #region 9-Location
                    //GPE
                    /// Country tagging
                    if (triggerWordTag == "nelgo".ToUpper())
                    {

                    }
                    /// City tagging
                    if (triggerWordTag == "nelgc".ToUpper())
                    {

                    }
                    /// Village tagging
                    if (triggerWordTag == "nelgv".ToUpper())
                    {

                    }
                    /// Street tagging
                    if (triggerWordTag == "nelgs".ToUpper())
                    {

                    }
                    /// Other tagging
                    if (triggerWordTag == "nelgt".ToUpper())
                    {

                    }
                    // Region
                    ///Continenntal tagging
                    if (triggerWordTag == "nelrc".ToUpper())
                    {

                    }
                    ///Domestic tagging
                    if (triggerWordTag == "nelrd".ToUpper())
                    {

                    }
                    //Geological Reg
                    /// Mountain tagging
                    if (triggerWordTag == "nelcm".ToUpper())
                    {

                    }
                    /// Island tagging
                    if (triggerWordTag == "nelci".ToUpper())
                    {

                    }
                    /// River tagging
                    if (triggerWordTag == "nelcr".ToUpper())
                    {

                    }
                    /// Lake tagging
                    if (triggerWordTag == "nelcl".ToUpper())
                    {

                    }
                    /// Sea tagging
                    if (triggerWordTag == "nelcs".ToUpper())
                    {

                    }
                    /// Bay tagging
                    if (triggerWordTag == "nelcb".ToUpper())
                    {

                    }
                    /// Other tagging
                    if (triggerWordTag == "nelco".ToUpper())
                    {

                    }
                    // Astral body tagging
                    if (triggerWordTag == "nelb".ToUpper())
                    {

                    }
                    // Address
                    ///Url tagging
                    if (triggerWordTag == "nelau".ToUpper())
                    {

                    }
                    ///Email tagging
                    if (triggerWordTag == "nelae".ToUpper())
                    {
                        //Do nothing
                    }
                    /// Phone number tagging
                    if (triggerWordTag == "nelah".ToUpper())
                    {
                        //Do nothing
                    }
                    /// Postal address tagging
                    if (triggerWordTag == "nelap".ToUpper())
                    {
                        //Do nothing
                    }
                    #endregion
                    #region 10-Organization
                    //International Org Tagging
                    if (triggerWordTag == "neoi".ToUpper())
                    {

                    }
                    //Sports Org Tagging
                    if (triggerWordTag == "neos".ToUpper())
                    {

                    }
                    //Corportion Tagging
                    if (triggerWordTag == "neoc".ToUpper())
                    {

                    }
                    //Political Org

                    ///Government Tagging
                    if (triggerWordTag == "neopg".ToUpper())
                    {

                    }
                    ///Military Tagging
                    if (triggerWordTag == "neopm".ToUpper())
                    {

                    }
                    ///Other Tagging
                    if (triggerWordTag == "neopo".ToUpper())
                    {

                    }
                    //Other Tagging
                    if (triggerWordTag == "neoo".ToUpper())
                    {

                    }
                    #endregion
                    #region 11-Title
                    //Title tagging
                    ///لازم اشوف وراها title تاني ولا لا 
                    ///لازم اشوف وراها nationality

                    if (triggerWordTag == "nei".ToUpper())
                    {
                        if (words[1].pos == "noun_prop")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }
                        else if (words[1].pos == "")
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }

                    }
                    #endregion
                    #region 12-Person
                    //Person tagging
                    if (triggerWordTag == "nep".ToUpper())
                    {

                        if (words[1].pos == "noun_prop" || words[1].pos == "")
                        {
                            if (words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "")
                            {
                                if (words[3].pos == "noun_prop" || words[3].pos == "noun" || words[3].pos == "")
                                {
                                    if (words[4].pos == "noun_prop" || words[4].pos == "noun" || words[4].pos == "")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {// move to next word add 1 to the location
                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                        diacLineWords[loc + 4 - 1] += "</font>";

                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }
                            else
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        else
                        {

                        }


                    }
                    #endregion
                    #region 13-God
                    //God tagging
                    if (triggerWordTag == "neg".ToUpper())
                    {

                    }
                    #endregion
                    #region 14-Job
                    //Job tagging
                    if (triggerWordTag == "nej".ToUpper())
                    {

                    }
                    #endregion
                    #region 15-Nationality
                    //Nationality tagging
                    if (triggerWordTag == "nea".ToUpper())
                    {

                    }
                    #endregion
                    #region 1-Timex
                    //Timex Tagging
                    //Timex Time tagging
                    // اي digit مكون من 4 ارقام وبعديه م او ه هوه فالاغلب تاريخ##########################
                    if (triggerWordTag == "nett".ToUpper())
                    {


                    }
                    //Timex Date tagging

                    /////################## ق.م؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟
                    if (triggerWordTag == "netd".ToUpper())
                    {
                        //بين عامي
                        if (words[0].gloss == "year" || words[0].gloss == "enactment;prescription")
                        {
                            if (words[1].pos == "digit")
                            {
                                //بين عامي
                                if (words[2].pos == "punc")
                                {
                                    if (words[3].pos == "digit")
                                    {
                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else
                            { }

                        }
                        //الاول من سبتمبر عام 1948
                        else if (words[0].pos == "adj_num")
                        {
                            if (words[1].bw == "min/PREP")
                            {
                                if (words[2].pos == "noun_prop")
                                {
                                    if (words[3].gloss == "year")
                                    {
                                        if (words[4].pos == "digit")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }


                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else
                                { }

                            }
                            else if (words[1].pos == "adj_num")
                            {
                                if (words[2].bw == "min/PREP")
                                {
                                    if (words[3].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    { }
                                }
                                else
                                { }

                            }
                            else
                            { }


                        }
                        else if (words[0].pos == "noun_prop" || words[0].pos == "")
                        {
                            //كانون الثاني\ يناير 1918

                            if (words[1].pos == "punc")
                            {
                                if (words[2].pos == "noun_prop")
                                {
                                    if (words[3].pos == "year")
                                    {
                                        if (words[4].pos == "digit")
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }


                                    }
                                    else if (words[3].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words[3].pos == "prep")
                                    {

                                        if (words[4].pos == "year")
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }

                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }


                            }
                            else if (words[2].pos == "punc")
                            {
                                if (words[3].pos == "noun_prop")
                                {

                                    if (words[4].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                            }


                            else if (words[1].bw == "min/PREP")
                            {

                                if (words[2].gloss == "year")
                                {
                                    if (words[3].pos == "digit")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }


                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }




                            }
                            else
                            { }



                        }
                        //بتاريخ 30\1\1976
                        else if (words[0].gloss == "date;history")
                        {
                            if (words[1].pos == "digit" && words[2].pos == "punc" && words[3].pos == "digit" && words[4].pos == "punc")
                            {

                                diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                diacLineWords[loc + 2 - 1] += "</font>";

                            }


                        }

                        else if (words[0].bw == "h/ABBREV" || words[0].bw == "m/ABBREV")
                        {
                            if (words[1].pos == "punc" && words[2].pos == "digit" && words[3].bw == "m/ABBREV")
                            {

                                diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                diacLineWords[loc + 4 - 1] += "</font>";
                            }

                            else if (words[1].pos == "punc" && words[2].pos == "digit" && words[3].bw == "m/ABBREV")
                            {
                                diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                diacLineWords[loc + 4 - 1] += "</font>";
                            }
                            else if (words[1].pos == "punc" && words[2].pos == "digit")
                            {
                                diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }

                        else if (words[0].bw == "q/ABBREV")
                        {
                            if (words[1].pos == "punc" && words[2].bw == "m/ABBREV")
                            {
                                diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }


                        }


                        else
                        { }

                    }
                    //Timex other tagging
                    if (triggerWordTag == "neto".ToUpper())
                    {

                    }
                    #endregion
                    if (words.Count == 5)
                    {
                        #region 5Words
                        //  
                        if (words[0].pos == "noun" && words[1].pos == "noun" && words[0].num == words[1].num)
                        {

                            if (words[2].pos == "noun" && words[1].pos == "noun" && words[2].num == words[1].num)
                            {
                                // first three are the same 
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";

                            }
                            else
                            {
                                // first two are the same
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";

                            }

                        }

                        #endregion
                    }
                    else if (words.Count == 4)
                    {

                        #region 4Words

                        #endregion
                    }
                    else if (words.Count == 3)
                    {

                        #region 3Words

                        #endregion

                    }
                    else if (words.Count == 2)
                    {
                        #region 2Words

                        #endregion

                    }



                    #region comment

                    // tag the words !

                    //if (diacLineWords.Count() > loc)
                    //{
                    //    loc++;
                    //    var count = theWord.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries).Length;

                    //    if (count == 1)
                    //    {

                    //        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc] + "</font>";
                    //    }
                    //    else
                    //    {

                    //        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];

                    //        diacLineWords[loc + count - 1] += "</font>";

                    //    }
                    //}
                    #endregion
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
