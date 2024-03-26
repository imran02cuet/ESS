using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for DLData
/// </summary>
public partial class DLData: DAAccess 
{
	public DLData()
	{
		
	}
    public DataTable GetUserInfo(string sUserID,string sPassword)
    {        
        DataTable dt = null;
        try
        {
            string sql = string.Format(@"Select a.UserID,a.UserType,a.Name from UserInfo a
                                        Where a.Active=1 AND 
                                        a.UserID='{0}' AND a.Password='{1}'", sUserID, sPassword);
            dt = FillDataTable(sql,"userinfo");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    public DataTable GetUserAndPassword(string sEmail)
    {
        DataTable dt = null;
        try
        {
            string sql = string.Format(@"Select a.UserID,a.Password from UserInfo a
                                        Where a.Active=1 AND  a.Email='{0}'", sEmail);
            dt = FillDataTable(sql, "userinfo");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    
    public DataTable GetSysAdminUserInfo(string sUserID, string sPassword)
    {
        DataTable dt = null;
        try
        {
            string sql = string.Format(@"Select UserID,UserType,Name from SysAdmin 
                                        Where UserID='{0}' AND Password='{1}'", sUserID, sPassword);
            dt = FillDataTable(sql, "userinfo");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    public int UpdateLogIn(string sUserID)
    {
        int nReturn = 0;
        try
        {
            string sql = string.Format(@"Update UserInfo SET Visits=Visits+1,LastLogin=getdate() Where UserID='{0}'", sUserID);
            nReturn = ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            nReturn = 0;
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return nReturn;
    }
    
    public DataTable GetMenuLevel1(int nUserType)
    {
        DataTable dt = null;
        try
        {
            string sql = "";
            if (nUserType == 1)//admin
                sql = string.Format(@"select FormID, FormName
                from FormAccess where [Admin]=1 ORDER BY FormName");
            if (nUserType == 2)//manager
                sql = string.Format(@"select FormID, FormName
                from FormAccess where GenUser=1 ORDER BY FormName");
            dt = FillDataTable(sql, "menu1");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }

    public DataTable GetDetailsUserLogs(string sUserID, string SDate, string Edate)
    {
        DataTable dt = null;
        string sql = "";
        try
        {
            sql = string.Format(@"SELECT UserID,CONVERT(VARCHAR,LogDate,100) as LogDate, SessionID, Activity
                FROM UserLog
                Where UserID='{0}'
                AND CONVERT(DATETIME,CONVERT(VARCHAR,LogDate,106)) BETWEEN '{1}' AND '{2}' ORDER BY LogDate  desc", sUserID, SDate, Edate);

            dt = FillDataTable(sql, "formbyid");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }

    #region User
    
    public DataTable GetUserByUserID(string sUserID)
    {
        DataTable dt = null;
        try
        {
            string sql = string.Format(@"Select a.UserID,a.Password,a.UserDesc,a.UserType,a.Name,a.Email,a.Active from UserInfo a
                                        Where a.UserID='{0}'", sUserID);
            dt = FillDataTable(sql, "userbyid");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    public DataTable GetSysAdmin()
    {
        DataTable dt = null;
        try
        {
            string sql = string.Format(@"SELECT UserID, Password, Name, UserType, UserDesc, Email, IDXStrings,MsgDelayTime FROM SysAdmin");
            dt = FillDataTable(sql, "user");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    public int UpdateSysAdmin(string sUserID,string sPassword,string sName, string sEmail, string sIDX,int nMsg)
    {
        int nReturn = 0;
        try
        {
            string sql = string.Format(@"Update SysAdmin SET Password='{1}', Name='{2}', Email='{3}', IDXStrings ='{4}',MsgDelayTime={5}
                Where UserID='{0}'", sUserID, sPassword, sName, sEmail, sIDX, nMsg);
            nReturn = ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            nReturn = 0;
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return nReturn;
    }
    public DataTable GetUserList(int nData)
    {
        //99 = Activity log, 0= Admin Account , 1=other user account(without sys admin and admin)
        DataTable dt = null;
        try
        {
            string sql="";
            if (nData==99)
                sql = string.Format(@"Select UserID,Password,UserDesc,Name,Email,Active,Visits,case when LastLogin=null then '' else convert(varchar,LastLogin,106) end as LastLogin from UserInfo Where UserType!=0 ORDER BY UserID");
            else if (nData == 0)
                sql = string.Format(@"Select UserID,Password,UserDesc,Name,Email,Active,Visits,case when LastLogin=null then '' else convert(varchar,LastLogin,106) end as LastLogin from UserInfo Where UserType in (1) ORDER BY UserID");
            else if (nData == 1)
                sql = string.Format(@"Select UserID,Password,UserDesc,Name,Email,Active,Visits,case when LastLogin=null then '' else convert(varchar,LastLogin,106) end as LastLogin from UserInfo Where UserType not in (0,1) ORDER BY UserID");
            dt = FillDataTable(sql, "userlist");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    
    public int GetUserExistorNot(string sUserID)
    {
        int _nReturn = 0;
        try
        {
            string sql = string.Format(@"Select count(*) from UserInfo where UserID='{0}'", sUserID);
            _nReturn = Convert.ToInt32(ExecuteScalar(sql));
        }
        catch (Exception ex)
        {
            _nReturn = 0;
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return _nReturn;
    }
    public DataTable AddUser(string sUserID,string sPassword, string sName, string sEmail, int nRoleID, string sRoleDesc,
        int nIsActive,string sSessionID,string sEntryUser)
    {
        DataTable oTable = new DataTable("adduser");
        SqlParameter[] oParameters = new SqlParameter[9];
        try
        {
            oParameters[0] = new SqlParameter("@UserID", sUserID);
            oParameters[1] = new SqlParameter("@Password", sPassword);
            oParameters[2] = new SqlParameter("@Name", sName);
            oParameters[3] = new SqlParameter("@Email", sEmail);
            oParameters[4] = new SqlParameter("@RoleID", nRoleID);
            oParameters[5] = new SqlParameter("@RoleDesc", sRoleDesc);
            oParameters[6] = new SqlParameter("@Active", nIsActive);
            oParameters[7] = new SqlParameter("@SessID", sSessionID);
            oParameters[8] = new SqlParameter("@EntryUserID", sEntryUser);
            oTable = FillDataTable("adduser", "spAddUser", oParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        return oTable;
    }
    public DataTable EditUser(string sUserID, string sPassword, string sName, string sEmail, int nRoleID, string sRoleDesc,
        int nIsActive, string sSessionID, string sEntryUser)
    {
        DataTable oTable = new DataTable("edituser");
        SqlParameter[] oParameters = new SqlParameter[9];
        try
        {
            oParameters[0] = new SqlParameter("@UserID", sUserID);
            oParameters[1] = new SqlParameter("@Password", sPassword);
            oParameters[2] = new SqlParameter("@Name", sName);
            oParameters[3] = new SqlParameter("@Email", sEmail);
            oParameters[4] = new SqlParameter("@RoleID", nRoleID);
            oParameters[5] = new SqlParameter("@RoleDesc", sRoleDesc);
            oParameters[6] = new SqlParameter("@Active", nIsActive);
            oParameters[7] = new SqlParameter("@SessID", sSessionID);
            oParameters[8] = new SqlParameter("@EntryUserID", sEntryUser);
            oTable = FillDataTable("edituser", "spUpdateUser", oParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        return oTable;
    }
    #endregion

    #region SystemForms
    public DataTable GetSystemForms()
    {
        DataTable dt = null;
        try
        {
            string sql = string.Format(@"SELECT FormID ,FormName,[Admin],Manager,Edit,[VIEW],Office FROM FormAccess  ORDER BY FormName");
            dt = FillDataTable(sql, "sysfrm");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    public DataTable GetFormByFormID(string sFormID)
    {
        DataTable dt = null;
        try
        {
            string sql = string.Format(@"SELECT FormID,FormName,FormCode,Admin,Manager,Edit,[VIEW] as sVIEW,Office FROM FormAccess
                                        Where FormID='{0}'", sFormID);
            dt = FillDataTable(sql, "formbyid");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return dt;
    }
    
    public int GetFormExistorNot(string sFormID)
    {
        int _nReturn = 0;
        try
        {
            string sql = string.Format(@"Select count(*) from FormAccess where FormID='{0}'", sFormID);
            _nReturn = Convert.ToInt32(ExecuteScalar(sql));
        }
        catch (Exception ex)
        {
            _nReturn = 0;
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return _nReturn;
    }
    public DataTable AddForm(string sFormID, string sFormName, string sFormCode,int nAdmin, int nManager, int nEdit, int nView,
        int nOffice, string sSessionID, string sEntryUser)
    {
        DataTable oTable = new DataTable("addform");
        SqlParameter[] oParameters = new SqlParameter[10];
        try
        {
            oParameters[0] = new SqlParameter("@FormID", sFormID);
            oParameters[1] = new SqlParameter("@FormName", sFormName);
            oParameters[2] = new SqlParameter("@FormCode", sFormCode);
            oParameters[3] = new SqlParameter("@Admin", nAdmin);
            oParameters[4] = new SqlParameter("@Manager", nManager);
            oParameters[5] = new SqlParameter("@Edit", nEdit);
            oParameters[6] = new SqlParameter("@View", nView);
            oParameters[7] = new SqlParameter("@Office", nOffice);
            oParameters[8] = new SqlParameter("@SessID", sSessionID);
            oParameters[9] = new SqlParameter("@EntryUserID", sEntryUser);
            oTable = FillDataTable("addform", "spAddForm", oParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        return oTable;
    }
    public DataTable EditForm(string sFormID, string sFormName, string sFormCode, int nAdmin, int nManager, int nEdit, int nView,
        int nOffice, string sSessionID, string sEntryUser)
    {
        DataTable oTable = new DataTable("editform");
        SqlParameter[] oParameters = new SqlParameter[10];
        try
        {
            oParameters[0] = new SqlParameter("@FormID", sFormID);
            oParameters[1] = new SqlParameter("@FormName", sFormName);
            oParameters[2] = new SqlParameter("@FormCode", sFormCode);
            oParameters[3] = new SqlParameter("@Admin", nAdmin);
            oParameters[4] = new SqlParameter("@Manager", nManager);
            oParameters[5] = new SqlParameter("@Edit", nEdit);
            oParameters[6] = new SqlParameter("@View", nView);
            oParameters[7] = new SqlParameter("@Office", nOffice);
            oParameters[8] = new SqlParameter("@SessID", sSessionID);
            oParameters[9] = new SqlParameter("@EntryUserID", sEntryUser);
            oTable = FillDataTable("editform", "spUpdateForm", oParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        return oTable;
    }
    public DataTable DeleteForm(string sFormID,string sSessionID, string sEntryUser)
    {
        DataTable oTable = new DataTable("delform");
        SqlParameter[] oParameters = new SqlParameter[3];
        try
        {
            oParameters[0] = new SqlParameter("@FormID", sFormID);
            oParameters[1] = new SqlParameter("@SessID", sSessionID);
            oParameters[2] = new SqlParameter("@EntryUserID", sEntryUser);
            oTable = FillDataTable("delform", "spDeleteForm", oParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        return oTable;
    }

    #endregion



    public int KeepLog(string sUserID,string sSessionID,string sActivity)
    {
        int nReturn = 0;
        try
        {
            string sql = string.Format(@"INSERT INTO UserLog(UserID, LogDate, SessionID, Activity) VALUES
		        ('{0}',GETDATE(),'{1}','{2}')", sUserID, sSessionID, sActivity);
            nReturn = ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            nReturn = 0;
            throw new Exception(ex.InnerException.ToString(), ex);
        }
        return nReturn;
    }
}
