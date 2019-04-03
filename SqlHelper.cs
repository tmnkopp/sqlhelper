using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Xml;
 
/// <summary>
/// Summary description for SqlHelper
/// </summary>
public class SqlHelper
{
    private string mstr_ConnectionString;
    private SqlConnection mobj_SqlConnection;
    private SqlCommand mobj_SqlCommand;
    private int mint_CommandTimeout = 30;

    public string ConnectionString { get; set; }
    
    public string ConnectionName
    {
        get {
            string _connName = "";
            foreach (ConnectionStringSettings css in System.Configuration.ConfigurationManager.ConnectionStrings)
            {
                if (css.ConnectionString == mstr_ConnectionString)
                {
                    _connName = css.Name;
                    break;
                }
            }
            return _connName;  
        } 
    }

    public enum ExpectedType
    {

        StringType = 0,
        NumberType = 1,
        DateType = 2,
        BooleanType = 3,
        ImageType = 4
    }


    private string GetConnString()
    {
        if (mstr_ConnectionString != "" && mstr_ConnectionString != null)
            return mstr_ConnectionString;

        if(ConfigurationManager.AppSettings["UseConnection"].ToString() != "")
            return System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["UseConnection"]].ToString();

        string _connstr = ""; 
        foreach (ConnectionStringSettings css in System.Configuration.ConfigurationManager.ConnectionStrings)
        {
            if (css.Name != "LocalSqlServer")
            {
                _connstr = css.ConnectionString; 
                break;
            }
        }
        if (_connstr == "" || _connstr == null)
        {
            _connstr = System.Configuration.ConfigurationManager.ConnectionStrings[0].ToString();
        }
        return _connstr;
    }

    public SqlHelper()
    {
        try
        {
            if (mstr_ConnectionString == "" || mstr_ConnectionString == null)
                mstr_ConnectionString = GetConnString();
            ConnectionString = mstr_ConnectionString;

            mobj_SqlConnection = new SqlConnection(mstr_ConnectionString);
            mobj_SqlCommand = new SqlCommand();
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.Connection = mobj_SqlConnection;

            //ParseConnectionString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error initializing data class." + Environment.NewLine + ex.Message);
        }
    }

    public SqlHelper(string _connName )
        : this() 
    {
        mstr_ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[_connName].ToString();
    }
 
    public void Dispose()
    {
        try
        {
            //Clean Up Connection Object
            if (mobj_SqlConnection != null)
            {
                if (mobj_SqlConnection.State != ConnectionState.Closed)
                {
                    mobj_SqlConnection.Close();
                }
                mobj_SqlConnection.Dispose();
            }

            //Clean Up Command Object
            if (mobj_SqlCommand != null)
            {
                mobj_SqlCommand.Dispose();
            }

        }

        catch (Exception ex)
        {
            throw new Exception("Error disposing data class." + Environment.NewLine + ex.Message);
        }

    }

    public void CloseConnection()
    {
        if (mobj_SqlConnection.State != ConnectionState.Closed) mobj_SqlConnection.Close();
    }
    public int GetExecuteScalarByCommand(string Command)
    {

        object identity = 0;
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.CommandType = CommandType.Text;

            mobj_SqlConnection.Open();

            mobj_SqlCommand.Connection = mobj_SqlConnection;
            identity = mobj_SqlCommand.ExecuteScalar();
            CloseConnection();
        }
        catch (Exception ex)
        {
            CloseConnection();
            throw ex;
        }
        return Convert.ToInt32(identity);
    }

    public void GetExecuteNonQueryByCommand(string Command)
    {
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.CommandType = CommandType.Text;

            mobj_SqlConnection.Open();

            mobj_SqlCommand.Connection = mobj_SqlConnection;
            mobj_SqlCommand.ExecuteNonQuery();

            CloseConnection();
        }
        catch (Exception ex)
        {
            CloseConnection();
            throw ex;
        }
    }

    public DataSet GetDatasetByCommand(string Command)
    {
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.CommandType = CommandType.Text;

            mobj_SqlConnection.Open();

            SqlDataAdapter adpt = new SqlDataAdapter(mobj_SqlCommand);
            DataSet ds = new DataSet();
            adpt.Fill(ds);
            return ds;
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            CloseConnection();
        }
    }

    public SqlDataReader GetReaderBySQL(string strSQL)
    {
        mobj_SqlConnection.Open();
        try
        {
            SqlCommand myCommand = new SqlCommand(strSQL, mobj_SqlConnection);
            return myCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            CloseConnection();
            throw ex;
        }
    }

    public SqlDataReader GetReaderByCmd(string Command)
    {
        SqlDataReader objSqlDataReader = null;
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandType = CommandType.Text;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;

            mobj_SqlConnection.Open();
            mobj_SqlCommand.Connection = mobj_SqlConnection;

            objSqlDataReader = mobj_SqlCommand.ExecuteReader();
            return objSqlDataReader;
        }
        catch (Exception ex)
        {
            CloseConnection();
            throw ex;
        }

    }


    public XmlReader GetXmlReaderByCmd(string Command)
    {
        XmlReader objSqlDataReader = null;
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandType = CommandType.Text;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;

            mobj_SqlConnection.Open();
            mobj_SqlCommand.Connection = mobj_SqlConnection;

            objSqlDataReader = mobj_SqlCommand.ExecuteXmlReader();
            return objSqlDataReader;
        }
        catch (Exception ex)
        {
            CloseConnection();
            throw ex;
        }

    }


    public void AddParameterToSQLCommand(string ParameterName, SqlDbType ParameterType)
    {
        try
        {
            mobj_SqlCommand.Parameters.Add(new SqlParameter(ParameterName, ParameterType));
        }

        catch (Exception ex)
        {
            throw ex;
        }
    }
 

    public void AddParameterToSQLCommand(string ParameterName, SqlDbType ParameterType, object Value)
    {
        try
        {
            mobj_SqlCommand.Parameters.Add(new SqlParameter(ParameterName, ParameterType));
            SetSQLCommandParameterValue(ParameterName, Value);
        }

        catch (Exception ex)
        {
            throw ex;
        }
    }


    public void SetSQLCommandParameterValue(string ParameterName, object Value)
    {
        try
        {
            mobj_SqlCommand.Parameters[ParameterName].Value = Value;
        }

        catch (Exception ex)
        {
            throw ex;
        }
    }



}
