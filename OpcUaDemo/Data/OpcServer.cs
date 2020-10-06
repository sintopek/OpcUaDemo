using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace OpcUaDemo.Data
{
    public class OpcService
    {
        public string Url { get; set; }

        private Session session;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<OpcObject> ConnectAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (string.IsNullOrEmpty(this.Url))
            {
                return new OpcObject();
            }

            Console.WriteLine("Step 1 - Create application configuration and certificate.");
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "MyHomework",
                ApplicationUri = Utils.Format(@"urn:{0}:MyHomework", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = "MyHomework" },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                    AutoAcceptUntrustedCertificates = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };
            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }

            var application = new ApplicationInstance
            {
                ApplicationName = "MyHomework",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };
            application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();

            //var selectedEndpoint = CoreClientUtils.SelectEndpoint("opc.tcp://" + Dns.GetHostName() + ":48010", useSecurity: true/* , operationTimeout: 15000 */);
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(this.Url, false);

            Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");

            this.session = Session.Create(config, new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)), false, "", 60000, null, null).GetAwaiter().GetResult();

            return null;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<OpcObject> InitializeTree()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var opcNode = new OpcObject();

            this.ExpandTree(opcNode);

            return opcNode;
        }

        public void ExpandTree(OpcObject parent)
        {
            NodeId nodeId;

            if (parent.IsExtended)
            {
                return;
            }

            if (parent.Key != "$")
            {
                nodeId = ExpandedNodeId.ToNodeId(parent.ExpandedNodeId, this.session.NamespaceUris);
            }
            else
            {
                nodeId = ObjectIds.ObjectsFolder;
            }

            ReferenceDescriptionCollection refs;
            Byte[] cp;

            Browse(
                nodeId,
                (uint)NodeClass.Variable | (uint)NodeClass.Object,
                out refs, 
                out cp);

            foreach (var rd in refs)
            {
                ReferenceDescriptionCollection nextRefs;
                byte[] nextCp;

                if (rd.NodeClass == NodeClass.Object)
                {
                    Browse(
                        ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris),
                        (uint)NodeClass.Object,
                        out nextRefs,
                        out nextCp);

                    var obj = new OpcObject
                    {
                        Key = rd.DisplayName.ToString(),
                        ExpandedNodeId = rd.NodeId,
                    };

                    if (!nextRefs.Any())
                    {
                        obj.HasChildren = false;
                    }

                    parent.Children.Add(obj);
                }
                else
                {
                    var node = ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris);
                    DataValue nodeValue = this.session.ReadValue(node);

                    var obj = new OpcValue
                    {
                        Key = rd.DisplayName.ToString(),
                        Value = nodeValue.Value as double?,
                        ExpandedNodeId = rd.NodeId,
                    };

                    parent.Values.Add(obj);
                }
            }

            parent.IsExtended = true;
        }

        public void Read(OpcValue value)
        {
            var node = ExpandedNodeId.ToNodeId(value.ExpandedNodeId, session.NamespaceUris);
            DataValue nodeValue = this.session.ReadValue(node);

            value.Value = nodeValue.Value as double?;
        }

        public void Browse(
            NodeId nodeId, 
            uint flags,
            out ReferenceDescriptionCollection refs,
            out byte[] cp)
        {
            this.session.Browse(
                null,
                null,
                nodeId,
                0u,
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,
                flags,
                out cp,
                out refs);
        }

        public void SetValue(OpcValue value)
        {
            var node = ExpandedNodeId.ToNodeId(value.ExpandedNodeId, session.NamespaceUris);

            var header = new RequestHeader();

            var collection = new List<WriteValue>
            {
                new WriteValue
                {
                    NodeId = node,
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value.Value))
                }
            };

            var nodesToWrite = new WriteValueCollection(collection);

            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            var response = this.session.Write(header, nodesToWrite, out results, out diagnosticInfos);
        }
    }
}
