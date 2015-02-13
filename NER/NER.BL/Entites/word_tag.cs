namespace NER.BL.Entites
{
  public  class WordTag
    {
        public string Word;
        public string Tag;
        public string Color;

        public WordTag(string word, string tag, string color)
        {
            Word = word;
            Tag = tag;
            Color = color;

        }
        public WordTag()
        {
            Word = string.Empty;
            Tag = string.Empty;
            Color = string.Empty;
        }
    }


}