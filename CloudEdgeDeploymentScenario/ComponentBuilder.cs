using BaSyx.Clients.AdminShell.Http;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Connectivity;
using BaSyx.Models.Extensions;
using BaSyx.Registry.Client.Http;
using BaSyx.Utils.ResultHandling;
using System.Collections.Generic;
using System.Threading;

namespace CloudEdgeDeploymentScenario
{
    public static class ComponentBuilder
    {

        public static RegistryHttpClient _registryClient;

        public static AssetAdministrationShell getAAS()
        {
            // Create the oven asset
            Asset ovenAsset = new Asset("OvenAsset", new Identifier("basyx.examples.OvenAsset", KeyType.Custom));

            // Create the AAS representing the oven
            AssetAdministrationShell ovenAAS = new AssetAdministrationShell("ovenAAS", new Identifier("basyx.examples.ovenAAS", KeyType.Custom));
            ovenAAS.Asset = ovenAsset;

            return ovenAAS;
        }

        public static AssetAdministrationShellDescriptor getAASDescriptor()
        {
            //Define Endpoints
            List<Endpoint> endpointList = new List<Endpoint>
            {
                new Endpoint(new ProtocolInformation("http://localhost:8081/shells/YmFzeXguZXhhbXBsZXMub3ZlbkFBUw=="), InterfaceName.SubmodelInterface)
            };

            //Init Descriptor with DocuSubmodel IDs
            AssetAdministrationShell aas = getAAS();
            AssetAdministrationShellDescriptor descriptor = new AssetAdministrationShellDescriptor(endpointList);
            descriptor.IdShort = aas.IdShort;
            descriptor.Identification = aas.Identification;

            return descriptor;
        }

        public static Submodel getDocuSubmodel()
        {
            Submodel docuSubmodel = new Submodel("oven_doc", new Identifier("basyx.examples.oven.oven_documentation_sm", KeyType.Custom));

            // Create the maximum temperature property and include it in the submodel
            Property maxTemp = new Property("max_temp");
            maxTemp.Value = 1000;
            docuSubmodel.SubmodelElements.Add(maxTemp);

            return docuSubmodel;
        }

        public static SubmodelDescriptor getDocuSubmodelDescriptor()
        {
            //Define Endpoints
            List<Endpoint> endpointList = new List<Endpoint>
            {
                new Endpoint(new ProtocolInformation("http://localhost:8081/shells/YmFzeXguZXhhbXBsZXMub3ZlbkFBUw==/aas/submodels/YmFzeXguZXhhbXBsZXMub3Zlbi5vdmVuX2RvY3VtZW50YXRpb25fc20/submodel"), InterfaceName.SubmodelInterface)
            };

            //Init Descriptor with DocuSubmodel IDs
            Submodel docuSubmodel = getDocuSubmodel();
            SubmodelDescriptor descriptor = new SubmodelDescriptor(endpointList);
            descriptor.IdShort = docuSubmodel.IdShort;
            descriptor.Identification = docuSubmodel.Identification;

            return descriptor;
        }

        public static Submodel getEdgeSubmodel()
        {
            // Create the edge submodel
            Submodel edgeSubmodel = new Submodel("temp", new Identifier("basyx.examples.oven.oven_temperature", KeyType.Custom));

            // The property in this Submodel contains the currently measured temperature ofthe oven
            // It is represented by a static value in this example
            Property property_currTemp = new Property("curr_temp");
            property_currTemp.Value = 31;
            edgeSubmodel.SubmodelElements.Add(property_currTemp);

            // The property in this Submodel contains the temperature the oven should have
            Property property_targetTemp = new Property("target_temp");
            property_targetTemp.Value = 250;
            edgeSubmodel.SubmodelElements.Add(property_targetTemp);

            edgeSubmodel.SubmodelElements.Add(getSetTargetTempOperation());
            edgeSubmodel.SubmodelElements.Add(getGetCurrTempOperation());

            return edgeSubmodel;
        }

        public static SubmodelDescriptor getEdgeSubmodelDescriptor()
        {
            //Define Endpoints
            List<Endpoint> endpointList = new List<Endpoint>
            {
                new Endpoint(new ProtocolInformation("http://localhost:8082/submodel/"), InterfaceName.SubmodelInterface)
            };

            //Init Descriptor with EdgeSubmodel IDs
            Submodel edgeSubmodel = getEdgeSubmodel();
            SubmodelDescriptor descriptor = new SubmodelDescriptor(endpointList);
            descriptor.IdShort = edgeSubmodel.IdShort;
            descriptor.Identification = edgeSubmodel.Identification;

            return descriptor;
        }

        private static Operation getSetTargetTempOperation()
        {
            Operation operation = new Operation("setTargetTemp");

            // Create a Property to be used as input variable
            // The ValueType is determined by the type of the given value
            // here 0, which results in ValueType Integer
            Property newTargetTemp = new Property("targetTemp");
            newTargetTemp.Value = 0;
            newTargetTemp.Kind = ModelingKind.Template;

            OperationVariable var = new OperationVariable();
            var.Value = newTargetTemp;

            operation.InputVariables.Add(var);

            // The supplier called when the Operation is invoked
            MethodCalledHandler consumer = (IOperation operation, IOperationVariableSet inputArguments, IOperationVariableSet inoutputArguments, IOperationVariableSet outputArguments, CancellationToken cancellationToken) 
                => //delegate method
            {
                SubmodelDescriptor submodelDescriptor = (SubmodelDescriptor)_registryClient.RetrieveSubmodelRegistration("basyx.examples.oven", "basyx.examples.oven.oven_temperature").Entity;

                SubmodelHttpClient submodelClient = new SubmodelHttpClient(submodelDescriptor);
                Submodel submodel = submodelClient.RetrieveSubmodel().Entity as Submodel;

                SubmodelElement elem = submodel.SubmodelElements.Retrieve("target_temp").Entity as SubmodelElement;
                elem.SetValue(inputArguments[0]);

                submodelClient.UpdateSubmodel(submodel);

                return new OperationResult(true);
            };

            return operation;
        }

        private static Operation getGetCurrTempOperation()
        {
            Operation operation = new Operation("getCurrTemp");

            // Create a Property to be used as output variable
            // The ValueType is determined by the type of the given value here 0, which results in ValueType Integer
            Property currentTemp = new Property("currTemp");
            currentTemp.Value = 0;
            currentTemp.Kind = ModelingKind.Template;

            OperationVariable var = new OperationVariable();
            var.Value = currentTemp;

            operation.OutputVariables.Add(var);

            // The supplier called when the Operation is invoked
            MethodCalledHandler supplier = (IOperation operation, IOperationVariableSet inputArguments, IOperationVariableSet inoutputArguments, IOperationVariableSet outputArguments, CancellationToken cancellationToken) 
                => //delegate method
            {
                SubmodelDescriptor submodelDescriptor = (SubmodelDescriptor)_registryClient.RetrieveSubmodelRegistration("basyx.examples.oven", "basyx.examples.oven.oven_temperature").Entity;

                SubmodelHttpClient submodelClient = new SubmodelHttpClient(submodelDescriptor);
                Submodel submodel = submodelClient.RetrieveSubmodel().Entity as Submodel;

                SubmodelElement elem = submodel.SubmodelElements.Retrieve("curr_temp").Entity as SubmodelElement;
                return (OperationResult)elem.GetValue();
            };

            return operation;
        }
    }
}
