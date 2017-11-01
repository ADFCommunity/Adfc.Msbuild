using System.Collections.Generic;
using System.Net.Http;

namespace Adfc.Msbuild.Validation
{
    public class ValidationsFactory
    {
        private HttpClient _httpClient;

        public ValidationsFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IReadOnlyList<IValidation> GetValidators()
        {
            List<IValidation> validators = new List<IValidation>()
            {
                new JsonSchemaValidation(_httpClient),
                new UniqueNameValidation()
            };
            return validators;
        }
    }
}
