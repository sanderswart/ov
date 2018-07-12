#region Using
using ovrs.Models;
using System.Collections;
using System;
#endregion
namespace ovrs
{
    /// <summary>
    /// This is where the Gameplay is defined
    /// </summary>
    public class GPSupport
    {
        #region Variables
        private static string mStaticClassName = "GPSupport";
        private string mClassName = mStaticClassName;
        #endregion
        #region Methods
        /// <summary>A drone runs into a pilot</summary>
        /// <param name="lDroneID">The ID of the Drone</param>
        /// <param name="lPilotID">The ID of the Pilot</param>
        /// <returns>An ArrayList of Components that changed ownership (from Pilot to Drone)</returns>
        public static ArrayList Encounter(long lDroneID, long lPilotID)
        {
            DBSupport oDBSupport = new DBSupport();

            // ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Drone is running into Pilot and grabs all supplies it can carry (with a maximum of x % of Pilots supplies)
            // ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Get available cargo capacity for Drone
            // (TODO: Assume it can hold the maximum allowed to take from Pilot for now)

            // Get all supplies Pilot carries (TODO: 'owns' for now)
            string sSql = @"SELECT c.*
                        FROM ov.component c
                            INNER JOIN ov.type t ON c.COMP_TYPE = t.TYPE_ID
                                WHERE COALESCE(c.COMP_VISIBLE, DATE_ADD(NOW(), INTERVAL 1 DAY)) > NOW()
                                AND COALESCE(c.COMP_DELETED, DATE_ADD(NOW(), INTERVAL 1 DAY)) > NOW()
                                AND c.COMP_PARENT = " + lPilotID
                                + " AND t.TYPE_PARENT > 1;"; // 'TYPE_PARENT > 1' -> Excludes Pilots and Drones
            ArrayList oPilotSupplies = oDBSupport.getComponents(sSql);

            //EventLogger.Log(String.Format("{0}::{1} - {2}", mStaticClassName, "Encounter", "Pilot " + lPilotID + " owns " + oPilotSupplies.Count + " components"));

            // Divide Pilot supplies into a collection that can be carried and does not exceed x % of current amount
            // Change ownership for this collection from current Pilot to Drones Pilot
            // (TODO: Take one from each component type for now)
            ArrayList oTypes = new ArrayList();
            ArrayList oCompsExchanged = new ArrayList();
            foreach (Component oComp in oPilotSupplies)
            {
                // Check if one of this Type was not taken already
                if (!oTypes.Contains(oComp.Type))
                {
                    // Add Type to check array
                    oTypes.Add(oComp.Type);
                    // Change owner for Component
                    if (oDBSupport.ChangeOwner(oComp.ID, lDroneID, lPilotID))
                    {
                        // Add Component to array
                        oComp.Parent = lDroneID;
                        oCompsExchanged.Add(oComp);
                    }
                    else
                    {
                        // TODO: Log and/or notify if not succeeded
                        //EventLogger.Log(String.Format("{0}::{1} - {2}", mStaticClassName, "Encounter", "Changing ownership did not succeed!"));
                    }
                }
            }

            // Inform Pilots UI to check its updated inventory
            // TODO: Push a notification to Pilot

            // Return all components that were confiscated
            return oCompsExchanged;
        }
        #endregion
    }
}