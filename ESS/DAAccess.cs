using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using System.Collections.Specialized;
using System.Data.SqlClient;


public class DAAccess
{
    protected static string CONNECTION_STRING;
    private static SqlConnection _oConnection = null;
    private static SqlTransaction _oTransaciton = null;
    /// <summary>
    /// This constructor reads the .config file
    /// </summary>
    #region Constructor
    public DAAccess()
    {
        try
        {
            //Reading appParams tag of app.config or web.config
            NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appSettings");
            try
            {
                //Reading Connection String
                string[] sTemps = oValues["DBConnStrings"].Split(';');
                string sPass = Decrypt(sTemps[2].Substring(9));
                CONNECTION_STRING = sTemps[0] + ";" + sTemps[1] + ";Password=" + sPass + ";" + sTemps[3];
            }
            catch
            {
                throw new Exception("Connection String Missing. ");
            }
        }
        catch (ConfigurationException e)
        {
            throw new Exception("Error in App.config file. " + e.Message + " " + e.Source);
        }

    }
    #endregion

    #region Password related function
    public static string Encrypt(string sText)
    {
        int i = 0;
        string sEncrypt = "", sKey = "saysthezenmaster";
        char cTextChar, cKeyChar;
        char[] cTextData, cKey;

        //Save Length of Pass
        sText = (char)(sText.Length) + sText;

        //Pad Password with space upto 10 Characters
        if (sText.Length < 10)
        {
            sText = sText + sText.PadRight((10 - sText.Length), ' ');
        }
        cTextData = sText.ToCharArray();

        //Make the key big enough
        while (sKey.Length < sText.Length)
        {
            sKey = sKey + sKey;
        }
        sKey = sKey.Substring(0, sText.Length);
        cKey = sKey.ToCharArray();

        //Encrypting Data
        for (i = 0; i < sText.Length; i++)
        {
            cTextChar = (char)cTextData.GetValue(i);
            cKeyChar = (char)cKey.GetValue(i);
            sEncrypt = sEncrypt + IntToHex((int)(cTextChar) ^ (int)(cKeyChar));
        }

        return sEncrypt;
    }

    public static string Decrypt(string sText)
    {
        int j = 0, i = 0, nLen = 0;
        string sTextByte = "", sDecrypt = "", sKey = "saysthezenmaster";//
        char[] cTextData, cKey;
        char cTextChar, cKeyChar;

        //Taking Lenght, half of Encrypting data  
        nLen = sText.Length / 2;

        //Making key is big Enough
        while (sKey.Length < nLen)
        {
            sKey = sKey + sKey;
        }
        sKey = sKey.Substring(0, nLen);
        cKey = sKey.ToCharArray();
        cTextData = sText.ToCharArray();

        //Decripting data
        for (i = 0; i < nLen; i++)
        {
            sTextByte = "";
            for (j = i * 2; j < (i * 2 + 2); j++)
            {
                sTextByte = sTextByte + cTextData.GetValue(j).ToString();
            }
            cTextChar = (char)HexToInt(sTextByte);
            cKeyChar = (char)cKey.GetValue(i);
            sDecrypt = sDecrypt + (char)((int)(cKeyChar) ^ (int)(cTextChar));
        }

        //Taking real password
        cTextData = sDecrypt.ToCharArray();
        sDecrypt = "";
        i = (int)(char)cTextData.GetValue(0);
        for (j = 1; j <= i; j++)
        {
            sDecrypt = sDecrypt + cTextData.GetValue(j).ToString();
        }

        return sDecrypt;
    }

    private static string IntToHex(int nIntData)
    {
        return Convert.ToString(nIntData, 16).PadLeft(2, '0');
    }

    private static int HexToInt(string sHexData)
    {
        return Convert.ToInt32(sHexData, 16);
    }
    #endregion

    #region Prepare Command
    private SqlCommand PrepareCommand(string sSQL)
    {
        SqlCommand oCommand;
        if (_oConnection == null)
        {
            _oConnection = new SqlConnection(CONNECTION_STRING);
        }
        if (_oConnection.State == ConnectionState.Closed)
        {
            _oConnection.Open();
        }
        oCommand = new SqlCommand(sSQL, _oConnection);
        if (_oTransaciton != null)
        {
            if (_oTransaciton.Connection != null)
            {
                oCommand.Transaction = _oTransaciton;
            }
            else
            {
                throw new Exception("Invalid Transaction");
            }
        }
        return oCommand;
    }

    private SqlCommand PrepareCommand(string sSPName, params SqlParameter[] oParameters)
    {
        SqlCommand oCommand;
        if (_oConnection == null)
        {
            _oConnection = new SqlConnection(CONNECTION_STRING);
        }
        if (_oConnection.State == ConnectionState.Closed)
        {
            _oConnection.Open();
        }
        oCommand = new SqlCommand(sSPName, _oConnection);
        for (int i = 0; i < oParameters.Length; i++)
        {
            oCommand.Parameters.Add(oParameters[i]);
        }
        oCommand.CommandType = CommandType.StoredProcedure;
        if (_oTransaciton != null)
        {
            if (_oTransaciton.Connection != null)
            {
                oCommand.Transaction = _oTransaciton;
            }
            else
            {
                throw new Exception("Invalid Transaction");
            }
        }
        return oCommand;
    }
    #endregion

