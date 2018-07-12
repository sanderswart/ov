#region Using
using System;
using System.Web;
using System.Web.Http;
using System.Collections;
#endregion
namespace ovrs.Controllers
{
    /// <summary>Get components in possession of component with given id</summary>
    public class INVController : ApiController
    {
        #region Variables
        private string mClassName = "INVController";
        #endregion
        #region HTTP Methods
        // GET: api/inv/5
        public ArrayList Get(long id)
        {
            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get(" + id + ")", "Executing - Caller ID: ")); // + HttpContext.Current.Request.UserHostAddress));

            ComponentPersistence oCP = new ComponentPersistence();
            ArrayList oComps = oCP.getComponentsInPossession(id);

            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get(" + id + ")", "Found : " + oComps.Count));

            return oComps;
        }
        #endregion
    }
}
