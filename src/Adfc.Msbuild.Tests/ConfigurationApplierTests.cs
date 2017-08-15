using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class ConfigurationApplierTests
    {
        [TestMethod]
        public void ShouldTransformSingleField()
        {
            var config = new JsonFile("config",
                "config.json",
                JObject.Parse("{'name':'$.name','value':'new name'}"),
                ArtefactCategory.Config);
            var document = new JsonFile("file",
                "file.json",
                JObject.Parse("{'name':'old name'}"),
                ArtefactCategory.Dataset);

            var target = new ConfigurationApplier();
            target.ApplyTransform(document, config.Json, config);

            const string expected = "{\"name\":\"new name\"}";
            Assert.AreEqual(expected, document.Json.ToString(Formatting.None));
        }

        [TestMethod]
        public void ShouldReportErrorIfJPathDoesNotExist()
        {
            const string jpath = "$.notexists";

            var config = new JsonFile("config",
                "config.json",
                JObject.Parse($@"{{
    'name':'{jpath}',
    'value':'not used'
}}"),
                ArtefactCategory.Config);
            var document = new JsonFile("content",
                "content.json",
                JObject.Parse("{}"),
                ArtefactCategory.Dataset);

            var target = new ConfigurationApplier();
            target.ApplyTransform(document, config.Json, config);

            Assert.AreEqual(1, target.Errors.Count(), "Should contain 1 transformation error");
            var error = target.Errors[0];
            Assert.IsTrue(error.Message.Contains(jpath), "Error does not contain jpath");
            Assert.IsTrue(error.Message.Contains(document.Identity), "Error message does not contain document's filename");
            Assert.AreEqual(config.Identity, error.FileName, "error.FileName is incorrect");
            Assert.AreEqual(ErrorCodes.Adfc0001.Code, error.Code, "Error code is incorrect");
            Assert.AreEqual(2, error.LineNumber, "Error line number is incorrect");
            Assert.AreEqual(24, error.LinePosition, "Error line position is incorrect");
        }

        [TestMethod]
        public void ShouldApplyMultipleConfigurationChanges()
        {
            var config = new JsonFile("config",
                "config.json",
                JObject.Parse("{'document.json': [{'name':'$.p1','value':'new v1'}," +
                "{'name':'$.p2','value':'new v2'}]}"),
                ArtefactCategory.Config);
            var document = new JsonFile("document",
                "document.json",
                JObject.Parse("{'p1':'v1','p2':'v2'}"),
                ArtefactCategory.Dataset);
            var confgTransforms = (JArray)config.Json[document.Identity];

            var target = new ConfigurationApplier();
            target.ApplyTransforms(document, confgTransforms, config);

            const string expected = @"{""p1"":""new v1"",""p2"":""new v2""}";
            Assert.AreEqual(expected, document.Json.ToString(Formatting.None));
        }

        [TestMethod]
        public void ShouldApplyConfigurationChangesToMultipleFiles()
        {
            var config = new JsonFile("config",
                "config.json",
                JObject.Parse(@"{
    'file1.json':[{'name':'$.name','value':'new file1'}],
    'file2.json':[{'name':'$.name','value':'new file2'}],
}"),
                ArtefactCategory.Config);
            var file1 = new JsonFile("file1",
                "file1.json",
                JObject.Parse("{'name':'file1'}"),
                ArtefactCategory.Dataset);
            var file2 = new JsonFile("file2",
                "file2.json",
                JObject.Parse("{'name':'file2'}"),
                ArtefactCategory.Dataset);
            var documents = new List<JsonFile>()
            {
                file1, file2
            };

            var target = new ConfigurationApplier();
            target.ApplyTransforms(documents, config);

            const string file1Expected = "{\"name\":\"new file1\"}";
            Assert.AreEqual(file1Expected, file1.Json.ToString(Formatting.None));
            const string file2Expected = "{\"name\":\"new file2\"}";
            Assert.AreEqual(file2Expected, file2.Json.ToString(Formatting.None));
        }

        [TestMethod]
        public void ShouldIgnoreJsonMetadata()
        {
            var config = new JsonFile("config",
                "config.json",
                JObject.Parse("{'$schema':'https://server/schema.json'}"),
                ArtefactCategory.Config);
            var documents = new List<JsonFile>();

            var target = new ConfigurationApplier();
            target.ApplyTransforms(documents, config);

            Assert.AreEqual(0, target.Errors.Count, "There should not be any errors");
        }

        [TestMethod]
        public void ShouldReportErrorWhenFileCannotBeFound()
        {
            var config = new JsonFile("config",
                "config.json",
                JObject.Parse("{'file.json':[{'name':'$.name','value':''}]}"),
                ArtefactCategory.Config);
            var documents = new List<JsonFile>();

            var target = new ConfigurationApplier();
            target.ApplyTransforms(documents, config);

            Assert.AreEqual(1, target.Errors.Count, "Unexpected number of errors");
            var error = target.Errors[0];
            Assert.IsTrue(error.Message.Contains("file.json"), "Error message does not contain document's filename");
            Assert.AreEqual(config.Identity, error.FileName, "error.FileName is incorrect");
            Assert.AreEqual(ErrorCodes.Adfc0003.Code, error.Code, "Error code is incorrect");
            Assert.AreEqual(1, error.LineNumber, "Error line number is incorrect");
            Assert.AreEqual(13, error.LinePosition, "Error line position is incorrect");
        }
    }
}
