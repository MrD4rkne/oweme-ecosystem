using Pulumi;
using Pulumi.Oci.Core;
using Pulumi.Oci.Core.Inputs;
using Pulumi.Oci.Identity;
using Pulumi.Oci.ObjectStorage;
using Pulumi.Oci.ObjectStorage.Inputs;
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
        string Named(string resource) => $"{Deployment.Instance.StackName}-{resource}"; // oweme-s-vm, oweme-p-vm

        // Virtual Cloud Network (VCN)
        var vcn = new Vcn("AppVcn", new VcnArgs
        {
            CompartmentId = compartmentId,
            CidrBlock = "10.0.0.0/16",
            DisplayName = Named("vcn")
        });

        // Internet Gateway
        var internetGateway = new InternetGateway("AppIg", new InternetGatewayArgs
        {
            DisplayName = Named("ig"),
            CompartmentId = compartmentId,
            VcnId = vcn.Id,
            Enabled = true
        });

        // Route Table
        var routeTable = new RouteTable("AppRouteTable", new RouteTableArgs
        {
            DisplayName = Named("route-table"),
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
            DisplayName = Named("security-list"),
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
            DisplayName = Named("subnet"),
            CompartmentId = compartmentId,
            VcnId = vcn.Id,
            CidrBlock = "10.0.1.0/24",
            RouteTableId = routeTable.Id,
            SecurityListIds = { securityList.Id }
        });

        // Ampere A1 Compute Instance
        var instance = new Instance("AppVm", new InstanceArgs
        {
            DisplayName = Named("vm"),
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

        // Off-VM database backups (issue #101).
        var ns = GetNamespace.Invoke(new GetNamespaceInvokeArgs { CompartmentId = compartmentId })
            .Apply(r => r.Namespace);

        // Object Storage can't run lifecycle rules without this. One per stack: the duplicate statement is harmless
        // and saves picking an owner stack. IAM takes a few seconds to apply; if the first `up` fails on the
        // lifecycle rule, run it again.
        // OCI requires this policy in the tenancy root (docs: usinglifecyclepolicies.htm, "Service Permissions").
        // The app compartment sits directly under root, so its parent is the tenancy.
        var tenancyId = GetCompartment.Invoke(new GetCompartmentInvokeArgs { Id = compartmentId })
            .Apply(c => c.CompartmentId);
        var region = new Config("oci").Require("region");
        var lifecyclePolicy = new Policy("BackupLifecycleIamPolicy", new PolicyArgs
        {
            CompartmentId = tenancyId,
            Name = Named("objectstorage-lifecycle"),
            Description = "Lets Object Storage run lifecycle rules on the oweme backup bucket",
            Statements = { $"Allow service objectstorage-{region} to manage object-family in compartment id {compartmentId}" }
        });

        var bucket = new Bucket("BackupBucket", new BucketArgs
        {
            CompartmentId = compartmentId,
            Namespace = ns,
            Name = Named("backups") // stacks share one compartment
        });

        _ = new ObjectLifecyclePolicy("BackupLifecycle", new ObjectLifecyclePolicyArgs
        {
            Namespace = ns,
            Bucket = bucket.Name,
            Rules =
            {
                new ObjectLifecyclePolicyRuleArgs
                {
                    Name = "delete-after-14-days",
                    Action = "DELETE",
                    Target = "objects",
                    TimeAmount = "14",
                    TimeUnit = "DAYS",
                    IsEnabled = true
                }
            }
        }, new CustomResourceOptions { DependsOn = { lifecyclePolicy } });

        // VM uploads and restores; the scheduled backup-check only reads.
        this.BackupParUrl = BackupPar("BackupParReadWrite", "AnyObjectReadWrite", ns, bucket);
        this.BackupCheckParUrl = BackupPar("BackupParRead", "AnyObjectRead", ns, bucket);
    }

    // PARs can't be updated: any input change replaces the PAR and issues a new URL.
    // Keep the expiry a literal, never DateTime.Now-based. The URL is returned only at creation.
    static Output<string> BackupPar(string name, string accessType, Output<string> ns, Bucket bucket) =>
        new Preauthrequest(name, new PreauthrequestArgs
        {
            Namespace = ns,
            Bucket = bucket.Name,
            Name = name,
            AccessType = accessType,
            BucketListingAction = "ListObjects",
            TimeExpires = "2099-01-01T00:00:00Z"
        }, new CustomResourceOptions
        {
            AdditionalSecretOutputs = { "fullPath", "accessUri" }
        }).FullPath;

    [Output]
    public Output<string?> VmPublicIp { get; set; }

    [Output]
    public Output<string> BackupParUrl { get; set; }

    [Output]
    public Output<string> BackupCheckParUrl { get; set; }
}