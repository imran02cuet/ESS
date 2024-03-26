using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace ESS
{
    public partial class Home1 : System.Web.UI.MasterPage
    {
        DLData _oDL = new DLData();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string userid = Session["userid"].ToString();
                if (userid == "")
                {
                    Response.Redirect("./Login.aspx");
                    return;
                }
            }
            catch
            {
                Response.Redirect("./Login.aspx");
                return;
            }
            if (!IsPostBack)
            {
                DataTable _menuLevel1 = new DataTable();
                _menuLevel1 = _oDL.GetMenuLevel1(Convert.ToInt32(Session["usertype"].ToString()));

                MenuItem _menitem;

                for (int i = 0; i <= _menuLevel1.Rows.Count - 1; i++)
                {
                    _menitem = new MenuItem();
                    _menitem.Text = _menuLevel1.Rows[i]["FormName"].ToString(); //menu text
                    //_menitem.Value = _menuLevel1.Rows[i]["FormID"].ToString(); //menuvalue
                    _menitem.Value = _menuLevel1.Rows[i]["FormID"].ToString(); //menuvalue
                    //_menitem.NavigateUrl = "./";
                    //_menitem.NavigateUrl = _menuLevel1.Rows[i]["FormCode"].ToString(); //menu URL                                
                    UserMenu.Items.Add(_menitem);
                }
                
            } 
        }
        protected void btnExit_Click(object sender, EventArgs e)
        {
            int log = _oDL.KeepLog(Session["userid"].ToString(), Session.SessionID, "ACCESS: LOGOUT");
            Session.Clear();
            Session.Abandon();
            Response.Redirect("./Login.aspx");
        }
        protected void UserMenu_MenuItemClick(object sender, MenuEventArgs e)
        {
            //iShowFrame(e.Item.Value.ToString());
        }
    }
}