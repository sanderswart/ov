#region Using
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections;
#endregion
namespace ovrs.Controllers
{
    public class TypeController : ApiController
    {
        #region HTTP Methods
        // GET: api/Type
        public ArrayList Get()
        {
            TypePersistence oTP = new TypePersistence();
            ArrayList oTypes = oTP.getTypes();

            return oTypes;
        }
        // GET: api/Type/5
        public Models.Type Get(long id)
        {
            TypePersistence oTP = new TypePersistence();
            Models.Type oType = oTP.getType(id);

            return oType;
        }
        // POST: api/Type
        public HttpResponseMessage Post([FromBody]Models.Type value)
        {
            TypePersistence oTP = new TypePersistence();
            long lID = oTP.saveType(value);
            HttpResponseMessage oResponse = Request.CreateResponse(HttpStatusCode.Created);
            oResponse.Headers.Location = new Uri(Request.RequestUri, String.Format("{0}", lID));
            return oResponse;
        }
        // PUT: api/Type/5
        public HttpResponseMessage Put([FromBody]Models.Type value)
        {
            TypePersistence oTP = new TypePersistence();
            HttpResponseMessage oResponse = Request.CreateResponse(oTP.putType(value) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
            return oResponse;
        }
        // DELETE: api/Type/5
        public HttpResponseMessage Delete(long id)
        {
            TypePersistence oTP = new TypePersistence();
            HttpResponseMessage oResponse = Request.CreateResponse(oTP.deleteType(id) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
            return oResponse;
        }
        #endregion
    }
}
