using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Adfc.Msbuild
{
    public class JsonFileLoader
    {
        public IList<BuildError> Errors
        {
            get
            {
                if (_errors == null)
                {
                    throw new InvalidOperationException(nameof(LoadAsync) + " must be called once");
                }
                return _errors;
            }
        }

        public JsonFile JsonFile
        {
            get
            {
                if (_errors == null)
                {
                    throw new InvalidOperationException(nameof(LoadAsync) + " must be called once");
                }
                return _jsonFile;
            }
        }

        private IList<BuildError> _errors;
        private JsonFile _jsonFile;

        public async Task LoadAsync(string identity, TextReader stream)
        {
            if (_errors != null)
            {
                throw new InvalidOperationException("Can only load one file per instance.");
            }

            _errors = new List<BuildError>();

            try
            {
                var text = await stream.ReadToEndAsync();
                var json = Parse(text, identity);
                var name = GetName(json, identity);
                var category = GetCategory(json, identity);

                _jsonFile = new JsonFile(name, identity, json, category);
            }
            catch(BuildErrorException exception)
            {
                _errors.Add(exception.BuildError);
            }
        }

        private static JObject Parse(string text, string identity)
        {
            try
            {
                return JObject.Parse(text);
            }
            catch (JsonReaderException exception)
            {
                var error = new BuildError
                {
                    Code = ErrorCodes.Adfc0004.Code,
                    FileName = identity,
                    Message = exception.Message,
                    LineNumber = exception.LineNumber,
                    LinePosition = exception.LinePosition
                };
                throw new BuildErrorException(error);
            }
        }

        private static string GetName(JObject json, string identity)
        {
            return GetName(json) ?? GetName(identity);
        }

        private static string GetName(JObject json)
        {
            var name = json["name"] as JValue;
            return name?.Value.ToString();
        }

        private static string GetName(string identity)
        {
            int filenameStart = identity.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            int extensionStart = identity.IndexOf(".", filenameStart);
            int length = extensionStart >= 0 ? extensionStart - filenameStart : identity.Length - 1 - filenameStart;
            return identity.Substring(filenameStart, length);
        }

        private static ArtefactCategory GetCategory(JObject json, string identity)
        {
            var schema = json["$schema"] as JValue;
            if (schema == null)
            {
                var error = new BuildError
                {
                    Code = ErrorCodes.Adfc0003.Code,
                    FileName = identity,
                    Message = "Unable to determine Data Factory artefact type. File does not contain $schema property."
                };
                throw new BuildErrorException(error);
            }

            var value = (string)schema.Value;
            var filename = Path.GetFileNameWithoutExtension(value).ToLowerInvariant();

            switch (filename)
            {
                case "microsoft.datafactory.table":
                    return ArtefactCategory.Dataset;

                case "microsoft.datafactory.linkedservice":
                    return ArtefactCategory.LinkedService;

                case "microsoft.datafactory.pipeline":
                    return ArtefactCategory.Pipeline;

                case "microsoft.datafactory.config":
                    return ArtefactCategory.Config;

                default:
                    throw new Exception("Unsupported schema " + value);
            }

        }
    }
}
