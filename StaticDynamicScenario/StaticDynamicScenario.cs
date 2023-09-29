using BaSyx.API.ServiceProvider;
using BaSyx.Common.UI.Swagger;
using BaSyx.Common.UI;
using BaSyx.Utils.Settings;
using NLog;
using BaSyx.Servers.AdminShell.Http;
using System.Collections.Generic;
using BaSyx.Components.Common;
using NLog.Web;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Connectivity;
using BaSyx.Registry.Client.Http;
using BaSyx.Registry.Server.Http;
using BaSyx.Registry.ReferenceImpl.FileBased;
using BaSyx.Models.Export;
using System;
using System.IO.Packaging;
using System.Linq;
using BaSyx.Discovery.mDNS;
using NLog.Targets;
using BaSyx.Clients.AdminShell.Http;

namespace StaticDynamicScenario
{
    public class StaticDynamicScenario
    {
        public static RegistryHttpClient registryClient;

        private static List<ServerApplication> running_ServerApplications = new List<ServerApplication>();
        private static ILogger logger = new NullLogger(new LogFactory());

        public StaticDynamicScenario()
        {
            // Startup the registry
            RegistryHttpServer regServer = startupRegistryServer();
            registryClient = new RegistryHttpClient();

            //Start the AAS Server (empty)
            AssetAdministrationShellRepositoryHttpServer aasServer = startupAASServer("http://localhost:8081", "https://localhost:8071");

            //Init AAS Client
            AssetAdministrationShellRepositoryHttpClient aasClient = new AssetAdministrationShellRepositoryHttpClient(new Uri("http://localhost:8081"));

            // Load .aasx file
            AASX aasx = new AASX("aasx/01_Festo.aasx"); //AASX Package Manager
            AssetAdministrationShellEnvironment_V2_0 aasx_enviroment = aasx.GetEnvironment_V2_0(); //Load AASX with correct AASX Enviroment
            List<AssetAdministrationShell> bundles = aasx_enviroment.AssetAdministrationShells.ConvertAll(x => (AssetAdministrationShell)x); //Load list of AAS (Bundles) from .aasx file
            AssetAdministrationShell aas = bundles.Find(x => x.IdShort.Equals("Festo_3S7PM0CP4BD")); // Get the correct AAS (Bundle) in AASX 

            //Push AAS from AASX Bundle to Server
            aasClient.CreateAssetAdministrationShell(aas);

            //Update AAS with Submodel and push to Server
            Submodel maintenance_submodel = new ExampleDynamicSubmodel(); // Init Submodel (SM_ID_SHORT = "maintenance")
            aas.Submodels.Add(maintenance_submodel); // Add the new Submodel to the AAS
            aasClient.UpdateAssetAdministrationShell(aas.Identification.Id, aas);

        }

        private static RegistryHttpServer startupRegistryServer()
        {

            //Servlet Settings 
            ServerSettings registryServerSettings = ServerSettings.CreateSettings();
            registryServerSettings.ServerConfig.Hosting.Urls.Add("http://localhost:4999");
            registryServerSettings.ServerConfig.Hosting.Urls.Add("https://localhost:5999");

            RegistryHttpServer regServer = new RegistryHttpServer(registryServerSettings);
            FileBasedRegistry fileBasedRegistry = new FileBasedRegistry();
            regServer.SetRegistryProvider(fileBasedRegistry);
            regServer.AddBaSyxUI(PageNames.AssetAdministrationShellRegistryServer);
            regServer.AddSwagger(Interface.AssetAdministrationShellRegistry);
            _ = regServer.RunAsync();
            return regServer;
        }

        private AssetAdministrationShellRepositoryHttpServer startupAASServer(string HTTP, string HTTPS)
        {
            ServerSettings aasRepositorySettings = ServerSettings.CreateSettings();
            aasRepositorySettings.ServerConfig.Hosting.ContentPath = "Content";
            aasRepositorySettings.ServerConfig.Hosting.Urls.Add(HTTP);
            aasRepositorySettings.ServerConfig.Hosting.Urls.Add(HTTPS);

            AssetAdministrationShellRepositoryHttpServer multiServer = new AssetAdministrationShellRepositoryHttpServer(aasRepositorySettings);
            multiServer.WebHostBuilder.UseNLog();
            AssetAdministrationShellRepositoryServiceProvider repositoryService = new AssetAdministrationShellRepositoryServiceProvider();

            repositoryService.UseAutoEndpointRegistration(multiServer.Settings.ServerConfig);

            multiServer.SetServiceProvider(repositoryService);

            multiServer.AddBaSyxUI(PageNames.AssetAdministrationShellRepositoryServer);
            multiServer.AddSwagger(Interface.AssetAdministrationShellRepository);
            _ = multiServer.RunAsync();
            return multiServer;
        }

    }
}
