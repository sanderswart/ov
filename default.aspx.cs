#region "Using"
using System;
using System.Collections;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using ovrs;
using Logger;
#endregion
namespace ovcs
{
    public partial class _default : System.Web.UI.Page
    {
        #region Variables
        private string mClassName = "_default";
        #endregion
        #region "Properties"
        public string APIKey { get { return Properties.Settings.Default.GoogleMapsJSAPI; } }
        public string MapStyles { get { return Properties.Settings.Default.GoogleMapsStyles; } }
        public string WebAPIUrl { get { return Properties.Settings.Default.WebAPIUrl; } }
       
        public ovrs.Models.Component Pilot
        {
            get { return (ovrs.Models.Component)(ViewState["PILOT"] ?? new ovrs.Models.Component()); }
            set { ViewState["PILOT"] = value; }
        }
        public ovrs.Models.Component Drone
        {
            get { return (ovrs.Models.Component)(ViewState["DRONE"] ?? new ovrs.Models.Component()); }
            set { ViewState["DRONE"] = value; }
        }
        #endregion
        #region "Enumerations"
        public enum enuCallbackType
        {
            None = 0,
            PlacePilot = 1,
            PlaceDrone = 2
        }
        #endregion
        #region "Event handler"
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        protected void Page_PreRender(object sender, EventArgs e)
        {
            pnlLogin.Visible = Pilot.ID == 0;
            pnlMap.Visible = !pnlLogin.Visible;
            droneInstrumentPanel.Visible = !pnlLogin.Visible;
        }
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text != "" && txtUsername.Text == txtPassword.Text)
            {
                long lPilotID;
                if (long.TryParse(txtUsername.Text.Substring(txtUsername.Text.Length - 1), out lPilotID))
                {
                    if (Login(lPilotID))
                    {
                        // Get Pilot
                        bool bWebAPICalled = CallWebAPI(WebAPIUrl + "component/" + lPilotID, enuCallbackType.PlacePilot);
                        EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "btnLogin_Click", bWebAPICalled ? "Web API called for Pilot" : "Web API NOT called for Pilot"));

                        if (bWebAPICalled)
                        {
                            // Get Drone for Pilot
                            bWebAPICalled = CallWebAPI(WebAPIUrl + "drone/" + lPilotID, enuCallbackType.PlaceDrone);
                            EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "btnLogin_Click", bWebAPICalled ? "Web API called for Drone" : "Web API NOT called for Drone"));
                        }

                    }
                }
            }
        }
        #endregion
        #region "Methods"
        private bool CallWebAPI(string sWebAPICall, enuCallbackType eCallbackType = enuCallbackType.None)
        {
            bool bCalled = false;

            try
            {
                if (sWebAPICall != "")
                {
                    WebRequest oRequest = WebRequest.Create(sWebAPICall);
                    HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
                    if (oResponse.StatusCode == HttpStatusCode.OK)
                    {
                        Stream oStream = oResponse.GetResponseStream();
                        StreamReader oReader = new StreamReader(oStream);
                        string sResponse = oReader.ReadToEnd();
                        oReader.Close();
                        oStream.Close();
                        oResponse.Close();
                        PlaceComponent(eCallbackType, sResponse);
                    }
                    bCalled = true;
                }
            }
            catch (Exception ex)
            {
                EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "CallWebAPI", ex));
            }

            return bCalled;
        }
        private bool Login(long lPilotID)
        {
            bool bResult = (lPilotID > 0);

            // Create Login procedure
            // TODO        

            return bResult;
        }
        private bool PlaceComponent(enuCallbackType eCallbackType, string sJSON)
        {
            EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "PlaceComponent", "Received: " + sJSON));

            List<ovrs.Models.Component> oList = JsonConvert.DeserializeObject<List<ovrs.Models.Component>>(sJSON);
            ovrs.Models.Component oComponent = oList[0];
            switch (eCallbackType)
            {
                case enuCallbackType.PlacePilot:
                case enuCallbackType.PlaceDrone:
                    return PlaceComponent(eCallbackType, oComponent);
            }
            return false;
        }
        private bool PlaceComponent(enuCallbackType eCallbackType, ovrs.Models.Component oComponent)
        {
            bool bResult = false;

            try
            {
                switch (eCallbackType)
                {
                    case enuCallbackType.PlacePilot:
                        EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "PlaceComponent", "Placing Pilot on map"));
                        Pilot = oComponent;
                        bResult = true;
                        break;
                    case enuCallbackType.PlaceDrone:
                        EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "PlaceComponent", "Placing Drone on map"));
                        Drone = oComponent;
                        bResult = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLogger.Log(String.Format("{0}::{1} - {2}", mClassName, "PlaceComponent", ex));
            }

            return bResult;
        }
        #endregion
    }
}