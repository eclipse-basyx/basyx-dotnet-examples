/*******************************************************************************
* Copyright (c) 2020, 2021 Robert Bosch GmbH
* Author: Constantin Ziesche (constantin.ziesche@bosch.com)
*
* This program and the accompanying materials are made available under the
* terms of the Eclipse Distribution License 1.0 which is available at
* https://www.eclipse.org/org/documents/edl-v10.html
*
* 
*******************************************************************************/
using BaSyx.AAS.Server.Http;
using BaSyx.API.Components;
using BaSyx.Common.UI;
using BaSyx.Common.UI.Swagger;
using BaSyx.Discovery.mDNS;
using BaSyx.Utils.Settings.Sections;
using BaSyx.Utils.Settings.Types;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Rewrite;

namespace HelloAssetAdministrationShell
{
    class Program
    {
        //Create logger for the application
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
       
        static void Main(string[] args)
        {
            logger.Info("Starting HelloAssetAdministrationShell's HTTP server...");

            //Loading server configurations settings from ServerSettings.xml;
            ServerSettings serverSettings = ServerSettings.LoadSettingsFromFile("ServerSettings.xml");

            //Initialize generic HTTP-REST interface passing previously loaded server configuration
            AssetAdministrationShellHttpServer server = new AssetAdministrationShellHttpServer(serverSettings);

            server.UsePathBase(serverSettings.ServerConfig.PathBase);

            //Configure the entire application to use your own logger library (here: Nlog)
            server.WebHostBuilder.UseNLog();

            //Instantiate Asset Administration Shell Service
            HelloAssetAdministrationShellService shellService = new HelloAssetAdministrationShellService();
            ServerConfiguration serverConfiguration;
            string websiteHostName = Environment.ExpandEnvironmentVariables("%WEBSITE_HOSTNAME%");
            if (string.IsNullOrEmpty(websiteHostName) || websiteHostName == "%WEBSITE_HOSTNAME%")
            {
                serverConfiguration = serverSettings.ServerConfig;
            }
            else
            {
                string websiteUrl = string.Format("https://{0}", websiteHostName);
                serverConfiguration = new ServerConfiguration()
                {
                    Hosting = new HostingConfiguration() { Urls = new List<string>() { websiteUrl } }
                };
            }
            shellService.UseAutoEndpointRegistration(serverConfiguration);

            //Assign Asset Administration Shell Service to the generic HTTP-REST interface
            server.SetServiceProvider(shellService);

            //Add Swagger documentation and UI
            server.AddSwagger(Interface.AssetAdministrationShell);

            //Add BaSyx Web UI
            server.AddBaSyxUI(PageNames.AssetAdministrationShellServer);

            //Action that gets executued when server is fully started
            server.ApplicationStarted = () =>
            {
                //Use mDNS discovery mechanism in the network. It is used to register at the Registry automatically.
                shellService.StartDiscovery();
            };

            //Action that gets executed when server is shutting down
            server.ApplicationStopping = () =>
            {
                //Stop mDNS discovery thread
                shellService.StopDiscovery();
            };

            //Run HTTP server
            server.Run();           
        }
    }
}
