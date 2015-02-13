namespace NER.BL.Entites
{

   public class ConcordenceReturn
    {
        public ConcordenceReturn(int lineNo, string prevWord, string nextWord)
        {
            LineNo = lineNo;
            NextWord = nextWord;
            PreviousWord = prevWord;
        }

        public ConcordenceReturn(int lineNo, string otherWord, bool isNext)
        {
            if (isNext)
            {
                LineNo = lineNo;
                NextWord = otherWord;
                PreviousWord = "";
            }
            else
            {
                LineNo = lineNo;
                NextWord = "";
                PreviousWord = otherWord;


            }
        }

       public string Word { get; set; } 
       public int LineNo { get; set; }

       public string PreviousWord { get; set; }

       public string NextWord { get; set; }
    }

}