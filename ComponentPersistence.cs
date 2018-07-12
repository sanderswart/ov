#region Using
using System;
using System.Text;
using ovrs.Models;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Threading;
using MySql.Data.MySqlClient;
#endregion
namespace ovrs
{
    public class ComponentPersistence
    {
        #region Variables
        private string mClassName = "ComponentPersistence";
        #endregion
        #region Constructors
        /// <summary>Constructor - Creating and opening database connection</summary>
        public ComponentPersistence()
        {
        }
        #endregion
        #region Methods
        /// <summary>Get all Components from database</summary>
        /// <returns>An ArrayList containing all Components in the database</returns>
        public ArrayList getComponents()
        {
            ArrayList compArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                String sSql = "SELECT * FROM component;";
                MySqlCommand cmd = new MySqlCommand(sSql, DBSupport.Conn);
                oMySQLReader = cmd.ExecuteReader();
                while (oMySQLReader.Read())
                {
                    compArray.Add(CreateComponent(oMySQLReader));
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getComponents", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;
            }
            return compArray;
        }
        /// <summary>Get all Components in range of Component with given ID</summary>
        /// <param name="lID">The id of the Component to start with</param>
        /// <returns>The Components in range if found, an empty ArrayList otherwise</returns>
        public ArrayList getComponent(long lID)
        {
            ArrayList oCompArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                Component oThisComponent = null;
                //String sSql = "SELECT * FROM component WHERE COMP_ID = " + lID.ToString() + ";";
                String sSql = "select_component;";
                MySqlCommand oCmd = new MySqlCommand(sSql, DBSupport.Conn);
                oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                oCmd.Parameters.AddWithValue("_comp_id", lID);
                oCmd.Parameters["_comp_id"].Direction = System.Data.ParameterDirection.Input;
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    oThisComponent = CreateComponent(oMySQLReader);
                    oMySQLReader.Close();

                    // For now just return the component
                    oCompArray.Add(oThisComponent);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getComponent", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;

            }
            return oCompArray;
        }
        /// <summary>Get Drone for Pilot with given ID</summary>
        /// <param name="lID">The id of the Pilot of the Drone to get</param>
        /// <returns>The Drone if found, an empty ArrayList otherwise</returns>
        public ArrayList getDrone(long lID)
        {
            ArrayList oCompArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                Component oThisDrone = null;
                String sSql = "select_drone;";
                MySqlCommand oCmd = new MySqlCommand(sSql, DBSupport.Conn);
                oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                oCmd.Parameters.AddWithValue("_pilot_id", lID);
                oCmd.Parameters["_pilot_id"].Direction = System.Data.ParameterDirection.Input;
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    ComponentPersistence oComponentPersistence = new ComponentPersistence();
                    oThisDrone = oComponentPersistence.CreateComponent(oMySQLReader);
                    oMySQLReader.Close();
                    oCompArray.Add(oThisDrone);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getDrone", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;

            }
            return oCompArray;
        }
        /// <summary>Get components in range of component with given ID</summary>
        /// <param name="lID">The id of the Pilot or Drone to get the components in range for</param>
        /// <returns>The components in range if found, an empty ArrayList otherwise</returns>
        public ArrayList getComponentsInRange(long lID)
        {
            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getComponentsInRange", String.Format("Calling for ID: {0}", lID)));

            ArrayList oCompArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                ComponentPersistence oCP = new ComponentPersistence();
                oCompArray = oCP.getComponent(lID);
                if (oCompArray.Count == 1)
                {
                    Component oComp = (Component)oCompArray[0];
                    Models.Type oType = new Models.Type();
                    if (oComp.Type == 2)
                    {
                        // For a Pilot, skip adding its Drone
                        oType.ID = 3;
                    }
                    else if (oComp.Type == 3)
                    {
                        // For a Drone, skip adding its Pilot
                        oType.ID = 2;
                    }
                    String sSql = "select_components_in_range;";
                    MySqlCommand oCmd = new MySqlCommand(sSql, DBSupport.Conn);
                    oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    oCmd.Parameters.AddWithValue("iLon", oComp.Lon);
                    oCmd.Parameters["iLon"].Direction = System.Data.ParameterDirection.Input;
                    oCmd.Parameters.AddWithValue("iLat", oComp.Lat);
                    oCmd.Parameters["iLat"].Direction = System.Data.ParameterDirection.Input;
                    oCmd.Parameters.AddWithValue("iOffset", 1); // Properties.Settings.Default.CIROffset);
                    oCmd.Parameters["iOffset"].Direction = System.Data.ParameterDirection.Input;
                    // Clear array
                    oCompArray = new ArrayList();
                    oMySQLReader = oCmd.ExecuteReader();
                    while (oMySQLReader.Read())
                    {
                        // Skip adding this Pilot/Drone and its Drone/Pilot (making it easier to process result client-side)
                        oComp = oCP.CreateComponent(oMySQLReader);
                        if (oComp.ID != lID && (oComp.Parent != lID || (oType.ID > 0 && oComp.Type != oType.ID)))
                        {
                            oCompArray.Add(oComp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getComponentsInRange", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;

            }
            return oCompArray;
        }
        /// <summary>Get components in possession of component with given ID</summary>
        /// <param name="lID">The id of the Pilot or Drone to get the components in possession for</param>
        /// <returns>The components in possession if found, an empty ArrayList otherwise</returns>
        public ArrayList getComponentsInPossession(long lID)
        {
            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getComponentsInPossession", String.Format("Calling for ID: {0}", lID)));

            ArrayList oCompArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                ComponentPersistence oCP = new ComponentPersistence();
                String sSql = "select_components_in_possession;";
                MySqlCommand oCmd = new MySqlCommand(sSql, DBSupport.Conn);
                oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                oCmd.Parameters.AddWithValue("_comp_id", lID);
                oCmd.Parameters["_comp_id"].Direction = System.Data.ParameterDirection.Input;
                oMySQLReader = oCmd.ExecuteReader();
                while (oMySQLReader.Read())
                {
                    oCompArray.Add(oCP.CreateComponent(oMySQLReader));
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getComponentsInPossession", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;

            }
            return oCompArray;
        }
        private struct Range
        {
            /// <summary>Offset from center of range</summary>
            public double Offset { get; set; }
            /// <summary>Longitude -/- Offset as string</summary>
            public string LonOMString { get; set; }
            /// <summary>Longitude +/+ Offset as string</summary>
            public string LonOPString { get; set; }
            /// <summary>Latitude -/- Offset as string</summary>
            public string LatOMString { get; set; }
            /// <summary>Latitude +/+ Offset as string</summary>
            public string LatOPString { get; set; }
            /// <summary>Longitude -/- Offset as double</summary>
            public double LonOMDouble { get; set; }
            /// <summary>Longitude +/+ Offset as double</summary>
            public double LonOPDouble { get; set; }
            /// <summary>Latitude -/- Offset as double</summary>
            public double LatOMDouble { get; set; }
            /// <summary>Latitude +/+ Offset as double</summary>
            public double LatOPDouble { get; set; }
            /// <summary>Constructor</summary>
            /// <param name="oComp">Component in center of Range</param>
            /// <param name="dOffset">The Offset to use to make the Range</param>
            public Range(Component oComp, double dOffset)
            {
                this.Offset = dOffset;

                this.LonOMString = (oComp.Lon - dOffset).ToString(CultureInfo.InvariantCulture);
                this.LonOPString = (oComp.Lon + dOffset).ToString(CultureInfo.InvariantCulture);
                this.LatOMString = (oComp.Lat - dOffset).ToString(CultureInfo.InvariantCulture);
                this.LatOPString = (oComp.Lat + dOffset).ToString(CultureInfo.InvariantCulture);

                this.LonOMDouble = double.Parse(this.LonOMString, CultureInfo.InvariantCulture);
                this.LonOPDouble = double.Parse(this.LonOPString, CultureInfo.InvariantCulture);
                this.LatOMDouble = double.Parse(this.LatOMString, CultureInfo.InvariantCulture);
                this.LatOPDouble = double.Parse(this.LatOPString, CultureInfo.InvariantCulture);
            }
            public override string ToString()
            {
                return string.Format("[Offset:{0};LonOMString:{1};LonOPString:{2};LatOMString:{3};LatOPString:{4};LonOMDouble:{5};LonOPDouble:{6};LatOMDouble:{7};LatOPDouble:{8};]", Offset, LonOMString, LonOPString, LatOMString, LatOPString, LonOMDouble, LonOPDouble, LatOMDouble, LatOPDouble);
            }
        }
        // Create a random object with a timer-generated seed.
        static void AutoSeedRandoms()
        {
            // Wait to allow the timer to advance.
            Thread.Sleep(1);

            Console.WriteLine(
                "\nRandom numbers from a Random object " +
                "with an auto-generated seed:");
            Random autoRand = new Random();

            RunIntNDoubleRandoms(autoRand);
        }
        // Generate random numbers from the specified Random object.
        static void RunIntNDoubleRandoms(Random randObj)
        {
            // Generate the first six random integers.
            for (int j = 0; j < 6; j++)
                Console.Write(" {0,10} ", randObj.Next());
            Console.WriteLine();

            // Generate the first six random doubles.
            for (int j = 0; j < 6; j++)
                Console.Write(" {0:F8} ", randObj.NextDouble());
            Console.WriteLine();
        }
        /// <summary>Update Component with ID to values in instance oComp</summary>
        /// <param name="lID">The ID of the Component to update</param>
        /// <param name="oComp">An instance to use to update the Component</param>
        /// <returns>True if update was succeeded, false otherwise</returns>
        public bool putComponent(Component oComp)
        {
            MySqlDataReader oMySQLReader = null;
            try
            {
                StringBuilder sbSQL = new StringBuilder("SELECT * FROM component WHERE COMP_ID = " + oComp.ID.ToString() + ";");
                MySqlCommand oCmd = new MySqlCommand(sbSQL.ToString(), DBSupport.Conn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    oMySQLReader.Close();

                    sbSQL.Clear();
                    sbSQL.Append("UPDATE component SET ");
                    if (oComp.Type > -1)
                    {
                        sbSQL.Append("COMP_TYPE = " + oComp.Type + ",");
                    }
                    if (oComp.Lon > -1)
                    {
                        sbSQL.Append("COMP_LON = " + oComp.Lon.ToString(CultureInfo.InvariantCulture) + ",");
                    }
                    if (oComp.Lat > -1)
                    {
                        sbSQL.Append("COMP_LAT = " + oComp.Lat.ToString(CultureInfo.InvariantCulture) + ",");
                    }
                    if (oComp.Parent > -1)
                    {
                        sbSQL.Append("COMP_PARENT = " + oComp.Parent.ToString() + ",");
                    }
                    sbSQL.Append("COMP_VISIBLE = " + (oComp.Visible == null ? "null" : "'" + oComp.Visible.ToString("yyyy-MM-dd HH:mm:ss")) + "',");
                    sbSQL.Append("COMP_MODIFIED = NOW(),");
                    sbSQL.Append("COMP_DELETED = " + (oComp.Deleted == null ? "null" : "'" + oComp.Deleted.ToString("yyyy-MM-dd HH:mm:ss")) + "'");
                    sbSQL.Append(" WHERE COMP_ID = " + oComp.ID.ToString() + ";");

                    oCmd = new MySqlCommand(sbSQL.ToString(), DBSupport.Conn);
                    oCmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "putComponent", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;
            }
            return false;
        }
        /// <summary>Delete Component with given id from database</summary>
        /// <param name="lID">The id of the Component to delete</param>
        /// <returns>True if Component was deleted, false otherwise</returns>
        public bool deleteComponent(long lID)
        {
            MySqlDataReader oMySQLReader = null;
            try
            {
                String sSql = "SELECT * FROM component WHERE COMP_ID = " + lID.ToString() + ";";
                MySqlCommand oCmd = new MySqlCommand(sSql, DBSupport.Conn);
                oMySQLReader = oCmd.ExecuteReader();
                if (oMySQLReader.Read())
                {
                    Component oComp = CreateComponent(oMySQLReader);
                    oMySQLReader.Close();
                    oComp.Deleted = DateTime.Now;
                    return putComponent(oComp);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "deleteComponent", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;
            }
            return false;
        }
        /// <summary>Save Component instance to database</summary>
        /// <param name="oComp">The Component to save</param>
        /// <returns>The ID that the new Component was given</returns>
        public long saveComponent(Component oComp)
        {
            try
            {
                String sSql = @"INSERT INTO component
                            (COMP_TYPE,
                            COMP_LON,
                            COMP_LAT,
                            COMP_PARENT,
                            COMP_CREATED,
                            COMP_MODIFIED)
                            VALUES
                            (" + oComp.Type + ", "
                                + oComp.Lon.ToString(CultureInfo.InvariantCulture) + ", "
                                + oComp.Lat.ToString(CultureInfo.InvariantCulture) + ", "
                                + oComp.Parent + ", '"
                                + oComp.Created.ToString("yyyy-MM-dd HH:mm:ss") + "', '"
                                + oComp.Modified.ToString("yyyy-MM-dd HH:mm:ss") + "');";

                MySqlCommand oCmd = new MySqlCommand(sSql, DBSupport.Conn);
                oCmd.ExecuteNonQuery();
                return oCmd.LastInsertedId;
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "saveComponent", ex));
            }
            return -1;
        }
        /// <summary>Create a new Component instance using the values in the MySqlDataReader</summary>
        /// <param name="oMySQLReader">The reader to get the new Component values from</param>
        /// <returns>A new Component</returns>
        public Component CreateComponent(MySqlDataReader oMySQLReader)
        {
            Component oComp = new Component();
            try
            {
                int iPropCount = 0;
                PropertyInfo[] oProperties = typeof(Component).GetProperties();
                foreach (PropertyInfo oProperty in oProperties)
                {
                    FillComponentProperty(oMySQLReader, ref oComp, iPropCount++, oProperty);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "CreateComponent", ex));
            }
            return oComp;
        }
        /// <summary>Check property type of iOrdinal property in oComp and retrieve value from oMySQLReader if not null</summary>
        /// <param name="oMySQLReader">The reader to get the property value from</param>
        /// <param name="oComp">The instance in which to set the property</param>
        /// <param name="iOrdinal">The position of the property in the instance</param>
        /// <param name="oProperty">The property info object</param>
        private void FillComponentProperty(MySqlDataReader oMySQLReader, ref Component oComp, int iOrdinal, PropertyInfo oProperty)
        {
            try
            {
                if (!oMySQLReader.IsDBNull(iOrdinal))
                {
                    if (oProperty.PropertyType == typeof(long)) { oProperty.SetValue(oComp, oMySQLReader.GetInt32(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(decimal)) { oProperty.SetValue(oComp, oMySQLReader.GetDecimal(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(double)) { oProperty.SetValue(oComp, oMySQLReader.GetDouble(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(DateTime)) { oProperty.SetValue(oComp, oMySQLReader.GetDateTime(iOrdinal)); return; }
                    if (oProperty.PropertyType == typeof(string)) { oProperty.SetValue(oComp, oMySQLReader.GetString(iOrdinal)); return; }
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "FillComponentProperty", ex));
            }
        }
        #region Inventory - Component
        /// <summary>Get all Components from database that have a parent</summary>
        /// <returns>An ArrayList containing all Components in the database having a parent</returns>
        public ArrayList getInventory()
        {
            ArrayList oCompArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                String sSql = "SELECT * FROM component WHERE COMP_Parent > 0;";
                MySqlCommand cmd = new MySqlCommand(sSql, DBSupport.Conn);
                oMySQLReader = cmd.ExecuteReader();
                while (oMySQLReader.Read())
                {
                    oCompArray.Add(CreateComponent(oMySQLReader));
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getInventory", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;
            }
            return oCompArray;
        }
        /// <summary>Get all Components having parent Component with given ID</summary>
        /// <param name="lID">The id of the parent Component to start with</param>
        /// <returns>The Components having parent Component if found, an empty ArrayList otherwise</returns>
        public ArrayList getInventory(long lID)
        {
            ArrayList oCompArray = new ArrayList();
            MySqlDataReader oMySQLReader = null;
            try
            {
                String sSql = "SELECT * FROM component WHERE COMP_Parent = " + lID.ToString() + ";";
                MySqlCommand cmd = new MySqlCommand(sSql, DBSupport.Conn);
                oMySQLReader = cmd.ExecuteReader();
                while (oMySQLReader.Read())
                {
                    oCompArray.Add(CreateComponent(oMySQLReader));
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "getInventory", ex));
            }
            finally
            {
                if (oMySQLReader != null && !oMySQLReader.IsClosed)
                    oMySQLReader.Close();
                oMySQLReader = null;

            }
            return oCompArray;
        }
        #endregion
        #endregion
    }
}