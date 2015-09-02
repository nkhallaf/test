﻿using System;
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
                        var theWord2 = diacLineWords[loc];
                        var words = BL.MadaMiraHandler.Analyse(theWord2);
                        if (words[0].pos == "noun_prop" || words[0].pos == "")
                        {

                        diacLineWords[loc] = "<font title='Tag-" + wordTag[wordTagIndex].Tag.ToUpper() + "' style='color:" + wordTag[wordTagIndex].Color + "'>" + diacLineWords[loc] + "</font>";
                        }
                    }
                    else
                    {
                        var theWord2 = string.Empty;

                        for (int inex = loc; inex < loc + count; inex++)
                            theWord2 = theWord2 + " " + diacLineWords[inex];

                        theWord2 = theWord2.Trim();

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

                if (lineWords[i].Contains(tagWords[0]))
                {
                    var BetweenTags = false;
                    for (int index = i; index < lineWords.Length; index++)
                    {
                        if (lineWords[index].Contains("</"))
                        {
                            BetweenTags = true;
                            index = lineWords.Length;
                        }
                        else if (lineWords[index].Contains("<font"))
                        {

                            index = lineWords.Length;
                        }


                    }

                    if (BetweenTags)
                        return locations;



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
                        else if (tagWords.Length > 2)
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



            return locations;

        }
        internal static void TagLineForTriggerWord(List<string> theText, List<TriggerWords> triggerWords, int wordTagIndex, int textIndex)
        {
            var theWord = triggerWords[wordTagIndex].TriggerWord;
            var theLine = theText[textIndex];
            theLine = theLine.Replace("(", " ");
            theLine = theLine.Replace(")", " ");
            theLine = theLine.Replace(@"""", @" ");

            string[] diacLineWords = theLine.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);



            string pureLine = TextEditing.RemoveDiacritics(theLine);
            pureLine = pureLine.Replace("(", " ");
            pureLine = pureLine.Replace(")", " ");
            if (pureLine.Contains(theWord))
            {
                //  locations of the trigger words
                var locations = GetLocationForGeneralWordTag(theWord, pureLine);

                if (locations.Count == 0)
                    return;

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



                    sentence = sentence.Replace("<", "");
                    sentence = sentence.Replace(">", "");
                    var words = BL.MadaMiraHandler.Analyse(sentence);
                    var triggerWordTag = triggerWords[wordTagIndex].Tag.ToUpper();
                    ////################ we need to start the tagging process after the last tagged word
                    //how to excelude puncutations???
                    //first Tagging through previous trigger words
                    //Second Tagging through Followed trigger words

                    //Fourth Tagging untagged NEs if it is exist in the text
                    //Fifth Tagging from Fixed tablenerto

                    // The title of the text needs to be analysed by MadaMira for searching for Prop-noun
                    ////لما يلاقيها خلاص ميبصش علي اللي بعدها
                    ////يبص علي الي قبلها
                    ////trigger words تكون جزء من الكلمه ويتاكد من ال lemmas
                    ////Title tagging
                    #region 6-Event
                    //occasion
                    #region Religious
                    ///Religious Festival
                    if (triggerWordTag == "neekr")
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَوْلِد_1" || words[0].lemma == "رَأْس_1" || words[0].lemma == "يَوْم_1" || words[0].lemma == "عِيد_1" || words[0].lemma == "لَيْلَة_1"))
                        {
                            if (words.Count >= 3)
                            {
                                if (words[1].pos == "adj_num" && words[2].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words[1].gloss == "year" && words[1].pos == "noun" && words[2].pos == "adj")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words[1].pos == "noun" && words[2].pos == "noun" && words[2].prc2 == "wa_conj")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words[1].pos == "noun" && words[2].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words.Count >= 4)
                            {
                                if (words.Count > 1 && (words[1].pos == "noun" && words[2].pos == "adj" && words[3].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                            }
                            else if (words.Count == 2)
                            {
                                if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                        }
                    }
                    #endregion
                    #region Event Game
                    ///Game tagging
                    else if (triggerWordTag == "neekg".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "لَعْب_1" || words[0].lemma == "دَوْرِيّ_2" || words[0].lemma == "مُونْدِيال_1" || words[0].lemma == "سَبّاق_1" || words[0].lemma == "بُطُولَة_1" || words[0].lemma == "نِهائِيّ_1" || words[0].lemma == "مُلْتَقَى_1" || words[0].lemma == "هَدّاف_1" || words[0].lemma == "أُولِمْبِياد_1" || words[0].lemma == "تَصْفِيَة_1" || words[0].lemma == "أُولِمْبِيّ_1"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 0 && (words[0].pos == "noun_prop" && words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "digit"))
                                        {
                                            ///اولمبياد برشلونة 1992


                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";

                                        }
                                        else if (words.Count > 2 && (words[2].pos != "digit"))
                                        {
                                            //اولمبياد اثينا كلمتان NES
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                    }
                                }
                                else if (words.Count > 0 && (words[0].pos == "noun_prop" && words[1].pos == "digit"))
                                {
                                    // مونديال 2009 
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";

                                }
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "digit"))
                                {
                                    // دوري 2009
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";

                                }
                                //بطولة العالم لالعاب القوى 

                                //بطولة المملكة للمسابقة الثقافية 
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "noun"))
                                        {
                                            if (words.Count > 2 && (words[2].prc1 == "li_prep" && words[3].pos == "noun"))
                                            {
                                                if (words.Count > 5 && (words[5].pos == "digit"))
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
                                            else if (words.Count > 2 && (words[2].prc1 != "li_prep" && words[3].pos == "noun"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "digit"))
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

                                            else if (words.Count > 2 && (words[2].prc1 == "li_prep" && words[3].pos == "adj"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "digit"))
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

                                            else if (words.Count > 2 && (words[2].prc1 != "li_prep" && words[2].pos == "noun"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "digit"))
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
                                            else if (words.Count > 3 && (words[3].pos == "adj_num" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";

                                            }
                                            //بطولة العالم للجري
                                            else if (words.Count > 2 && (words[2].prc1 == "li_prep"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }


                                        }
                                        /// دوري الدرجة الاولى
                                        else if (words.Count > 2 && (words[2].pos == "adj_num" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";

                                        }
                                        else if (words.Count > 2 && (words[2].pos == "adj"))
                                        {
                                            //دورة الالعاب العربية السابعة
                                            if (words.Count > 3 && (words[3].pos == "adj_num"))
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                            //// كاس الامم الاوروبية لكرة القدم
                                            else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc1 == "li_prep"))
                                            {
                                                //لكرة القدم
                                                if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc0 == "Al_det"))
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 5 - 1] += "</font>";

                                                }
                                                else if (words.Count > 4 && (words[4].pos != "noun" | words[4].prc0 == "Al_det"))
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
                                        else if (words.Count > 2 && (words[2].pos == "noun_prop" && words[2].prc0 == "0" && words[1].prc0 == "0"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "digit"))
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
                                //المسابقة السباعية
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "adj"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        // العاب اولمبية صيفية 1980
                                        //// noun+adj + adj +number
                                        if (words.Count > 2 && (words[2].pos == "adj" && words[3].pos == "digit"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                        // الدوري الممتاز للرجبي //// i have to access the first two letters of the word "لل"
                                        else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") && words[2].word == "للرجبي"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                            //البطولة العربية الاولى 1978
                                        //noun+adj+num+digit
                                        else if (words.Count > 2 && (words[2].pos == "adj_num"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "digit"))
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
                                        else if (words.Count > 2 && (words[2].pos == "adj"))
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        // البطولة العربية للناشئات في 
                                        // البطولة الاولمبية لكرة القدم
                                        else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                            else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 != "Al_det"))
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
                                    ////معرفين بال
                                    else if (words.Count > 0 && (words[0].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                    ////ثلاث كلمات معرفين بال
                                    else if (words.Count > 0 && (words[0].prc0 == "Al_det" && words[1].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                    {
                                        if (words.Count >= 3)
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }

                                           ///Noun + noun
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";

                                    }
                                }

                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3 && words[2].pos == "adj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                // دورة لوس انجلوس في
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3 && words[2].pos == "noun_prop")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                //  دورة طوكيو في 
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun_prop"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                // دورة استكهولم في
                                else if (words.Count > 0 && (words[0].pos == "noun" && (words[1].pos == "" && words[1].word != "font")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 0 && (words[0].pos == "noun_prop" && (words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "digit"))
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
                            }
                        }
                    }
                    #endregion
                    #region Event Conference
                    ///Conference tagging
                    ///########ندوه بعنوان ""
                    else if (triggerWordTag == "neekc".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَعْرِض_1" || words[0].lemma == "مُلْتَقَى_1" || words[0].lemma == "نَدْوَة_1" || words[0].lemma == "مَهْرَجان_1" || words[0].lemma == "مُؤْتَمَر_1"))
                        {
                            if (words.Count >= 2)
                            {
                                //مؤتمر امستردام

                                if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "digit"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        //ندوة ( ابو حيان التوحيدي )


                                        else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun_prop"))
                                        {
                                            if (words.Count > 2 && (words[2].pos == "noun" || words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                            {
                                                if (words.Count >= 4)
                                                {
                                                    if (words.Count > 3 && (words[3].pos == "noun" || words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
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

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                    }
                                }

                                ///موتمر 2001

                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "digit"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }


                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "adj"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        // المهرجان الدولي للاغنية بالقاهرة
                                        if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "noun_prop" && words[3].prc1 == "bi_prep"))
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 4 - 1] += "</font>";
                                                }
                                                //الندوة التاسيسية لمركز الدراسات والبحوث العربية
                                                else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
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

                                        }
                                    }
                                    //المؤتمر الهندسي السعودي السادس
                                    else if (words.Count > 2 && (words[2].pos == "adj"))
                                    {
                                        if (words.Count > 2 && (words[2].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                        {
                                            if (words.Count >= 3)
                                            {

                                                if (words.Count > 3 && (words[3].pos == "adj_num" && words[3].prc0 == "Al_det"))
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
                                    //القمة العربية
                                    else if (words.Count > 0 && (words[0].prc0 == "Al_det" && words[1].prc0 == "Al_det" && words[0].num == "s" && words[1].num == "s"))
                                    {
                                        if (words.Count > 2 && (words[2].bw == "fiy/PREP" || words[2].bw == "Ean/PREP"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                                        }
                                        //الندوة الربيعية الاولى للجامعة الصيفية العربية الاوربية
                                        else if (words.Count > 2 && (words[2].pos == "adj_num" && words[2].prc0 == "Al_det"))
                                        {
                                            if (words.Count >= 3)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep"))
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
                                                    {
                                                        if (words.Count > 4)
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (words.Count > 3)
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 4 - 1] += "</font>";
                                                        }
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
                                    //الندوة الربيعية الاولى للجامعة الصيفية العربية الاوربية
                                    else if (words.Count > 2 && (words[2].pos == "adj_num" && words[2].prc0 == "Al_det"))
                                    {
                                        if (words.Count >= 3)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep"))
                                            {
                                                if (words.Count >= 4)
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
                                                    {

                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 5 - 1] += "</font>";

                                                    }
                                                }
                                                else if (words.Count >= 3)
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
                                    else if (words.Count > 2 && (words[2].bw == "fiy/PREP" || words[2].bw == "Ean/PREP"))
                                    {
                                        if (words.Count >= 4)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[4].pos == "adj" && words[4].prc0 == "Al_det"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                                }

                                   //مؤتمر احياء الاعياد الاولمبية القديمة
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun"))
                                {
                                    if (words.Count >= 3)
                                    {

                                        if (words.Count > 2 && (words[2].pos == "noun" && words[2].num == "s" && words[1].num == "s"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "adj" && words[3].num == "s" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].num == "s" && words[4].prc0 == "Al_det"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
                                                    }

                                                    else
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 4 - 1] += "</font>";
                                                    }

                                                }

                                            //ندوة حول الرواية والقصة الخليجية والكويتية
                                                else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc2 == "wa_conj"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "adj"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
                                                        else if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 4 - 1] += "</font>";
                                                    }

                                                }
                                                else if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
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
                                        else if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";

                                        }
                                        //ندوة جامعة عدن
                                        else if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                //ندوة رواق عوشة بنت حسين
                                                if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
                                                    }
                                                    else if (words.Count > 4 && (words[4].pos != "noun_prop"))
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
                                        }
                                        //ندوة " العرب في الاعلام الغربي "
                                        //الندوة العلمية عن الادب العربي الحديث #### الحديث adj
                                        else if (words.Count > 2 && (words[2].bw == "fiy/PREP" || words[2].bw == "Ean/PREP"))
                                        {
                                            if (words.Count > 4)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[4].pos == "adj" && words[4].prc0 == "Al_det"))
                                                {

                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 5 - 1] += "</font>";
                                                }
                                                else
                                                {

                                                }
                                            }
                                        }


                                        else if (words.Count >= 4)
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                    }
                                }
                            }
                        }

                    }
                    #endregion
                    #region Event Incident
                    //Incident
                    else if (triggerWordTag == "neei".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "هَزِيمَة_1" || words[0].lemma == "مُوَقِّع_1" || words[0].lemma == "نَكْبَة_1" || words[0].lemma == "غَزْو_1" || words[0].lemma == "زِلْزال_1" || words[0].lemma == "حَرْب_1" || words[0].lemma == "صُلْح_1" || words[0].lemma == "مَعْرَكَة_1" || words[0].lemma == "كارِثَة_1" || words[0].lemma == "حَمْلَة_1" || words[0].lemma == "ٱِنْتِفاضَة_1" || words[0].lemma == "ثَوْرَة_1" || words[0].lemma == "نَكْسَة_1" || words[0].lemma == "عُدْوان_1" || words[0].lemma == "فَتَح-َ_1"))
                        {
                            if (words.Count >= 2)
                            {
                                ///####معركة دي لا جونا دي لا خاندا في 
                                ///#####معركة سانور في 
                                ///<tokenized scheme="ATB">
                                ////<tok id="0" form0="س+"/>
                                /////<tok id="1" form0="انور"/>
                                ////// </tokenized>
                                #region
                                if (words.Count > 0 && ((words[0].pos == "noun" || words[0].pos == "verb") && words[1].pos == "noun_prop"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "adj_num"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "digit"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                    {
                                        if (words.Count > 2 && (words[2].gloss == "bin" || words[2].gloss == "Ibn" || words[2].gloss == "Abu"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                            else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                            else if (words.Count > 3 && (words[3].pos != "noun_prop"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                        else if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                    else if (words.Count > 2 && (words[2].pos == "prep" && words[2].gloss == "from"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun" && words[3].gloss == "year"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "digit"))
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
                                    else if (words.Count > 2 && (words[2].pos == "noun" && words[2].gloss == "year"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "digit"))
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
                                    else if (words.Count > 2 && (words[2].pos == "noun"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
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
                                    else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
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
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "adj"))
                                {

                                    if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                    else if (words.Count > 2 && (words[2].pos == "adj_num" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "noun_prop" && words[2].prc1 == "li_prep"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "prep" && words[2].gloss == "on;above"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                        else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                                    else if (words.Count > 2 && (words[2].pos == "adj_num"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        //diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        //diacLineWords[loc + 2 - 1] += "</font>";
                                    }

                                }
                                #endregion
                                #region
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "digit"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun" && words[3].gloss == "year"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "digit"))
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
                                        else if (words.Count > 3 && (words[3].pos == "digit"))
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
                                else if (words.Count > 0 && ((words[0].pos == "noun" || words[0].pos == "verb") && words[1].pos == "noun") && words[1].prc1 != "li_prep")
                                {
                                    if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                        {
                                            if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                                    else if (words.Count > 2 && (words[2].pos == "noun"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                        else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                        {
                                            if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                                    else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                    {
                                        if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                                else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "adj_num" || words[1].pos == "noun_num") && words[1].prc1 != "li_prep")
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun" && words[2].gloss == "year"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "digit"))
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
                                    else if (words.Count > 2 && (words[2].pos == "digit"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].gloss == "from" && words[2].pos == "prep"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "digit"))
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
                                else if (words.Count > 0 && (words[0].pos == "noun" && (words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop"))
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
                                    else if (words.Count > 2 && (words[2].pos == "digit"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "adj_num"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "adj_num"))
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
                                    else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                        else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                                        //diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        //diacLineWords[loc + 2 - 1] += "</font>";
                                    }

                                }
                            }
                        }
                    }
                    #endregion
                    #region Event Other
                    //Other
                    else if (triggerWordTag == "neeo".ToUpper())
                    {
                        if (words.Count >= 2)
                        {

                            #region
                            if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj_num"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "digit"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 2 && (words[2].gloss == "bin" || words[2].gloss == "Ibn" || words[2].gloss == "Abu"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                        else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                        else if (words.Count > 3 && (words[3].pos != "noun_prop"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                    else if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {

                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                else if (words.Count > 2 && (words[2].pos == "prep" && words[2].gloss == "from"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" && words[3].gloss == "year"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "digit"))
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
                                else if (words.Count > 2 && (words[2].pos == "noun" && words[2].gloss == "year"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "digit"))
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
                                else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
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
                                else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
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
                            else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "adj"))
                            {

                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                                else if (words.Count > 2 && (words[2].pos == "adj_num" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun_prop" && words[2].prc1 == "li_prep"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "prep" && words[2].gloss == "on;above"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                                else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
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
                            else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "digit"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" && words[3].gloss == "year"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "digit"))
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
                                    else if (words.Count > 3 && (words[3].pos == "digit"))
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
                            else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "noun"))
                            {
                                if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                    {
                                        if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                                else if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                                else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                            else if (words.Count > 0 && (words[0].pos == "noun" && words[1].pos == "adj_num" || words[1].pos == "noun_num"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun" && words[2].gloss == "year"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "digit"))
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
                                else if (words.Count > 2 && (words[2].pos == "digit"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].gloss == "from" && words[2].pos == "prep"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "digit"))
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
                            else if (words.Count > 0 && (words[0].pos == "noun" && (words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
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
                                else if (words.Count > 2 && (words[2].pos == "digit"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "adj_num"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj_num"))
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
                                else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
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
                    }
                    #endregion
                    #endregion
                    #region 12-Person
                    //Person tagging
                    else if (triggerWordTag == "nep".ToUpper())
                    {


                        if (words.Count > 2 && (words[0].lemma == "طِفْل_1" && words[1].lemma == "هُما_1" && words[2].pos == "punc"))
                        {
                            if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[2].pos == "adj_num"))
                            {
                                if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[2].pos == "adj_num"))
                                {
                                    diacLineWords[loc + 3] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 3];
                                    diacLineWords[loc + 5 - 1] += "</font>";
                                }
                                else
                                {// move to next word add 1 to the location
                                    diacLineWords[loc + 3] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 3];
                                    diacLineWords[loc + 4 - 1] += "</font>";

                                }

                            }
                            else
                            {
                                diacLineWords[loc + 3] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 3];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                        }
                        else if (words.Count > 1 && ((words[0].lemma == "أَشاع_1" && words[1].lemma == "عَلَى_1") || (words[0].lemma == "رِزْق_1" && words[1].lemma == "ٱِبْن_1") || (words[0].lemma == "كان-ُ_1" && words[1].lemma == "مُؤَسِّس_1") || (words[0].lemma == "إِفْراج_1" && words[1].lemma == "عَن_1") || (words[0].lemma == "أَطْلَق_1" && words[1].lemma == "سَراح_1") || (words[0].lemma == "لِدَة_2" && words[1].lemma == "هُما_1")))
                        {
                            if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj_num"))
                            {
                                if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[2].pos == "adj_num"))
                                {
                                    if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[2].pos == "adj_num"))
                                    {
                                        diacLineWords[loc + 2] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 2];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {// move to next word add 1 to the location
                                        diacLineWords[loc + 2] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 2];
                                        diacLineWords[loc + 4 - 1] += "</font>";

                                    }

                                }
                                else
                                {
                                    diacLineWords[loc + 2] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 2];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }
                            else
                            {
                                //diacLineWords[loc + 2] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 2];
                                //diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        if (words.Count > 0 && (words[0].lemma == "أُسْلُوب_1" || words[0].lemma == "ٱِلْتَحَق_1" || words[0].lemma == "عاد-ُ_1" || words[0].lemma == "كَتَب-ُ_1" || words[0].lemma == "يا_1" || words[0].lemma == "قال-ُ_1" || words[0].lemma == "لَحَن-َ_1" || words[0].lemma == "أَب_1" || words[0].lemma == "ٱِعْتَكَف_1" || words[0].lemma == "ٱِنْتَقَل_1" || words[0].lemma == "سِجْن_1" || words[0].lemma == "تَوَلِّي_1" || words[0].lemma == "وَلَد_1" || words[0].lemma == "كان-ُ_1" || words[0].lemma == "تَوَلَّى_1" || words[0].lemma == "وَلَد-ِ_1" || words[0].lemma == "والِد_1" || words[0].lemma == "ٱِسْم_1" || words[0].lemma == "تَوَفَّى_1" || words[0].lemma == "هُوَ_1" || words[0].lemma == "أَبِي_1" || words[0].lemma == "أَبُو_1"))
                        {
                            if (words.Count > 1 && ((words[1].pos == "noun_prop") && words[1].prc1 != "bi_prep"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "noun_prop" && words[2].prc1 == "0" && words[2].prc1 == "0") || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj_num" || words[2].gloss == "daughter;girl"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font")))
                                        {
                                            var loc2 = 0;
                                            var sentence2 = string.Empty;
                                            for (var z = locations.Count - 1; z >= 0; z--)
                                            {
                                                loc2 = loc + 5;
                                                // get 5 words after that location 
                                                var sentenceWordsCount2 = diacLineWords.Count();
                                                var remainingWords2 = sentenceWordsCount2 - loc2;



                                                if (remainingWords2 > 5)
                                                {
                                                    for (int Index = loc2; Index < loc2 + 5; Index++)
                                                    {
                                                        sentence2 += (diacLineWords[Index] + " ");
                                                    }
                                                }
                                                else
                                                {
                                                    for (int Index = loc2; Index < sentenceWordsCount2; Index++)
                                                    {
                                                        sentence2 += (diacLineWords[Index] + " ");
                                                    }

                                                }
                                            }



                                            sentence2 = sentence2.Replace("<", "");
                                            sentence2 = sentence2.Replace(">", "");
                                            var words2 = BL.MadaMiraHandler.Analyse(sentence2);

                                            if (words2.Count > 0 && (words2[0].pos == "noun_prop" || words2[0].pos == ""))
                                            {
                                                if (words2.Count > 1 && (words2[1].pos == "noun_prop" || (words2[1].pos == "" && words2[1].word != "font")))
                                                {
                                                    if (words2.Count > 2 && (words2[2].pos == "noun_prop" || (words2[2].pos == "" && words2[2].word != "font")))
                                                    {
                                                        if (words2.Count > 3 && (words2[3].pos == "noun_prop" || words2[3].pos == ""))
                                                        {
                                                            if (words2.Count > 4 && (words2[4].pos == "noun_prop" || (words2[4].pos == "" && words2[4].word != "font")))
                                                            {
                                                                diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                                                diacLineWords[loc2 + 4] += "</font>";
                                                            }
                                                            else
                                                            {
                                                                diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                                                diacLineWords[loc2 + 3] += "</font>";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                                            diacLineWords[loc2 + 2] += "</font>";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                                        diacLineWords[loc2 + 1] += "</font>";
                                                    }
                                                }
                                                else
                                                {
                                                    diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                                    diacLineWords[loc + 5 - 1] += "</font>";
                                                }
                                            }




                                            else
                                            {
                                                diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                        }
                                        else
                                        {// move to next word add 1 to the location
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 4 - 1] += "</font>";

                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else
                                {
                                    diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 1 && (((words[1].pos == "noun_prop") || (words[1].pos == "" && words[1].word != "font")) && (words[1].prc1 == "bi_prep" || words[1].prc1 == "") && words[0].lemma == "وَلَد-ِ_1"))
                            {

                                var CountryTagId = 16;
                                var Country = Tags.GetTag(CountryTagId);
                                var CountryTag = Country.Tag;
                                var CountryColor = Country.Color;
                                diacLineWords[loc + 1] = "<font title='Trigger word-" + CountryTag.ToUpper() + "' style='color:" + CountryColor + "'>" + diacLineWords[loc + 1];
                                diacLineWords[loc + 1] += "</font>";
                            
                            }
                            else if (words.Count > 1 && ((words[1].pos == "noun_prop" && words[1].prc1 == "0" && words[1].prc2 == "0") || (words[1].pos == "noun" && words[1].prc1 == "0" && words[1].prc2 == "0" && words[1].prc3 == "0")) && (words[0].lemma == "أَبِي_1" || words[0].lemma == "أَبُو_1"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "noun_prop" && words[1].prc1 == "0" && words[2].prc1 == "0") || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj_num"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
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
                    }

                    #endregion
                    #region 2-Numex
                    //Percent and Ordinal Tagged through the Fixed tagging table
                    else if (triggerWordTag == "neup".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مِئَة_1" || words[0].lemma == "%_0"))
                        {

                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";

                        }

                    }
                    else if (triggerWordTag == "neuo".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].pos == "adj_num" || words[0].gloss == "first" || words[0].gloss == "second;next" || words[0].gloss == "third" || words[0].gloss == "fourth"))
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }


                    }
                    //Measurments Temperature                  
                    //else if (triggerWordTag == "neumt".ToUpper())
                    //{
                    //    ///do nothing
                    //}
                    ////Measurments Speed
                    //else if (triggerWordTag == "neums".ToUpper())
                    //{
                    //    ///do nothing
                    //}
                    #region Space
                    //Measurments Space tagging
                    else if (triggerWordTag == "neuma".ToUpper())
                    {
                        if (words.Count >= 2)
                        {
                            var percedingSentence = string.Empty;
                            var percedingLocation = loc <= 5 ? 0 : loc - 5;


                            for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                            {
                                percedingSentence += (diacLineWords[perItemIndex] + " ");
                            }
                            percedingSentence = percedingSentence.Replace("<", "");
                            percedingSentence = percedingSentence.Replace(">", "");
                            var percWords = new List<WordInfoItem>();
                            if (percedingSentence != string.Empty)
                            {

                                percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                                percWords.Reverse();
                            }

                            if (percWords.Count > 0 && (percWords[0].pos == "digit"))
                            {
                                if (words.Count > 0 && (words[0].gloss == "inclination;tendency;sympathy" || words[0].gloss == "(A.D.);M;13th;meter" || words[0].gloss == "hectare" || words[0].gloss == "feddan_(4.2_sq.m.,_Ar.Eg.Sud.;_5.7_sq.m_Lev.)"))
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                                else if (words.Count > 0 && (words[0].gloss == "kilometer(s)" || words[0].gloss == "how" || words[0].gloss == "kilo;kilogram" || words[0].gloss == "centimeter"))
                                {
                                    if (words.Count > 1 && (words[1].gloss == "square;quadruple;tetragonal" || words[1].gloss == "2"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }


                                }
                            }
                        }
                        else if (words.Count < 2)
                        {
                            var percedingSentence = string.Empty;
                            var percedingLocation = loc <= 5 ? 0 : loc - 5;


                            for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                            {
                                percedingSentence += (diacLineWords[perItemIndex] + " ");
                            }
                            percedingSentence = percedingSentence.Replace("<", "");
                            percedingSentence = percedingSentence.Replace(">", "");
                            var percWords = new List<WordInfoItem>();
                            if (percedingSentence != string.Empty)
                            {
                                percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                                percWords.Reverse();
                            }
                            if (percWords[0].pos == "digit")
                            {
                                if (words.Count > 0 && (words[0].gloss == "inclination;tendency;sympathy" || words[0].gloss == "(A.D.);M;13th;meter" || words[0].gloss == "hectare" || words[0].gloss == "feddan_(4.2_sq.m.,_Ar.Eg.Sud.;_5.7_sq.m_Lev.)"))
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                            }
                        }
                    }
                    #endregion
                    #region Weight
                    //Measurments Weight tagging
                    else if (triggerWordTag == "neumw".ToUpper())
                    {
                        var percedingSentence = string.Empty;
                        var percedingLocation = loc <= 5 ? 0 : loc - 5;


                        for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                        {
                            percedingSentence += (diacLineWords[perItemIndex] + " ");
                        }
                        percedingSentence = percedingSentence.Replace("<", "");
                        percedingSentence = percedingSentence.Replace(">", "");
                        var percWords = new List<WordInfoItem>();
                        if (percedingSentence != string.Empty)
                        {
                            percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                            percWords.Reverse();
                        }


                        if (percWords[0].pos == "digit")
                        {
                            if (words.Count > 0 && (words[0].gloss == "ton" || words[0].gloss == "metric;metrical" || words[0].gloss == "kilogram" || words[0].gloss == "gram" || words[0].gloss == "ratl_(weight_measure_=_3_kg)" || words[0].gloss == "ratls_(weight_measure_=_3_kg)" || words[0].gloss == "ounce" || words[0].gloss == "qantar_(weight_measure)" || words[0].gloss == "kilo;kilogram" || words[0].gloss == "tons" || words[0].gloss == "Pound"))
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }
                        }

                    }
                    #endregion
                    ////Measurments Volume tagging
                    //else if (triggerWordTag == "neumv".ToUpper())
                    //{
                    //    ///do nothing
                    //}
                    #region Extent
                    //Measurments Extent tagging
                    else if (triggerWordTag == "neume".ToUpper())
                    {
                        if (words.Count >= 2)
                        {
                            var percedingSentence = string.Empty;
                            var percedingLocation = loc <= 5 ? 0 : loc - 5;


                            for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                            {
                                percedingSentence += (diacLineWords[perItemIndex] + " ");
                            }
                            percedingSentence = percedingSentence.Replace("<", "");
                            percedingSentence = percedingSentence.Replace(">", "");
                            var percWords = new List<WordInfoItem>();
                            if (percedingSentence != string.Empty)
                            {
                                percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                                percWords.Reverse();
                            }
                            if (percWords[0].pos == "digit")
                            {
                                if (words.Count > 0 && (words[0].gloss == "millimeter"
|| words[0].gloss == "inclination;tendency;sympathy"
|| words[0].gloss == "nanometer"
|| words[0].gloss == "inclination;tendencies;sympathies"
|| words[0].gloss == "meter"
|| words[0].gloss == "centimeter"
|| words[0].gloss == "inch"
|| words[0].gloss == "kilometer(s)"
|| words[0].gloss == "(A.D.);M;13th;meter"
|| words[0].gloss == "inches"
|| words[0].gloss == "yard"
|| words[0].gloss == "yokes_of_oxen"
))
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                                else if (words.Count > 0 && (words[0].gloss == "kilo;kilogram"))
                                {
                                    if (words.Count > 1 && (words[1].gloss == "meter"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }


                                }
                            }
                        }
                        else if (words.Count < 2)
                        {
                            var percedingSentence = string.Empty;
                            var percedingLocation = loc <= 5 ? 0 : loc - 5;


                            for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                            {
                                percedingSentence += (diacLineWords[perItemIndex] + " ");
                            }
                            percedingSentence = percedingSentence.Replace("<", "");
                            percedingSentence = percedingSentence.Replace(">", "");
                            var percWords = new List<WordInfoItem>();
                            if (percedingSentence != string.Empty)
                            {
                                percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                                percWords.Reverse();
                            }
                            if (percWords[0].pos == "digit")
                            {
                                if (words.Count > 0 && (words[0].gloss == "millimeter" || words[0].gloss == "inclination;tendency;sympathy" || words[0].gloss == "nanometer" || words[0].gloss == "inclination;tendencies;sympathies" || words[0].gloss == "meter" || words[0].gloss == "centimeter" || words[0].gloss == "inch" || words[0].gloss == "kilometer(s)" || words[0].gloss == "(A.D.);M;13th;meter" || words[0].gloss == "inches" || words[0].gloss == "yard" || words[0].gloss == "yokes_of_oxen"))
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                            }
                        }

                    }
                    #endregion
                    ////Measurments other tagging
                    //else if (triggerWordTag == "neumo".ToUpper())
                    //{

                    //}
                    #endregion

                    #region 4-Disease
                    //Disease tagging
                    else if (triggerWordTag == "ned".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].pos == "noun"))
                        {
                            if (words.Count > 0 && (words[0].gloss == "disease;epidemic" || words[0].gloss == "disease;illness"))
                            {
                                if (words.Count == 2)
                                {
                                    ///وباء السكري
                                    if (words.Count > 1 && (words[1].pos == "noun" && words[1].prc1 != "bi_prep" && words[1].prc2 != "wa_conj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                            }
                            //التلقيح ضد الجدري
                            else if (words.Count > 0 && (words[0].gloss == "inoculation;pollination;impregnation" && words[1].gloss == "contrary;against;opposed;anti-;counter-"))
                            {
                                if (words.Count >= 3)
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                            //أمراض مستعصيه مثل الحمى والملاريا والجدري

                            else if (words.Count > 0 && (words[0].gloss == "diseases;illnesses"))
                            {
                                if (words.Count > 1 && (words[1].gloss == "difficult;incurable" && words[1].pos == "adj" && words[1].prc1 != "bi_prep" && words[1].prc2!="wa_conj"))
                                {
                                    if (words.Count > 2 && (words[2].gloss == "like;such_as" && words[2].pos == "noun"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun" || words[3].pos == "adj"))
                                        {
                                            if (words.Count > 4 && (words[4].prc2 == "wa_conj"))
                                            {
                                                if (words.Count >= 3)
                                                {

                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 5 - 1] += "</font>";

                                                }
                                            }
                                            else
                                            {
                                                if (words.Count >= 3)
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 4 - 1] += "</font>";
                                                }
                                            }


                                        }
                                        else
                                        { }
                                    }
                                    else { }
                                }
                                //أمراض  مثل الحمى والملاريا والجدري
                                else if (words.Count > 1 && (words[1].gloss == "like;such_as" && words[1].pos == "noun" && words[1].prc1 != "bi_prep" && words[1].prc2 != "wa_conj"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun" || words[2].pos == "adj"))
                                    {
                                        if (words.Count >= 3)
                                        {
                                            if (words.Count > 3 && (words[3].prc2 == "wa_conj"))
                                            {
                                                if (words.Count >= 3)
                                                {

                                                    if (words.Count > 4 && (words[4].prc2 == "wa_conj"))
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


                            }
                            ///السكري
                            else if (words.Count > 0 && (words[0].gloss != "disease;epidemic" || words[0].gloss != "disease;illness"))
                            {
                                if (words.Count >= 1)
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                            }


                        }
                        ///اصيب بالملاريا
                        ////انتهكته الحمي
                        /// i neeed to select the second word only

                        else if (words.Count > 0 && (words[0].pos == "verb"))
                        {
                            if (words.Count > 0 && (words[0].per == "3" && words[1].pos == "noun" && words[1].prc0 == "Al_det"))
                            {
                                if (words.Count >= 2)
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 0 && (words[0].per == "3" && words[1].pos == "noun" && words[1].prc1 == "bi_prep"))
                            {
                                if (words.Count >= 2)
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            ///توفي اثر ازمة قلبية
                            ///we still need to tag the last two words
                            else if (words.Count > 0 && (words[0].per == "3" && words[1].gloss == "right_after"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun" || words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" || words[3].pos == "adj"))
                                    {
                                        if (words.Count >= 3)
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        if (words.Count >= 2)
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
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
                    #region chemical
                    //Chemical Tagging
                    else if (triggerWordTag == "nenc".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "حَمْض_1" || words[0].lemma == "حَجَر_1" || words[0].lemma == "مِلْح_1" || words[0].lemma == "أُكْسِيد_1" || (words[0].lemma == "غاز_1" && words[0].prc0 == "Al_det") || words[0].lemma == "عُنْصُر_1") )
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun_prop" ||(words[1].pos == "noun" && words[1].prc0 == "Al_det")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else { }
                            }
                        }
                        else if (words.Count > 0 && (words[0].lemma == "حَدِيد_1" || words[0].lemma == "نَفْط_1" || words[0].lemma == "رُخام_1" || words[0].lemma == "نَحّاس_1" || words[0].lemma == ""))
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }

                    }
                    #endregion
                    //Living thing
                    /////Animal Tagging
                    //else if (triggerWordTag == "nenla".ToUpper())
                    //{
                    //    //Do nothing
                    //}
                    /////Bird Tagging
                    //else if (triggerWordTag == "nenlb".ToUpper())
                    //{
                    //    //Do nothing
                    //}
                    #region Insect
                    ///Insect Tagging
                    else if (triggerWordTag == "nenli".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "ذُباب_1" || words[0].lemma == "نَمْل_1" || words[0].lemma == "دُود_1"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && (words[1].pos == "noun" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";

                                }
                            }
                            else if (words.Count >= 3)
                            {

                                if (words.Count > 1 && (words[1].pos == "abbrev" && words[2].pos == "abbrev"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }

                        }


                    }
                    #endregion
                    #region Plant
                    ///Plant Tagging

                    else if (triggerWordTag == "nenlp".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "شَجَرَة_1" || words[0].lemma == "نَبْتَة_1" || words[0].lemma == "عُشْب_1" || words[0].lemma == "نَبات_1" || words[0].lemma == "خَشَب_1"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && (words[1].pos == "noun" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";

                                }
                                else if (words[1].pos == "noun_prop")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words.Count >= 3)
                            {
                                if (words.Count > 1 && (words[1].pos == "noun" && words[2].prc0 == "Al_det" && words[2].pos == "noun"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";

                                }
                            }

                        }

                    }
                    #endregion
                    /////Micro-Organism Tagging
                    //else if (triggerWordTag == "nenlm".ToUpper())
                    //{
                    //    //Do nothing
                    //}
                    /////Other Tagging
                    //else if (triggerWordTag == "nenlo".ToUpper())
                    //{
                    //    //Do nothing
                    //}
                    #endregion
                    #region 11-Title
                    //Title tagging
                    ///لازم اشوف وراها title تاني ولا لا 
                    ///لازم اشوف وراها nationality

                    else if (triggerWordTag == "nei".ToUpper())
                    {
                        if (words.Count >= 2)
                        {
                            if (words.Count > 0 && (words[0].lemma == "دُكْتُور_1"
                                || words[0].lemma == "خَلِيفَة_1"
                                || words[0].lemma == "شَيْخ_1"
                                || words[0].lemma == "مَلِك_2"
                                || words[0].lemma == "شَيْخ_1"
                                || words[0].lemma == "د_1"
                                || words[1].lemma == "._0"
                                || words[0].lemma == "مُلْك_1"
                                || words[0].lemma == "سَيِّد_2"
                                || words[0].lemma == "مُهَنْدِس_1"
                                || words[0].lemma == "زَعِيم_1"
                                || words[0].lemma == "رَئِيس_1"
                                || words[0].lemma == "فَرِيق_1"
                                || words[0].lemma == "طَيّار_1"
                                || words[0].lemma == "مُلازِم_1"
                                || words[0].lemma == "لِواء_1"
                                || words[0].lemma == "فَقِيه_1"
                                || words[0].lemma == "مُفَسِّر_1"
                                || words[0].lemma == "عَلّام_1"
                                || words[0].lemma == "شَرِيف_2"
                                || words[0].lemma == "أَمِير_1"
                                || words[0].lemma == "سُلْطان_1"
                                || words[0].lemma == "حاجّ_2"
                                || words[0].lemma == "عالَم_1"
                                || words[0].lemma == "برُوفِسُور_1"
                                || words[0].lemma == "ناقَد_1"
                                || words[0].lemma == "نَبِيّ_1"
                                || words[0].lemma == "فارِس_1"
                                || words[0].lemma == "جِنِرال_1"
                                || words[0].lemma == "قائِد_1"
                                || words[0].lemma == "كُولُونِيل_1"
                                || words[0].lemma == "مُؤَرِّخ_1"
                                || words[0].lemma == "كَوَّن_1"
                                || words[0].lemma == "سائِق_1"
                                || words[0].lemma == "عَقِيد_1"
                                || words[0].lemma == "خُدَيْوِيّ_1"
                                || words[0].lemma == "أَفَنْدِيّ_1"
                                || words[0].lemma == "مُوسِيقار_1"
                                || words[0].lemma == "خَبِير_1"
                                || words[0].lemma == "مُؤَلَّف_2"
                                || words[0].lemma == "إِمْبِراطُور_1"
                                || words[0].lemma == "قُنْصُل_1"
                                || words[0].lemma == "نَحّات_1"
                                || words[0].lemma == "والِي_1"
                                || words[0].lemma == "قاضِي_1"
                                || words[0].lemma == "نائِب_1"
                                || words[0].lemma == "قِدِّيس_1"
                                || words[0].lemma == "مُشِير_2"
                                || words[0].lemma == "مُمَثِّل_1"
                                || words[0].lemma == "مُطْرِب_1"
                                || words[0].lemma == "شاعِر_1"
                                || words[0].lemma == "لَيّ_1"
                                || words[0].lemma == "فَرِيد_1"
                                || words[0].lemma == "وَزِير_1"
                                || words[0].lemma == "سِناتُور_1"
                                || words[0].lemma == "مَمْلُوك_2"
                                || words[0].lemma == "مُفْتِي_1"
                                || words[0].lemma == "باي_1"
                                || words[0].lemma == "شَهِيد_1"
                                || words[0].lemma == "خَطِيب_1"
                                || words[0].lemma == "مُدَرِّب_1"
                                || words[0].lemma == "مُفَكِّر_1"
                                || words[0].lemma == "أَدِيب_2"
                                || words[0].lemma == "داعِي_3"
                                || words[0].lemma == "حافِظ_2"
                                || words[0].lemma == "مُقْرِئ_1"
                                || words[0].lemma == "مُجاهِد_1"
                                || words[0].lemma == "مُفَكِّر_1"
                                || words[0].lemma == "رَبّاع_1"
                                || words[0].lemma == "كابْتِن_1"
                                || words[0].lemma == "عاهِل_1"
                                || words[0].lemma == "مُعَلَّق_1"
                                || (words[0].lemma == "صُحُفِيّ_1" && words[0].gloss != "journalistic;press;newspaper")
                                || words[0].lemma == "عَدّاء_1"
                                || words[0].lemma == "حارِس_1"
                                || words[0].lemma == "خَلِيفَة_2"
                                || words[0].lemma == "أُسْتاذ_1" || words[0].lemma == "سَيِّد_1" || words[0].lemma == "قَسّ_1" || words[0].lemma == "سَيِّدَة_1" || words[0].lemma == "إِمام_1"))
                            {
                                if (words.Count > 1 && (words[1].lemma == "دُكْتُور_1"
                                || words[1].lemma == "خَلِيفَة_1"
                                || words[1].lemma == "شَيْخ_1"
                                || words[1].lemma == "د_1"
                                || words[1].lemma == "._1"
                                || words[1].lemma == ":_0"
                                || words[1].lemma == "مُلْك_1"
                                || words[1].lemma == "سَيِّد_2"
                                || words[1].lemma == "مُهَنْدِس_1"
                                || words[1].lemma == "زَعِيم_1"
                                || words[1].lemma == "رَئِيس_1"
                                || words[1].lemma == "فَرِيق_1"
                                || words[1].lemma == "مَلِك_2"
                                || words[1].lemma == "طَيّار_1"
                                || words[1].lemma == "مُلازِم_1"
                                || words[1].lemma == "لِواء_1"
                                || words[1].lemma == "فَقِيه_1"
                                || words[1].lemma == "مُفَسِّر_1"
                                || words[1].lemma == "عَلّام_1"
                                || words[1].lemma == "شَرِيف_2"
                                || words[1].lemma == "أَمِير_1"
                                || words[1].lemma == "سُلْطان_1"
                                || words[1].lemma == "حاجّ_2"
                                || words[1].lemma == "عالَم_1"
                                || words[1].lemma == "برُوفِسُور_1"
                                || words[1].lemma == "ناقَد_1"
                                || words[1].lemma == "نَبِيّ_1"
                                || words[1].lemma == "فارِس_1"
                                || words[1].lemma == "جِنِرال_1"
                                || words[1].lemma == "قائِد_1"
                                || words[1].lemma == "كُولُونِيل_1"
                                || words[1].lemma == "مُؤَرِّخ_1"
                                || words[1].lemma == "كَوَّن_1"
                                || words[1].lemma == "سائِق_1"
                                || words[1].lemma == "عَقِيد_1"
                                || words[1].lemma == "خُدَيْوِيّ_1"
                                || words[1].lemma == "أَفَنْدِيّ_1"
                                || words[1].lemma == "مُوسِيقار_1"
                                || words[1].lemma == "خَبِير_1"
                                || words[1].lemma == "مُؤَلَّف_2"
                                || words[1].lemma == "إِمْبِراطُور_1"
                                || words[1].lemma == "قُنْصُل_1"
                                || words[1].lemma == "نَحّات_1"
                                || words[1].lemma == "والِي_1"
                                || words[1].lemma == "قاضِي_1"
                                || words[1].lemma == "نائِب_1"
                                || words[1].lemma == "قِدِّيس_1"
                                || words[1].lemma == "مُشِير_2"
                                || words[1].lemma == "مُمَثِّل_1"
                                || words[1].lemma == "مُطْرِب_1"
                                || words[1].lemma == "شاعِر_1"
                                || words[1].lemma == "لَيّ_1"
                                || words[1].lemma == "فَرِيد_1"
                                || words[1].lemma == "وَزِير_1"
                                || words[1].lemma == "سِناتُور_1"
                                || words[1].lemma == "مَمْلُوك_2"
                                || words[1].lemma == "مُفْتِي_1"
                                || words[1].lemma == "باي_1"
                                || words[1].lemma == "شَهِيد_1"
                                || words[1].lemma == "خَطِيب_1"
                                || words[1].lemma == "مُدَرِّب_1"
                                || words[1].lemma == "مُفَكِّر_1"
                                || words[1].lemma == "أَدِيب_2"
                                || words[1].lemma == "داعِي_3"
                                || words[1].lemma == "حافِظ_2"
                                || words[1].lemma == "مُقْرِئ_1"
                                || words[1].lemma == "مُجاهِد_1"
                                || words[1].lemma == "مُفَكِّر_1"
                                || words[1].lemma == "رَبّاع_1"
                                || words[1].lemma == "كابْتِن_1"
                                || words[1].lemma == "عاهِل_1"
                                || words[1].lemma == "مُعَلَّق_1"
                                || (words[1].lemma == "صُحُفِيّ_1" && words[0].gloss == "journalistic;press;newspaper")
                                || words[1].lemma == "عَدّاء_1"
                                || words[1].lemma == "حارِس_1"
                                    || words[1].lemma == "خَلِيفَة_2"
                                    || words[1].lemma == "أُسْتاذ_1" || words[1].lemma == "سَيِّد_1" || words[1].lemma == "قَسّ_1" || words[1].lemma == "سَيِّدَة_1" || words[0].lemma == "إِمام_1"))
                                {

                                    if (words.Count > 2 && (words[2].lemma.Contains("يّ_1") || words[2].lemma.Contains("يّ_2") || words[2].lemma.Contains("يّ_3")))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")) && (words[3].prc1 != "bi_prep"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 1 - 1] += "</font>";
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                            var NationalTagId = 111;
                                            var National = Tags.GetTag(NationalTagId);
                                            var Nationaltag = National.Tag;
                                            var NationalColor = National.Color;
                                            diacLineWords[loc + 2] = "<font title='Trigger word-" + Nationaltag.ToUpper() + "' style='color:" + NationalColor + "'>" + diacLineWords[loc + 2];
                                            diacLineWords[loc + 2] += "</font>";
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
                                            {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                diacLineWords[loc + 4] += "</font>";
                                            }
                                            else
                                            {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                diacLineWords[loc + 1] += "</font>";
                                            }

                                        }
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")) && (words[2].prc1 != "bi_prep"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";
                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                        diacLineWords[loc + 2 - 1] += "</font>";

                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")) && (words[3].prc1 != "bi_prep"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")) && (words[4].prc1 != "bi_prep"))
                                            {
                                                var loc2 = 0;
                                                 var sentence2 = string.Empty;
                                                for (var z = locations.Count - 1; z >= 0; z--)
                                                {
                                                     loc2 = loc + 5;
                                                   
                                                    // get 5 words after that location 
                                                    var sentenceWordsCount2 = diacLineWords.Count();
                                                    var remainingWords2 = sentenceWordsCount2 - loc2;



                                                    if (remainingWords2 > 5)
                                                    {
                                                        for (int Index = loc2; Index < loc2 + 5; Index++)
                                                        {
                                                            sentence2 += (diacLineWords[Index] + " ");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int Index = loc2; Index < sentenceWordsCount2; Index++)
                                                        {
                                                            sentence2 += (diacLineWords[Index] + " ");
                                                        }

                                                    }
                                                }



                                                sentence2 = sentence2.Replace("<", "");
                                                sentence2 = sentence2.Replace(">", "");
                                                var words2 = BL.MadaMiraHandler.Analyse(sentence2);

                                                if (words2.Count > 0 && (words2[0].pos == "noun_prop" || words2[0].pos == "") && (words2[0].prc1 != "bi_prep"))
                                                {
                                                    if (words2.Count > 1 && (words2[1].pos == "noun_prop" || (words2[1].pos == "" && words2[1].word != "font")) && (words2[1].prc1 != "bi_prep"))
                                                    {
                                                        if (words2.Count > 2 && (words2[2].pos == "noun_prop" || (words2[2].pos == "" && words2[2].word != "font")) && (words2[2].prc1 != "bi_prep"))
                                                        {
                                                            if (words2.Count > 3 && (words2[3].pos == "noun_prop" || words2[3].pos == "") && (words2[3].prc1 != "bi_prep"))
                                                            {
                                                                var personTagId = 1;
                                                                var person = Tags.GetTag(personTagId);
                                                                var persontag = person.Tag;
                                                                var personColor = person.Color;
                                                                diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                                diacLineWords[loc2 + 3] += "</font>";


                                                            }
                                                            else
                                                            {
                                                                var personTagId = 1;
                                                                var person = Tags.GetTag(personTagId);
                                                                var persontag = person.Tag;
                                                                var personColor = person.Color;
                                                                diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                                diacLineWords[loc2 + 2] += "</font>";
                                                            }


                                                        }
                                                        else
                                                        {
                                                            var personTagId = 1;
                                                            var person = Tags.GetTag(personTagId);
                                                            var persontag = person.Tag;
                                                            var personColor = person.Color;
                                                            diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                            diacLineWords[loc2 + 1] += "</font>";
                                                        }


                                                    }
                                                    else
                                                    {
                                                        var personTagId = 1;
                                                        var person = Tags.GetTag(personTagId);
                                                        var persontag = person.Tag;
                                                        var personColor = person.Color;
                                                        diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                        diacLineWords[loc2] += "</font>";
                                                    }
                                                }
                                                else
                                                {
                                                    var personTagId = 1;
                                                    var person = Tags.GetTag(personTagId);
                                                    var persontag = person.Tag;
                                                    var personColor = person.Color;
                                                    diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                    diacLineWords[loc + 4] += "</font>";
                                                }
                                            }
                                            else
                                            {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                diacLineWords[loc + 3] += "</font>";

                                            }
                                        }
                                        else
                                        {
                                            var personTagId = 1;
                                            var person = Tags.GetTag(personTagId);
                                            var persontag = person.Tag;
                                            var personColor = person.Color;
                                            diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                            diacLineWords[loc + 2] += "</font>";
                                        }

                                    }
                                    else if (words.Count > 2 && (words[2].lemma == "/_0" || words[2].lemma == "._0"))
                                    {
                                        var loc2 = 0;
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";
                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")) && (words[3].prc1 != "bi_prep"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")) && (words[4].prc1 != "bi_prep"))
                                            {
                                                var sentence2 = string.Empty;
                                                for (var z = locations.Count - 1; z >= 0; z--)
                                                {
                                                    loc2 = loc + 5;
                                                    // get 5 words after that location 
                                                    var sentenceWordsCount2 = diacLineWords.Count();
                                                    var remainingWords2 = sentenceWordsCount2 - loc2;



                                                    if (remainingWords2 > 5)
                                                    {
                                                        for (int Index = loc2; Index < loc2 + 5; Index++)
                                                        {
                                                            sentence2 += (diacLineWords[Index] + " ");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int Index = loc2; Index < sentenceWordsCount2; Index++)
                                                        {
                                                            sentence2 += (diacLineWords[Index] + " ");
                                                        }

                                                    }
                                                }



                                                sentence2 = sentence2.Replace("<", "");
                                                sentence2 = sentence2.Replace(">", "");
                                                var words2 = BL.MadaMiraHandler.Analyse(sentence2);

                                                if (words2.Count > 0 && (words2[0].pos == "noun_prop" || words2[0].pos == "") && (words2[0].prc1 != "bi_prep"))
                                                {
                                                    if (words2.Count > 1 && (words2[1].pos == "noun_prop" || (words2[1].pos == "" && words2[1].word != "font")) && (words2[1].prc1 != "bi_prep"))
                                                    {
                                                        if (words2.Count > 2 && (words2[2].pos == "noun_prop" || (words2[2].pos == "" && words2[2].word != "font")) && (words2[2].prc1 != "bi_prep"))
                                                        {
                                                            if (words2.Count > 3 && (words2[3].pos == "noun_prop" || words2[3].pos == "") && (words2[3].prc1 != "bi_prep"))
                                                            {
                                                                var personTagId = 1;
                                                                var person = Tags.GetTag(personTagId);
                                                                var persontag = person.Tag;
                                                                var personColor = person.Color;
                                                                diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                                diacLineWords[loc2 + 3] += "</font>";


                                                            }
                                                            else
                                                            {
                                                                var personTagId = 1;
                                                                var person = Tags.GetTag(personTagId);
                                                                var persontag = person.Tag;
                                                                var personColor = person.Color;
                                                                diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                                diacLineWords[loc2 + 2] += "</font>";
                                                            }


                                                        }
                                                        else
                                                        {
                                                            var personTagId = 1;
                                                            var person = Tags.GetTag(personTagId);
                                                            var persontag = person.Tag;
                                                            var personColor = person.Color;
                                                            diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                            diacLineWords[loc2 + 1] += "</font>";
                                                        }


                                                    }
                                                    else
                                                    {
                                                        var personTagId = 1;
                                                        var person = Tags.GetTag(personTagId);
                                                        var persontag = person.Tag;
                                                        var personColor = person.Color;
                                                        diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                        diacLineWords[loc2] += "</font>";
                                                    }

                                                }

                                                else
                                                {
                                                    var personTagId = 1;
                                                    var person = Tags.GetTag(personTagId);
                                                    var persontag = person.Tag;
                                                    var personColor = person.Color;
                                                    diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                    diacLineWords[loc + 4] += "</font>";
                                                }
                                            }
                                            else
                                            {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                diacLineWords[loc + 3] += "</font>";

                                            }
                                        }
                                    }
                                    else
                                    {
                                        var personTagId = 1;
                                        var person = Tags.GetTag(personTagId);
                                        var persontag = person.Tag;
                                        var personColor = person.Color;
                                        diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                        diacLineWords[loc + 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 1 && (words[1].lemma.Contains("يّ_1") || words[1].lemma.Contains("يّ_2") || words[1].lemma.Contains("يّ_3")))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")) && (words[2].prc1 != "bi_prep"))
                                    {
                                        var loc2 = 0;
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";
                                        var NationalTagId = 111;
                                        var National = Tags.GetTag(NationalTagId);
                                        var Nationaltag = National.Tag;
                                        var NationalColor = National.Color;
                                        diacLineWords[loc + 1] = "<font title='Trigger word-" + Nationaltag.ToUpper() + "' style='color:" + NationalColor + "'>" + diacLineWords[loc + 1];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")) && (words[3].prc1 != "bi_prep"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "adj_num") && (words[4].prc1 != "bi_prep"))
                                            {

                                                var sentence2 = string.Empty;
                                                for (var z = locations.Count - 1; z >= 0; z--)
                                                {
                                                    loc2 = loc + 5;
                                                    // get 5 words after that location 
                                                    var sentenceWordsCount2 = diacLineWords.Count();
                                                    var remainingWords2 = sentenceWordsCount2 - loc2;



                                                    if (remainingWords2 > 5)
                                                    {
                                                        for (int Index = loc2; Index < loc2 + 5; Index++)
                                                        {
                                                            sentence2 += (diacLineWords[Index] + " ");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int Index = loc2; Index < sentenceWordsCount2; Index++)
                                                        {
                                                            sentence2 += (diacLineWords[Index] + " ");
                                                        }

                                                    }
                                                }



                                                sentence2 = sentence2.Replace("<", "");
                                                sentence2 = sentence2.Replace(">", "");
                                                var words2 = BL.MadaMiraHandler.Analyse(sentence2);

                                                if (words2.Count > 0 && (words2[0].pos == "noun_prop" || words2[0].pos == "") && (words2[0].prc1 != "bi_prep"))
                                                {
                                                    if (words2.Count > 1 && (words2[1].pos == "noun_prop" || (words2[1].pos == "" && words2[1].word != "font")) && (words2[1].prc1 != "bi_prep"))
                                                    {
                                                        if (words2.Count > 2 && (words2[2].pos == "noun_prop" || (words2[2].pos == "" && words2[2].word != "font")) && (words2[2].prc1 != "bi_prep"))
                                                        {
                                                            if (words2.Count > 3 && (words2[3].pos == "noun_prop" || words2[3].pos == "") && (words2[3].prc1 != "bi_prep"))
                                                            {
                                                                var personTagId = 1;
                                                                var person = Tags.GetTag(personTagId);
                                                                var persontag = person.Tag;
                                                                var personColor = person.Color;
                                                                diacLineWords[loc] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc];
                                                                diacLineWords[loc2 + 3] += "</font>";


                                                            }
                                                            else
                                                            {
                                                                var personTagId = 1;
                                                                var person = Tags.GetTag(personTagId);
                                                                var persontag = person.Tag;
                                                                var personColor = person.Color;
                                                                diacLineWords[loc] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc];
                                                                diacLineWords[loc2 + 2] += "</font>";
                                                            }


                                                        }
                                                        else
                                                        {
                                                            var personTagId = 1;
                                                            var person = Tags.GetTag(personTagId);
                                                            var persontag = person.Tag;
                                                            var personColor = person.Color;
                                                            diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 3];
                                                            diacLineWords[loc2 + 1] += "</font>";
                                                        }


                                                    }
                                                    else
                                                    {
                                                        var personTagId = 1;
                                                        var person = Tags.GetTag(personTagId);
                                                        var persontag = person.Tag;
                                                        var personColor = person.Color;
                                                        diacLineWords[loc] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc2] += "</font>";
                                                    }

                                                }
                                                else
                                                {
                                                    var personTagId = 1;
                                                    var person = Tags.GetTag(personTagId);
                                                    var persontag = person.Tag;
                                                    var personColor = person.Color;
                                                    diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                    diacLineWords[loc + 4] += "</font>";
                                                }
                                            }
                                            else
                                            {


                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                diacLineWords[loc + 4 - 1] += "</font>";
                                            }
                                       
                                        }


                                        else
                                        {
                                            var personTagId = 1;
                                            var person = Tags.GetTag(personTagId);
                                            var persontag = person.Tag;
                                            var personColor = person.Color;
                                            diacLineWords[loc + 3] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                            diacLineWords[loc + 2] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        //var personTagId = 1;
                                        //var person = Tags.GetTag(personTagId);
                                        //var persontag = person.Tag;
                                        //var personColor = person.Color;
                                        //diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                        //diacLineWords[loc + 1] += "</font>";
                                    }

                                }
                                else if (words.Count > 1 && (words[1].pos == "noun_prop" || ((words[1].pos == "" && words[1].word != "font") && words[1].word != "font") || words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "adj_num" || words[1].pos == "noun_num") && (words[1].prc1 != "bi_prep" && words[1].prc2!="wa_conj"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj_num" || words[2].pos == "noun_num") && (words[2].prc1 != "bi_prep" && words[2].prc2 != "wa_conj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";

                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "adj_num" || words[3].pos == "noun_num") && (words[3].prc1 != "bi_prep"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "adj_num" || words[4].pos == "noun_num") && (words[3].prc1 != "bi_prep"))
                                            {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 1] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 1];
                                                diacLineWords[loc + 4] += "</font>";
                                            }
                                            else
                                            {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 1] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 1];
                                                diacLineWords[loc + 3] += "</font>";

                                            }
                                        }
                                        else
                                        {
                                            var personTagId = 1;
                                            var person = Tags.GetTag(personTagId);
                                            var persontag = person.Tag;
                                            var personColor = person.Color;
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 2] += "</font>";
                                        }

                                    }
                                    else if (words.Count > 2 && (words[2].lemma.Contains("يّ_1") || words[2].lemma.Contains("يّ_2") || words[2].lemma.Contains("يّ_3")))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")) && (words[3].prc1 != "bi_prep"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                            var NationalTagId = 111;
                                            var National = Tags.GetTag(NationalTagId);
                                            var Nationaltag = National.Tag;
                                            var NationalColor = National.Color;
                                            diacLineWords[loc + 2] = "<font title='Trigger word-" + Nationaltag.ToUpper() + "' style='color:" + NationalColor + "'>" + diacLineWords[loc + 2];
                                            diacLineWords[loc + 2] += "</font>";

                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "adj_num" || words[4].pos == "noun_num") && (words[4].prc1 != "bi_prep"))
                                            {

                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                diacLineWords[loc + 4] += "</font>";
                                            }
                                            else
                                            {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                                diacLineWords[loc + 3] += "</font>";
                                            }
                                        }
                                        else if (words.Count > 1 && (words[0].lemma == "دُكْتُور_1"
                                || words[0].lemma == "خَلِيفَة_1"
                                || words[0].lemma == "شَيْخ_1"
                                || words[0].lemma == "شَيْخ_1"
                                || words[0].lemma == "د_1"
                                || words[1].lemma == "._0"

                                || words[0].lemma == "سَيِّد_2"
                                || words[0].lemma == "مُهَنْدِس_1"
                                || words[0].lemma == "زَعِيم_1"
                                || words[0].lemma == "رَئِيس_1"
                                || words[0].lemma == "فَرِيق_1"
                                || words[0].lemma == "طَيّار_1"

                                || words[0].lemma == "مُلازِم_1"
                                || words[0].lemma == "لِواء_1"
                                || words[0].lemma == "فَقِيه_1"
                                || words[0].lemma == "مُفَسِّر_1"
                                || words[0].lemma == "عَلّام_1"
                                //|| words[0].lemma == "شَرِيف_2"
                                //|| words[0].lemma == "أَمِير_1"
                                //|| words[0].lemma == "سُلْطان_1"
                                //|| words[0].lemma == "حاجّ_2"
                                //|| words[0].lemma == "عالَم_1"
                                || words[0].lemma == "برُوفِسُور_1"
                                || words[0].lemma == "ناقَد_1"
                                || words[0].lemma == "نَبِيّ_1"
                                //|| words[0].lemma == "فارِس_1"
                                || words[0].lemma == "جِنِرال_1"
                                || words[0].lemma == "قائِد_1"
                                || words[0].lemma == "كُولُونِيل_1"
                                || words[0].lemma == "مُؤَرِّخ_1"
                                || words[0].lemma == "كَوَّن_1"
                                || words[0].lemma == "سائِق_1"
                                || words[0].lemma == "عَقِيد_1"
                                || words[0].lemma == "خُدَيْوِيّ_1"
                                //|| words[0].lemma == "أَفَنْدِيّ_1"
                                || words[0].lemma == "مُوسِيقار_1"
                                || words[0].lemma == "خَبِير_1"
                                || words[0].lemma == "مُؤَلَّف_2"
                                || words[0].lemma == "إِمْبِراطُور_1"
                                || words[0].lemma == "قُنْصُل_1"
                                || words[0].lemma == "نَحّات_1"
                                || words[0].lemma == "والِي_1"
                                || words[0].lemma == "قاضِي_1"
                                || words[0].lemma == "نائِب_1"
                                || words[0].lemma == "قِدِّيس_1"
                                || words[0].lemma == "مُشِير_2"
                                || words[0].lemma == "مُمَثِّل_1"
                                || words[0].lemma == "مُطْرِب_1"
                                || words[0].lemma == "شاعِر_1"
                                || words[0].lemma == "لَيّ_1"
                                || words[0].lemma == "فَرِيد_1"
                                || words[0].lemma == "وَزِير_1"
                                || words[0].lemma == "سِناتُور_1"
                                || words[0].lemma == "مَمْلُوك_2"
                                || words[0].lemma == "مُفْتِي_1"
                                || words[0].lemma == "باي_1"
                                || words[0].lemma == "شَهِيد_1"
                                || words[0].lemma == "خَطِيب_1"
                                || words[0].lemma == "مُدَرِّب_1"
                                || words[0].lemma == "مُفَكِّر_1"
                                || words[0].lemma == "أَدِيب_2"
                                || words[0].lemma == "داعِي_3"
                                || words[0].lemma == "حافِظ_2"
                                || words[0].lemma == "مُقْرِئ_1"
                                || words[0].lemma == "مُجاهِد_1"
                                || words[0].lemma == "مُفَكِّر_1"
                                || words[0].lemma == "رَبّاع_1"
                                || words[0].lemma == "كابْتِن_1"
                                || words[0].lemma == "عاهِل_1"
                                || words[0].lemma == "مُعَلَّق_1"
                                || (words[0].lemma == "صُحُفِيّ_1" && words[0].gloss != "journalistic;press;newspaper")
                                //|| words[0].lemma == "عَدّاء_1"
                                //|| words[0].lemma == "حارِس_1"
                                //|| words[0].lemma == "خَلِيفَة_2"
                                //|| words[0].lemma == "أُسْتاذ_1" || words[0].lemma == "سَيِّد_1"
                                            ))
                                        {
                                            if (words.Count > 2 && words[2].pos != "noun_prop")
                                            {
                                                var JobsTagId = 110;
                                                var Jobs = Tags.GetTag(JobsTagId);
                                                var Jobstag = Jobs.Tag;
                                                var JobsColor = Jobs.Color;

                                                if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det") && (words[2].prc1 != "bi_prep"))
                                                {
                                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det") && (words[3].prc1 != "bi_prep"))
                                                    {

                                                        diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 4 - 1] += "</font>";

                                                    }
                                                    else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc2 == "wa_conj") )
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc0 == "Al_det" && words[4].prc2 == "wa_conj"))
                                                        {

                                                            diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
                                                        else
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 4 - 1] += "</font>";
                                                        }


                                                    }

                                                    else
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 3 - 1] += "</font>";
                                                    }



                                                }
                                                diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 1] += "</font>";
                                            }
                                            else
                                            { }
                                        }

                                    }
                                    else if (words.Count > 1 && (words[1].pos == "noun_prop" || ((words[1].pos == "" && words[1].word != "font") && words[1].word != "font")))
                                    {
                                        if ((words.Count > 1 && (words[1].pos == "noun_prop" && words[1].lemma == "ٱِبْن_2")))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 1 - 1] += "</font>";
                                            var personTagId = 1;
                                            var person = Tags.GetTag(personTagId);
                                            var persontag = person.Tag;
                                            var personColor = person.Color;
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 2] += "</font>";

                                        }
                                        else
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 1 - 1] += "</font>";
                                            var personTagId = 1;
                                            var person = Tags.GetTag(personTagId);
                                            var persontag = person.Tag;
                                            var personColor = person.Color;
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 1] += "</font>";
                                        }
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1 - 1] += "</font>";
                                        var personTagId = 1;
                                        var person = Tags.GetTag(personTagId);
                                        var persontag = person.Tag;
                                        var personColor = person.Color;
                                        diacLineWords[loc + 2] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 2];
                                        diacLineWords[loc + 1] += "</font>";
                                    }
                                    else if (words.Count > 1 && (words[0].lemma == "دُكْتُور_1"
                                || words[0].lemma == "خَلِيفَة_1"
                                || words[0].lemma == "شَيْخ_1"
                                || words[0].lemma == "شَيْخ_1"
                                || words[0].lemma == "د_1"
                                || words[1].lemma == "._0"
                                || words[0].lemma == "مُلْك_1"
                                || words[0].lemma == "سَيِّد_2"
                                || words[0].lemma == "مُهَنْدِس_1"
                                || words[0].lemma == "زَعِيم_1"
                                || words[0].lemma == "رَئِيس_1"
                                || words[0].lemma == "فَرِيق_1"
                                        || words[0].lemma == "مَلِك_2"
                                || words[0].lemma == "طَيّار_1"
                                || words[0].lemma == "مُلازِم_1"
                                || words[0].lemma == "لِواء_1"
                                || words[0].lemma == "فَقِيه_1"
                                || words[0].lemma == "مُفَسِّر_1"
                                || words[0].lemma == "عَلّام_1"
                                //|| words[0].lemma == "شَرِيف_2"
                                //|| words[0].lemma == "أَمِير_1"
                                //|| words[0].lemma == "سُلْطان_1"
                                //|| words[0].lemma == "حاجّ_2"
                                //|| words[0].lemma == "عالَم_1"
                                //|| words[0].lemma == "برُوفِسُور_1"
                                //|| words[0].lemma == "ناقَد_1"
                                //|| words[0].lemma == "نَبِيّ_1"
                                //|| words[0].lemma == "فارِس_1"
                                || words[0].lemma == "جِنِرال_1"
                                || words[0].lemma == "قائِد_1"
                                || words[0].lemma == "كُولُونِيل_1"
                                || words[0].lemma == "مُؤَرِّخ_1"
                                || words[0].lemma == "كَوَّن_1"
                                || words[0].lemma == "سائِق_1"
                                || words[0].lemma == "عَقِيد_1"
                                || words[0].lemma == "خُدَيْوِيّ_1"
                                || words[0].lemma == "أَفَنْدِيّ_1"
                                || words[0].lemma == "مُوسِيقار_1"
                                || words[0].lemma == "خَبِير_1"
                                || words[0].lemma == "مُؤَلَّف_2"
                                || words[0].lemma == "إِمْبِراطُور_1"
                                || words[0].lemma == "قُنْصُل_1"
                                || words[0].lemma == "نَحّات_1"
                                || words[0].lemma == "والِي_1"
                                || words[0].lemma == "قاضِي_1"
                                || words[0].lemma == "نائِب_1"
                                || words[0].lemma == "قِدِّيس_1"
                                || words[0].lemma == "مُشِير_2"
                                || words[0].lemma == "مُمَثِّل_1"
                                || words[0].lemma == "مُطْرِب_1"
                                || words[0].lemma == "شاعِر_1"
                                || words[0].lemma == "لَيّ_1"
                                
                                || words[0].lemma == "وَزِير_1"
                                || words[0].lemma == "سِناتُور_1"
                                || words[0].lemma == "مَمْلُوك_2"
                                || words[0].lemma == "مُفْتِي_1"
                                || words[0].lemma == "باي_1"
                                || words[0].lemma == "شَهِيد_1"
                                || words[0].lemma == "خَطِيب_1"
                                || words[0].lemma == "مُدَرِّب_1"
                                //|| words[0].lemma == "مُفَكِّر_1"
                                || words[0].lemma == "أَدِيب_2"
                                || words[0].lemma == "داعِي_3"
                                || words[0].lemma == "حافِظ_2"
                                || words[0].lemma == "مُقْرِئ_1"
                                || words[0].lemma == "مُجاهِد_1"
                                || words[0].lemma == "مُفَكِّر_1"
                                || words[0].lemma == "رَبّاع_1"
                                || words[0].lemma == "كابْتِن_1"
                                || words[0].lemma == "عاهِل_1"
                                || words[0].lemma == "مُعَلَّق_1"
                                || (words[0].lemma == "صُحُفِيّ_1" && words[0].gloss != "journalistic;press;newspaper")
                                || words[0].lemma == "عَدّاء_1"
                                || words[0].lemma == "حارِس_1"
                                || words[0].lemma == "خَلِيفَة_2"
                                
))
                                    {
                                        if (words.Count > 2 && words[2].pos != "noun_prop")
                                        {
                                            var JobsTagId = 110;
                                            var Jobs = Tags.GetTag(JobsTagId);
                                            var Jobstag = Jobs.Tag;
                                            var JobsColor = Jobs.Color;

                                            if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                            {
                                                if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                                {

                                                    diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 4 - 1] += "</font>";

                                                }
                                                else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc2 == "wa_conj"))
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc0 == "Al_det" && words[4].prc2 == "wa_conj"))
                                                    {

                                                        diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 5 - 1] += "</font>";
                                                    }
                                                    else
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 4 - 1] += "</font>";
                                                    }


                                                }

                                                else
                                                {
                                                    //diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                    //diacLineWords[loc + 3 - 1] += "</font>";
                                                }



                                            }
                                            //diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                            //diacLineWords[loc + 1] += "</font>";
                                        }
                                        else
                                        { }
                                    }

                                }
                                else if (words.Count > 1 && (words[1].lemma == "دُكْتُور_1"
                                || words[1].lemma == "خَلِيفَة_1"

                                || words[1].lemma == "مُلْك_1"

                                || words[1].lemma == "مُهَنْدِس_1"

                                || words[1].lemma == "رَئِيس_1"
                                || words[1].lemma == "فَرِيق_1"
                                || words[1].lemma == "طَيّار_1"
                                || words[1].lemma == "مُلازِم_1"
                                || words[1].lemma == "لِواء_1"
                                || words[1].lemma == "فَقِيه_1"
                                || words[1].lemma == "مُفَسِّر_1"
                                || words[1].lemma == "عَلّام_1"
                                || words[1].lemma == "شَرِيف_2"



                                
                                
                                || words[1].lemma == "جِنِرال_1"

                                || words[1].lemma == "كُولُونِيل_1"
                                || words[1].lemma == "مُؤَرِّخ_1"
                                || words[1].lemma == "كَوَّن_1"
                                || words[1].lemma == "سائِق_1"
                                || words[1].lemma == "عَقِيد_1"
                                || words[1].lemma == "خُدَيْوِيّ_1"

                                || words[1].lemma == "مُوسِيقار_1"
                                || words[1].lemma == "خَبِير_1"
                                || words[1].lemma == "مُؤَلَّف_2"

                                || words[1].lemma == "قُنْصُل_1"
                                || words[1].lemma == "نَحّات_1"
                                || words[1].lemma == "والِي_1"
                                || words[1].lemma == "قاضِي_1"
                                || words[1].lemma == "نائِب_1"
                                || words[1].lemma == "قِدِّيس_1"
                                || words[1].lemma == "مُشِير_2"
                                || words[1].lemma == "مُمَثِّل_1"
                                || words[1].lemma == "مُطْرِب_1"
                                || words[1].lemma == "شاعِر_1"


                                || words[1].lemma == "وَزِير_1"
                                || words[1].lemma == "سِناتُور_1"
                                || words[1].lemma == "مَمْلُوك_2"
                                || words[1].lemma == "مُفْتِي_1"


                                || words[1].lemma == "خَطِيب_1"
                                || words[1].lemma == "مُدَرِّب_1"
                                || words[1].lemma == "مُفَكِّر_1"
                                || words[1].lemma == "أَدِيب_2"
                                || words[1].lemma == "داعِي_3"
                                || words[1].lemma == "حافِظ_2"
                                || words[1].lemma == "مُقْرِئ_1"
                                || words[1].lemma == "مُجاهِد_1"
                                || words[1].lemma == "مُفَكِّر_1"
                                || words[1].lemma == "رَبّاع_1"
                                || words[1].lemma == "كابْتِن_1"

                                || words[1].lemma == "مُعَلَّق_1"
                                || (words[1].lemma == "صُحُفِيّ_1" && words[0].gloss != "journalistic;press;newspaper")
                                || words[1].lemma == "عَدّاء_1"
                                || words[1].lemma == "حارِس_1"
                                    || words[1].lemma == "خَلِيفَة_2"
                                    || words[1].lemma == "أُسْتاذ_1" || words[1].lemma == "سَيِّد_1"))
                                {
                                    if (words.Count > 2 && words[2].pos != "noun_prop")
                                    {
                                        var JobsTagId = 110;
                                        var Jobs = Tags.GetTag(JobsTagId);
                                        var Jobstag = Jobs.Tag;
                                        var JobsColor = Jobs.Color;

                                        if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 4 - 1] += "</font>";

                                            }
                                            else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc2 == "wa_conj"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc0 == "Al_det" && words[4].prc2 == "wa_conj"))
                                                {

                                                    diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 5 - 1] += "</font>";
                                                }
                                                else
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 4 - 1] += "</font>";
                                                }


                                            }

                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }



                                        }
                                        diacLineWords[loc] = "<font title='Trigger word-" + Jobstag.ToUpper() + "' style='color:" + JobsColor + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 1] += "</font>";
                                    }
                                    else
                                    { }

                                }
                                else if  ((words.Count > 1 && (words[1].pos == "noun_prop" && words[1].lemma=="ٱِبْن_2")))
                                {
                                                var personTagId = 1;
                                                var person = Tags.GetTag(personTagId);
                                                var persontag = person.Tag;
                                                var personColor = person.Color;
                                                diacLineWords[loc +1] = "<font title='Trigger word-" + persontag.ToUpper() + "' style='color:" + personColor + "'>" + diacLineWords[loc + 1];
                                                diacLineWords[loc + 2] += "</font>";
                                
                                }

                            }


                        }



                    }
                    #endregion
                    #region 7-Product
                    #region Clothing
                    ////Clothing tagging
                    //else if (triggerWordTag == "nerc".ToUpper())
                    //{
                    //    //do nothing
                    //}
                    #endregion
                    #region Drug
                    ////Drug tagging
                    //else if (triggerWordTag == "nerd".ToUpper())
                    //{
                    //    //Do nothing
                    //}
                    #endregion
                    #region Weapon
                    ////Weapon tagging
                    //else if (triggerWordTag == "nerw".ToUpper())
                    //{

                    //}
                    #endregion
                    #region Award
                    //Award tagging
                    else if (triggerWordTag == "nera".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "جائِزَة_1" || words[0].lemma == "وِسام_1" || words[0].lemma == "مِيدالِيَة_1" || words[0].lemma == "قِلادَة_1" || words[0].lemma == "وِشاح_1" || words[0].lemma == "شَهادَة_1" || words[0].lemma == "حاز-ُ_1" || words[0].lemma == "حَصَل-ُ_1" || words[0].lemma == "وِسام_2"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || words[1].lemma == "عَلَى_1"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "abbrev" || (words[2].pos == "adj" && words[2].prc0 == "Al_det")))
                                {

                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "abbrev"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].prc1 == "li_prep"))
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
                                else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep" && words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "adj" && words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "adj" && words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[4].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 5 - 1] += "</font>";
                                }
                                else { }
                            }

                            else if (words.Count > 1 && (((words[1].pos == "" && words[1].word != "font") && words[1].word != "font") || words[1].lemma == "عَلَى_1"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].pos == "abbrev" || words[1].lemma == "عَلَى_1"))
                            {
                                if (words.Count > 2 && (words[2].pos == "abbrev"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "abbrev"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "abbrev" || words[4].pos == "noun_prop"))
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
                            else if (words.Count > 1 && (words[1].pos == "noun" || words[1].lemma == "عَلَى_1"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && words[2].gloss == "state;country" && words[3].pos == "adj" && words[3].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 3 && words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc2 == "wa_conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                    else if (words[2].prc0 == "Al_det" && words[2].prc2 == "wa_conj")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words[2].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj_num"))
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
                                    else if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc0 == "Al_det" && words[4].prc1 == "li_prep"))
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
                                else if (words[1].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 1 && (words[1].pos == "adj" || words[1].lemma == "عَلَى_1"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep" ))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
                                    {

                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc +  5- 1] += "</font>";
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
                            else if (words.Count > 4 && (words[1].pos == "adj_num" && words[2].pos == "adj_num" && words[3].bw == "min/PREP" && words[4].pos == "noun_prop"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 5 - 1] += "</font>";
                            }
                        }

                    }
                    #endregion

                    #region Vehicle
                    //Vehicle
                    #region Car
                    ///Car tagging

                    else if (triggerWordTag == "nervc".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مُحَرِّك_1"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun_prop"))
                                {
                                    if (words.Count > 2)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                        {
                                            if (words.Count > 3)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop" || words[3].pos == "noun"))
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
                                }
                                else if (words.Count > 1 && (words[1].gloss == "type;kind;form" && words[1].pos == "noun"))
                                {
                                    if (words.Count > 2)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                        {
                                            if (words.Count > 3)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") || words[3].pos == "noun_prop"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";

                                                        }
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
                                }
                            }
                        }
                    }
                    #endregion
                    /////Train tagging
                    //else if (triggerWordTag == "nervt".ToUpper())
                    //{
                    //    // do nothing
                    //}
                    ///Aircraft tagging
                    #region AirCraft
                    else if (triggerWordTag == "nerva".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "طائِرَة_1" || words[0].lemma == "هِلِكُوبْتَر_1"))
                        {

                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop" || words[3].pos == "noun"))
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

                                }
                                else if (words.Count > 1 && (words[1].gloss == "type;kind;form" && words[1].pos == "noun"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") || words[3].pos == "noun_prop"))
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
                                                }
                                                else
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 3 - 1] += "</font>";
                                                }

                                            }
                                        }
                                    }
                                }
                                else if (words.Count > 1 && (words[1].gloss == "from" && words[1].pos == "prep"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "noun"))
                                        {
                                            if (words.Count > 2 && (words[2].gloss == "model;type;calibre" || words[2].gloss == "type;kind;form"))
                                            {
                                                if (words.Count >= 4)
                                                {
                                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                                    {
                                                        if (words.Count > 4)
                                                        {
                                                            if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") || words[3].pos == "noun_prop"))
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
                                                    }

                                                    else
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 3 - 1] += "</font>";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region Ship
                    ///Ship Tagging
                    else if (triggerWordTag == "nervs".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "سَفِينَة_1" || words[0].lemma == "باخِرَة_1"))
                        {

                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop" || words[3].pos == "noun"))
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
                                }
                                else if (words.Count > 1 && (words[1].gloss == "type;kind;form" && words[1].pos == "noun"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") || words[3].pos == "noun_prop"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";

                                                        }
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
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    /////Other tagging
                    //else if (triggerWordTag == "nervo".ToUpper())
                    //{
                    //    // do nothing
                    //}
                    #endregion
                    #region FoodAndDrinks
                    //Food And Drinks tagging
                    else if (triggerWordTag == "nerf".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].gloss == "eat;consume" || words[0].gloss == "drink"))
                        {
                            if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun_prop" || words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop" || words[3].pos == "noun"))
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
                    else if (triggerWordTag == "ners".ToUpper())
                    {

                        if (words.Count > 0 && ((words[0].gloss == "study" || words[0].gloss == "sciences" || words[0].gloss == "substance;material" || words[0].gloss == "methods;approaches;programs;curricula" || words[0].gloss == "area;field;arena;context;opportunity" || (words[0].gloss == "world" && words[0].num != "d" && words[0].num != "p") || words[0].gloss == "studying;checking;examining" || words[0].gloss == "science" || words[0].gloss == "be_known;be_found_out" || words[0].gloss == "know;find_out") && words[0].enc0 != "3fs_poss"))
                        {
                            if (words.Count > 1 && (words[1].bw == ":/PUNC"))
                            {
                                if (words.Count >= 3)
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun"))
                                    {
                                        if (words.Count >= 4)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc2 == "wa_conj"))
                                            {
                                                if (words.Count > 4)
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 5 - 1] += "</font>";
                                                    }
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




                                    else if (words.Count > 2 && (words[2].bw == ":/PUNC"))
                                    {
                                        if (words.Count >= 4)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun"))
                                            {
                                                if (words.Count > 4)
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 5 - 1] += "</font>";
                                                    }
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

                                        }
                                    }

                                    else if (words.Count > 3 && (words[3].bw == ":/PUNC"))
                                    {
                                        if (words.Count > 4)
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun"))
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                }

                            }
                            else if (words.Count > 1 && (words[1].pos == "noun" && words[1].prc2 != "wa_conj" && words[1].prc1 != "li_prep"))
                            {
                                if (words.Count >= 3)
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun"))
                                    {
                                        if (words.Count >= 4)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc2 == "wa_conj"))
                                            {
                                                if (words.Count > 4)
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 5 - 1] += "</font>";
                                                    }
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
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }

                            }
                            else if (words.Count > 2 && (words[1].pos == "noun" && words[2].prc2 == "wa_conj"))
                            {

                                if (words.Count > 3 && (words[2].pos == "noun" && words[3].prc2 == "wa_conj"))
                                {
                                    if (words.Count > 4 && (words[3].pos == "noun" && words[4].prc2 == "wa_conj"))
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
                        else if (words.Count > 0 && (words[0].gloss == "certificate;witness;testimony" || words[0].gloss == "doctor;Doctor;Dr." || words[0].gloss == "professor"))
                        {
                            if (words.Count > 1 && (words[1].bw == "fiy/PREP"))
                            {
                                if (words.Count >= 3)
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun"))
                                    {
                                        if (words.Count >= 4)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun"))
                                            {
                                                if (words.Count > 4)
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 5 - 1] += "</font>";
                                                    }
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
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {

                                    }
                                }

                            }
                            else if (words.Count > 1 && (words[1].prc1 == "bi_prep" || words[1].prc1 == "li_prep" && words[1].pos == "noun"))
                            {
                                if (words.Count >= 3)
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun"))
                                    {
                                        if (words.Count >= 4)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun"))
                                            {
                                                if (words.Count > 4)
                                                {
                                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc + 5 - 1] += "</font>";
                                                    }
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
                    else if (triggerWordTag == "nertp".ToUpper())
                    {
                        if (words.Count >= 2)
                        {
                            if (words.Count > 0 && (words[0].lemma == "لَوْحَة_1"))
                            {
                                if (words.Count > 1 && (words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "abbrev"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "abbrev"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
                                                        else if (words.Count > 2 && (words[2].pos == "abbrev" && words[3].pos == "abbrev" && words[4].pos == "adj"))
                                                        {

                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";

                                                        }
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

                                else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                            }
                        }

                    }
                    #endregion
                    #region Prog
                    ///Program tagging
                    else if (triggerWordTag == "nerto".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "بَرْنامَج_1"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun_prop"))
                                {
                                    if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                    {
                                        if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop" || words[3].pos == "noun"))
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
                            }
                            else if (words.Count >= 3)
                            {
                                if (words.Count > 2 && (words[2].pos == "prep"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count >= 4)
                                {
                                    if (words.Count > 3 && (words[3].pos == "prep"))
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
                            }

                        }

                    }
                    #endregion
                    #region Movie
                    ///movie tagging
                    /// how too tag those??????????
                    ///فيلم سينمائي بعنوان
                    ///الافلام والمسلسلات نذكر منها:
                    ///فيلم وثائقي من انتاج ايراني بعنوان ()

                    else if (triggerWordTag == "nertm".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مُسَلْسَل_1" || words[0].lemma == "فِيلْم_1"))
                        {
                            /////فيلم : ام كلثوم 
                            if (words[0].gloss == "film;movie" || words[0].gloss == "serial;sequence;soap_opera" || words[0].gloss == "films;movies")
                            {
                                if (words.Count > 1 && (words[1].pos == "punc"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                        if (words.Count > 1 && (words[1].pos == "prep" || words[1].pos == "conj"))
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

                                else if (words.Count > 1 && (words[1].pos == "noun_prop" || words[1].pos == "noun" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "conj" || words[1].pos == "prep" || words[1].pos == "digit"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                else if (words.Count > 1 && (words[1].pos == "adj"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "punc"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                    else if (words.Count > 2 && (words[2].pos == "noun" && words[2].gloss == "address" && words[2].prc1 == "bi_prep"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                        else if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit"))
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
                                            if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                    else if (words.Count > 2 && (words[2].gloss == "like;such_as"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                        else if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit"))
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
                                            if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                    if (words.Count > 1 && (words[1].pos == "prep" || words[1].pos == "conj"))
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

                    else if (triggerWordTag == "nerts".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَسْرَحِيّ_1" || words[0].lemma == "مَسْرَحِيَّة_1"))
                        {
                            if (words[0].gloss == "theatrical" || words[0].gloss == "plays_(theater)" || words[0].gloss == "play_(theater)")
                            {

                                if (words.Count > 1 && (words[1].pos == "punc"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                        if (words.Count > 1 && (words[1].pos == "prep" || words[1].pos == "conj"))
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

                                else if (words.Count > 1 && (words[1].pos == "noun_prop" || words[1].pos == "noun" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "conj" || words[1].pos == "prep" || words[1].pos == "digit"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit" || words[2].pos == "adj"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                else if (words.Count > 1 && (words[1].pos == "adj"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "punc"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                    else if (words.Count > 2 && (words[2].pos == "noun" && words[2].gloss == "address" && words[2].prc1 == "bi_prep"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                        else if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                    else if (words.Count > 2 && (words[2].gloss == "like;such_as"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                        else if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj"))
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
                                            if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                    if (words.Count > 1 && (words[1].pos == "prep" || words[1].pos == "conj"))
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
                    }
                    #endregion
                    #region Music
                    ///Music tagging
                    else if (triggerWordTag == "nertu".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "أُورْكِسْترا_1"
|| words[0].lemma == "أُغْنِيَة_1"
|| words[0].lemma == "نَشِيد_1"
|| words[0].lemma == "قَصِيدَة_1"
|| words[0].lemma == "سِمْفُونِيّ_1"))
                        { }
                    }
                    #endregion
                    #region Book
                    ///Book tagging
                    ///Go Back to Excel Sheet
                    else if (triggerWordTag == "nertb".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "كِتاب_1" || words[0].lemma == "تَأْلِيف_1" || words[0].lemma == "قُرْآن_1" || words[0].lemma == "كَتَب-ُ_1" || words[0].lemma == "قَصِيدَة_1" || words[0].lemma == "تَفْسِير_1" || words[0].lemma == "مُعْجَم_1" || words[0].lemma == "مَصْدَر_1" || words[0].lemma == "أَلْف_1" || words[0].lemma == "مَرْجِع_2" || words[0].lemma == "مُجَلَّد_1" || words[0].lemma == "" || words[0].lemma == "شِعْر_1" || words[0].lemma == "دِيوان_1" || words[0].lemma == "أَصْدَر_1" || words[0].lemma == "مُؤَلِّف_1" || words[0].gloss == "pierce;examine" || words[0].gloss == "read" || words[0].lemma == "ناظِم_1" || words[0].lemma == "أَصْل_1" || words[0].lemma == "مُؤَلِّف_1" || words[0].lemma == "رِسالَة_1" || words[0].lemma == "إِلِياذَة_1" || words[0].lemma == "مُجَلَّد_1" || words[0].lemma == "قِصَّة_1" || words[0].lemma == "تَرْجَم_1" || words[0].lemma == "مَوْسُوعَة_1" || words[0].lemma == "رَجْم_1"))
                        {
                            
                                if (words.Count > 1 && (words[1].pos == "punc"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {
                                                if (words[5].pos == "punc")
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 4 - 1] += "</font>";
                                                }
                                                else
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 5 - 1] += "</font>";
                                                }
                                               

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                        if (words.Count > 1 && (words[1].pos == "prep" || words[1].pos == "conj"))
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

                                else if (words.Count > 1 && ((words[1].pos == "noun_prop" || (words[1].pos == "noun" && words[1].enc0 != "3ms_poss") || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "conj" || words[1].pos == "prep" || words[1].pos == "digit") && words[1].prc2 != "wa_conj") && words[0].per != "3")
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun" || ((words[2].pos == "" && words[2].word != "font") && words[1].pos != "") || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit" || words[2].pos == "adj") )
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || ((words[3].pos == "" && words[3].word != "font") && words[2].pos != "") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit" || words[3].pos == "adj"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || ((words[4].pos == "" && words[4].word != "font") && words[3].pos != "") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 2 - 1] += "</font>";
                                            }
                                            else if (words[0].lemma == "قُرْآن_1")
                                            {
                                                var BookTagId = 79;
                                                var Book = Tags.GetTag(BookTagId);
                                                var Booktag = Book.Tag;
                                                var BookColor = Book.Color;
                                                if (words.Count > 1 && (words[0].bw == "Al/DET+kariym/NOUN_PROP"))
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + Booktag.ToUpper() + "' style='color:" + BookColor + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 2 - 1] += "</font>";
                                                }
                                                else
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + Booktag.ToUpper() + "' style='color:" + BookColor + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc + 1 - 1] += "</font>";
                                                }
                                            }
                                            else
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                                
                                            }
                                           

                                    }
                                    else if (words[0].prc0 != "Al_det" && words[1].prc0 != "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 1 && (words[1].pos == "adj"))
                                {
                                    if (words.Count > 2 && (words[2].pos == "punc" || words[2].pos == "noun_prop" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "conj" || words[2].pos == "prep" || words[2].pos == "digit"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
                                            {
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                            else
                                            {
                                                //diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                //diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "noun" && words[2].gloss == "address" && words[2].prc1 == "bi_prep"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                        else if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                            if (words.Count > 2 && (words[2].pos == "prep" || words[2].pos == "conj"))
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
                                    else if (words.Count > 2 && (words[2].gloss == "like;such_as"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "conj" || words[3].pos == "prep" || words[3].pos == "digit"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit" || words[4].pos == "adj"))
                                            {

                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 5 - 1] += "</font>";

                                            }
                                            else
                                            {
                                                if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                        else if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "conj" || words[4].pos == "prep" || words[4].pos == "digit" || words[4].pos == "adj"))
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
                                            if (words.Count > 3 && (words[3].pos == "prep" || words[3].pos == "conj"))
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
                                    else if (words[0].lemma == "قُرْآن_1")
                                    {
                                        var BookTagId = 79;
                                        var Book = Tags.GetTag(BookTagId);
                                        var Booktag = Book.Tag;
                                        var BookColor = Book.Color;
                                        if (words.Count > 1 && (words[0].bw == "Al/DET+kariym/NOUN_PROP" || (words[0].pos == "adj" && words[0].prc0 == "Al_det")))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + Booktag.ToUpper() + "' style='color:" + BookColor + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + Booktag.ToUpper() + "' style='color:" + BookColor + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 1 - 1] += "</font>";
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
                    else if (triggerWordTag == "nerpn".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "شَهْرِيّ_1" || words[0].lemma == "صَحِيفَة_1" || words[0].lemma == "نَشْرَة_1" || words[0].lemma == "جُورْنال_1" || words[0].lemma == "جَرِيدَة_1"))
                        {
                            #region noun Before
                            if (words.Count > 0 && (words[0].pos == "noun" || words[0].pos == "noun_prop"))
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "part_det" || words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop"))
                                {

                                    if (words.Count > 1 && (words[1].pos == "adj"))
                                    {
                                        if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun_prop"))
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
                                        if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun"))
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
                                else if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj" || words[2].pos == "part_det"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "adj" || words[3].pos == "part_det"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "adj" || words[4].pos == "part_det"))
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
                                    if (words.Count > 1 && (words[1].pos == "punc"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
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
                                        else if (words.Count > 4 && (words[4].pos == "punc"))
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";


                                        }
                                        else if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
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
                    }

                    #endregion
                    #region Magazine
                    ///Magazine tagging
                    else if (triggerWordTag == "nerpm".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَجَلَّة_1"))
                        {
                            if (words.Count > 0 && (words[0].pos == "noun" || words[0].pos == "noun_prop"))
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "part_det" || words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop"))
                                {

                                    if (words.Count > 1 && (words[1].pos == "adj"))
                                    {
                                        if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun_prop"))
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
                                        if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "part_det" || words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun"))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "adj" || words[3].pos == "part_det" || words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun"))
                                            {
                                                if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "part_det" || words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun"))
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
                                else if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj" || words[2].pos == "part_det"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "adj" || words[3].pos == "part_det"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "adj" || words[4].pos == "part_det"))
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
                                    if (words.Count > 1 && (words[1].pos == "punc"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "punc"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
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
                                        else if (words.Count > 4 && (words[4].pos == "punc"))
                                        {

                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";


                                        }
                                        else if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";

                                        }



                                        else
                                        {
                                            
                                            //diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            //diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }


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
                    else if (triggerWordTag == "nermr".ToUpper())
                    {
                        var percedingSentence = string.Empty;
                        var percedingLocation = loc <= 5 ? 0 : loc - 5;


                        for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                        {
                            percedingSentence += (diacLineWords[perItemIndex] + " ");
                        }
                        percedingSentence = percedingSentence.Replace("<", "");
                        percedingSentence = percedingSentence.Replace(">", "");
                        var percWords = new List<WordInfoItem>();
                        if (percedingSentence != string.Empty)
                        {
                            percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                            percWords.Reverse();
                        }
                        if (percWords.Count > 1 && (percWords[1].pos == "noun_prop" || percWords[0].lemma == "دُكْتُور_1"
                                  || percWords[1].lemma == "خَلِيفَة_1"
                                  || percWords[0].lemma == "شَيْخ_1"
                                  || percWords[0].lemma == "د_1"
                                  || percWords[1].lemma == "._0"
                                  || percWords[0].lemma == "مُلْك_1"
                                  || percWords[0].lemma == "سَيِّد_2"
                                  || percWords[0].lemma == "مُهَنْدِس_1"
                                  || percWords[0].lemma == "زَعِيم_1"
                                  || percWords[0].lemma == "رَئِيس_1"
                                  || percWords[0].lemma == "فَرِيق_1"
                                  || percWords[0].lemma == "طَيّار_1"
                                  || percWords[0].lemma == "مُلازِم_1"
                                  || percWords[0].lemma == "لِواء_1"
                                  || percWords[0].lemma == "فَقِيه_1"
                                  || percWords[0].lemma == "مُفَسِّر_1"
                                  || percWords[0].lemma == "عَلّام_1"
                                  || percWords[0].lemma == "شَرِيف_2"
                                  || percWords[0].lemma == "أَمِير_1"
                                  || percWords[0].lemma == "سُلْطان_1"
                                  || percWords[0].lemma == "حاجّ_2"
                                  || percWords[0].lemma == "عالَم_1"
                                  || percWords[0].lemma == "برُوفِسُور_1"
                                  || percWords[0].lemma == "ناقَد_1"
                                  || percWords[0].lemma == "نَبِيّ_1"
                                  || percWords[0].lemma == "فارِس_1"
                                  || percWords[0].lemma == "جِنِرال_1"
                                  || percWords[0].lemma == "قائِد_1"
                                  || percWords[0].lemma == "كُولُونِيل_1"
                                  || percWords[0].lemma == "مُؤَرِّخ_1"
                                  || percWords[0].lemma == "كَوَّن_1"
                                  || percWords[0].lemma == "سائِق_1"
                                  || percWords[0].lemma == "عَقِيد_1"
                                  || percWords[0].lemma == "خُدَيْوِيّ_1"
                                  || percWords[0].lemma == "أَفَنْدِيّ_1"
                                  || percWords[0].lemma == "مُوسِيقار_1"
                                  || percWords[0].lemma == "خَبِير_1"
                                  || percWords[0].lemma == "مُؤَلَّف_2"
                                  || percWords[0].lemma == "إِمْبِراطُور_1"
                                  || percWords[0].lemma == "قُنْصُل_1"
                                  || percWords[0].lemma == "نَحّات_1"
                                  || percWords[0].lemma == "والِي_1"
                                  || percWords[0].lemma == "قاضِي_1"
                                  || percWords[0].lemma == "نائِب_1"
                                  || percWords[0].lemma == "قِدِّيس_1"
                                  || percWords[0].lemma == "مُشِير_2"
                                  || percWords[0].lemma == "مُمَثِّل_1"
                                  || percWords[0].lemma == "مُطْرِب_1"
                                  || percWords[0].lemma == "شاعِر_1"
                                  || percWords[0].lemma == "لَيّ_1"
                                  || percWords[0].lemma == "فَرِيد_1"
                                  || percWords[0].lemma == "وَزِير_1"
                                  || percWords[0].lemma == "سِناتُور_1"
                                  || percWords[0].lemma == "مَمْلُوك_2"
                                  || percWords[0].lemma == "مُفْتِي_1"
                                  || percWords[0].lemma == "باي_1"
                                  || percWords[0].lemma == "شَهِيد_1"
                                  || percWords[0].lemma == "خَطِيب_1"
                                  || percWords[0].lemma == "مُدَرِّب_1"
                                  || percWords[0].lemma == "مُفَكِّر_1"
                                  || percWords[0].lemma == "أَدِيب_2"
                                  || percWords[0].lemma == "داعِي_3"
                                  || percWords[0].lemma == "حافِظ_2"
                                  || percWords[0].lemma == "مُقْرِئ_1"
                                  || percWords[0].lemma == "مُجاهِد_1"
                                  || percWords[0].lemma == "مُفَكِّر_1"
                                  || percWords[0].lemma == "رَبّاع_1"
                                  || percWords[0].lemma == "كابْتِن_1"
                                  || percWords[0].lemma == "عاهِل_1"
                                  || percWords[0].lemma == "مُعَلَّق_1"
                                  || percWords[0].lemma == "صُحُفِيّ_1"
                                  || percWords[0].lemma == "عَدّاء_1"
                                  || percWords[0].lemma == "حارِس_1"
                                  || percWords[0].lemma == "خَلِيفَة_2"))
                        { }

                        else
                        {
                            //مذهب ديانه
                            if (words.Count > 0 && (words[0].gloss == "manner;path" || words[0].gloss == "religion;creed"))
                            {
                                if (words.Count > 1 && (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else
                                { }

                            }

                            //مصحف والمصحف
                            else if (words.Count > 0 && (words[0].gloss == "volume"))
                            {

                                if (words.Count > 1 && (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "noun_prop"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else
                                { }

                            }
                            // اعتنق واعتناقه
                            else if (words.Count > 0 && (words[0].gloss == "adoption;embracing" || words[0].gloss == "embrace;adopt"))
                            {
                                if (words.Count > 1 && (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else
                                { }
                            }
                            else
                            {
                                if (words.Count > 1 && (words[1].pos == "adj" || (words[1].pos == "noun" && words[1].prc0 == "Al_det") || words[1].pos == "prop_noun"))
                                {
                                    if (words.Count > 2 && ((words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].pos != "adj" && words[1].pos != "noun") || (words[2].pos == "noun" && words[1].pos != "noun" && words[1].pos != "adj") || words[2].pos == "prop_noun"))
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
                                else if (words.Count > 0 && (words[0].lemma == "إِسْلام_1" || words[0].lemma == "صُوفِيّ_3" || words[0].lemma == "صُوفِيّ_3" || words[0].lemma == "صُوفِيّ_1" || words[0].lemma == "كاثُولِيكِيّ_1" || words[0].lemma == "شِيعَة_1" || words[0].gloss == "enactment;prescription" || words[0].lemma == "قُرْآن_1"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }
                            }
                        }
                    }
                    #endregion
                    #region Sport
                    ///Sport tagging
                    else if (triggerWordTag == "nerms".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "لُعْبَة_1"
|| words[0].lemma == "رِياضَة_1"
|| words[0].lemma == "كُرَة_1"
|| words[0].lemma == "وَثْب_1"
|| words[0].lemma == "رَمَى-ِ_1"
|| words[0].lemma == "سِباق_1"
|| words[0].lemma == "سِباحَة_1"
|| words[0].lemma == "لَعِب-َ_1"
|| words[0].lemma == "لاعِب_1"
|| words[0].lemma == "مَلْعَب_1"
|| words[0].lemma == "مارَس_1"
|| words[0].lemma == "بُطُولَة_1"
|| words[0].lemma == "بَطَل_1"
|| words[0].lemma == "مُنْتَخَب_1"
|| words[0].lemma == "مُباراة_1"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == " noun_prop"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                                else if (words.Count > 1 && (words[1].pos == "noun_prop" && (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count >= 3)
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun_prop" && words[2].pos == "noun_num"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun_prop" && words[2].pos == "punc"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det" && words[2].prc2 == "wa_conj"))
                                        {
                                            if (words.Count >= 4)
                                            {

                                                if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc2 == "wa_conj"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc0 == "Al_det" && words[4].prc2 == "wa_conj"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
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
                                        else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }
                                        else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "bi_prep"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }


                                    }



                                }
                            }


                        }



                    }
                    #endregion
                    #region theory
                    ///theory tagging
                    else if (triggerWordTag == "nermt".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].gloss == "thought;thinking" || words[0].gloss == "theoretical;speculative" || words[0].gloss == "method;procedure" || words[0].gloss == "theory"))
                        {
                            if (words.Count > 1 && (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "prop_noun" || (words[1].pos == "" && words[1].word != "font")) && words[1].prc2 != "wa_conj")
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "noun" || words[2].pos == "prop_noun" || (words[2].pos == "" && words[2].word != "font")) && words[2].prc2 != "wa_conj")
                                {

                                    if (words.Count > 3 && (words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep" && words[3].pos == "noun" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
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
                                    if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                    {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                    }

                                }
                            }
                            else
                            {
                                //diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                //diacLineWords[loc + 1 - 1] += "</font>";
                            }


                        }
                        else
                        { }
                    }
                    #endregion
                    /////Plan tagging
                    //else if (triggerWordTag == "nermp".ToUpper())
                    //{
                    //    // do nothing
                    //}
                    /////Culture tagging
                    //else if (triggerWordTag == "nermc".ToUpper())
                    //{
                    //    // do nothing
                    //}
                    #region Movement
                    ///movement tagging
                    else if (triggerWordTag == "nermm".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].gloss == "movement;activity;organization" && words[0].pos == "noun"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun" || words[2].pos == "adj" || words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun" || words[2].pos == "adj"))
                                    {
                                        if (words.Count > 2 && (words[2].prc0 == "Al_det" || words[2].prc0 == "Al_det"))
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
                            else if (words.Count > 0 && (words[0].pos == ""))
                            {

                                if (words.Count > 1 && (words[1].pos == "noun" || words[1].pos == "adj" || words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count > 2 && (words[2].pos == "noun" || words[2].pos == "adj" || words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun" || words[3].pos == "adj" || words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
                                        {
                                            if (words.Count > 3 && (words[3].pos == "noun" || words[3].pos == "adj" || words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
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
                            {
                                //diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                //diacLineWords[loc + 1 - 1] += "</font>";
                            }

                        }
                        else
                        {
                           
                        }
                    }
                    #endregion
                    #endregion
                    #region Rule
                    //Rule
                    #region Law
                    ///Law tagging
                    else if (triggerWordTag == "nerrl".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].gloss == "law;statutes;regulations" || words[0].gloss == "constitution;statute" || words[0].gloss == "laws;regulations;rules;statutes"))
                        {
                            if (words.Count > 1 && (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "digit"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "noun" || words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "digit"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" || words[3].pos == "noun" || words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "digit"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "digit"))
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
                        else if (words.Count > 0 && (words[0].gloss == "article;clause"))
                        {
                            if (words.Count > 1 && (words[1].pos == "digit"))
                            {
                                if (words.Count > 2 && (words[2].bw == "min/PREP" || words[2].bw == "fiy/PREP"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" || words[4].pos == "noun" || words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "digit"))
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
                    else if (triggerWordTag == "nerrt".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].gloss == "document;charter" || words[0].gloss == "treaty;accord" || words[0].gloss == "treaty;accord;pact" || words[0].gloss == "agreement;accord;treaty"))
                        {
                            if (words.Count > 1 && (words[1].pos == "adj" || words[1].pos == "noun" || words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "digit"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "noun" || words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "digit"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" || words[3].pos == "noun" || words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "digit"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "digit"))
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
                    else if (triggerWordTag == "nerl".ToUpper())
                    {
                        /// اجاد اجادته
                        if (words.Count > 0 && (words[0].gloss == "be_proficient_at;do_well" && words[0].pos == "verb"))
                        {
                            if (words.Count > 1 && (words[1].gloss == "language"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || (words[3].pos == "" && words[3].word != "font")))
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
                            else if (words.Count > 1 && (words[1].gloss == "languages"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || (words[4].pos == "" && words[4].word != "font")))
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
                        else if (words.Count > 0 && (words[0].gloss == "language" || words[0].gloss == "languages"))
                        {
                         
                            if (words.Count > 1 && (words[1].pos == "adj" || (words[1].pos == "" && words[1].word != "font")))
                            {

                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc2 == "wa_conj" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || (words[3].pos == "" && words[3].word != "font" || words[3].pos == "noun" && words[3].prc2 == "wa_conj" )))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || (words[4].pos == "" && words[4].word != "font" || words[4].pos == "noun" && words[4].prc2 == "wa_conj")))
                                        {
                                            var loc2 = 0;
                                            var sentence2 = string.Empty;
                                            for (var z = locations.Count - 1; z >= 0; z--)
                                            {
                                                loc2 = loc + 5;
                                                // get 5 words after that location 
                                                var sentenceWordsCount2 = diacLineWords.Count();
                                                var remainingWords2 = sentenceWordsCount2 - loc2;



                                                if (remainingWords2 > 5)
                                                {
                                                    for (int Index = loc2; Index < loc2 + 5; Index++)
                                                    {
                                                        sentence2 += (diacLineWords[Index] + " ");
                                                    }
                                                }
                                                else
                                                {
                                                    for (int Index = loc2; Index < sentenceWordsCount2; Index++)
                                                    {
                                                        sentence2 += (diacLineWords[Index] + " ");
                                                    }

                                                }
                                            }



                                            sentence2 = sentence2.Replace("<", "");
                                            sentence2 = sentence2.Replace(">", "");
                                            var words2 = BL.MadaMiraHandler.Analyse(sentence2);
                                           

                                             if (words2.Count > 0 && (words2[0].pos == "adj" && words2[0].prc2 == "wa_conj" || (words2[0].pos == "" && words2[0].word != "font")))
                                        
                                            {

                                                if (words2.Count > 1 && (words2[1].pos == "adj" && words2[1].prc2 == "wa_conj" || (words2[1].pos == "" && words2[1].word != "font" || words2[1].pos == "noun" && words2[1].prc2 == "wa_conj")))
                                                {

                                                    if (words2.Count > 2 && (words2[2].pos == "adj" && words2[2].prc2 == "wa_conj" || (words2[2].pos == "" && words2[2].word != "font" || words2[2].pos == "noun" && words2[2].prc2 == "wa_conj")))
                                                    {
                                                        if (words2.Count > 3 && (words2[3].pos == "adj" && words2[3].prc2 == "wa_conj" || (words2[3].pos == "" && words2[3].word != "font" || words2[3].pos == "noun" && words2[3].prc2 == "wa_conj")))
                                                        {
                                                            if (words2.Count > 4 && (words2[4].pos == "adj" && words2[4].prc2 == "wa_conj" || (words2[4].pos == "" && words2[4].word != "font" || words2[4].pos == "noun" && words2[4].prc2 == "wa_conj")))
                                                            {
                                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                                diacLineWords[loc2 + 5 - 1] += "</font>";
                                                            }
                                                            else
                                                            {
                                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                                diacLineWords[loc2 + 4 - 1] += "</font>";
                                                            }
                                                        }

                                                        else
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc2 + 3 - 1] += "</font>";
                                                        }
                                                    
                                                    }
                                                    else
                                                    {
                                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                        diacLineWords[loc2 + 2 - 1] += "</font>";
                                                    }
                                                }
                                                else
                                                {
                                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                    diacLineWords[loc2 + 1 - 1] += "</font>";
                                                }
                                             
                                             
                                             }
                                            else{
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";}
                                            
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
                        else if (words.Count > 0 && (words[0].gloss == "translate;interpret" || words[0].gloss == "translator;interpreter"))
                        {
                            ///ترجم كذلك أو ترجم ايضا 
                            if (words.Count > 1 && (words[1].gloss == "that" || words[1].gloss == "also,_too,_as_well_(as),_along_with,_in_addition_(to)"))
                            {
                                if (words.Count > 2 && (words[2].bw == "&lt;ilaY/PREP"))
                                {
                                    //الي العربيه
                                    if (words.Count > 3 && (words[3].pos == "adj"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj"))
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

                                    else if (words.Count > 3 && (words[3].gloss == "language" || words[3].gloss == "languages"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" || (words[4].pos == "" && words[4].word != "font")))
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


                            else if (words.Count > 1 && (words[1].bw == "&lt;ilaY/PREP"))
                            {
                                //الي العربيه
                                if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj"))
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
                                else if (words.Count > 2 && (words[2].gloss == "language" || words[2].gloss == "languages"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || (words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].bw == "min/PREP"))
                            {

                                if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].bw == "&lt;ilaY/PREP"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj"))
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
                                else if (words.Count > 2 && (words[2].gloss == "language" || words[2].gloss == "languages"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || (words[4].pos == "" && words[4].word != "font")))
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


                            else if (words.Count > 2 && (words[2].bw == "&lt;ilaY/PREP"))
                            {
                                if (words.Count > 3 && (words[3].pos == "adj"))
                                {
                                    if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj"))
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
                        else if (words.Count > 0 && (words[0].gloss == "study;learn" || words[0].gloss == "speak;discuss" || words[0].gloss == "be_published;be_issued"))
                        {
                            if (words.Count > 1 && (words[1].gloss == "language" || words[1].gloss == "languages"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj" || (words[4].pos == "" && words[4].word != "font")))
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


                            else if (words.Count > 1 && (words[1].pos == "adj" && words[1].bw == "bi/PREP"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[3].prc2 == "wa_conj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj"))
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
                    else if (triggerWordTag == "neru".ToUpper())
                    {
                        var percedingSentence = string.Empty;
                        var percedingLocation = loc <= 5 ? 0 : loc - 5;


                        for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                        {
                            percedingSentence += (diacLineWords[perItemIndex] + " ");
                        }
                        percedingSentence = percedingSentence.Replace("<", "");
                        percedingSentence = percedingSentence.Replace(">", "");
                        var percWords = new List<WordInfoItem>();
                        if (percedingSentence != string.Empty)
                        {
                            percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                            percWords.Reverse();
                        }
                        if (percWords.Count > 0 && (percWords[0].pos == "digit" || percWords[0].pos == "noun_num" || percWords[0].pos == "adj_num"))
                        {
                            //// i need ti check if the previous word is digit
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }
                    }
                    #endregion

                    #region other
                    //Other tagging
                    else if (triggerWordTag == "nero".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].gloss == "canal;channel" || words[0].gloss == "screen" || words[0].gloss == "testing;experimenting;probing" || words[0].gloss == "canals;channels" || words[0].gloss == "network;system"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun" && words[2].pos == "adj"))
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }

                            else if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].gloss == "like;such_as" || words[2].gloss == "punc"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" || words[3].pos == "noun_prop" || words[3].pos == "abbrev"))
                                    {

                                        if (words.Count > 4 && (words[4].pos == "noun" || words[4].pos == "noun_prop" || words[4].pos == "abbrev"))
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
                                    else if (words.Count > 3 && (words[3].pos == "punc"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun" || words[4].pos == "noun_prop" || words[4].pos == "abbrev"))
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
                                        if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
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

                            else if (words.Count > 1 && (words[1].pos == "abbrev" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "abbrev" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "abbrev" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "abbrev" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun_prop"))
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

                            else if (words.Count > 1 && (words[1].pos == "noun" || words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "abbrev" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "abbrev" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "abbrev" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun_prop"))
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
                    else if (triggerWordTag == "nec".ToUpper())
                    {
                        if (words.Count <= 5)
                        {
                            ////how to tagging from the second word
                            if (words.Count > 0 && (words[0].gloss == "color;tint" && words[0].pos == "noun"))
                            {
                                if (words.Count > 1 && (words[1].gloss == "yellow" || words[1].gloss == "red" || words[1].gloss == "blue" || words[1].gloss == "black" || words[1].gloss == "white"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else
                                { }


                            }
                            else if (words.Count > 0 && (words[0].gloss == "yellow" || words[0].gloss == "red" || words[0].gloss == "blue" || words[0].gloss == "black" || words[0].gloss == "white"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }
                        }

                    }
                    #endregion
                    #region 8-Facility
                    #region Archeological place
                    //Archaeological place tagging
                    else if (triggerWordTag == "nefa".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "هَرَم_2" || words[0].lemma == "مَقْبَر_1"
|| words[0].lemma == "قَصْر_1"
|| words[0].lemma == "قَلْعَة_1"
|| words[0].lemma == "بُرْج_2"
|| words[0].lemma == "مَعْبَد_1"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && (words[1].pos == "noun"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
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
                                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                diacLineWords[loc + 3 - 1] += "</font>";
                                            }
                                        }
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                    {
                                        if (words.Count >= 4)
                                        {
                                            if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                                else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                        {
                                            if (words.Count >= 4)
                                            {

                                                if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
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
                                else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
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
                            }
                        }
                    }
                    #endregion
                    #region GOE
                    //GOE
                    #region POrt
                    /// Port tagging
                    else if (triggerWordTag == "nefgp".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مِيناء_1" || words[0].lemma == "مَرْفَأ_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "adj"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                else if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                            else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                    }
                    #endregion
                    #region Airport
                    /// Airport tagging
                    else if (triggerWordTag == "nefga".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَطار_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                    else if (words.Count > 2 && (words[2].pos == "adj" && words[2].lemma == "دُوَلِيّ_1"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].pos == "noun_prop"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                    }
                    #endregion
                    /// Station tagging
                    else if (triggerWordTag == "nefgs".ToUpper())
                    {
                        // Do nothing
                    }
                    #region Worship Place
                    /// Worship Place tagging
                    else if (triggerWordTag == "nefgw".ToUpper())
                    {
                        if (words.Count > 0 && ((words[0].lemma == "جامِع_1" || words[0].lemma == "مَسْجِد_1" || words[0].lemma == "كَنِيسَة_1" || words[0].lemma == "كاتِدرائِيَّة_1") && words[0].diac != "جامِعَةُ"))
                        {
                            if (words.Count >= 2)
                            {
                                if (words.Count > 1 && (words[1].pos == "noun_prop"))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "noun" && words[2].prc0 == "Al_det")))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
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
                                                diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
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
                                else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                                {
                                    if (words.Count >= 3)
                                    {
                                        if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj"))
                                        {
                                            if (words.Count >= 4)
                                            {
                                                if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                                {
                                                    if (words.Count > 4)
                                                    {
                                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
                                                        {
                                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                                            diacLineWords[loc + 5 - 1] += "</font>";
                                                        }
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
                                else if (words.Count > 1 && (words[1].pos == "noun"))
                                {
                                    if (words.Count > 1 && (words[1].lemma == "قِدِّيس_1"))
                                    {
                                        if (words.Count >= 3)
                                        {
                                            if (words.Count > 2 && (words[2].pos == "noun" || words[2].pos == "noun_prop" || words[2].pos == "adj" || (words[2].pos == "" && words[2].word != "font")))
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
                                    else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }


                                }
                                else if (words.Count > 1 && (words[1].pos == "adj"))
                                {
                                    if (words.Count > 1 && (words[1].lemma == "إِنْجِيلِيّ_1" || words[1].lemma == "كاثُولِيكِيّ_1"))
                                    {
                                        if (words.Count >= 3)
                                        {
                                            if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det"))
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
                                    else if (words[1].prc0 == "Al_det")
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }




                                }
                            }
                        }
                    }
                    #endregion
                    #region Cinema
                    /// Theater and Cinema tagging
                    else if (triggerWordTag == "nefgt".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَسْرَح_1" || words[0].lemma == "سِينَما_1" || words[0].lemma == "أُوبِرا_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj" || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                            else if (words.Count > 1 && (words[1].pos == "abbrev"))
                            {
                                if (words.Count > 2 && (words[2].pos == "abbrev"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "abbrev" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "abbrev" || words[4].pos == "noun_prop"))
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
                    }
                    #endregion
                    /// Zoo tagging
                    else if (triggerWordTag == "nefgz".ToUpper())
                    {
                        // Do nothing
                    }
                    #region Museum
                    /// Museum tagging
                    else if (triggerWordTag == "nefgm".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَتْحَف_1" || words[0].lemma == "مَعْبَد_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") || words[1].pos == "verb"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj" || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
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
                                else if (words.Count > 2 && (words[2].gloss == "-"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") && words[1].prc1 == "li_prep"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun_prop" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                        }
                    }
                    #endregion
                    #region Sports
                    /// Sports Facility tagging
                    else if (triggerWordTag == "nefgf".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَلْعَب_1" || words[0].lemma == "إِسْتاد_1")
|| words[0].lemma == "حَلْبَة_1"
|| words[0].lemma == "مَرْكَز_1" || words[1].lemma == "تَدْرِيب_1"
|| words[0].lemma == "نادِي_1"
|| words[0].lemma == "صالَة_1"
|| words[0].lemma == "مَسْبَح_1"
|| words[0].lemma == "كُرَة_1"
)
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "adj" || words[2].pos == "adj_num"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || words[3].pos == "noun_num"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj" || words[2].pos == "noun_prop" || words[2].pos == "noun" || words[2].pos == "adj_num"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[2].pos == "noun_num"))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
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
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words.Count > 1 && (words[1].pos == "adj" && words[1].prc0 == "Al_det"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && (words[1].pos == "adj_num" && words[2].pos == "noun_num" && words[3].pos == "noun_prop"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }
                    }
                    #endregion
                    #region Park
                    /// Park tagging
                    else if (triggerWordTag == "nefgk".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مُتَنَزَّه_1" || words[0].lemma == "حَدِيقَة_1" || words[0].lemma == "مَحْمِيّ_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "adj" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj" || words[2].pos == "noun_prop" || words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "adj" || words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                        }
                    }
                    #endregion
                    #region Puplic inst
                    /// Public institution tagging
                    else if (triggerWordTag == "nefgi".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "بَلَدِيَّة_1" || words[0].lemma == "مَبْنَى_1")
|| words[0].lemma == "مُسْتَشْفَى_1"
|| words[0].lemma == "مَكْتَبَة_1"
|| words[0].lemma == "سِفارَة_1"
|| words[0].lemma == "جَمْعِيَّة_1"
|| words[0].lemma == "بَنْك_1"
|| words[0].lemma == "مَكْتَب_1"
                           || words[0].lemma == "فُنْدُق_1"

)
                        {
                            if (words.Count > 1 && (words[1].pos == "noun" && words[1].prc2 != "wa_conj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det" && words[2].prc2 != "wa_conj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop") && words[2].prc2 != "wa_conj")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun"))
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
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 2 && ((words[1].pos == "noun_prop") || (words[2].pos == "noun_prop")))
                            {

                                if (words.Count > 3 && ((words[3].pos == "noun_prop")))
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
                        }
                    }
                    #endregion
                    #region School
                    /// School tagging
                    else if (triggerWordTag == "nefgh".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَدْرَسَة_1" || words[0].lemma == "جامِعَة_1"
|| words[0].lemma == "مَرْكَز_1"
|| words[0].lemma == "مَعْهَد_1"
|| words[0].lemma == "كُلِّيّ_1"
|| words[0].lemma == "أَكادِيمِيّ_2"
|| words[0].lemma == "حَرَم_1" || words[0].lemma == "جامِع_1" || words[0].lemma == "كُلِّيَّة_1" || words[0].lemma == "كُلْوَة_1"
))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || words[2].pos == "adj" || (words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].prc1 == "li_prep" || words[3].pos == "adj"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].prc1 == "li_prep" || words[4].pos == "noun"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" || words[2].pos == "noun_prop" || words[2].prc1 == "li_prep"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "adj" || words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") || words[3].pos == "noun_prop"))
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
                                else if (words.Count > 1 && (words[1].word != "font"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }


                            }
                            else if (words.Count > 1 && (words[1].pos == "noun"))
                            {

                                if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc2 == "wa_conj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {

                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font") || words[4].pos == "adj_num"))
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
                                else if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc2 == "wa_conj" && words[4].prc0 == "Al_det"))
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
                                    diacLineWords[loc + 1] += "</font>";
                                }
                            }
                        }
                    }
                    #endregion
                    #endregion
                    #region Line
                    //Line
                    #region Canal
                    /// Canal tagging
                    else if (triggerWordTag == "neflc".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "قَناة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "abbrev"))
                                {

                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "abbrev"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else if (words.Count > 2 && (words[2].pos == "abbrev" && words[3].pos == "abbrev" && words[4].pos == "adj"))
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
                            else if (words.Count > 1 && (words[1].pos == "noun_prop" && words[2].pos == "adj" && words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 4 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun_prop"))
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
                    }
                    #endregion
                    ///// Tunnel tagging
                    //else if (triggerWordTag == "neflt".ToUpper())
                    //{
                    //    // Do nothing
                    //}
                    #region Bridge
                    /// Bridge tagging
                    else if (triggerWordTag == "neflb".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "جِسْر_1" || words[0].lemma == "كُوبْرِي_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "abbrev"))
                                {

                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "abbrev" || words[3].pos == "adj"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else if (words.Count > 2 && (words[2].pos == "abbrev" && words[3].pos == "abbrev" && words[4].pos == "adj"))
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
                            else if (words.Count > 1 && (words[1].pos == "noun_prop" && words[2].pos == "adj" && words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 4 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") || words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") || words[4].pos == "noun_prop"))
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
                    }
                    #endregion
                    #endregion
                    ////Other tagging

                    //else if (triggerWordTag == "nefo".ToUpper())
                    //{

                    //}
                    #endregion
                    #region 9-Location
                    //GPE
                    /// Country tagging
                    #region Country
                    else if (triggerWordTag == "nelgo".ToUpper())
                    {

                        if (words.Count > 0 && (words[0].lemma == "بَلَد_1" || words[0].lemma == "جُمْهُورِيَّة_1" || words[0].lemma == "دَوْلَة_1" || words[0].lemma == "مَمْلَكَة_1" || words[0].lemma == "وِلايَة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || (words[2].pos == "adj" && words[2].prc0 == "Al_det")))
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
                            else if (words.Count > 1 && (words[1].pos == "adj" && words[1].prc0 == "Al_det"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || (words[2].pos == "adj" && words[2].prc0 == "Al_det")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else if (words.Count > 2 && (words[2].pos == "noun_prop"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            //else if (words.Count > 2 && (words[2].prc2 == "0" && words[2].prc3 == "0" && words[1].prc3 == "0"))
                            //{
                            //    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            //    diacLineWords[loc + 2 - 1] += "</font>";
                            //}
                        }


                    }
                    #endregion
                    /// City tagging
                    #region city
                    else if (triggerWordTag == "nelgc".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مُدِين_1" || words[0].lemma == "مَدِينَة_1" || words[0].lemma == "مُحافِظ_1" || words[0].lemma == "وِلايَة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det") || (words[1].pos == "adj" && words[1].prc0 == "Al_det") || words[1].pos == "verb"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
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

                        }

                    }
                    #endregion
                    /// Village tagging\
                    #region Village
                    else if (triggerWordTag == "nelgv".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "قَرْيَة_1" || words[0].lemma == "بَلْدَة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc3 == "0" && words[1].prc2 == "0" && words[1].prc1 == "0" && words[1].prc0 == "Al_det")))
                            {
                                if (words[1].word == "font")
                                { }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                        }
                    }
                    #endregion
                    /// Street tagging
                    else if (triggerWordTag == "nelgs".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "شارِع_1" || words[0].lemma == "طَرِيق_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc3 == "0" && words[1].prc2 == "0" && words[1].prc1 == "0" && words[1].prc0 == "Al_det")))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font")))
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

                    }
                    /// Other tagging
                    else if (triggerWordTag == "nelgt".ToUpper())
                    {

                    }
                    #region Region
                    // Region
                    ///Continenntal tagging
                    else if (triggerWordTag == "nelrc".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "قارَّة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                    }
                    ///Domestic tagging
                    else if (triggerWordTag == "nelrd".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مِنْطَقَة_1"   || words[0].lemma == "وَسَط_1" || words[0].lemma == "شَمال_1" || words[0].lemma == "جَنُوب_1" || words[0].lemma == "شَرْق_1" || words[0].lemma == "صَعِيد_1" || words[0].lemma == "إِقْلِيم_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det" && words[1].prc1 == "0" && words[1].prc2 == "0" && words[1].prc3 == "0") | (words[1].pos == "adj" && words[1].prc0 == "Al_det" && words[1].prc1 == "0" && words[1].prc2 == "0" && words[1].prc3 == "0" && words[0].prc0 == "Al_det")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && (words[1].lemma == "وَسَط_1" || words[1].lemma == "شَمال_1" || words[1].lemma == "جَنُوب_1" || words[1].lemma == "شَرْق_1" || words[1].lemma == "غَرْبِيّ_1" || words[1].lemma == "شَرْقِيّ_1" || words[1].lemma == "جَنُوبِيّ_1" || words[1].lemma == "شَرْقِيّ_1"))
                            {

                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }


                        }
                        else if (words.Count > 0 && (words[0].lemma == "شَمالِيّ_1" || words[0].lemma == "شَمالِيّ_1" || words[0].lemma == "أَوْسَط_1" || words[0].lemma == "شَرْقِيّ_1" || words[0].lemma == "جَنُوبِيّ_1" || words[0].lemma == "شَرْقِيّ_1"))
                        {
                            var percedingSentence = string.Empty;
                            var percedingLocation = loc <= 5 ? 0 : loc - 5;


                            for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                            {
                                percedingSentence += (diacLineWords[perItemIndex] + " ");
                            }
                            percedingSentence = percedingSentence.Replace("<", "");
                            percedingSentence = percedingSentence.Replace(">", "");
                            var percWords = new List<WordInfoItem>();
                            if (percedingSentence != string.Empty)
                            {
                                percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                                percWords.Reverse();
                            }
                            if (percWords.Count > 0 && (percWords[0].pos == "" || percWords[0].pos == "noun_prop"))
                            {
                                diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }


                    }
                    #endregion
                    //Geological Reg
                    #region mountain
                    /// Mountain tagging
                    else if (triggerWordTag == "nelcm".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "جَبَل_1" || words[0].lemma == "جَبَل_2" || words[0].lemma == "مُرْتَفَع_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det") || (words[1].pos == "adj" && words[1].prc0 == "Al_det")))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
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

                        }
                        else if (words.Count > 0 && (words[0].lemma == "سِلْسِلَة_1"))
                        {
                            if (words.Count > 1 && (words[1].lemma == "جَبَل_1" || words[1].lemma == "جَبَل_2"))
                            {

                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }

                            }
                        }
                    }
                    #endregion
                    #region Island
                    /// Island tagging
                    else if (triggerWordTag == "nelci".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "جَزِيرَة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det") || (words[1].pos == "adj" && words[1].prc0 == "Al_det")))
                            {

                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
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
                            else if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 3 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && (words[0].prc0 != "Al_det"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        else if (words.Count > 0 && (words[0].lemma == "شِبْه_1"))
                        {
                            if (words.Count > 1 && (words[1].lemma == "جَزِيرَة_1"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                            }
                        }

                    }
                    #endregion
                    #region Revier
                    /// River tagging
                    else if (triggerWordTag == "nelcr".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "نَهْر_1" || words[0].lemma == "وادِي_1" || words[0].lemma == "نُهَيْر_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && (words[0].prc0 != "Al_det"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                        else if ((words.Count > 0 && (words[0].lemma == "ضِفاف_1")))
                        {
                            if (words.Count > 1 && (words[1].lemma == "نَهْر_1" || words[1].lemma == "وادِي_1" || words[1].lemma == "نُهَيْر_1"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || (words[2].pos == "noun" && words[1].prc0 == "Al_det")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                        }
                    }
                    #endregion
                    #region lake
                    /// Lake tagging
                    else if (triggerWordTag == "nelcl".ToUpper())
                    {

                        if (words.Count > 0 && (words[0].lemma == "بُحَيْرَة_1" || words[0].lemma == "حَيْرَة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || words[1].pos == "noun"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                    }
                    #endregion
                    #region sea
                    /// Sea tagging
                    else if (triggerWordTag == "nelcs".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "بَحْر_1" || words[0].lemma == "حَرّ_1" || words[0].lemma == "مُحِيط_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det") || (words[1].pos == "adj" && words[1].prc0 == "Al_det")))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det"))
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

                        }
                        else if ((words.Count > 0 && (words[0].lemma == "ساحِل_1")))
                        {
                            if (words.Count > 1 && (words[1].lemma == "بَحْر_1" || words[1].lemma == "حَرّ_1" || words[1].lemma == "مُحِيط_1"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font") || (words[2].pos == "noun" && words[2].prc0 == "Al_det") || (words[2].pos == "adj" && words[2].prc0 == "Al_det")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                        }
                    }
                    #endregion
                    #region Bay
                    /// Bay tagging
                    else if (triggerWordTag == "nelcb".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "خَلِيج_2" || words[0].lemma == "خَلِيج_1" || words[0].lemma == "مَضِيق_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det") || (words[1].pos == "adj" && words[1].prc0 == "Al_det")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                    }
                    #endregion
                    #region other
                    /// Other tagging
                    else if (triggerWordTag == "nelco".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "قَناة_1" || words[0].lemma == "تَلّ_2" || words[0].lemma == "شَلّال_1" || words[0].lemma == "نَبْع_1" || words[0].lemma == "صَحْراء_1" || words[0].lemma == "هَضْبَة_1" || words[0].lemma == "أَرْض_1" || words[0].lemma == "مَحْمِيّ_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det") || (words[1].pos == "adj" && words[1].prc0 == "Al_det")))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }

                        }
                    }
                    #endregion
                    #region Astral body
                    // Astral body tagging
                    else if (triggerWordTag == "nelb".ToUpper())
                    {
                        if (words.Count > 1 && (words[1].lemma == "شَمْس_1" || words[1].lemma == "قَمَر_1"))
                        {
                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                            diacLineWords[loc + 2 - 1] += "</font>";
                        }
                    }
                    #endregion
                    // Address
                    ///Url tagging
                    else if (triggerWordTag == "nelau".ToUpper())
                    {

                    }
                    ///Email tagging
                    else if (triggerWordTag == "nelae".ToUpper())
                    {
                        //Do nothing
                    }
                    /// Phone number tagging
                    else if (triggerWordTag == "nelah".ToUpper())
                    {
                        //Do nothing
                    }
                    /// Postal address tagging
                    else if (triggerWordTag == "nelap".ToUpper())
                    {
                        //Do nothing
                    }
                    #endregion
                    #region 10-Organization
                    ////المفروض فالاورجنيزين اعدور فالكلمات علي الlemmas لكل نوع وبعدين اصنفه وبعد كده لو معرفتش انهي واحد احطه other
                    #region International Org Tagging
                    else if (triggerWordTag == "neoi".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "عالَمِيّ_1" || words[0].lemma == "دُوَلِيّ_1" || words[0].lemma == "مَجْلِس_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun" && (words[1].lemma == "عالَمِيّ_1" || words[1].lemma == "دُوَلِيّ_1")))
                            {
                                if (words.Count > 3 && (words[2].pos == "adj" && words[3].pos == "adj" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && (words[2].lemma == "عالَمِيّ_1" || words[2].lemma == "دُوَلِيّ_1" || words[3].lemma == "عالَمِيّ_1" || words[3].lemma == "دُوَلِيّ_1")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && words[3].pos == "adj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && words[3].pos == "noun" && words[3].prc2 == "wa_conj" && words[4].pos == "adj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                }
                            }
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 3 && (words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && words[3].pos == "adj"))
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
                            else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                        }

                    }
                    #endregion
                    #region Sports Org Tagging
                    else if (triggerWordTag == "neos".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "كَرّ_1" || words[0].lemma == "رِياضِيّ_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun" && (words[1].lemma == "كَرّ_1" || words[1].lemma == "رِياضِيّ_1")))
                            {
                                if (words.Count > 3 && (words[2].pos == "adj" && words[3].pos == "adj" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && (words[2].lemma == "كَرّ_1" || words[2].lemma == "رِياضِيّ_1" || words[3].lemma == "كَرّ_1" || words[3].lemma == "رِياضِيّ_1")))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                            }
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && words[3].pos == "adj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && words[3].pos == "noun" && words[3].prc2 == "wa_conj" && words[4].pos == "adj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                }
                            }
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc2 == "wa_conj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det" && words[3].pos == "adj"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }

                                }
                                else if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && (words[1].pos == "abbrev"))
                            {
                                if (words.Count > 2 && (words[2].pos == "abbrev"))
                                {

                                    if (words.Count > 3 && (words[3].pos == "abbrev"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "abbrev"))
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

                                //|| words[4].lemma == "كَرّ_1" || words[4].lemma == "رِياضِيّ_1"
                            }
                        }
                    }
                    #endregion
                    #region Corporation
                    //Corportion Tagging
                    else if (triggerWordTag == "neoc".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "دار_1" || words[0].lemma == "مَطْبَع_1")
|| words[0].lemma == "دِيوان_1"
|| words[0].lemma == "ناشِر_1"
|| words[0].lemma == "شَرِكَة_1"
|| words[0].lemma == "مَحَطَّة_1"
|| words[0].lemma == "وِكالَة_1"
|| words[0].lemma == "مَصْنَع_1"
|| words[0].lemma == "مَحْكَمَة_1"
|| words[0].lemma == "هَيْئَة_1"
|| words[0].lemma == "مَجْلِس_1"
|| words[0].lemma == "لَجْنَة_1"
|| words[0].lemma == "ٱِتِّحاد_1"
)
                        {

                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "noun_prop" || words[2].pos == "abbrev"))
                                {

                                    if (words.Count > 3 && (words[3].pos == "noun_prop" || (words[3].pos == "" && words[3].word != "font") || words[3].pos == "abbrev"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop" || (words[4].pos == "" && words[4].word != "font")))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else if (words.Count > 2 && (words[2].pos == "abbrev" && words[3].pos == "abbrev" && words[4].pos == "adj"))
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
                            else if (words.Count > 1 && (words[1].pos == "noun_prop" && words[2].pos == "adj" && words[3].pos == "adj" && words[3].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 4 - 1] += "</font>";
                            }
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font") || words[2].pos == "adj"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font") && words[2].pos != ""))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font") && words[2].pos != ""))
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
                            else if (words.Count > 1 && (words[1].pos == "abbrev"))
                            {
                                if (words.Count > 2 && (words[2].pos == "abbrev"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "abbrev"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "abbrev" || words[4].pos == "noun_prop"))
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
                            else if (words.Count > 2 && (words[1].pos == "adj" && words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                            {
                                if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
                                {
                                    if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                            else if (words.Count > 2 && ((words[1].pos == "" && words[1].word != "font") && words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                            {

                                if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc2 == "wa_conj" && words[3].prc0 == "Al_det"))
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

                            else if (words.Count > 1 && (words[1].pos == "adj" && words[1].prc0 == "Al_det"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det" && words[1].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" || words[3].pos == "noun_prop" || words[3].pos == "noun") && words[3].prc1 == "li_prep")
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc2 == "wa_conj"))
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
                            else if (words.Count > 0 && (words[0].pos == "noun"))
                            {
                                if (words.Count > 1 && (words[1].pos == "adj_comp"))
                                {

                                    if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                                else if (words.Count > 2 && (words[1].pos == "noun" && words[2].pos == "adj_comp" && words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                                else if (words.Count > 2&& (words[1].pos == "noun" && words[2].pos == "noun" && words[2].prc1 == "li_prep" && words[2].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                                else if (words.Count >2 && (words[1].pos == "noun" && words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                                else if (words.Count > 2 && (words[1].pos == "noun"))
                                {
                                    if (words.Count >3 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")) || (words[3].pos == "adj" && words[3].prc0 == "Al_det") || (words[2].pos == "adj" && words[2].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                                else if (words.Count > 1 && (words[1].pos == "adj" || words[1].pos == "adj_comp"))
                                {

                                    if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }
                                
                                                              
                                
                                }

                            }
                            else if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 3 && (words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")) || (words[3].pos == "adj" && words[3].prc0 == "Al_det") || (words[2].pos == "adj" && words[2].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                    }
                    #endregion

                    #region political
                    //Political Org
                    #region Government
                    ///Government Tagging
                    else if (triggerWordTag == "neopg".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "حُكُومَة_1" || words[0].lemma == "وِزارَة_1"))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 3 && (words[3].pos == "noun" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc2 == "wa_conj" && words[2].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                                else if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    if (words.Count > 2 && (words[2].prc0 == "Al_det" && (words[3].pos == "" && words[3].word != "font")))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 2 && (words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }

                                }
                                else if (words.Count > 2 && (words[2].pos == "prep" && words[2].bw == "EalaY/PREP" && words[3].pos == "noun"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj"))
                                {
                                    if (words.Count > 1 && (words[1].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
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
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 1 && (words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font")))
                            {

                                if (words.Count > 2 && (words[2].pos == "noun_prop" || ((words[2].pos == "" && words[2].word != "font") && words[1].pos != "")))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
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
                                    if (words[1].word != "font")
                                    {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }

                            }
                        }
                    }
                    #endregion
                    #region military
                    ///Military Tagging
                    else if (triggerWordTag == "neopm".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "حَرْبِيّ_1" || words[0].lemma == "تَحْرِير_1"
|| words[0].lemma == "سِلاح_1"
|| words[0].lemma == "رُكْن_1"
|| words[0].lemma == "مِدْفَعِيّ_1"
|| words[0].lemma == "قُوَّة_1"
|| words[0].lemma == "حَرَس_1"
|| words[0].lemma == "وَطَنِيّ_1"
|| words[0].lemma == "جَيْش_1"
|| words[0].lemma == "دِفاع_1"
|| words[0].lemma == "أَمْن_1"
|| words[0].lemma == "مُخابَرَة_1"
|| words[0].lemma == "مُوساد_1"
|| words[0].lemma == "جِهاد_1"
|| words[0].lemma == "سَلام_1"
))
                        {
                            if (words.Count > 1 && (words[1].pos == "noun"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[2].prc0 == "Al_det" && words[3].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                    else if (words.Count > 1 && (words[1].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 3 - 1] += "</font>";
                                    }


                                }
                                else if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                                    else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc1 == "li_prep"))
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
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }

                            }
                            else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                            {
                                if (words.Count > 2 && ((words[2].pos == "" && words[2].word != "font")))
                                {
                                    if (words.Count > 3 && ((words[3].pos == "" && words[3].word != "font")))
                                    {
                                        if (words.Count > 4 && ((words[4].pos == "" && words[4].word != "font")))
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
                            else if (words.Count > 1 && (words[1].pos == "adj"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                                else if (words.Count > 1 && (words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "noun_prop"))
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
                                    //diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    //diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words.Count > 1 && (words[1].pos == "adj_num"))
                            {
                                if (words.Count > 2 && (words[2].pos == "adj" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }


                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc +2- 1] += "</font>";
                                }

                            }
                        }
                    }
                    #endregion
                    #region other
                    ///Other Tagging
                    else if (triggerWordTag == "neopo".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "جَمْعِيَّة_1" || words[0].lemma == "حِزْب_1" || words[0].lemma == "مُنَظَّمَة_1" || words[0].lemma == "تَنْظِيم_1" || words[0].lemma == "جَماعَة_1" || words[0].lemma == "كَتِيبَة_1"))
                        {
                            if (words.Count > 1 && ((words[1].pos == "adj" && words[1].prc0 == "Al_det") || words[1].pos == "noun_prop" || (words[1].pos == "" && words[1].word != "font") || (words[1].pos == "noun" && words[1].prc0 == "Al_det")))
                            {

                                if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc1 == "li_prep"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det"))
                                    {

                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                                else if (words.Count > 2 && (words[1].prc2 != "wa_conj" && words[2].pos == "adj" && words[1].prc0 == "Al_det" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].prc2 != "wa_conj" && words[1].prc2 != ""))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                        }
                    }
                    #endregion
                    #endregion
                    //Other Tagging
                    else if (triggerWordTag == "neoo".ToUpper())
                    {
                        if (words.Count > 1 && (words[1].pos == "noun" ))
                        {
                            if (words.Count > 2)
                            {
                                if ((words[2].pos == "noun_prop" || (words[2].pos == "" && words[2].word != "font")) || (words[2].pos == "adj" && words[2].prc0 == "Al_det") || (words[2].pos == "adj" && words[2].prc0 == "Al_det") || (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                {
                                    if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det") || (words[3].pos == "adj_comp"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "adj" && words[4].prc0 == "Al_det"))
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
                                else if (words[1].prc0 == "Al_det")
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                        }
                        else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 2 - 1] += "</font>";
                        }
                    }
                    #endregion


                    #region 13-God
                    //God tagging
                    else if (triggerWordTag == "neg".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].pos == "noun_prop"))
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }
                        else if (words.Count > 0 && (words[0].pos == "noun"))
                        {
                            if (words.Count > 0 && (words[0].pos == "noun_prop"))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else if (words.Count > 0 && (words[0].pos == ""))
                            {
                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }
                            else { }

                        }
                        else
                        {

                        }
                    }
                    #endregion
                    #region 14-Job
                    //Job tagging
                    else if (triggerWordTag == "nej".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "عَمَل_1" || words[0].lemma == "ٱِشْتَغَل_1" || words[0].lemma == "عَيْن_1" || words[0].lemma == "أَعان_1" || words[0].lemma == "عَيَّن_1"))
                        {
                            if (words.Count > 1 && (words[1].lemma == "دُكْتُور_1"
                                || words[1].lemma == "خَلِيفَة_1"
                                || words[1].lemma == "شَيْخ_1"

                                || words[1].lemma == "مُلْك_1"

                                || words[1].lemma == "مُهَنْدِس_1"
                                || words[1].lemma == "زَعِيم_1"
                                || words[1].lemma == "رَئِيس_1"
                                || words[1].lemma == "فَرِيق_1"
                                || words[1].lemma == "طَيّار_1"
                                || words[1].lemma == "مُلازِم_1"
                                || words[1].lemma == "لِواء_1"
                                || words[1].lemma == "فَقِيه_1"
                                || words[1].lemma == "مُفَسِّر_1"
                                || words[1].lemma == "عَلّام_1"
                                

                                
                                || words[1].lemma == "ناقَد_1"
                                || words[1].lemma == "نَبِيّ_1"
                               
                                || words[1].lemma == "جِنِرال_1"
                                || words[1].lemma == "قائِد_1"
                                || words[1].lemma == "كُولُونِيل_1"
                                || words[1].lemma == "مُؤَرِّخ_1"
                                || words[1].lemma == "كَوَّن_1"
                                || words[1].lemma == "سائِق_1"
                                || words[1].lemma == "عَقِيد_1"
                                || words[1].lemma == "خُدَيْوِيّ_1"
                                || words[1].lemma == "أَفَنْدِيّ_1"
                                || words[1].lemma == "مُوسِيقار_1"
                                || words[1].lemma == "خَبِير_1"
                                || words[1].lemma == "مُؤَلَّف_2"
                                || words[1].lemma == "إِمْبِراطُور_1"
                                || words[1].lemma == "قُنْصُل_1"
                                || words[1].lemma == "نَحّات_1"
                                || words[1].lemma == "والِي_1"
                                || words[1].lemma == "قاضِي_1"
                                || words[1].lemma == "نائِب_1"
                                || words[1].lemma == "قِدِّيس_1"
                                || words[1].lemma == "مُشِير_2"
                                || words[1].lemma == "مُمَثِّل_1"
                                || words[1].lemma == "مُطْرِب_1"
                                || words[1].lemma == "شاعِر_1"


                                || words[1].lemma == "وَزِير_1"
                                || words[1].lemma == "سِناتُور_1"
                                || words[1].lemma == "مَمْلُوك_2"
                                || words[1].lemma == "مُفْتِي_1"
                                || words[1].lemma == "باي_1"
                                || words[1].lemma == "شَهِيد_1"
                                || words[1].lemma == "خَطِيب_1"
                                || words[1].lemma == "مُدَرِّب_1"
                                || words[1].lemma == "مُفَكِّر_1"
                                || words[1].lemma == "أَدِيب_2"
                                || words[1].lemma == "داعِي_3"
                                || words[1].lemma == "حافِظ_2"
                                || words[1].lemma == "مُقْرِئ_1"
                                || words[1].lemma == "مُجاهِد_1"
                                || words[1].lemma == "مُفَكِّر_1"
                                || words[1].lemma == "رَبّاع_1"
                                || words[1].lemma == "كابْتِن_1"
                                || words[1].lemma == "عاهِل_1"
                                || words[1].lemma == "مُعَلَّق_1"
                                || words[1].lemma == "صُحُفِيّ_1"
                                || words[1].lemma == "عَدّاء_1"
                                || words[1].lemma == "حارِس_1"
                                    || words[1].lemma == "خَلِيفَة_2"
                                    || words[1].lemma == "أُسْتاذ_1"))
                            {
                                diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                diacLineWords[loc + 1 - 1] += "</font>";

                            }
                            else { }
                        }
                        else if (words.Count > 0 && (words[0].lemma == "مُدِير_1" || words[0].lemma == "وَكَيْلِ" || words[0].lemma == "مُفَتِّش_1"))
                        {
                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                            diacLineWords[loc + 1 - 1] += "</font>";
                        }
                        else if (words.Count > 1 && ((words[1].lemma == "دُكْتُور_1"

                                || words[1].lemma == "مُهَنْدِس_1"

                                || words[1].lemma == "رَئِيس_1"

                                || words[1].lemma == "طَيّار_1"
                                || words[1].lemma == "مُلازِم_1"
                                || words[1].lemma == "لِواء_1"


                                || words[1].lemma == "عالَم_1"
                                || words[1].lemma == "برُوفِسُور_1"
                                || words[1].lemma == "ناقَد_1"
                                || words[1].lemma == "نَبِيّ_1"
                                || words[1].lemma == "فارِس_1"
                                || words[1].lemma == "جِنِرال_1"
                                || words[1].lemma == "قائِد_1"
                                || words[1].lemma == "كُولُونِيل_1"
                                || words[1].lemma == "مُؤَرِّخ_1"
                                || words[1].lemma == "كَوَّن_1"
                                || words[1].lemma == "سائِق_1"
                                || words[1].lemma == "عَقِيد_1"
                                || words[1].lemma == "خُدَيْوِيّ_1"
                                || words[1].lemma == "أَفَنْدِيّ_1"
                                || words[1].lemma == "مُوسِيقار_1"
                                || words[1].lemma == "خَبِير_1"
                                || words[1].lemma == "مُؤَلَّف_2"
                                || words[1].lemma == "إِمْبِراطُور_1"
                                || words[1].lemma == "قُنْصُل_1"
                                || words[1].lemma == "نَحّات_1"
                                || words[1].lemma == "والِي_1"
                                || words[1].lemma == "قاضِي_1"
                                || words[1].lemma == "نائِب_1"
                                || words[1].lemma == "قِدِّيس_1"
                                || words[1].lemma == "مُشِير_2"
                                || words[1].lemma == "مُمَثِّل_1"
                                || words[1].lemma == "مُطْرِب_1"
                                || words[1].lemma == "شاعِر_1"


                                || words[1].lemma == "وَزِير_1"
                                || words[1].lemma == "سِناتُور_1"
                                || words[1].lemma == "مَمْلُوك_2"
                                || words[1].lemma == "مُفْتِي_1"
                                || words[1].lemma == "باي_1"
                                || words[1].lemma == "شَهِيد_1"
                                || words[1].lemma == "خَطِيب_1"
                                || words[1].lemma == "مُدَرِّب_1"
                                || words[1].lemma == "مُفَكِّر_1"
                                || words[1].lemma == "أَدِيب_2"

                                || words[1].lemma == "مُعَلَّق_1"
                                || words[1].lemma == "صُحُفِيّ_1"
                                || words[1].lemma == "عَدّاء_1"
                                || words[1].lemma == "حارِس_1"
                                    || words[1].lemma == "خَلِيفَة_2"
                                    || words[1].lemma == "أُسْتاذ_1") && (words[2].pos != "noun_prop")))
                        {
                            if (words.Count > 2 && (words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                            {
                                if (words.Count > 3 && (words[3].pos == "adj" && words[3].prc0 == "Al_det"))
                                {

                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 4 - 1] += "</font>";

                                }
                                else if (words.Count > 3 && (words[3].pos == "noun" && words[3].prc0 == "Al_det" && words[3].prc2 == "wa_conj"))
                                {
                                    if (words.Count > 4 && (words[4].pos == "noun" && words[4].prc0 == "Al_det" && words[4].prc2 == "wa_conj"))
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


                            else if (words.Count > 1 && (words[1].prc2 != "wa_conj"))
                            {
                                diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }

                        }

                    }
                    #endregion
                    #region 15-Nationality
                    //Nationality tagging
                    else if (triggerWordTag == "nea".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "جِنْسِيَّة_1"))
                        {
                            var percedingSentence = string.Empty;
                            var percedingLocation = loc <= 5 ? 0 : loc - 5;

                            for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                            {
                                percedingSentence += (diacLineWords[perItemIndex] + " ");
                            }
                            var percWords = new List<WordInfoItem>();
                            if (percedingSentence != string.Empty)
                            {
                                percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                                percWords.Reverse();
                            }
                            if (words.Count > 1 && (words[1].pos == "adj" && (words[1].lemma.Contains("يّ_1") || words[1].lemma.Contains("يّ_2") || words[1].lemma.Contains("يّ_3") || words[1].lemma.Contains("يّ_1"))))
                            {

                                diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                diacLineWords[loc + 2 - 1] += "</font>";

                            }
                            else if (percWords.Count > 0 && (percWords[0].pos == "adj" && (percWords[0].lemma.Contains("يّ_1") || percWords[0].lemma.Contains("يّ_2") || percWords[0].lemma.Contains("يّ_3"))))
                            {
                                diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                diacLineWords[loc + 1 - 1] += "</font>";
                            }

                        }
                    }
                    #endregion
                    #region 1-Timex
                    //Timex Tagging
                    //Timex Time tagging
                    // اي digit مكون من 4 ارقام وبعديه م او ه هوه فالاغلب تاريخ##########################
                    else if (triggerWordTag == "nett".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "مَساء_1" || words[0].lemma == "صَباح_1" || words[0].lemma == "فَجْر_1" || words[0].lemma == "مَغْرِب_2" || words[0].lemma == "لَيْل_1" || words[0].lemma == "نَهار_1" || words[0].lemma == "عَصْر_1"))
                        {
                            var percedingSentence = string.Empty;
                            var percedingLocation = loc <= 5 ? 0 : loc - 5;


                            for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                            {
                                percedingSentence += (diacLineWords[perItemIndex] + " ");
                            }
                            percedingSentence = percedingSentence.Replace("<", "");
                            percedingSentence = percedingSentence.Replace(">", "");
                            var percWords = new List<WordInfoItem>();
                            if (percedingSentence != string.Empty)
                            {
                                percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                                percWords.Reverse();
                            }
                            if (percWords.Count >= 1)
                            {
                                if (percWords[0].pos == "digit" || percWords[0].pos == "adj_num")
                                {
                                    diacLineWords[loc-1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc ] += "</font>";
                                }
                                else if (percWords[1].pos == "adj_num" && percWords[0].pos == "noun_quant" && percWords[0].prc2 == "wa_conj")
                                {
                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc- 1];
                                    diacLineWords[loc ] += "</font>";
                                }
                                else if (percWords[1].gloss == "today")
                                {
                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc] += "</font>";
                                }
                            }

                        }


                    }
                    //Timex Date tagging

                    /////################## ق.م؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟؟
                    else if (triggerWordTag == "netd".ToUpper())
                    {
                        var percedingSentence = string.Empty;
                        var percedingLocation = loc <= 5 ? 0 : loc - 5;


                        for (int perItemIndex = percedingLocation; perItemIndex < loc; perItemIndex++)
                        {
                            percedingSentence += (diacLineWords[perItemIndex] + " ");
                        }
                        percedingSentence = percedingSentence.Replace("<", "");
                        percedingSentence = percedingSentence.Replace(">", "");
                        var percWords = new List<WordInfoItem>();
                        if (percedingSentence != string.Empty)
                        {

                            percWords = BL.MadaMiraHandler.Analyse(percedingSentence);
                            percWords.Reverse();
                        }

                        //بين عامي
                        if (words.Count > 0 && (words[0].gloss == "year" || words[0].gloss == "enactment;prescription"))
                        {
                            if (words.Count > 1 && (words[1].pos == "digit"))
                            {
                                //بين عامي
                                if (words.Count > 2 && (words[2].pos == "punc"))
                                {

                                    if (words.Count > 3 && (words[3].pos == "digit"))
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
                                else if (words.Count > 2 && (words[2].bw == "h/ABBREV"))
                                {

                                    if (words.Count > 3 && (words[3].pos == "punc" || words[3].pos == "prep"))
                                    {

                                        if (words.Count > 4 && (words[4].pos == "digit"))
                                        {
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }

                                    }
                                    else if (words.Count > 3 && (words[3].pos == "digit"))
                                    {
                                        if (words.Count > 4 && (words[4].pos == "m/ABBREV"))
                                        {
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 5 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
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
                        if (percWords.Count > 0 && percWords[0].pos == "digit")
                        {
                            if (words.Count > 1 && (words[1].pos == "noun_prop"))
                            {
                                if (words.Count > 2 && (words[2].bw == "m/ABBREV"))
                                {
                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && (words[2].pos == "digit"))
                                {
                                    if (words.Count > 2 && (words[2].bw == "m/ABBREV"))
                                    {
                                        diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                        diacLineWords[loc + 5 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                        diacLineWords[loc + 4 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }



                            }
                            else
                            {
                                diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                diacLineWords[loc + 2 - 1] += "</font>";
                            }


                        }
                        //الاول من سبتمبر عام 1948
                        else if (words.Count > 0 && (words[0].pos == "adj_num"))
                        {
                            if (words.Count > 1 && (words[1].bw == "min/PREP"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count == 5)
                                    {
                                        if (words.Count > 3 && (words[3].gloss == "year"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "digit"))
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
                                }
                                else
                                { }

                            }
                            else if (words.Count > 1 && (words[1].pos == "adj_num"))
                            {
                                if (words.Count > 2 && (words[2].bw == "min/PREP"))
                                {
                                    if (words.Count == 5)
                                    {
                                        if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                        else
                                        { }
                                    }
                                }
                                else
                                { }

                            }
                            else
                            { }


                        }
                        else if (words.Count > 0 && (words[0].pos == "noun_prop" || words[0].pos == ""))
                        {
                            //كانون الثاني\ يناير 1918

                            if (words.Count > 1 && (words[1].pos == "punc"))
                            {
                                if (words.Count > 2 && (words[2].pos == "noun_prop"))
                                {
                                    if (words.Count == 5)
                                    {
                                        if (words.Count > 3 && (words[3].pos == "year"))
                                        {
                                            if (words.Count > 4 && (words[4].pos == "digit"))
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
                                        else if (words.Count > 3 && (words[3].pos == "digit"))
                                        {
                                            diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }

                                        else if (words.Count > 3 && (words[3].pos == "prep"))
                                        {

                                            if (words.Count > 4 && (words[4].pos == "year"))
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
                                }
                                else
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }


                            }

                            else if (words.Count > 2 && (words[2].pos == "punc"))
                            {
                                if (words.Count > 3 && (words[3].pos == "noun_prop"))
                                {

                                    if (words.Count > 4 && (words[4].pos == "digit"))
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



                            else if (words.Count > 1 && (words[1].bw == "min/PREP"))
                            {

                                if (words.Count > 2 && (words[2].gloss == "year"))
                                {
                                    if (words.Count == 5)
                                    {
                                        if (words.Count > 3 && (words[3].pos == "digit"))
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
                                    diacLineWords[loc + 1 - 1] += "</font>";
                                }




                            }
                            else if (words.Count > 1 && (words[1].pos == "digit"))
                            {
                                if (words.Count > 2 && (words[2].bw == "m/ABBREV" || words[2].bw == "h/ABBREV"))
                                {
                                    if (percWords.Count > 0 && percWords[0].pos == "digit")
                                    {
                                        diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }
                                else
                                {
                                    if (percWords.Count > 0 && percWords[0].pos == "digit")
                                    {
                                        diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                    else
                                    {
                                        diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }
                                }


                            }
                            else if (words.Count > 0 && (words[0].pos == "noun_prop"))
                            {
                                if (percWords.Count > 0 && percWords[0].pos == "prep")
                                {
                                    if (percWords.Count > 1 && percWords[1].pos == "noun_num" || percWords[1].pos == "adj_num")
                                    {
                                        if (percWords.Count > 2 && ((percWords[2].pos == "noun_num" || percWords[2].pos == "adj_num") && percWords[1].prc2 == "wa_conj"))
                                        {
                                            diacLineWords[loc - 3] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 3];
                                            diacLineWords[loc + 1 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc - 2] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 2];
                                            diacLineWords[loc + 1 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    { }

                                }
                                else if (percWords.Count > 0 && percWords[0].pos == "digit")
                                {
                                    if (words.Count > 1 && (words[1].pos == "digit"))
                                    {
                                        if (words.Count > 2 && (words[2].bw == "m/ABBREV"))
                                        {
                                            diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 3];
                                            diacLineWords[loc + 4 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 3];
                                            diacLineWords[loc + 3 - 1] += "</font>";
                                        }



                                    }
                                    else
                                    {
                                        diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 3];
                                        diacLineWords[loc + 2 - 1] += "</font>";
                                    }


                                }



                            }

                            else
                            { }



                        }
                        //بتاريخ 30\1\1976
                        else if (words.Count > 0 && (words[0].gloss == "date;history"))
                        {
                            if (words.Count == 5)
                            {
                                if (words.Count > 1 && (words[1].pos == "digit" && words[2].pos == "punc" && words[3].pos == "digit" && words[4].pos == "punc"))
                                {

                                    diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                    diacLineWords[loc + 2 - 1] += "</font>";

                                }
                            }

                        }
                        else if (words.Count > 0 && (words[0].gloss == "/"))
                        {
                            if (words.Count > 1 && (words[1].pos == "digit"))
                            {
                                if (percWords.Count > 0 && (percWords[0].pos == "digit"))
                                {
                                    if (percWords.Count > 1 && (percWords[1].pos == "punc"))
                                    {
                                        if (percWords.Count > 2 && (percWords[2].pos == "digit"))
                                        {

                                            diacLineWords[loc - 3] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 3];
                                            diacLineWords[loc + 2 - 1] += "</font>";
                                        }
                                        else
                                        {
                                            diacLineWords[loc - 3] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 3];
                                            diacLineWords[loc + 1 - 1] += "</font>";
                                        }

                                    }
                                    else
                                    {
                                        //diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                        //diacLineWords[loc +2 - 1] += "</font>";
                                    }

                                }
                                else if (words.Count > 2 && words[2].pos == "punc" && words[3].pos == "digit")
                                {
                                    diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 2 && words[2].lemma == "مِيلاد_1")
                                {
                                    diacLineWords[loc + 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc + 1];
                                    diacLineWords[loc + 2] += "</font>";
                                }

                            }


                        }

                        else if (words.Count > 0 && (words[0].bw == "h/ABBREV" || words[0].bw == "m/ABBREV"))
                        {
                            if (words.Count == 5)
                            {
                                if (words.Count > 1 && (words[1].pos == "punc" && words[2].pos == "digit" && words[3].bw == "m/ABBREV"))
                                {

                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }

                                else if (words.Count > 1 && (words[1].pos == "punc" && words[2].pos == "digit" && words[3].bw == "m/ABBREV"))
                                {
                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc + 4 - 1] += "</font>";
                                }
                            }
                            else if (words.Count != 5)
                            {
                                if (words.Count > 1 && (words[1].pos == "punc" && words[2].pos == "digit"))
                                {
                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }


                        }

                        else if (words.Count > 0 && (words[0].bw == "q/ABBREV"))
                        {
                            if (words.Count == 3)
                            {
                                if (words.Count > 1 && (words[1].pos == "punc" && words[2].bw == "m/ABBREV"))
                                {
                                    diacLineWords[loc - 1] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc - 1];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }

                        }

                        else
                        { }

                    }
                    //Timex other tagging
                    #region Timex Other
                    else if (triggerWordTag == "neto".ToUpper())
                    {
                        if (words.Count > 0 && (words[0].lemma == "عَصْر_1" || words[0].lemma == "تَوْقِيت_1" || words[0].lemma == "قَرْن_1"))
                        {
                            if (words.Count >= 3)
                            {
                                if (words.Count > 1 && (words[1].pos == "adj" && words[1].prc0 == "Al_det" && words[2].pos == "adj" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font") && words[2].pos == "noun" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun" && words[1].prc0 == "Al_det" && words[2].pos == "adj" && words[2].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "adj_num" && words[2].pos == "adj"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 3 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "adj_comp" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun_quant"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "adj_comp"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "adj_num"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }
                            else if (words.Count == 2)
                            {
                                if (words.Count > 1 && (words[1].pos == "adj_comp" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun_quant"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun" && words[1].prc0 == "Al_det"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "noun_prop"))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && ((words[1].pos == "" && words[1].word != "font")))
                                {
                                    diacLineWords[loc ] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc ];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                                else if (words.Count > 1 && (words[1].pos == "adj_comp"))
                                {
                                    diacLineWords[loc] = "<font title='Trigger word-" + triggerWords[wordTagIndex].Tag.ToUpper() + "' style='color:" + triggerWords[wordTagIndex].Color + "'>" + diacLineWords[loc];
                                    diacLineWords[loc + 2 - 1] += "</font>";
                                }
                            }



                        }

                    }
                    #endregion
                    #endregion




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
