using Newtonsoft.Json.Linq;

namespace Adfc.Msbuild
{
    public class JsonFile
    {
        public JsonFile(string identity, JObject json)
        {
            Identity = identity;
            Json = json;
        }

        public string Identity { get; set; }

        public JObject Json { get; set; }
    }
}
