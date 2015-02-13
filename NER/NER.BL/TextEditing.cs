using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace NER.BL
{
    public class TextEditing
    {

        public static char[] DelToAddSpaceToIt = { '-', ':', '"', ';', '}', '{', '[', ']', '(', ')', '،', '.', '!', '?', '؟', '‘', '؛', '/' };
        public static char[] DelToSPlitWith = { ' ' };


        public static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }


        public static string RemoveSpaces(string word)
        {
            var ar = word.Split(DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);
            word = string.Empty;
            var words = ar.ToList();
            for (int i = 0; i < words.Count; i++)
            {
                if (i != words.Count - 1)
                {
                    word += words[i] + " ";
                }
                else
                {
                    word += words[i];
                }

            }
            return word;
        }


        public static bool IsExistInTheLine(string the_word, string pure_line)
        {

            var lineWords = pure_line.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);
            var tagWords = the_word.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);

            if (tagWords.Length == 1)
            {
                return lineWords.Any(item => IsTheWord(the_word, item));
            }
            /// here we get the fisrt word's location

            var startIndex = -1;
            for (int i = 0; i < lineWords.Length; i++)
            {
                if (IsTheWord(tagWords[0], lineWords[i]))
                {
                    startIndex = i;
                }
            }

            return lineWords.Length >= startIndex + tagWords.Length && tagWords[tagWords.Length - 1] == lineWords[startIndex + tagWords.Length - 1];
        }

        public static bool IsTheWord(string theWord, string thLineWord)
        {
            return thLineWord == theWord;
        }

        public static List<int> Getindex(string[] theLine, string theWord)
        {
            List<int> returnIndeces = new List<int>();
            var wordList = theWord.Split(DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);


            for (int i = 0; i < theLine.Length; i++)
            {
                if (wordList[0] == theLine[i] && wordList.Length + i <= theLine.Length)
                {
                    bool isTheWord = true;
                    for (int wordLenghtCounter = 0; wordLenghtCounter < wordList.Length; wordLenghtCounter++)
                    {
                        if (wordList[wordLenghtCounter] == theLine[i + wordLenghtCounter])
                        {
                            isTheWord = isTheWord ? true : false;
                        }
                        else
                        {
                            isTheWord = false;
                        }
                    }

                    if (isTheWord)
                    {

                        returnIndeces.Add(i);
                    }

                }

            }


            return returnIndeces;

        }


        internal static string FixSpaces(string line)
        {
            line = DelToAddSpaceToIt.Aggregate(line, (current, item) => current.Replace(item.ToString(), string.Concat(' ', item, ' ')));
            return RemoveSpaces(line);
        }
    }
}
