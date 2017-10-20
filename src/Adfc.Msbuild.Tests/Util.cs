using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adfc.Msbuild.Tests
{
    internal static class Util
    {
        public static async Task<JsonFile> LoadJsonAsync(string identity, string json)
        {
            var loader = new JsonFileLoader();
            using (var reader = new StringReader(json)) {
                await loader.LoadAsync(identity, reader);
            }

            Assert.IsNotNull(loader.JsonFile);

            return loader.JsonFile;
        }
    }
}
