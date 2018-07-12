using System;

namespace ovrs.Models
{
    public class Type
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Parent { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Deleted { get; set; }
    }
}