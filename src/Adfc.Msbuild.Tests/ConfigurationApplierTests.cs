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
    }
}
