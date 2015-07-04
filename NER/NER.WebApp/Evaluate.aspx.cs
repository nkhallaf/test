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

namespace NER.WebApp
{
    public partial class Evaluate : System.Web.UI.Page
    {

        public class TagComparer : IEqualityComparer<Tags>
        {
            bool IEqualityComparer<Tags>.Equals(Tags x, Tags y)
            {
                return (x.TagWord.Equals(y.TagWord) && x.Tag.Equals(y.Tag));
            }

            int IEqualityComparer<Tags>.GetHashCode(Tags obj)
            {
                if (Object.ReferenceEquals(obj, null))
                    return 0;

                return obj.Tag.GetHashCode() + obj.TagWord.GetHashCode();
            }
        }
        public class Tags
        {
            public string Tag { get; set; }
            public string TagWord { get; set; }

            public bool Equals(Tags other)
            {

                //Check whether the compared object is null.
                if (Object.ReferenceEquals(other, null)) return false;

                //Check whether the compared object references the same data.
                if (Object.ReferenceEquals(this, other)) return true;

                //Check whether the products' properties are equal.
                return Tag == other.Tag && TagWord == other.TagWord;
            }
            public override int GetHashCode()
            {

                //Get hash code for the Name field if it is not null.
                int hashProductName = Tag == null ? 0 : Tag.GetHashCode();

                //Get hash code for the Code field.
                int hashProductCode = TagWord.GetHashCode();

                //Calculate the hash code for the product.
                return hashProductName ^ hashProductCode;
            }


        }


        public class ExportedTags
        {

            public string Tag { get; set; }
            public string TagWord { get; set; }
            public string Status { get; set; }

        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var result1 = new List<Tags>();
            var result2 = new List<Tags>();

            int counter = 0;
            string line;
            char[] splittingDelimeters = { '<', '>', ' ', '.', ',' };
            var SecondResult = false;


            if (!FileUpload1.HasFile)
            {
                return;


            }
            System.IO.StreamReader file = new StreamReader(FileUpload1.FileContent, Encoding.GetEncoding(1256));

            while ((line = file.ReadLine()) != null)
            {
                if (line != "<doc>" && line != "</doc>")
                {

                    if (line == "++++++++++++")
                    {
                        SecondResult = true;

                    }

                    var items = line.Split(splittingDelimeters, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i].Contains("ne") && items[i].Length <= 5)
                        {
                            // start tag    i 
                            for (int g = i + 1; g < items.Length; g++)
                            {
                                if (items[g] == "/" + items[i])
                                {
                                    // search for the colsed tag index  g
                                    var tagWords = string.Empty;
                                    for (int x = i + 1; x < g; x++)
                                    {
                                        tagWords = tagWords + " " + items[x];

                                    }
                                    // get the words 
                                    if (SecondResult)

                                        result2.Add(new Tags
                                        {
                                            Tag = items[i],
                                            TagWord = tagWords
                                        });

                                    else
                                        result1.Add(new Tags
                                        {
                                            Tag = items[i],
                                            TagWord = tagWords
                                        });
                                    i = g;
                                    break;
                                }


                            }

                        }


                    }



                }
                counter++;
            }

            file.Close();

            var exportedResult = new List<ExportedTags>();

            foreach (var item in result1)
            {
                if (result2.Contains(item, new TagComparer()))
                {
                    exportedResult.Add(new ExportedTags { Status = "Common", Tag = item.Tag, TagWord = item.TagWord });
                }
                else
                {
                    exportedResult.Add(new ExportedTags { Status = "Upper", Tag = item.Tag, TagWord = item.TagWord });
                }
            }

            foreach (var item in result2)
            {
                if (!result1.Contains(item, new TagComparer()))
                {
                    exportedResult.Add(new ExportedTags { Status = "Lower", Tag = item.Tag, TagWord = item.TagWord });
                }
            }




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
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;attachment;filename="+ FileUpload1.FileName +"-Evaluation.xls");
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

    }
}