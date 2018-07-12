#region Using
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ovrs.Models;
using System.Collections;
using Microsoft.Extensions.Logging;
#endregion
namespace ovrs.Controllers
{
    public class InventoryController : ApiController
    {
        #region Variables
        private string mClassName = "InventoryController";
        #endregion
        #region Properties
        ILogger Logger { get; } = ApplicationLogging.CreateLogger<InventoryController>();
        #endregion
        #region HTTP Methods
        // GET: api/Component
        public ArrayList Get()
        {
            
            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get()", "Executing"));

            ComponentPersistence oCP = new ComponentPersistence();
            ArrayList oComps = oCP.getInventory();

            return oComps;
        }
        // GET: api/Component/5
        public ArrayList Get(long id)
        {
            //EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "Get(" + id + ")", "Executing - Caller ID: ")); // + HttpContext.Current.Request.UserHostAddress));

            ComponentPersistence oCP = new ComponentPersistence();
            ArrayList oComps = oCP.getInventory(id);

            return oComps;
        }
        // POST: api/Component
        public HttpResponseMessage Post([FromBody]Component value)
        {
            ComponentPersistence oCP = new ComponentPersistence();
            long lID = oCP.saveComponent(value);
            HttpResponseMessage oResponse = Request.CreateResponse(HttpStatusCode.Created);
            oResponse.Headers.Location = new Uri(Request.RequestUri, String.Format("{0}", lID));
            return oResponse;
        }
        // PUT: api/Component/5
        public HttpResponseMessage Put([FromBody]Component value)
        {
            ComponentPersistence oCP = new ComponentPersistence();
            HttpResponseMessage oResponse = Request.CreateResponse(oCP.putComponent(value) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
            return oResponse;
        }
        // DELETE: api/Component/5
        public HttpResponseMessage Delete(long id)
        {
            ComponentPersistence oCP = new ComponentPersistence();
            HttpResponseMessage oResponse = Request.CreateResponse(oCP.deleteComponent(id) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
            return oResponse;
        }
        #endregion
    }
}
