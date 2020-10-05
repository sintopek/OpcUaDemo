using Opc.Ua;

namespace OpcUaDemo.Data
{
    public class OpcValue
    {
        public string Key { get; set; }

        public ExpandedNodeId ExpandedNodeId { get; set; }

        public double? Value { get; set; }
    }
}
