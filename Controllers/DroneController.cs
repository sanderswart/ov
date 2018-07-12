#region Using
using System;
using System.Web;
using System.Web.Http;
using ovrs.Models;
using System.Collections;
#endregion
namespace ovrs.Controllers
{
    public class DroneController : ApiController
    {
        #region Variables
        private string mClassName = "DroneController";
        #endregion
        #region HTTP Methods
        // GET: api/Drone/5
        /// <summary>Get Drone for Pilot with given ID</summary>
        /// <param name="lID">The id of the Pilot of the Drone to get</param>
        /// <returns>The Drone if found, an empty ArrayList otherwise</returns>
        public ArrayList Get(long id)
        {
            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get(" + id + ")", "Executing - Caller ID: ")); // + HttpContext.Current.Request.UserHostAddress));

            ComponentPersistence oCP = new ComponentPersistence();
            ArrayList oComps = oCP.getDrone(id);

            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get(" + id + ")", "Found: " + ((Component)oComps[0]).ToJSON()));

            return oComps;
        }
        #endregion
    }
}
