using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adfc.Msbuild
{
    public class ConfigurationApplier
    {
        public ConfigurationApplier()
        {
            Errors = new List<BuildError>();
        }

        public IList<BuildError> Errors { get; }

        public void ApplyTransform(JsonFile document, JObject config, JsonFile configFile)
        {
            var name = config["name"];
            var value = config["value"];

            var toReplace = document.Json.SelectToken(name.ToString());
            if (toReplace == null)
            {
                var lineInfo = name as IJsonLineInfo;

                var error = new BuildError
                {
                    Code = ErrorCodes.Adfc0001.Code,
                    Message = $"{name} not found in {document.Identity}",
                    FileName = configFile.Identity,
                    LineNumber = lineInfo?.LineNumber,
                    LinePosition = lineInfo?.LinePosition
                };

                Errors.Add(error);
                return;
            }

            var parent = (JProperty)toReplace.Parent;
            parent.Value = value;
        }

        public void ApplyTransforms(JsonFile document, JArray transforms, JsonFile config)
        {
            foreach (JObject transform in transforms)
            {
                ApplyTransform(document, transform, config);
            }
        }

        public void ApplyTransforms(IList<JsonFile> documents, JsonFile config)
        {
            foreach (JProperty file in config.Json.Children())
            {
                if (file.Name[0] == '$')
                {
                    continue;
                }

                var document = documents.SingleOrDefault(d => string.Equals(d.Identity, file.Name));
                if (document == null)
                {
                    var lineInfo = file as IJsonLineInfo;

                    var error = new BuildError
                    {
                        Code = ErrorCodes.Adfc0003.Code,
                        Message = $"{file.Name} not found",
                        FileName = config.Identity,
                        LineNumber = lineInfo?.LineNumber,
                        LinePosition = lineInfo?.LinePosition
                    };

                    Errors.Add(error);
                }
                else
                {
                    var transforms = file.Value as JArray;

                    ApplyTransforms(document, transforms, config);
                }
            }
        }
    }
}
