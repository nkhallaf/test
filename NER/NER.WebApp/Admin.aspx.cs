using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NER.WebApp
{
    public partial class Admin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ID"] != null)
            {
                var id = int.Parse(Session["ID"].ToString());
                if (!BL.Users.IsAdmin(id))
                    Response.Redirect("default.aspx");


                if (!IsPostBack)
                {
                    var emps = BL.Users.GetAll();
                    RepeaterFiles.DataSource = emps;
                    RepeaterFiles.DataBind();


                }
            }
            else
            {
                Response.Redirect("login.aspx");
            }
        }


        protected void RepeaterFiles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {


            switch (e.CommandName)
            {
                case "Delete":
                    BL.Users.DeleteUser(int.Parse(e.CommandArgument.ToString()));
                    var emps = BL.Users.GetAll();
                    RepeaterFiles.DataSource = emps;
                    RepeaterFiles.DataBind();

                    break;
                case "Update":
                    var user = BL.Users.GetUser(int.Parse(e.CommandArgument.ToString()));

                    InputEmail.Value = user.Name;
                    InputPassword.Value = user.Password;
                    CheckBox1.Checked = user.Admin;
                    HiddenFieldIDD.Value = user.ID.ToString();
                    LabelUserName.Text = "Update user";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "dialog", "LoadDialog();", true);
                    break;
            }


        }

        protected void ImageButton1_Click(object sender, ImageClickEventArgs e)
        {

            InputEmail.Value = string.Empty;
            InputPassword.Value = string.Empty;
            CheckBox1.Checked = false;

            LabelUserName.Text = "Add user";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "dialog", "LoadDialog();", true);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            if (LabelUserName.Text == "Add user")
                BL.Users.Add(InputEmail.Value, InputPassword.Value, CheckBox1.Checked);
            else
            {
                BL.Users.Update(int.Parse(HiddenFieldIDD.Value), InputEmail.Value, InputPassword.Value, CheckBox1.Checked);
            }

            var emps = BL.Users.GetAll();
            RepeaterFiles.DataSource = emps;
            RepeaterFiles.DataBind();
        }
    }
}