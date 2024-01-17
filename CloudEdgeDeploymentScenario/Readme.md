# Cloud Edge Deployment Scenario
This example shows the enrichment of AAS data loaded from an aasx file with a dynamically created Submodel. 

The following components are used: 

* [AAS Server](https://wiki.eclipse.org/BaSyx_/_Documentation_/_Components_/_AAS_Server)

* [AAS Registry](https://wiki.eclipse.org/BaSyx_/_Documentation_/_Components_/_Registry)

![Architecture Overview](https://wiki.eclipse.org/images/thumb/4/46/CloudEdgeDeploymentScenario.png/600px-CloudEdgeDeploymentScenario.png)



In the first step, the AAS-Server and Registry-Server are initiated using the methods startupRegistryServer() and startupAASServer() respectively.

An AAS-Client is also set up to facilitate communication with the AAS-Server.

Following this, the AASX Class is employed to extract AAS data from a provided .aasx file.

The extracted AAS information from the file is then created on the AAS-Server with the assistance of the AAS-Client.

In addition, a new Submodel is added to the AAS-Server as an update to the now existing AAS on the AAS-Server.


```
// Startup the registry
RegistryHttpServer regServer = startupRegistryServer();
registryClient = new RegistryHttpClient();

//Start the AAS Server (empty)
AssetAdministrationShellRepositoryHttpServer aasServer = startupAASServer("http://localhost:8081", "https://localhost:8071");

//Init AAS Client
AssetAdministrationShellRepositoryHttpClient aasClient = new AssetAdministrationShellRepositoryHttpClient(new Uri("http://localhost:8081"));

// Load .aasx file
AASX aasx = new AASX("aasx/EXAMPLE.aasx"); //AASX Package Manager
AssetAdministrationShellEnvironment_V2_0 aasx_enviroment = aasx.GetEnvironment_V2_0(); //Load AASX with correct AASX Enviroment
List<AssetAdministrationShell> bundles = aasx_enviroment.AssetAdministrationShells.ConvertAll(x => (AssetAdministrationShell)x); //Load list of AAS (Bundles) from .aasx file
AssetAdministrationShell aas = bundles.Find(x => x.IdShort.Equals("EXAMPLE_3S7PM0CP4BD")); // Get the correct AAS (Bundle) in AASX 

//Push AAS from AASX Bundle to Server
aasClient.CreateAssetAdministrationShell(aas);

//Update AAS with Submodel and push to Server
Submodel maintenance_submodel = new ExampleDynamicSubmodel(); // Init Submodel (SM_ID_SHORT = "maintenance")
aas.Submodels.Add(maintenance_submodel); // Add the new Submodel to the AAS
aasClient.UpdateAssetAdministrationShell(aas.Identification.Id, aas);
