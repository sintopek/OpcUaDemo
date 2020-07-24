using System;
using System.Collections.Generic;
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
        public async Task<OpcNode> GetValueAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (string.IsNullOrEmpty(this.Url))
            {
                return new OpcNode();
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

            var node = new NodeId("ns=2;i=2");
            DataValue namespaceArrayNodeValue = this.session.ReadValue(node);

            var opcNode = new OpcNode
            {
                Value = namespaceArrayNodeValue.Value.ToString()
            };

            return opcNode;
        }

        public void SetValue(string value)
        {
            var node = new NodeId("ns=2;i=2");

            var header = new RequestHeader();

            var collection = new List<WriteValue>
            {
                new WriteValue
                {
                    NodeId = node,
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value))
                }
            };

            var nodesToWrite = new WriteValueCollection(collection);

            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            var response = this.session.Write(header, nodesToWrite, out results, out diagnosticInfos);
        }
    }
}
