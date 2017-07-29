using Newtonsoft.Json.Linq;

namespace Adfc.Msbuild
{
    public class JsonFile
    {
        public JsonFile(string name, string identity, JObject json, ArtefactCategory category)
        {
            Name = name;
            Identity = identity;
            Json = json;
            Category = category;
        }

        public string Name { get; set; }

        public string Identity { get; set; }

        public JObject Json { get; set; }

        public ArtefactCategory Category { get; set; }
    }
}
