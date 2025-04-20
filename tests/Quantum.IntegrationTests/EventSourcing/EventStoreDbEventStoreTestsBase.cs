using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Quantum.EventSourcing.EventStoreDB;

namespace Quantum.IntegrationTests.EventSourcing
{
    public class EventStoreDbEventStoreTestsBase : IDisposable
    {
        private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();
        private readonly CreateContainerResponse _createContainerResponse;
        public EventStoreDbConfig EventStoreDbConfig;
        public EventStoreDbEventStoreTestsBase()
        {
            var hostPort = new Random((int)DateTime.UtcNow.Ticks).Next(10000, 12000);
            _createContainerResponse = CreateContainer(hostPort).Result;
            
            EventStoreDbConfig = new EventStoreDbConfig
            {
                Port = hostPort.ToString(),
                Url = "localhost",
                ConnectionName = "IntegrationTestConnection"
            };
        }


        private async Task<CreateContainerResponse> CreateContainer(int hostPort)
        {
            var image = "eventstore/eventstore";
            var tag = "21.10.0-buster-slim";

            //look for image
            var images = await _client.Images.ListImagesAsync(new ImagesListParameters()
            {

            }, CancellationToken.None);

            //check if container exists
            var esImage = images.FirstOrDefault(i => i.RepoTags.Contains($"{image}:{tag}"));
            if (esImage == null)
                throw new Exception($"Docker image for {image}:{tag} not found.");

            var createContainerResponse = await _client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = $"{image}:{tag}",
                //Name = "EventStoreDb.IntegrationTest " + hostPort,

                Env = new List<string>
                {
                    "EVENTSTORE_CLUSTER_SIZE=1",
                    "EVENTSTORE_RUN_PROJECTIONS=All",
                    "EVENTSTORE_START_STANDARD_PROJECTIONS=true",
                    "EVENTSTORE_EXT_TCP_PORT=1113",
                    $"EVENTSTORE_HTTP_PORT={hostPort}",
                    "EVENTSTORE_INSECURE=true",
                    "EVENTSTORE_ENABLE_EXTERNAL_TCP=true",
                    "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP=true"
                },
                Volumes = new Dictionary<string, EmptyStruct> { },

                WorkingDir = "",
                ExposedPorts = new Dictionary<string, EmptyStruct>()
                {
                    [hostPort.ToString()] = new EmptyStruct()
                },
                HostConfig = new HostConfig()
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>()
                    {
                        [hostPort.ToString()] = new List<PortBinding>()
                            {new PortBinding() {HostIP = "0.0.0.0", HostPort = $"{hostPort}"}}
                    }
                },
            });

            if (!await _client.Containers.StartContainerAsync(createContainerResponse.ID, new ContainerStartParameters()
            {
                DetachKeys = $"d={image}"
            }, CancellationToken.None))
            {
                throw new Exception($"Could not start container: {createContainerResponse.ID}");
            }

            return createContainerResponse;
        }


        public void Dispose() => DeleteContainer().Wait();
        //public void Dispose() { }

        private async Task DeleteContainer()
        {
            if (await _client.Containers.StopContainerAsync(_createContainerResponse.ID, new ContainerStopParameters(), CancellationToken.None))
            {
                //delete container
                await _client.Containers.RemoveContainerAsync(_createContainerResponse.ID, new ContainerRemoveParameters(), CancellationToken.None);
            }
        }
    }
}