using System;
using System.Runtime.Serialization;

namespace ovrs.Models
{
    [Serializable]
    [DataContract]
    public class Component
    {
        [DataMember]
        public long ID { get; set; }
        [DataMember]
        public long Type { get; set; }
        [DataMember]
        public double Lon { get; set; }
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public double DestLon { get; set; }
        [DataMember]
        public double DestLat { get; set; }
        [DataMember]
        public double Heading { get; set; }
        [DataMember]
        public double Speed { get; set; }
        [DataMember]
        public long Parent { get; set; }
        [DataMember]
        public DateTime Visible { get; set; }
        [DataMember]
        public DateTime Created { get; set; }
        [DataMember]
        public DateTime Modified { get; set; }
        [DataMember]
        public DateTime Deleted { get; set; }
        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        public override string ToString()
        {
            return string.Format("ID: {0};Type: {1};Lon: {2};Lat: {3};DestLon: {4};DestLat: {5};Heading: {6};Speed: {7};Parent: {8};Visible: {9};Created: {10};Modified: {11};Deleted: {12}", ID, Type, Lon, Lat, DestLon, DestLat, Heading, Speed, Parent, Visible, Created, Modified, Deleted);
        }
    }
}