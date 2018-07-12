#region Using
using System;
using System.Collections;
using System.Globalization;
using ovrs.Models;
using MySql.Data.MySqlClient;
#endregion
namespace ovrs
{
    public class DBSupport
    {
        #region Variables
        private static string mClassName = "DBSupport";
        /// <summary>Connection object</summary>
        public static MySqlConnection Conn;
        #endregion
        #region Constructors
        static DBSupport()
        {
            try
            {
                Conn = new MySqlConnection();
                Conn.ConnectionString = "server=localhost;uid=ovuser;pwd=ovpassword;database=ov;"; // Properties.Settings.Default.MySqlConn;
                Conn.Open();

                //EventLogger.Log(String.Format("{0}::{1} - {2}: {3}", mClassName, "DBSupport", "Connection", Conn.State));
            }
            catch (MySqlException ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "DBSupport", ex));
            }
        }
        #endregion
        public ArrayList getComponents(string sSql)
        {
            ArrayList oCompArray = new ArrayList();
            try
            {
                Component oComp = null;
                ComponentPersistence oCP = new ComponentPersistence();
                MySqlDataReader oMySQLReader = null;
                MySqlCommand oCmd = new MySqlCommand(sSql, Conn);
                oMySQLReader = oCmd.ExecuteReader();
                while (oMySQLReader.Read())
                {
                    oComp = oCP.CreateComponent(oMySQLReader);
                    oCompArray.Add(oComp);
                }
                oMySQLReader.Close();
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getComponents", ex));
            }
            return oCompArray;
        }
        /// <summary>Change ownership for given Component from current to new owner and check current ownership (default = true)</summary>
        /// <param name="lCompID">The ID of the Component to update the Parent for</param>
        /// <param name="lNewOwnerID">The new Parent</param>
        /// <param name="lCurrentOwnerID">The current Parent</param>
        /// <param name="bCheckOwner">True (default) to check the current owner, false otherwise</param>
        /// <returns>True if exactly one row was affected, false otherwise</returns>
        public bool ChangeOwner(long lCompID, long lNewOwnerID, long lCurrentOwnerID = 0, bool bCheckOwner = true)
        {
            string sSql = @"UPDATE ov.component c
                            SET c.COMP_PARENT = " + lNewOwnerID
                            + " WHERE c.COMP_ID = " + lCompID;
            if (bCheckOwner)
            {
                sSql += " AND c.COMP_PARENT = " + lCurrentOwnerID;
            }
            sSql += ";";
            MySqlCommand oCmd = new MySqlCommand(sSql, Conn);
            int iRows = oCmd.ExecuteNonQuery();
            // TODO: Log and/or notify if rows affected <> 1
            return iRows == 1;
        }
    }
}