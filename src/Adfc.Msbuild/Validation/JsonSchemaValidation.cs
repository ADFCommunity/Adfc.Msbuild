using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System.Collections.Generic;

namespace Adfc.Msbuild.Validation
{
    public class JsonSchemaValidation : IValidation
    {
        private HttpClient _httpClient;

        public JsonSchemaValidation(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyCollection<BuildError>> ValidateAsync(IList<JsonFile> jsonFiles)
        {
            var errors = new List<BuildError>();
            foreach (var jsonFile in jsonFiles)
            {
                var result = await ValidateAsync(jsonFile);
                if (result != null)
                {
                    errors.Add(result);
                }
            }
            return errors;
        }

        public async Task<BuildError> ValidateAsync(JsonFile jsonFile)
        {
            var result = await ValidateExplicitSchemaAsync(jsonFile);
            if (result == null)
            {
                result = await ValidateImplicitSchemaAsync(jsonFile);
            }
            return result;
        }

        private async Task<BuildError> ValidateExplicitSchemaAsync(JsonFile jsonFile)
        {
            var schemaValue = GetSchemaValue(jsonFile.Json);
            if (schemaValue == null)
            {
                return null;
            }

            return await ValidateSchemaAsync(jsonFile, schemaValue, ErrorCodes.Adfc0005.Code);
        }

        private Task<string> DownloadSchema(string schemaValue)
        {
            return _httpClient.GetStringAsync(schemaValue);
        }

        private string GetSchemaValue(JObject json)
        {
            var schema = json["$schema"];
            if (schema != null)
            {
                return (string)((JValue)schema).Value;
            }
            return null;
        }

        private async Task<BuildError> ValidateImplicitSchemaAsync(JsonFile jsonFile)
        {
            // check for $schema property, and skip implicit schema validation if the value is
            // the default schema for the artefact type.
            var schemaUrl = JsonSchemaUri.GetUri(jsonFile.Category);
            if (string.Equals(schemaUrl, GetSchemaValue(jsonFile.Json), StringComparison.Ordinal))
            {
                return null;
            }

            return await ValidateSchemaAsync(jsonFile, schemaUrl, ErrorCodes.Adfc0006.Code);
        }

        private async Task<BuildError> ValidateSchemaAsync(JsonFile jsonFile, string schemaValue, string errorCode)
        {
            try
            {
                var schemaJson = await DownloadSchema(schemaValue);
                var schema = await JsonSchema4.FromJsonAsync(schemaJson);

                var validationErrors = schema.Validate(jsonFile.Json);

                if (validationErrors.Count == 0)
                {
                    return null;
                }

                var message = new StringBuilder();
                message.AppendLine("Failed JSON schema validation");
                foreach (var ve in validationErrors)
                {
                    message.AppendLine(ve.ToString());
                }

                var error = new BuildError()
                {
                    Code = errorCode,
                    FileName = jsonFile.Identity,
                    Message = message.ToString()
                };

                return error;
            }
            catch (Exception exception)
            {
                var error = new BuildError()
                {
                    Code = ErrorCodes.Adfc0007.Code,
                    Message = "Error trying to validate JSON: " + exception.Message
                };
                return error;
            }
        }
    }
}
