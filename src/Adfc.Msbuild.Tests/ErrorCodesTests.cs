using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class ErrorCodesTests
    {
        [TestMethod]
        public void EnsureAllCodesAreInDictionary()
        {
            var errorCodesType = typeof(ErrorCodes);
            var publicStaticFields = errorCodesType.GetFields(BindingFlags.Static | BindingFlags.Public);
            var errorCodes = publicStaticFields.Where(e=>e.FieldType == errorCodesType);
            var dict = ErrorCodes.All.ToDictionary(d => d.Key, d => d.Value);

            foreach (var errorCode in errorCodes)
            {
                var value = (ErrorCodes)errorCode.GetValue(null);
                Assert.IsTrue(dict.ContainsKey(value.Code), "All error codes lookup doesn't contain " + value.Code);
                Assert.AreSame(value, dict[value.Code]);
                Assert.AreEqual(errorCode.Name.ToUpperInvariant(), value.Code);
                dict.Remove(value.Code);
            }

            Assert.AreEqual(0, dict.Count, "Lookup contains unexpected code(s): " + string.Join(", ", dict.Keys));
        }
    }
}
