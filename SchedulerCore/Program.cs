using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;

namespace SchedulerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();

            IKubernetes client = new Kubernetes(config);
            //ApiClient.setDebugging(true);

            Console.WriteLine("Starting Request!");

            // ListPods(client);


            //var metaDataDictionary = new Dictionary<string, string>();
            //metaDataDictionary.Add("name", "web-pod1");
            //metaDataDictionary.Add("namespace", "default");
            //client.CreateNamespacedPod(new k8s.Models.V1Pod()
            //{
            //    ApiVersion = "v1",
            //    Kind = "Pod",
            //    Metadata = new k8s.Models.V1ObjectMeta() { Labels = metaDataDictionary },
            //    Spec = new k8s.Models.V1PodSpec()
            //    {
            //        Containers = new List<k8s.Models.V1Container>() {
            //              new k8s.Models.V1Container() {
            //                   Image = "abhishekagg24/stableapp:v1",
            //                    Name = "web-ctr1",
            //                    Ports = new List<k8s.Models.V1ContainerPort>(){
            //                         new k8s.Models.V1ContainerPort() { ContainerPort = 8080 }

            //                    }
            //              }
            //         }

            //    },
            //    //Status = new k8s.Models.V1PodStatus() {  }

            //}            
            //, "default");

            //V1Pod obj = CreatePodDefinition();
            //client.CreateNamespacedPod(obj, "default");

             V1Deployment depl = CreateDeploymentDefinition();
             var deployment =  client.CreateNamespacedDeployment(depl, "default");

            //if (deployment != null)
            //{
            ListPods(client);
            // }


            Console.ReadKey();
        }


        private static V1Pod CreatePodDefinition()
        {
            Guid podIdentifier = Guid.NewGuid();
            var podidentifierstring = podIdentifier.ToString();
            string podName = "pod" + podidentifierstring;
            string containerPortName = "containerport";
            string containerName = "container";


            V1ObjectMeta meta = new V1ObjectMeta();
            meta.Name = podName;

            V1EnvVar addr = new V1EnvVar();
            addr.Name = "ACTIVITY_PARTITION_NAME";
            addr.Value = "bam_EnrichedFPAttributesV2_Instances";

            //V1EnvVar port = new V1EnvVar();
            //addr.name("var2");
            //addr.value("value2");

            //V1ResourceRequirements res = new V1ResourceRequirements();
            //Map<String, String> limits = new HashMap<>();
            //limits.put("cpu", "300m");
            //limits.put("memory", "500Mi");
            //res.limits(limits);

            V1ContainerPort port = new V1ContainerPort();
            port.Name = containerPortName;
            port.ContainerPort = 8080;

            V1Container container = new V1Container();
            container.Name = containerName;
            container.Image = "bamreplacementwebapp:dev";
            //container.Image = "nbsbamreplacementmigrationservice:dev";
            //container.Image = "migrationservice:dev";
            container.Ports = new List<V1ContainerPort>();
            container.Ports.Add(port);
            container.Env = new List<V1EnvVar>();
            container.Env.Add(addr);
            //container.Args = new List<string>();
            //container.Args.Add("bam_EnrichedFPAttributesV2_Instances");
            // container.Args.Add("bar");

            // container.env(Arrays.asList(addr, port));
            //container.resources(res);

            V1PodSpec spec = new V1PodSpec();
            spec.Containers = new List<V1Container>();
            spec.Containers.Add(container);

            V1Pod podBody = new V1Pod();
            podBody.ApiVersion = "v1";
            podBody.Kind = "Pod";
            podBody.Metadata = meta;
            podBody.Spec = spec;

            return podBody;

        }


        private static V1Deployment CreateDeploymentDefinition()
        {

            Guid podIdentifier = Guid.NewGuid();
            var podidentifierstring = podIdentifier.ToString();
            string podName = "pod-" + podidentifierstring;
            string containerPortName = "containerport";
            string containerName = "container";
            string deploymentName = "poddeployment-" + podIdentifier;

            V1ObjectMeta deplMetadata = new V1ObjectMeta();
            deplMetadata.Name = deploymentName;
            deplMetadata.Labels = new Dictionary<string, string>();
            deplMetadata.Labels.Add("component", "migrationservice");

            V1DeploymentSpec deplSpec = new V1DeploymentSpec();
            deplSpec.Selector = new V1LabelSelector();
            deplSpec.Selector.MatchLabels = new Dictionary<string, string>();
            deplSpec.Selector.MatchLabels.Add("app", "bamreplacement");
            deplSpec.Replicas = 3; // to be tokenized

            deplSpec.Strategy = new V1DeploymentStrategy();
            deplSpec.Strategy.Type = "RollingUpdate";
            deplSpec.Strategy.RollingUpdate = new V1RollingUpdateDeployment();
            deplSpec.Strategy.RollingUpdate.MaxSurge = 1;
            deplSpec.Strategy.RollingUpdate.MaxUnavailable = 0;

            V1ObjectMeta podMetadata = new V1ObjectMeta();
            podMetadata.Name = podName;
            podMetadata.Labels = new Dictionary<string, string>();
            podMetadata.Labels.Add("app", "bamreplacement");

            V1ContainerPort port = new V1ContainerPort();
            port.Name = containerPortName;
            port.ContainerPort = 8080;

            V1EnvVar addr = new V1EnvVar();
            addr.Name = "ACTIVITY_PARTITION_NAME";
            addr.Value = "bam_EnrichedFPAttributesV2_Instances";

            V1Container container = new V1Container();
            container.Name = containerName;
            // container.Image = "abhishekagg24/stableapp:v1";
            //container.Image = "nbsbamreplacementmigrationservice:dev";
            container.Image = "bamreplacementwebapp:dev";
            container.Ports = new List<V1ContainerPort>();
            container.Ports.Add(port);
            container.Env = new List<V1EnvVar>();
            container.Env.Add(addr);



            V1PodSpec podSpec = new V1PodSpec();
            podSpec.Containers = new List<V1Container>();
            podSpec.Containers.Add(container);

            deplSpec.Template = new V1PodTemplateSpec();
            deplSpec.Template.Metadata = podMetadata;
            deplSpec.Template.Spec = podSpec;


            V1Deployment deployment = new V1Deployment();
            deployment.ApiVersion = "apps/v1";
            deployment.Kind = "Deployment";
            deployment.Metadata = deplMetadata;
            deployment.Spec = deplSpec;

            return deployment;
        }


        private static void ListPods(IKubernetes client)
        {
            var list = client.ListNamespacedPod("default");

            foreach (var item in list.Items)
            {
                Console.WriteLine(item.Metadata.Name);
            }

            if (list.Items.Count == 0)
            {
                Console.WriteLine("Empty!");
            }

        }
    }
}
