#region Using
using System;
using System.Web;
using System.Web.Http;
using System.Collections;
#endregion
namespace ovrs.Controllers
{
    /// <summary>Get components in range of component with given id</summary>
    public class CIRController : ApiController
    {
        #region Variables
        private string mClassName = "CIRController";
        #endregion
        #region HTTP Methods
        // GET: api/cir/5
        public ArrayList Get(long id)
        {
            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get(" + id + ")", "Executing - Caller ID: ")); // + HttpContext.Current.Request.UserHostAddress));

            ComponentPersistence oCP = new ComponentPersistence();
            ArrayList oComps = oCP.getComponentsInRange(id);

            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get(" + id + ")", "Found : " + oComps.Count));

            return oComps;
        }
        #endregion
    }
}
