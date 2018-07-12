#region Using
using System;
using ovrs.Models;
using System.Collections;
using System.Reflection;
using MySql.Data.MySqlClient;
#endregion
namespace ovrs
{
    public class PropPersistence
    {
        #region Variables
        private string mClassName = "PropPersistence";
        /// <summary>Connection object</summary>
        private MySqlConnection gConn;
        #endregion
        #region Constructors
        /// <summary>Constructor - Creating and opening database connection</summary>
        public PropPersistence()
        {
            try
            {
                gConn = new MySqlConnection();
                gConn.ConnectionString = "server=localhost;uid=ovuser;pwd=ovpassword;database=ov;"; // Properties.Settings.Default.MySqlConn;
                gConn.Open();
            }
            catch (MySqlException ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "PropPersistence", ex));
            }
        }
        #endregion
        #region Methods
        /// <summary>Get all Props from database</summary>
        /// <returns>An ArrayList containing all Props in the database</returns>
        public ArrayList getProps()
        {
            ArrayList typeArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                String sSql = "SELECT * FROM prop;";
                MySqlCommand cmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = cmd.ExecuteReader();
                while (oMySQLReader.Read())
                {
                    typeArray.Add(CreateProp(oMySQLReader));
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getProps", ex));
            }
            return typeArray;
        }
        /// <summary>Get Prop with given ID</summary>
        /// <param name="lID">The id of the Prop to fetch</param>
        /// <returns>The Prop if found, an empty instance otherwise</returns>
        public Prop getProp(long lID)
        {
            Prop oProp = new Prop();
            try
            {
                MySqlDataReader oMySQLReader = null;
                String sSql = "SELECT * FROM prop WHERE PROP_ID = " + lID.ToString() + ";";
                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    oProp = CreateProp(oMySQLReader);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getProp", ex));
            }
            return oProp;
        }
        /// <summary>Update Prop with ID to values in instance oProp</summary>
        /// <param name="lID">The ID of the Prop to update</param>
        /// <param name="oProp">An instance to use to update the Prop</param>
        /// <returns>True if update was succeeded, false otherwise</returns>
        public bool putProp(Prop oProp)
        {
            try
            {
                MySqlDataReader oMySQLReader = null;
                String sSql = "SELECT * FROM prop WHERE PROP_ID = " + oProp.ID.ToString() + ";";
                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    oMySQLReader.Close();

                    sSql = @"UPDATE prop 
                                    SET COMP_ID = " + oProp.Comp_ID + ","
                                        + " COMP_TYPE = " + oProp.Comp_Type + ","
                                        + " PROP_TYPE = " + oProp.Type + ","
                                        + " PROP_NAME = '" + oProp.Name + "',"
                                        + " PROP_VALUE = '" + oProp.Value + "',"
                                        + " PROP_MODIFIED = NOW(),"
                                        + " PROP_DELETED = " + (oProp.Deleted == null ? "null" : "'" + oProp.Deleted.ToString("yyyy-MM-dd HH:mm:ss")) + "'"
                                    + " WHERE PROP_ID = " + oProp.ID.ToString() + ";";

                    oCmd = new MySqlCommand(sSql, gConn);
                    oCmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "putProp", ex));
            }
            return false;
        }
        /// <summary>Delete Prop with given id from database</summary>
        /// <param name="lID">The id of the Prop to delete</param>
        /// <returns>True if Prop was deleted, false otherwise</returns>
        public bool deleteProp(long lID)
        {
            try
            {
                MySqlDataReader oMySQLReader = null;
                String sSql = "SELECT * FROM prop WHERE PROP_ID = " + lID.ToString() + ";";
                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    Prop oProp = CreateProp(oMySQLReader);
                    oMySQLReader.Close();
                    oProp.Deleted = DateTime.Now;
                    return putProp(oProp);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "deleteProp", ex));
            }
            return false;
        }
        /// <summary>Save Prop instance to database</summary>
        /// <param name="oProp">The Prop to save</param>
        /// <returns>The ID that the new Prop was given</returns>
        public long saveProp(Prop oProp)
        {
            try
            {
                String sSql = @"INSERT INTO prop
                            (COMP_ID,
                            COMP_TYPE,
                            PROP_TYPE
                            PROP_NAME,
                            PROP_VALUE,
                            PROP_PARENT,
                            PROP_CREATED,
                            PROP_MODIFIED)
                            VALUES
                            (" + oProp.Comp_ID + ", "
                                + oProp.Comp_Type + ", "
                                + oProp.Type + ", '"
                                + oProp.Name + "', '"
                                + oProp.Value + "', '"
                                + oProp.Created.ToString("yyyy-MM-dd HH:mm:ss") + "', '"
                                + oProp.Modified.ToString("yyyy-MM-dd HH:mm:ss") + "');";

                MySqlCommand oCmd = new MySqlCommand(sSql, gConn);
                oCmd.ExecuteNonQuery();
                return oCmd.LastInsertedId;
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "saveProp", ex));
            }
            return -1;
        }
        /// <summary>Create a new Prop instance using the values in the MySqlDataReader</summary>
        /// <param name="oMySQLReader">The reader to get the new Prop values from</param>
        /// <returns>A new Prop</returns>
        private Prop CreateProp(MySqlDataReader oMySQLReader)
        {
            Prop oProp = new Prop();
            try
            {
                int iPropCount = 0;
                PropertyInfo[] oProperties = typeof(Prop).GetProperties();
                foreach (PropertyInfo oProperty in oProperties)
                {
                    FillPropProperty(oMySQLReader, ref oProp, iPropCount++, oProperty);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "CreateProp", ex));
            }
            return oProp;
        }
        /// <summary>Check property type of iOrdinal property in oProp and retrieve value from oMySQLReader if not null</summary>
        /// <param name="oMySQLReader">The reader to get the property value from</param>
        /// <param name="oProp">The instance in which to set the property</param>
        /// <param name="iOrdinal">The position of the property in the instance</param>
        /// <param name="oProperty">The property info object</param>
        private void FillPropProperty(MySqlDataReader oMySQLReader, ref Prop oProp, int iOrdinal, PropertyInfo oProperty)
        {
            try
            {
                if (!oMySQLReader.IsDBNull(iOrdinal))
                {
                    if (oProperty.PropertyType == typeof(long)) { oProperty.SetValue(oProp, oMySQLReader.GetInt32(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(decimal)) { oProperty.SetValue(oProp, oMySQLReader.GetDecimal(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(DateTime)) { oProperty.SetValue(oProp, oMySQLReader.GetDateTime(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(string)) { oProperty.SetValue(oProp, oMySQLReader.GetString(iOrdinal)); return; }
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "FillPropProperty", ex));
            }
        }
        #endregion
    }
}