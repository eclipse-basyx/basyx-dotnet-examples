/*******************************************************************************
* Copyright (c) 2023 Fraunhofer IESE
* Author: Philippe Barbie (philippe.barbie@iese.fraunhofer.de)
*
* This program and the accompanying materials are made available under the
* terms of the MIT License which is available at
* https://github.com/eclipse-basyx/basyx-dotnet/blob/main/LICENSE
*
* SPDX-License-Identifier: MIT
*******************************************************************************/
using BaSyx.Servers.AdminShell.Http;
using BaSyx.API.ServiceProvider;
using BaSyx.Common.UI;
using BaSyx.Common.UI.Swagger;
using BaSyx.Discovery.mDNS;
using BaSyx.Utils.Settings;
using NLog;
using NLog.Web;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BaSyx.Components.Common;
using System;

namespace CloudEdgeDeploymentScenario
{
    class Program
    {
        static void Main(string[] args)
        {

            //Init new Scenario
            CloudEdgeDeploymentScenario _ = new CloudEdgeDeploymentScenario();

            Console.ReadLine();

        }
    }
}
