#region Using
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections;
using ovrs.Models;
#endregion
namespace ovrs.Controllers
{
    public class PropController : ApiController
    {
        #region HTTP Methods
        // GET: api/Prop
        public ArrayList Get()
        {
            PropPersistence oPP = new PropPersistence();
            ArrayList oProps = oPP.getProps();

            return oProps;
        }
        // GET: api/Prop/5
        public Prop Get(long id)
        {
            PropPersistence oPP = new PropPersistence();
            Prop oProp = oPP.getProp(id);

            return oProp;
        }
        // POST: api/Prop
        public HttpResponseMessage Post([FromBody]Prop value)
        {
            PropPersistence oPP = new PropPersistence();
            long lID = oPP.saveProp(value);
            HttpResponseMessage oResponse = Request.CreateResponse(HttpStatusCode.Created);
            oResponse.Headers.Location = new Uri(Request.RequestUri, String.Format("{0}", lID));
            return oResponse;
        }
        // PUT: api/Prop/5
        public HttpResponseMessage Put([FromBody]Prop value)
        {
            PropPersistence oPP = new PropPersistence();
            HttpResponseMessage oResponse = Request.CreateResponse(oPP.putProp(value) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
            return oResponse;
        }
        // DELETE: api/Prop/5
        public HttpResponseMessage Delete(long id)
        {
            PropPersistence oPP = new PropPersistence();
            HttpResponseMessage oResponse = Request.CreateResponse(oPP.deleteProp(id) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
            return oResponse;
        }
        #endregion
    }
}
