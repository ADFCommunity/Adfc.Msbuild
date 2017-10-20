namespace Adfc.Msbuild.Tests
{
    public class JsonSample
    {
        public const string Dataset = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Table.json'}";

        public const string LinkedService = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.LinkedService.json'}";

        public const string Pipeline = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Pipeline.json'}";

        public const string Config = "{'$schema':'http://datafactories.schema.management.azure.com/vsschemas/V1/Microsoft.DataFactory.Config.json'}";
    }
}
