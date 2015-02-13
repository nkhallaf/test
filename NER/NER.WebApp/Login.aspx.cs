using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NER.WebApp
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ButtonLogin_Click(object sender, EventArgs e)
        {
            int id = BL.Users.CheckLogin(TextBoxEmail.Text.ToLower(), TextBoxPassword.Text.ToLower());
            if (id != 0)
            {
                Session["ID"] = id;
                Response.Redirect("default.aspx");
              

            }
            else
            {


                Label1.Visible = true;
            }


        }
    }
}