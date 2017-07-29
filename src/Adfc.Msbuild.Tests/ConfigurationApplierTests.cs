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
            var config = new JsonFile("config.json", JObject.Parse("{'name':'$.name','value':'new name'}"));
            var document = new JsonFile("file.json", JObject.Parse("{'name':'old name'}"));

            var target = new ConfigurationApplier();
            target.ApplyTransform(document, config.Json, config);

            const string expected = "{\"name\":\"new name\"}";
            Assert.AreEqual(expected, document.Json.ToString(Formatting.None));
        }

        [TestMethod]
        public void ShouldRecordWarningIfJPathDoesNotExist()
        {
            const string jpath = "$.notexists";

            var config = new JsonFile(
                "config.json",
                JObject.Parse($@"{{
    'name':'{jpath}',
    'value':'not used'
}}"));
            var document = new JsonFile("content.json", JObject.Parse("{}"));

            var target = new ConfigurationApplier();
            target.ApplyTransform(document, config.Json, config);

            Assert.AreEqual(1, target.Errors.Count(), "Should contain 1 transformation error");
            var error = target.Errors[0];
            Assert.IsTrue(error.Message.Contains(jpath), "Error does not contain jpath");
            Assert.IsTrue(error.Message.Contains(document.Identity), "Error message does not contain document's filename");
            Assert.AreEqual(config.Identity, error.FileName, "error.FileName is incorrect");
            Assert.AreEqual(ErrorCodes.Adfc0001, error.Code, "Error code is incorrect");
            Assert.AreEqual(2, error.LineNumber, "Error line number is incorrect");
            Assert.AreEqual(24, error.LinePosition, "Error line position is incorrect");
        }

        [TestMethod]
        public void ShouldApplyMultipleConfigurationChanges()
        {
            var config = new JsonFile("config.json",
                JObject.Parse("{'document.json': [{'name':'$.p1','value':'new v1'}," +
                "{'name':'$.p2','value':'new v2'}]}"));
            var document = new JsonFile("document.json",
                JObject.Parse("{'p1':'v1','p2':'v2'}"));
            var confgTransforms = (JArray)config.Json[document.Identity];

            var target = new ConfigurationApplier();
            target.ApplyTransforms(document, confgTransforms, config);

            const string expected = @"{""p1"":""new v1"",""p2"":""new v2""}";
            Assert.AreEqual(expected, document.Json.ToString(Formatting.None));
        }

        [TestMethod]
        public void ShouldApplyConfigurationChangesToMultipleFiles()
        {
            var config = new JsonFile("config.json",
                JObject.Parse(@"{
    'file1.json':[{'name':'$.name','value':'new file1'}],
    'file2.json':[{'name':'$.name','value':'new file2'}],
}"));
            var file1 = new JsonFile("file1.json",
                JObject.Parse("{'name':'file1'}"));
            var file2 = new JsonFile("file2.json",
                JObject.Parse("{'name':'file2'}"));
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
            var config = new JsonFile("config.json",
                JObject.Parse("{'$schema':'https://server/schema.json'}"));
            var documents = new List<JsonFile>();

            var target = new ConfigurationApplier();
            target.ApplyTransforms(documents, config);

            Assert.AreEqual(0, target.Errors.Count, "There should not be any errors");
        }
    }
}
