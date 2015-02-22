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
            var fileName = e.CommandName;
            var theText = LoadTheFileText(e.CommandArgument.ToString());
            Label1.Text = BL.Colorizing.ColorizeTheText(theText, fileName,BL.Status.Tag);
            LabelFileName.Text = fileName;


            ScriptManager.RegisterStartupScript(this, this.GetType(), "dialog", "LoadDialog();", true);
        }

        protected void ImageButton1_Click(object sender, ImageClickEventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                if (Session["ID"] != null)
                {
                    var id = int.Parse(Session["ID"].ToString());

                    var savePath = Server.MapPath("Docs\\") + id+"\\";
                    
                    savePath += FileUpload1.FileName;

                    FileUpload1.SaveAs(savePath);

                    RetreiveDocs(id);
                }
            }

        }


    }
}
