using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Reflection;
using System.Text;
using System.IO;
using NER.BL;
namespace NER.WebApp
{
    public partial class WebForm1 : System.Web.UI.Page
    {
       
        protected void Page_Load(object sender, EventArgs e)
        {

        }
       
        protected void Button1_Click(object sender, EventArgs e)
        {
            var result1 = new List<Tags>();
            var result2 = new List<Tags>();

            int counter = 0;
            string line;
            char[] splittingDelimeters = { '<', '>', ' ',  ',' };
          
            //'.',

            if (!FileUpload1.HasFile)
            {
                return;


            }


            


            System.IO.StreamReader file = new StreamReader(FileUpload1.FileContent, Encoding.GetEncoding(1256));

            while ((line = file.ReadLine()) != null)
            {
                if (line != "<doc>" && line != "</doc>")
                {

                     var items = line.Split(splittingDelimeters, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < items.Length; i++)
                    {
                        var tagWords = string.Empty;
                        if (items[i].Contains("ne") && items[i].Length <= 5)
                        {
                            result1.Add(new Tags
                        {
                            Tag = " ",
                            TagWord = items[i]
                        });
                           
                        }
                        else
                        {
                            result1.Add(new Tags
                            {
                                Tag = " ",
                                TagWord = items[i]
                            });
                        }

                       
                    }



                }
                counter++;
              

            }

            while ((result1 != null))
            {
                for (int x = 0, y = 0; y < result1.Count - 1; y++, x++)
                {
                    
                        if (result1[x].TagWord.Contains("/ne"))
                        {
                            if (result1[x - 1].Tag.Contains("ne"))
                            {

                                result1[x-1].Tag =  result1[x-1].Tag + " + E "+ result1[x].TagWord;

                                result1.RemoveAt(x);
                            }
                            else
                            {
                                result1[x].Tag = "E  " + result1[x].Tag.Replace(result1[x].Tag, result1[x].TagWord.ToString());

                                result1[x].TagWord =  result1[x].TagWord.Replace(result1[x].TagWord, result1[y - 1].TagWord.ToString());
                                result1.RemoveAt(x - 1);
                               
                            }
                           
                            

                        }

                        else if (result1[x].TagWord.Contains("ne") && result1[x].TagWord.Contains("/") == false)
                        {

                            result1[x].Tag = "I  " + result1[x].Tag.Replace(result1[x].Tag, result1[x].TagWord.ToString());

                                result1[x].TagWord = result1[x].TagWord.Replace(result1[x].TagWord, result1[y + 1].TagWord.ToString());
                            
                            


                            result1.RemoveAt(x + 1);



                        }
                       
                    

                }
               
                break;
            }
            file.Close();

           
            while ((result1 != null))
            {
                for (int i = 0, h = 0; i < result1.Count - 1; h++, i++)
                {
                    if (result1[i].TagWord.Contains("ne") && result1[i].TagWord.Contains("/") == false)
                    {

                        result1[i].Tag = result1[i].Tag.Replace(result1[i].Tag, result1[i].TagWord.ToString());

                        result1[i].TagWord = result1[i].TagWord.Replace(result1[i].TagWord, result1[h + 1].TagWord.ToString());




                        result1.RemoveAt(i + 1);



                    }
                }
               
                break;
            }
            ////to analys و&& digit
          
            while ((result1 != null))
            {
                for (int i = 0, h = 0; i < result1.Count - 1; h++, i++)
                {
                    if (result1[i].TagWord.StartsWith("و"))
                    {
                        var Digit = new List<Tags>();
                        var checkword = result1[i].TagWord.ToString();
                        var words = BL.MadaMiraHandler.Analyse(checkword);
                        if (words.Count > 1 && (words[0].gloss == "and" && words[1].pos == "digit"))
                        {
                            result1[i].TagWord = words[0].word;
                            int x = i + 1;
                            Digit.Add(new Tags
                        {
                            Tag = " ",
                            TagWord = words[1].word
                        });

                            result1.InsertRange(x, Digit);
                        
                        }

                    }
                }

                break;
            }

            

            var exportedResult = result1;

            



            //var x1 = result1.Except(result2, new TagComparer()).Select(x => new ExportedTags { Status = "Upper", Tag = x.Tag, TagWord = x.TagWord }).ToList();
            //var x2 = result2.Except(result1, new TagComparer()).Select(x => new ExportedTags { Status = "Lower", Tag = x.Tag, TagWord = x.TagWord }).ToList();
            //var xCommon = result1.Intersect(result2, new TagComparer()).Select(x => new ExportedTags { Status = "Common", Tag = x.Tag, TagWord = x.TagWord }).ToList();

            //exportedResult.AddRange(x1);
            //exportedResult.AddRange(x2);
            //exportedResult.AddRange(xCommon);



            var dt = ToDataTable(exportedResult);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;attachment;filename=" + FileUpload1.FileName + "-NEs.xls");
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";




            System.IO.StringWriter stringWrite = new System.IO.StringWriter();

            System.Web.UI.HtmlTextWriter htmlWrite =
            new HtmlTextWriter(stringWrite);
            DataGrid grdExcel = new DataGrid();
            grdExcel.AllowPaging = false;
            grdExcel.DataSource = dt;
            grdExcel.DataBind();
            foreach (DataGridItem i in grdExcel.Items)
            {

                foreach (TableCell tc in i.Cells)
                    tc.Attributes.Add("class", "text");

            }
            grdExcel.RenderControl(htmlWrite);
            string style = @"<style> .text {  } </style> ";
            HttpContext.Current.Response.Write(style);
            HttpContext.Current.Response.Write(stringWrite.ToString());
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();




        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
          
            char[] splittingDelimeters = { '<', '>', ' ', '.', ',' };
             if (!FileUpload2.HasFile)
            {
                return;


            }


            
            System.IO.StreamReader file = new StreamReader(FileUpload2.FileContent, Encoding.GetEncoding(1256));
            var theFile = new List<string>();
            string line;


            while ((line = file.ReadLine()) != null)
            {

                line = WebApp.Helpers.TextEditing.FixSpaces(line);
                theFile.Add(line);

            }
            file.Close();

            //string[] lineWords = line.Split(TextEditing.DelToSPlitWith, StringSplitOptions.RemoveEmptyEntries);

            while ((theFile) != null)
            {
                var Final = new List<WordInfoItem>();;
                

                for (int item = 0; item < theFile.Count; item++)
                {
                    if (theFile[item].Length > 0)
                    {

                        var words = BL.MadaMiraHandler.Analyse(theFile[item].ToString());
                        for (int word = 0; word < words.Count; word++)
                        {
                        
                        Final.Add(words[word]);
                        
                        }
                        
                    }
                    else
                    { }
                }
                var exportedResult = Final;
                
                var dt = ToDataTable(exportedResult);
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.Charset = "UTF8";
                HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;attachment;filename=" + FileUpload2.FileName + "-MatLab.xls");
                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";




                System.IO.StringWriter stringWrite = new System.IO.StringWriter();

                System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
                DataGrid grdExcel = new DataGrid();
                grdExcel.AllowPaging = false;
                grdExcel.DataSource = dt;
                grdExcel.DataBind();
                foreach (DataGridItem i in grdExcel.Items)
                {

                    foreach (TableCell tc in i.Cells)
                        tc.Attributes.Add("class", "text");

                }
                grdExcel.RenderControl(htmlWrite);
                string style = @"<style> .text {  } </style> ";
                HttpContext.Current.Response.Write(style);
                HttpContext.Current.Response.Write(stringWrite);
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
              
            }
          

            

        }

       
    }
}