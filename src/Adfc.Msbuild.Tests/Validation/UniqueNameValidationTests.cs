using Adfc.Msbuild.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests.Validation
{
    [TestClass]
    public class UniqueNameValidationTests
    {
        [TestMethod]
        public async Task ShouldValidateEmptyList()
        {
            var artefacts = new List<JsonFile>();

            var target = new UniqueNameValidation();

            var result = await target.ValidateAsync(artefacts);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ShouldValidateSingleArtefact()
        {
            var artefacts = new List<JsonFile>()
            {
                await Util.LoadJsonAsync("file.json", TestResource.LinkedService)
            };

            var target = new UniqueNameValidation();

            var result = await target.ValidateAsync(artefacts);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ShouldValidateArtefactsWithUniqueNames()
        {
            var artefacts = new List<JsonFile>()
            {
                await Util.LoadJsonAsync("linkedservice.json", TestResource.LinkedService),
                await Util.LoadJsonAsync("dataset.json", TestResource.Dataset),
                await Util.LoadJsonAsync("pipeline.json", TestResource.Pipeline)
            };

            var target = new UniqueNameValidation();

            var result = await target.ValidateAsync(artefacts);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ShouldReportErrorWhenTwoArtefactsHaveSameName()
        {
            var artefacts = new List<JsonFile>()
            {
                await Util.LoadJsonAsync("file1.json", TestResource.Dataset),
                await Util.LoadJsonAsync("file2.json", TestResource.Dataset)
            };

            var target = new UniqueNameValidation();

            var result = await target.ValidateAsync(artefacts);

            Assert.AreEqual(2, result.Count);

            var error = result.FirstOrDefault(r => r.FileName == "file1.json");
            Assert.IsNotNull(error);
            Assert.AreEqual(ErrorCodes.Adfc0008.Code, error.Code);

            error = result.FirstOrDefault(r => r.FileName == "file2.json");
            Assert.IsNotNull(error);
            Assert.AreEqual(ErrorCodes.Adfc0008.Code, error.Code);
        }

        [TestMethod]
        public async Task ShouldAllowArtefactsOfDifferentCategoryToUseSameName()
        {
            var artefacts = new List<JsonFile>()
            {
                new JsonFile("test", "file1.json", new JObject(new JProperty("name", "test")), ArtefactCategory.Dataset),
                new JsonFile("test", "file2.json", new JObject(new JProperty("name", "test")), ArtefactCategory.LinkedService)
            };

            var target = new UniqueNameValidation();

            var result = await target.ValidateAsync(artefacts);

            Assert.AreEqual(0, result.Count);
        }
    }
}
