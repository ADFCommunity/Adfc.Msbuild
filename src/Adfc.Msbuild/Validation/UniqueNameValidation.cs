using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Validation
{
    public class UniqueNameValidation : IValidation
    {
        public Task<IReadOnlyCollection<BuildError>> ValidateAsync(IList<JsonFile> artefacts)
        {
            var errors = new List<BuildError>();

            foreach (var artefactsPerCategory in artefacts.GroupBy(a => a.Category))
            {
                foreach (var artefactsByName in artefactsPerCategory.GroupBy(a => a.Name))
                {
                    if (artefactsByName.Count() > 1)
                    {
                        foreach (var artefact in artefactsByName)
                        {
                            var nameToken = artefact.Json["name"];
                            var lineInfo = nameToken as IJsonLineInfo;
                            var error = new BuildError
                            {
                                Code = ErrorCodes.Adfc0008.Code,
                                Message = $"More than one {artefact.Category} named {artefact.Name}",
                                FileName = artefact.Identity,
                                LineNumber = lineInfo?.LineNumber,
                                LinePosition = lineInfo?.LinePosition
                            };
                            errors.Add(error);
                        }
                    }
                }
            }

            return Task.FromResult<IReadOnlyCollection<BuildError>>(errors);
        }
    }
}
