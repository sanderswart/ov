#region Using
using System;
using System.Collections;
using System.Reflection;
using MySql.Data.MySqlClient;
#endregion
namespace ovrs
{
    public class TypePersistence
    {
        #region Variables
        private string mClassName = "TypePersistence";
        /// <summary>Connection object</summary>
        private MySqlConnection gConn;
        #endregion
        #region Constructors
        /// <summary>Constructor - Creating and opening database connection</summary>
        public TypePersistence()
        {
            try
            {
                gConn = new MySqlConnection();
                gConn.ConnectionString = "server=localhost;uid=ovuser;pwd=ovpassword;database=ov;"; // Properties.Settings.Default.MySqlConn;
                gConn.Open();
            }
            catch (MySqlException ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "TypePersistence", ex));
            }
        }
        #endregion
        #region Methods
        /// <summary>Get all Types from database</summary>
        /// <returns>An ArrayList containing all Types in the database</returns>
        public ArrayList getTypes()
        {
            ArrayList typeArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                String sSql = "SELECT * FROM type;";
                MySqlCommand cmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = cmd.ExecuteReader();
                while (oMySQLReader.Read())
                {
                    typeArray.Add(CreateType(oMySQLReader));
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getTypes", ex));
            }
            return typeArray;
        }
        /// <summary>Get Type with given ID</summary>
        /// <param name="lID">The id of the Type to fetch</param>
        /// <returns>The Type if found, an empty instance otherwise</returns>
        public Models.Type getType(long lID)
        {
            Models.Type oType = new Models.Type();
            try
            {
                MySqlDataReader oMySQLReader = null;
                String sSql = "SELECT * FROM type WHERE TYPE_ID = " + lID.ToString() + ";";
                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    oType = CreateType(oMySQLReader);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getType", ex));
            }
            return oType;
        }
        /// <summary>Update Type with ID to values in instance oType</summary>
        /// <param name="lID">The ID of the Type to update</param>
        /// <param name="oType">An instance to use to update the Type</param>
        /// <returns>True if update was succeeded, false otherwise</returns>
        public bool putType(Models.Type oType)
        {
            try
            {
                MySqlDataReader oMySQLReader = null;
                String sSql = "SELECT * FROM type WHERE TYPE_ID = " + oType.ID.ToString() + ";";
                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    oMySQLReader.Close();

                    sSql = @"UPDATE type 
                                    SET TYPE_NAME = " + oType.Name + ","
                                        + " TYPE_DESCRIPTION = " + oType.Description + ","
                                        + " TYPE_PARENT = " + oType.Parent.ToString() + ","
                                        + " TYPE_MODIFIED = NOW(),"
                                        + " TYPE_DELETED = " + (oType.Deleted == null ? "null" : "'" + oType.Deleted.ToString("yyyy-MM-dd HH:mm:ss")) + "'"
                                    + " WHERE TYPE_ID = " + oType.ID.ToString() + ";";

                    oCmd = new MySqlCommand(sSql, gConn);
                    oCmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "putType", ex));
            }
            return false;
        }
        /// <summary>Delete Type with given id from database</summary>
        /// <param name="lID">The id of the Type to delete</param>
        /// <returns>True if Type was deleted, false otherwise</returns>
        public bool deleteType(long lID)
        {
            try
            {
                MySqlDataReader oMySQLReader = null;
                String sSql = "SELECT * FROM type WHERE TYPE_ID = " + lID.ToString() + ";";
                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    Models.Type oType = CreateType(oMySQLReader);
                    oMySQLReader.Close();
                    oType.Deleted = DateTime.Now;
                    return putType(oType);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "deleteType", ex));
            }
            return false;
        }
        /// <summary>Save Type instance to database</summary>
        /// <param name="oType">The Type to save</param>
        /// <returns>The ID that the new Type was given</returns>
        public long saveType(Models.Type oType)
        {
            try
            {
                String sSql = @"INSERT INTO type
                            (TYPE_NAME,
                            TYPE_DESCRIPTION,
                            TYPE_PARENT,
                            TYPE_CREATED,
                            TYPE_MODIFIED)
                            VALUES
                            ('" + oType.Name + "', '"
                                + oType.Description + "', "
                                + oType.Parent + ", '"
                                + oType.Created.ToString("yyyy-MM-dd HH:mm:ss") + "', '"
                                + oType.Modified.ToString("yyyy-MM-dd HH:mm:ss") + "');";

                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oCmd.ExecuteNonQuery();
                return oCmd.LastInsertedId;
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "saveType", ex));
            }
            return -1;
        }
        /// <summary>Create a new Type instance using the values in the MySqlDataReader</summary>
        /// <param name="oMySQLReader">The reader to get the new Type values from</param>
        /// <returns>A new Type</returns>
        private Models.Type CreateType(MySqlDataReader oMySQLReader)
        {
            Models.Type oType = new Models.Type();
            try
            {
                int iPropCount = 0;
                PropertyInfo[] oProperties = typeof(Models.Type).GetProperties();
                foreach (PropertyInfo oProperty in oProperties)
                {
                    FillTypeProperty(oMySQLReader, ref oType, iPropCount++, oProperty);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "CreateType", ex));
            }
            return oType;
        }
        /// <summary>Check property type of iOrdinal property in oType and retrieve value from oMySQLReader if not null</summary>
        /// <param name="oMySQLReader">The reader to get the property value from</param>
        /// <param name="oType">The instance in which to set the property</param>
        /// <param name="iOrdinal">The position of the property in the instance</param>
        /// <param name="oProperty">The property info object</param>
        private void FillTypeProperty(MySqlDataReader oMySQLReader, ref Models.Type oType, int iOrdinal, PropertyInfo oProperty)
        {
            try
            {
                if (!oMySQLReader.IsDBNull(iOrdinal))
                {
                    if (oProperty.PropertyType == typeof(long)) { oProperty.SetValue(oType, oMySQLReader.GetInt32(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(decimal)) { oProperty.SetValue(oType, oMySQLReader.GetDecimal(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(DateTime)) { oProperty.SetValue(oType, oMySQLReader.GetDateTime(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(string)) { oProperty.SetValue(oType, oMySQLReader.GetString(iOrdinal)); return; }
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "FillTypeProperty", ex));
            }
        }
        #endregion
    }
}