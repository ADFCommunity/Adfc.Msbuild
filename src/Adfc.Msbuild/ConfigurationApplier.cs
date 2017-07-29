﻿using System.Collections.Generic;
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
                    Code = ErrorCodes.Adfc0001,
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
    }
}
