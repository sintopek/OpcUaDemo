using Opc.Ua;
using System.Collections;
using System.Collections.Generic;

namespace OpcUaDemo.Data
{
    public class OpcObject
    {
        public OpcObject()
        {
            this.Key = "$";
            this.HasChildren = true;
            this.Values = new List<OpcValue>();
            this.Children = new List<OpcObject>();
        }

        public string Key { get; set; }
        
        public ExpandedNodeId ExpandedNodeId { get; set; }

        public IList<OpcValue> Values { get; set; }

        public IList<OpcObject> Children { get; set; }

        public bool HasChildren { get; set; }

        public bool IsExtended { get; set; }
    }
}