    #region ExecuteScalar
    /// <summary>
    /// Executes scalar
    /// </summary>
    /// <param name="sSQL">SQL Quaries</param>
    /// <returns>Returns an object that contains the value of the SQL Query. The Return 
    /// Value needs to be Casted
    /// </returns>
    protected object ExecuteScalar(string sSQL)
    {
        SqlCommand oCommand = new SqlCommand();
        string sErrorMessage;
        int nErrCount = 0;
        object oReturn;
        try
        {
            oCommand = PrepareCommand(sSQL);
            oReturn = oCommand.ExecuteScalar();
        }
        catch (InvalidOperationException oInvaE)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            throw new Exception(oInvaE.Message);
        }
        catch (SqlException oSQLException)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oCommand.Dispose();
        return oReturn;
    }
    #endregion

    #region ExecuteNonQuery
    /// <summary>
    /// This function executes Non-Query SQL: UPDATE,INSERT,DELETE, etc
    /// </summary>
    /// <param name="sSQL">Cotains the T-SQL</param>
    /// <returns>Returns the row affected by update,insert and delete query. Other type of query will return -1</returns>
    protected int ExecuteNonQuery(string sSQL)
    {
        SqlCommand oCommand = new SqlCommand();
        string sErrorMessage;
        int nErrCount = 0;
        int nReturn;
        try
        {
            oCommand = PrepareCommand(sSQL); ;
            nReturn = oCommand.ExecuteNonQuery();
        }
        catch (SqlException oSQLException)
        {
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oCommand.Dispose();
        return nReturn;
    }
    #endregion

    #region ExecuteReader
    /// <summary>
    /// Executes a sql and returns row(s) in SQLDataReader
    /// </summary>
    /// <param name="sSQL">Cotains the T-SQL</param>
    /// <returns>Returns a SqlDataReader that contains the result of the query string</returns>
    protected SqlDataReader ExecuteReader(string sSQL)
    {
        SqlCommand oCommand = new SqlCommand();
        string sErrorMessage;
        int nErrCount = 0;
        SqlDataReader oReader = null;
        try
        {
            oCommand = PrepareCommand(sSQL);
            oReader = oCommand.ExecuteReader();
        }
        catch (SqlException oSQLException)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oCommand.Dispose();
        return oReader;
    }

    protected SqlDataReader ExecuteReader(string sSPName, params SqlParameter[] oParameters)
    {
        SqlCommand oCommand = new SqlCommand();
        string sErrorMessage;
        int nErrCount = 0;
        SqlDataReader oReader = null;
        try
        {
            oCommand = PrepareCommand(sSPName, oParameters);
            oReader = oCommand.ExecuteReader();
        }
        catch (SqlException oSQLException)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oCommand.Dispose();
        return oReader;
    }
    #endregion

    #region FillDataSet
    /// <summary>
    /// Executes a sql and returns a dataset
    /// </summary>
    /// <param name="sSQL">Cotains the T-SQL</param>
    /// <returns>Returns a SqlDataReader that contains the result of the query string</returns>
    protected DataSet FillDataSet(string sSQL, string sTableName)
    {
        SqlDataAdapter oAdapter = new SqlDataAdapter();
        string sErrorMessage;
        int nErrCount = 0;
        DataSet oSet = new DataSet();
        try
        {
            oAdapter.SelectCommand = PrepareCommand(sSQL);
            oAdapter.Fill(oSet, sTableName);
        }
        catch (SqlException oSQLException)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oAdapter.Dispose();
        return oSet;
    }
    protected DataTable FillDataTable(string sSQL, string sTableName)
    {
        SqlDataAdapter oAdapter = new SqlDataAdapter();
        string sErrorMessage;
        int nErrCount = 0;
        DataTable oTable = new DataTable(sTableName);
        try
        {
            oAdapter.SelectCommand = PrepareCommand(sSQL);
            oAdapter.Fill(oTable);
        }
        catch (SqlException oSQLException)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oAdapter.Dispose();
        return oTable;
    }

    protected DataTable FillDataTable(string sTableName, string sSPName, params SqlParameter[] oParameters)
    {
        SqlDataAdapter oAdapter = new SqlDataAdapter();
        string sErrorMessage;
        int nErrCount = 0;
        DataTable oTable = new DataTable(sTableName);
        try
        {
            oAdapter.SelectCommand = PrepareCommand(sSPName, oParameters);
            oAdapter.Fill(oTable);
        }
        catch (SqlException oSQLException)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oAdapter.Dispose();
        return oTable;
    }

    protected DataSet FillDataSet(string sTableName, string sSPName, params SqlParameter[] oParameters)
    {
        SqlDataAdapter oAdapter = new SqlDataAdapter();
        string sErrorMessage;
        int nErrCount = 0;
        DataSet oSet = new DataSet();
        try
        {
            oAdapter.SelectCommand = PrepareCommand(sSPName, oParameters);
            oAdapter.Fill(oSet, sTableName);
        }
        catch (SqlException oSQLException)
        {
            if (_oConnection != null)
            {
                if (_oConnection.State == ConnectionState.Open)
                {
                    _oConnection.Close();
                }
            }
            sErrorMessage = "";
            foreach (SqlError oError in oSQLException.Errors)
            {
                nErrCount++;
                sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
            }
            throw new Exception("SQL ERROR: " + sErrorMessage);
        }
        oAdapter.Dispose();
        return oSet;
    }

    //protected DataTable FillDataTable(string sTableName, string sSPName, params SqlParameter[] oParameters)
    //{
    //    SqlDataAdapter oAdapter = new SqlDataAdapter();
    //    string sErrorMessage;
    //    int nErrCount = 0;
    //    DataTable oTable = new DataTable(sTableName);
    //    try
    //    {
    //        oAdapter.SelectCommand = PrepareCommand(sSPName, oParameters);
    //        oAdapter.Fill(oTable);
    //    }
    //    catch (SqlException oSQLException)
    //    {
    //        if (_oConnection != null)
    //        {
    //            if (_oConnection.State == ConnectionState.Open)
    //            {
    //                _oConnection.Close();
    //            }
    //        }
    //        sErrorMessage = "";
    //        foreach (SqlError oError in oSQLException.Errors)
    //        {
    //            nErrCount++;
    //            sErrorMessage = sErrorMessage + nErrCount.ToString() + ". " + oError.Message;
    //        }
    //        throw new Exception("SQL ERROR: " + sErrorMessage);
    //    }
    //    oAdapter.Dispose();
    //    return oTable;
    //}
    #endregion

   

    #region Transaction

    #region Begin Tran
    /// <summary>
    /// It checks for an already open transaction. if there exist an open connection, then
    /// the function throws an exception. It opens a connection, if it is already closed
    /// If the connection property of the transaction is null then the transaction is no
    /// longer valid and it opens a new connection and a transaction
    /// </summary>
    public static void BeginTran()
    {
        if (_oTransaciton == null)
        {
            if (_oConnection == null)
            {
                _oConnection = new SqlConnection(CONNECTION_STRING);
            }
            if (_oConnection.State == ConnectionState.Closed)
            {
                _oConnection.Open();
            }
            _oTransaciton = _oConnection.BeginTransaction();

        }
        else
        {
            if (_oTransaciton.Connection != null)
            {
                if (_oTransaciton.Connection.State == ConnectionState.Open)
                {
                    _oTransaciton.Connection.Close();
                    _oTransaciton = null;
                    throw new Exception("Cannot start a new transaction, already when a new transaction is open");
                }
            }
            else
            {
                _oConnection = new SqlConnection(CONNECTION_STRING);
                _oConnection.Open();
                _oTransaciton = _oConnection.BeginTransaction();
            }
        }
    }

    /// <summary>
    /// Caution must be taken when committing transaction, as this procedure is called 
    /// in the funtction where the transaction was not created. It checks for an already 
    /// open transaction. if there exist an open connection, then the function continues 
    /// with the existing connection. It opens a connection, if it is already closed
    /// If the connection property of the transaction is null then the transaction is no
    /// longer valid and it opens a new connection and a transaction
    /// </summary>
    public static void BeginExistingTran()
    {
        if (_oTransaciton == null)
        {
            if (_oConnection == null)
            {
                _oConnection = new SqlConnection(CONNECTION_STRING);
            }
            if (_oConnection.State == ConnectionState.Closed)
            {
                _oConnection.Open();
            }
            _oTransaciton = _oConnection.BeginTransaction();
        }
        else
        {
            if (_oTransaciton.Connection == null)
            {
                _oConnection = new SqlConnection(CONNECTION_STRING);
                _oConnection.Open();
                _oTransaciton = _oConnection.BeginTransaction();
            }
        }
    }
    #endregion

    #region CommitTran
    /// <summary>
    /// At first it checks whether there is any transaction (that is not null). If there is
    /// none then it throws an exception.If the connection object of the transaction 
    /// is null then transaction is no longer valid.
    /// </summary>
    public static void CommitTran()
    {
        if (_oTransaciton == null)
        {
            throw new Exception("Nothing to COMMIT. No transaction exist");
        }
        else
        {
            if (_oTransaciton.Connection == null)
            {
                throw new Exception("No Connection Open");
            }
            else
            {
                _oTransaciton.Commit();
                _oTransaciton.Dispose();
                _oTransaciton = null;
            }
        }
        if (_oConnection.State != ConnectionState.Closed)
        {
            _oConnection.Close();

        }
    }
    #endregion

    #region RollBack Tran
    public static void RollBackTran()
    {
        if (_oTransaciton != null)
        {
            if (_oTransaciton.Connection != null)
            {
                _oTransaciton.Rollback();
                _oTransaciton = null;
            }
        }
        if (_oConnection.State != ConnectionState.Closed)
        {
            _oConnection.Close();
        }
    }
    #endregion

    #endregion

}
