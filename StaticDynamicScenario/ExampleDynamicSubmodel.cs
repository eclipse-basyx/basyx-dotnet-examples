using BaSyx.Models.AdminShell;

namespace StaticDynamicScenario
{
    public class ExampleDynamicSubmodel : Submodel
    {

        private static readonly string SM_ID_SHORT = "maintenance";
        private static readonly string SM_ID = "maintenanceInformationSubmodel";
        private static readonly string PROPERTY_ID_SHORT = "interval";
        private static readonly string PROPERTY_VALUE = "2 months";
        private static readonly Identifier IDENTIFIER = new Identifier(SM_ID, KeyType.Custom);

        public ExampleDynamicSubmodel() : base(SM_ID_SHORT, IDENTIFIER)
        {
            Property interval_prop = new Property(PROPERTY_ID_SHORT);
            interval_prop.Value = PROPERTY_VALUE;

            this.SubmodelElements.Add(interval_prop);
        }
    }
}
