# Cloud Edge Deployment Scenario
This example shows the setup and usage of a distributed deployment. It contains two servers as shown in the illustration.

The following components are used: 

* [AAS Server](https://wiki.eclipse.org/BaSyx_/_Documentation_/_Components_/_AAS_Server)

* [AAS Registry](https://wiki.eclipse.org/BaSyx_/_Documentation_/_Components_/_Registry)

![Architecture Overview](https://wiki.eclipse.org/images/thumb/4/46/CloudEdgeDeploymentScenario.png/600px-CloudEdgeDeploymentScenario.png)



Server one is hosted directly on a smart device and provides values measured directly on the device (e.g. a current temperature). It is called "EdgeServer" in the scenario.

The second server is a cloud server, which hosts the Asset Administration Shell and another submodel containing static property values of the device (e.g. a maximum temperature). It is called "CloudServer".

The RegistryServer, EdgeServer and CloudServer are started in the methodes _startupRegistryServer()_, _startupEdgeServer()_ and _startupCloudServer()_ called by the constructor of the CloudEdgeDeploymentScenario class. 

A new Submodel called "docuSubmodel", and an example AAS "OvenAsset" are created for later usage.
The "OvenAsset" AAS is dierectly given to the startupEdgeServer(aas) method, when starting the Edge-Server. The Edge-Server is now a representation  

Also a AAS-Client (CloudClient) is initialized to communicate with the Cloud-Server, and a Submodel-Client (Edge-Client) to communicate with the Edge-Server.

First the "OvenAsset" AAS is pushed onto the Cloud-Server with the help of the Cloud-Client.
After that the Cloud-Server "OvenAsset" AAS is updated by uploading the docuSubmodel with help of the Cloud-Client.

```
RegistryHttpServer regServer = startupRegistryServer();

registryClient = new RegistryHttpClient();
ComponentBuilder._registryClient = registryClient;

//Create AAS and docuSubmodel 
AssetAdministrationShell aas = ComponentBuilder.getAAS();
Submodel docuSubmodel = ComponentBuilder.getDocuSubmodel();

//Start the CloudServer
AssetAdministrationShellRepositoryHttpServer cloudServer = startupCloudServer();

//Start the EdgeServer
SubmodelHttpServer edgeServer = startupEdgeServer(aas.Identification.Id);

//Init CloudClient
AssetAdministrationShellRepositoryHttpClient cloudClient = new AssetAdministrationShellRepositoryHttpClient(new Uri("http://localhost:8081"));
           
//Push AAS to Cloud Server
cloudClient.CreateAssetAdministrationShell(aas);

//Update AAS with docuSubmodel to Cloud Server
aas.Submodels.Add(docuSubmodel);
cloudClient.UpdateAssetAdministrationShell(aas.Identification.Id, aas);

//Add EdgeServer Submodel to CloudServer
SubmodelHttpClient edgeClient = new SubmodelHttpClient(new Uri("http://localhost:8082"));![image](https://github.com/eclipse-basyx/basyx-dotnet-examples/assets/77283144/e885a20e-5161-4d4a-9531-49055dcb02de)
