using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using NER.WebApp.Helpers;
using System.Text;
using NER.BL;

namespace NER.WebApp
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["ID"] != null)
            {
                var id = int.Parse(Session["ID"].ToString());


                if (!IsPostBack)
                {
                    RetreiveDocs(id);

                }


            }
            else
            {

                Response.Redirect("login.aspx");
            }








        }

        private void RetreiveDocs(int id)
        {
            var subPath = Server.MapPath("Docs\\") + id;

            bool exists = System.IO.Directory.Exists(subPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);
            else
            {
                DirectoryInfo d = new DirectoryInfo(subPath);
                var info = d.GetFiles("*.txt").ToList();

                RepeaterFiles.DataSource = info;
                RepeaterFiles.DataBind();

            }
        }

        private List<string> LoadTheFileText(string file)
        {
            try
            {
                var strm = new StreamReader(file, Encoding.GetEncoding(1256));
                var theFile = new List<string>();
                string line;


                while ((line = strm.ReadLine()) != null)
                {

                    line = WebApp.Helpers.TextEditing.FixSpaces(line);
                    theFile.Add(line);

                }
                strm.Close();

                return theFile;
            }
            catch
            {
                var str = new List<string> { "file not found     " };

                return str;
            }
        }

        protected void RepeaterFiles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {

            var theText = LoadTheFileText(e.CommandArgument.ToString());
            var fileName = e.CommandArgument.ToString().Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last();

            var result = BL.Colorizing.ColorizeTheText(theText);


            switch (e.CommandName)
            {
                case "Display":

                    Label1.Text = result;
                    LabelFileName.Text = fileName;
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "dialog", "LoadDialog();", true);
                    break;
                case "Download":
                    var resultReplaced = result.Replace("<font title='Tag-", "<").Replace("<font title='Trigger word-", "<").Replace("' style='color:", string.Empty).Replace("'>", ">");
                    var colors = BL.Colorizing.GetAllColors();
                    foreach (var color in colors)
                    {
                        resultReplaced = resultReplaced.Replace(color, string.Empty);
                    }



                    var lines = resultReplaced.Split(new[] { "</font>" }, StringSplitOptions.RemoveEmptyEntries);

                    var taggedDocument = "<Document>";

                    foreach (var line in lines)
                    {
                        if (line.Contains("<NE"))
                        {
                            var lineParts = line.Split(new[] { "<NE" }, StringSplitOptions.RemoveEmptyEntries);
                            var FirstPart = string.Empty;
                            var SecondPart = string.Empty;

                            if (lineParts.Count() == 2)
                            {
                                FirstPart = lineParts.First();
                            }
                            SecondPart = lineParts.Last();
                            var endTagLocation = SecondPart.IndexOf('>');
                            if (endTagLocation < 0)
                            {
                            }
                            else
                            {
                                var TagResume = SecondPart.Substring(0, endTagLocation);

                                var resultLine = line + "</NE" + TagResume + ">";

                                taggedDocument += resultLine;
                            }
                        }
                        else { taggedDocument += line; }
                    }
                    taggedDocument += "</Document>";
                    var subPath = Server.MapPath("DownloadCache\\");
                    System.IO.File.WriteAllText(subPath + "\\" + fileName.Split('.')[0] + ".xml", taggedDocument.Replace("<br/>", string.Empty));

                    Response.ContentType = "text/plain";
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName.Split('.')[0] + ".xml");
                    Response.TransmitFile(subPath + "\\" + fileName.Split('.')[0] + ".xml");
                    Response.End();

                    break;
            }


        }

        protected void ImageButton1_Click(object sender, ImageClickEventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                if (Session["ID"] != null)
                {
                    var id = int.Parse(Session["ID"].ToString());

                    var savePath = Server.MapPath("Docs\\") + id + "\\";

                    savePath += FileUpload1.FileName;

                    FileUpload1.SaveAs(savePath);

                    RetreiveDocs(id);
                }
            }

        }


    }
}
