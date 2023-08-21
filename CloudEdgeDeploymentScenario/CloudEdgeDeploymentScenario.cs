using BaSyx.API.ServiceProvider;
using BaSyx.Common.UI.Swagger;
using BaSyx.Common.UI;
using BaSyx.Utils.Settings;
using BaSyx.Servers.AdminShell.Http;
using NLog.Web;
using BaSyx.Models.AdminShell;
using BaSyx.Registry.Client.Http;
using BaSyx.Registry.Server.Http;
using System;
using BaSyx.Clients.AdminShell.Http;
using BaSyx.Registry.ReferenceImpl.FileBased;
using BaSyx.Models.Connectivity;
using System.Collections.Generic;

namespace CloudEdgeDeploymentScenario
{
    public class CloudEdgeDeploymentScenario
    {

        public static RegistryHttpClient registryClient;

        public CloudEdgeDeploymentScenario()
        {
            RegistryHttpServer regServer = startupRegistryServer();

            registryClient = new RegistryHttpClient();
            ComponentBuilder._registryClient = registryClient;

            //Create AAS and docuSubmodel 
            AssetAdministrationShell aas = ComponentBuilder.getAAS();
            Submodel docuSubmodel = ComponentBuilder.getDocuSubmodel();
            Submodel edgeSubmodel = ComponentBuilder.getEdgeSubmodel();

            //Start the CloudServer
            AssetAdministrationShellRepositoryHttpServer cloudServer = startupCloudServer();

            //Start the EdgeServer
            SubmodelHttpServer edgeServer = startupEdgeServer(aas.Identification.Id, edgeSubmodel);

            //Init CloudClient
            AssetAdministrationShellRepositoryHttpClient cloudClient = new AssetAdministrationShellRepositoryHttpClient(new Uri("http://localhost:8081"));
           
            //Push AAS to Cloud Server
            cloudClient.CreateAssetAdministrationShell(aas);

            //Update AAS with docuSubmodel to Cloud Server
            aas.Submodels.Add(docuSubmodel);
            cloudClient.UpdateAssetAdministrationShell(aas.Identification.Id, aas);

            //Add EdgeServer Submodel to CloudServer
            SubmodelHttpClient edgeClient = new SubmodelHttpClient(new Uri("http://localhost:8082"));

            //// URLS :
            ////http://localhost:4999/ui (Registry UI)
            ////http://localhost:8081/ui (Cloud Server UI)
            ////http://localhost:8082/ui (Edge Submodel UI)
            ////
            ////http://localhost:4999/registry/shell-descriptors
            ////http://localhost:8081/shells/YmFzeXguZXhhbXBsZXMub3ZlbkFBUw==
            ////http://localhost:8082/submodel/
            ////http://localhost:8081/shells/YmFzeXguZXhhbXBsZXMub3ZlbkFBUw==/aas/submodels/YmFzeXguZXhhbXBsZXMub3Zlbi5vdmVuX2RvY3VtZW50YXRpb25fc20=/submodel

        }

        private static RegistryHttpServer startupRegistryServer()
        {

            //Servlet Settings 
            ServerSettings registryServerSettings = ServerSettings.CreateSettings();
            registryServerSettings.ServerConfig.Hosting.Urls.Add("http://localhost:4999");
            registryServerSettings.ServerConfig.Hosting.Urls.Add("https://localhost:5999");

            RegistryHttpServer regServer =  new RegistryHttpServer(registryServerSettings);
            FileBasedRegistry fileBasedRegistry = new FileBasedRegistry();
            regServer.SetRegistryProvider(fileBasedRegistry);
            regServer.AddBaSyxUI(PageNames.AssetAdministrationShellRegistryServer);
            regServer.AddSwagger(Interface.AssetAdministrationShellRegistry);
            _ = regServer.RunAsync();
            return regServer;
        }

        private SubmodelHttpServer startupEdgeServer(string aas_id, Submodel edgeSubmodel)
        {
            //Servlet Settings 
            ServerSettings edgeSubmodelServerSettings = ServerSettings.CreateSettings();
            edgeSubmodelServerSettings.ServerConfig.Hosting.ContentPath = "Content";
            edgeSubmodelServerSettings.ServerConfig.Hosting.Urls.Add("http://localhost:8082");
            edgeSubmodelServerSettings.ServerConfig.Hosting.Urls.Add("https://localhost:8072");

            // Create a new SubmodelServlet containing the edgeSubmodel
            SubmodelHttpServer smServlet = new SubmodelHttpServer(edgeSubmodelServerSettings);
            smServlet.SetServiceProvider(edgeSubmodel.CreateServiceProvider());
            smServlet.AddBaSyxUI(PageNames.SubmodelServer);
            smServlet.AddSwagger(Interface.Submodel);
            _ = smServlet.RunAsync();

            registryClient.CreateSubmodelRegistration(aas_id, ComponentBuilder.getEdgeSubmodelDescriptor());


            return smServlet;
        }

        private AssetAdministrationShellRepositoryHttpServer startupCloudServer()
        {
            ServerSettings aasRepositorySettings = ServerSettings.CreateSettings();
            aasRepositorySettings.ServerConfig.Hosting.ContentPath = "Content";
            aasRepositorySettings.ServerConfig.Hosting.Urls.Add("http://localhost:8081");
            aasRepositorySettings.ServerConfig.Hosting.Urls.Add("https://localhost:8071");

            AssetAdministrationShellRepositoryHttpServer multiServer = new AssetAdministrationShellRepositoryHttpServer(aasRepositorySettings);
            multiServer.WebHostBuilder.UseNLog();
            AssetAdministrationShellRepositoryServiceProvider repositoryService = new AssetAdministrationShellRepositoryServiceProvider();

            repositoryService.UseAutoEndpointRegistration(multiServer.Settings.ServerConfig);

            multiServer.SetServiceProvider(repositoryService);

            AssetAdministrationShellDescriptor aasDescriptor = ComponentBuilder.getAASDescriptor();

            registryClient.CreateAssetAdministrationShellRegistration(aasDescriptor);
            registryClient.CreateSubmodelRegistration(aasDescriptor.Identification.Id, ComponentBuilder.getDocuSubmodelDescriptor());

            multiServer.AddBaSyxUI(PageNames.AssetAdministrationShellRepositoryServer);
            multiServer.AddSwagger(Interface.AssetAdministrationShellRepository);
            _ = multiServer.RunAsync();
            return multiServer;
        }
    }
}
