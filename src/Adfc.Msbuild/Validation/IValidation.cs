using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Validation
{
    public interface IValidation
    {
        Task<IReadOnlyCollection<BuildError>> ValidateAsync(IList<JsonFile> artefacts);
    }
}
