using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using Microsoft;

namespace WebApplication1
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            if (!FileUpload1.HasFile)
            {
                return;


            }
            string line;
           List<string> list = new List<string>();
            System.IO.StreamReader file = new StreamReader(FileUpload1.FileContent, Encoding.Default);
            while ((line = file.ReadLine()) != null)
            {

                list.Add(line);

            }
            
            file.Close();
            string Qnumber = "";
            string Question = "";
            string A = "";
            string B = ""; string C = ""; string D = "";
            string Ans = "";
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains("QUESTION"))
                {
                    ////// this is the file name 
                    Qnumber = list[i].ToString();
                    
                    
                    Question = "1. " + list[i + 1];
                    for (int h = i + 1; h < list.Count; h++)
                   {
                       if (list[h].StartsWith("A."))
                       {
                           string AnsA = list[h];
                           string[] FinalAnsA = AnsA.Split();
                           for (int X = 1; X < FinalAnsA.Length; X++)
                           {
                               A = A + " " + FinalAnsA[X];
                           }
                           A = "1. " + A;
                       }
                       else if (list[h].StartsWith("B."))
                       {
                           string AnsB = list[h];
                           string[] FinalAnsB = AnsB.Split();
                           for (int X = 1; X < FinalAnsB.Length; X++)
                           {
                               B =  B + " " + FinalAnsB[X];
                           }
                           B = "2. " + B;
                       }
                       else if (list[h].StartsWith("C."))
                       {
                           string AnsC = list[h];
                           string[] FinalAnsC = AnsC.Split();
                           for (int X = 1; X < FinalAnsC.Length; X++)
                           {
                               C = C + " " + FinalAnsC[X];
                           }
                           C = "3. " + C;
                       }
                       else if (list[h].StartsWith("D."))
                       {

                           string AnsD = list[h];
                           string[] FinalAnsD = AnsD.Split();
                           for (int X = 1; X < FinalAnsD.Length; X++)
                           {
                               D =  D + " " + FinalAnsD[X];
                             
                           }
                           D = "4. " + D;
                       }
                           ///////this is the answer
                        else if (list[h].StartsWith("Answer:"))
                       { string Answer = list[h];
                       string[] FinalAns = Answer.Split();
                        Ans = FinalAns[1];
                        
                        
                        }
                    }
                }
                CreateDocument(Qnumber, Question, A, B, C, D,Ans);
            }

           
            
               

        }
        private void CreateDocument(string FileName, string Question, string AnsA, string AnsB, string AnsC, string AnsD, string Answer) 
        {
            try
            {

               
                Microsoft.Office.Interop.Word._Document objDoc;
                

                //Create an instance for word app
                Microsoft.Office.Interop.Word.Application winword = new Microsoft.Office.Interop.Word.Application();
                
                        

                //Set status for word application is to be visible or not.
                winword.Visible = false;

                //Create a missing variable for missing value
                object missing = System.Reflection.Missing.Value;

                //Create a new document
                Microsoft.Office.Interop.Word.Document document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);
                object oMissing = System.Reflection.Missing.Value;
                object oEndOfDoc = "\\endofdoc";

                objDoc = winword.Documents.Add(ref oMissing, ref oMissing,
                ref oMissing, ref oMissing);

                int i = 5;
                int j = 1;
                Microsoft.Office.Interop.Word.Table objTable;
                Microsoft.Office.Interop.Word.Range wrdRng = objDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
                objTable = objDoc.Tables.Add(wrdRng, 5, 1, ref oMissing, ref oMissing);
                objTable.Range.ParagraphFormat.SpaceAfter = 7;

                objTable.Cell(1, 1).Range.Text = Question;
                objTable.Cell(2, 1).Range.Text = AnsA;
                objTable.Cell(3, 1).Range.Text = AnsB;
                objTable.Cell(4, 1).Range.Text = AnsC;
                objTable.Cell(5, 1).Range.Text = AnsD;


                objTable.Rows[1].Range.Font.Bold = 1;
                
                objTable.Borders.Shadow = false;
               

                //Save the document
                object filename = @"E:\" + FileName + ".docx";
                objDoc.SaveAs2(ref filename);
                objDoc.Close(ref missing, ref missing, ref missing);
                objDoc = null;
                winword.Quit(ref missing, ref missing, ref missing);
                winword = null;
                
            }
            catch (Exception ex)
            {
               
            }
        }
        private void CreateTableInWordDocument()
        {
            object oMissing = System.Reflection.Missing.Value;
            object oEndOfDoc = "\\endofdoc";
            Microsoft.Office.Interop.Word._Application objWord;
            Microsoft.Office.Interop.Word._Document objDoc;
            objWord = new Microsoft.Office.Interop.Word.Application();
            objWord.Visible = true;
            objDoc = objWord.Documents.Add(ref oMissing, ref oMissing,
                ref oMissing, ref oMissing);

            int i = 5;
            int j = 1;
            Microsoft.Office.Interop.Word.Table objTable;
            Microsoft.Office.Interop.Word.Range wrdRng = objDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            objTable = objDoc.Tables.Add(wrdRng, 5, 1, ref oMissing, ref oMissing);
            objTable.Range.ParagraphFormat.SpaceAfter = 7;

           
            objTable.Rows[1].Range.Font.Bold = 1;
            objTable.Rows[1].Range.Font.Italic = 1;
            objTable.Borders.Shadow = false;
           
        }


        ////Add header into the document
        //        foreach (Microsoft.Office.Interop.Word.Section section in document.Sections)
        //        {
        //            //Get the header range and add the header details.
        //            Microsoft.Office.Interop.Word.Range headerRange = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
        //            headerRange.Fields.Add(headerRange, Microsoft.Office.Interop.Word.WdFieldType.wdFieldPage);
        //            headerRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
        //            headerRange.Font.ColorIndex = Microsoft.Office.Interop.Word.WdColorIndex.wdBlue;
        //            headerRange.Font.Size = 10;
        //            headerRange.Text = "Header text goes here";
        //        }

        //        //Add the footers into the document
        //        foreach (Microsoft.Office.Interop.Word.Section wordSection in document.Sections)
        //        {
        //            //Get the footer range and add the footer details.
        //            Microsoft.Office.Interop.Word.Range footerRange = wordSection.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
        //            footerRange.Font.ColorIndex = Microsoft.Office.Interop.Word.WdColorIndex.wdDarkRed;
        //            footerRange.Font.Size = 10;
        //            footerRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
        //            footerRange.Text = "Footer text goes here";
        //        }

        //        //adding text to document
        //        document.Content.SetRange(0, 0);
        //        document.Content.Text = "This is test document " + Environment.NewLine;

        //        //Add paragraph with Heading 1 style
        //        Microsoft.Office.Interop.Word.Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
        //        object styleHeading1 = "Heading 1";
        //        para1.Range.set_Style(ref styleHeading1);
        //        para1.Range.Text = "Para 1 text";
        //        para1.Range.InsertParagraphAfter();

        //        //Add paragraph with Heading 2 style
        //        Microsoft.Office.Interop.Word.Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
        //        object styleHeading2 = "Heading 2";
        //        para2.Range.set_Style(ref styleHeading2);
        //        para2.Range.Text = "Para 2 text";
        //        para2.Range.InsertParagraphAfter();
    }
}
