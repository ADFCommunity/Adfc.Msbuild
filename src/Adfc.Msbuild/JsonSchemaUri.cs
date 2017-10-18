using System;

namespace Adfc.Msbuild
{
    public static class JsonSchemaUri
    {
        public const string Config = "http://datafactories.schema.management.azure.com/vsschemas/V1/Microsoft.DataFactory.Config.json";

        public const string Dataset = "http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Table.json";

        public const string LinkedService = "http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.LinkedService.json";

        public const string Pipeline = "http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Pipeline.json";

        public static string GetUri(ArtefactCategory category)
        {
            switch (category)
            {
                case ArtefactCategory.Config:
                    return JsonSchemaUri.Config;

                case ArtefactCategory.Dataset:
                    return JsonSchemaUri.Dataset;

                case ArtefactCategory.LinkedService:
                    return JsonSchemaUri.LinkedService;

                case ArtefactCategory.Pipeline:
                    return JsonSchemaUri.Pipeline;

                default:
                    throw new ArgumentException();
            }
        }
    }
}
