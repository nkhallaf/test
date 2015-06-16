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


        public class Tags
        {
            public string Tag { get; set; }
            public string TagWord { get; set; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var result = new List<Tags>();

            int counter = 0;
            string line;
            char[] splittingDelimeters = { '<', '>', ' ', '.', ',' };



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
                                    result.Add(new Tags
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

            var dt = ToDataTable(result);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;attachment;filename=Evaluation.xls");
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