using Pulumi;
using Pulumi.Oci.Core;
using Pulumi.Oci.Core.Inputs;
using System.Threading.Tasks;

class Program
{
    static Task<int> Main() => Deployment.RunAsync<MyStack>();
}

class MyStack : Stack
{
    public MyStack()
    {
        var config = new Config();
        var compartmentId = config.Require("compartmentId");
        var availabilityDomain = config.Require("availabilityDomain");
        var imageId = config.Require("imageId"); 
        var sshPublicKey = config.Require("sshPublicKey");

        // Virtual Cloud Network (VCN)
        var vcn = new Vcn("AppVcn", new VcnArgs
        {
            CompartmentId = compartmentId,
            CidrBlock = "10.0.0.0/16",
            DisplayName = "app-vcn"
        });

        // Internet Gateway
        var internetGateway = new InternetGateway("AppIg", new InternetGatewayArgs
        {
            CompartmentId = compartmentId,
            VcnId = vcn.Id,
            Enabled = true
        });

        // Route Table
        var routeTable = new RouteTable("AppRouteTable", new RouteTableArgs
        {
            CompartmentId = compartmentId,
            VcnId = vcn.Id,
            RouteRules = 
            {
                new RouteTableRouteRuleArgs
                {
                    NetworkEntityId = internetGateway.Id,
                    Destination = "0.0.0.0/0",
                    DestinationType = "CIDR_BLOCK"
                }
            }
        });

        // Security List
        var securityList = new SecurityList("AppSecurityList", new SecurityListArgs
        {
            CompartmentId = compartmentId,
            VcnId = vcn.Id,
            EgressSecurityRules = 
            {
                new SecurityListEgressSecurityRuleArgs
                {
                    Destination = "0.0.0.0/0",
                    Protocol = "all"
                }
            },
            IngressSecurityRules = 
            {
                new SecurityListIngressSecurityRuleArgs { Protocol = "6", Source = "0.0.0.0/0", TcpOptions = new SecurityListIngressSecurityRuleTcpOptionsArgs { Min = 22, Max = 22 } },
                new SecurityListIngressSecurityRuleArgs { Protocol = "6", Source = "0.0.0.0/0", TcpOptions = new SecurityListIngressSecurityRuleTcpOptionsArgs { Min = 80, Max = 80 } },
                new SecurityListIngressSecurityRuleArgs { Protocol = "6", Source = "0.0.0.0/0", TcpOptions = new SecurityListIngressSecurityRuleTcpOptionsArgs { Min = 443, Max = 443 } }
            }
        });

        // Public Subnet
        var subnet = new Subnet("AppSubnet", new SubnetArgs
        {
            CompartmentId = compartmentId,
            VcnId = vcn.Id,
            CidrBlock = "10.0.1.0/24",
            RouteTableId = routeTable.Id,
            SecurityListIds = { securityList.Id }
        });

        // Ampere A1 Compute Instance
        var instance = new Instance("AppVm", new InstanceArgs
        {
            CompartmentId = compartmentId,
            AvailabilityDomain = availabilityDomain,
            Shape = "VM.Standard.A1.Flex",
            ShapeConfig = new InstanceShapeConfigArgs
            {
                Ocpus = 2,
                MemoryInGbs = 12
            },
            CreateVnicDetails = new InstanceCreateVnicDetailsArgs
            {
                SubnetId = subnet.Id,
                AssignPublicIp = "true"
            },
            SourceDetails = new InstanceSourceDetailsArgs
            {
                SourceType = "image",
                // Corrected property name from ImageId to SourceId in OCI v4.x SDK
                SourceId = imageId 
            },
            Metadata = 
            {
                { "ssh_authorized_keys", sshPublicKey }
            }
        });

        this.VmPublicIp = instance.PublicIp;
    }

    [Output]
    public Output<string?> VmPublicIp { get; set; }
}