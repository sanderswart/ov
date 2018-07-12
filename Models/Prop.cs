using System;

namespace ovrs.Models
{
    public class Prop
    {
        public long ID { get; set; }
        public long Comp_ID { get; set; }
        public long Comp_Type { get; set; }
        public long Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Deleted { get; set; }
    }
}