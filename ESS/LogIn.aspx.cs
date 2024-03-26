using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace ESS
{
    public partial class LogIn : System.Web.UI.Page
    {
        DLData _oDL = new DLData();
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Page.Title = "User Login";
            if (!IsPostBack)
            {
                
            }
        }
        protected void ASPNET_MSgBox(string L_Message)
        {
            string L_Script = "";
            L_Script = "<script> alert('" + L_Message + "');</script>";
            Page.RegisterClientScriptBlock("Information!!", L_Script);
        }
        protected void btnLogIn_Click(object sender, EventArgs e)
        {
            DataTable dtUser;
            try
            {
                //int isAdmin = 0;
                //string aa = DAAccess.Encrypt("CloudTech44");
                dtUser = _oDL.GetUserInfo(txtUserID.Text.Trim(), txtPassword.Text.Trim());
                //if (DateTime.Now > DateTime.ParseExact("01 Nov 2013", "dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture))
                //  dtUser = null;                
                if (dtUser.Rows.Count > 0)
                {
                    //if (isAdmin == 0)
                    //{
                    //    int i = _oDL.UpdateLogIn(txtUserID.Text.Trim());
                    //}
                    //int log = _oDL.KeepLog(txtUserID.Text, Session.SessionID, "ACCESS: LOGIN");

                    Session["userid"] = dtUser.Rows[0]["UserID"].ToString();
                    Session["name"] = dtUser.Rows[0]["Name"].ToString();
                    Session["usertype"] = dtUser.Rows[0]["UserType"].ToString();
                    Response.Redirect("./Home.aspx");
                    return;                    
                }
                else
                {
                    ASPNET_MSgBox("Wrong User ID or Password. Try again!!!");
                }
            }
            catch
            {
                ASPNET_MSgBox("ERROR: CONTACT WITH ADMINISTRATOR!!!");
            }
        }
    }
}