using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;

namespace Adfc.Msbuild
{
    public class AdfcBuild : Microsoft.Build.Utilities.Task
    {
        [Required]
        public ITaskItem[] JsonFiles { get; set; }

        [Required]
        public string OutputPath { get; set; }

        public override bool Execute()
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable("AdfcBuildDebug") != null)
            {
                Console.WriteLine("PID = " + System.Diagnostics.Process.GetCurrentProcess().Id);
                var timeOut = Task.Delay(TimeSpan.FromSeconds(60));
                while (!System.Diagnostics.Debugger.IsAttached && !timeOut.IsCompleted) ;
            }
#endif

            ExecuteAsync().Wait();

            return !Log.HasLoggedErrors;
        }

        private async Task ExecuteAsync()
        {
            var jsonFiles = await LoadJsonFiles();
            var (configs, artefacts) = FilterJsonFiles(jsonFiles);
            if (!Log.HasLoggedErrors)
            {
                foreach (var config in configs)
                {
                    var transformedArtefacts = ApplyConfig(config, artefacts);
                    await SaveOutput(config, transformedArtefacts);
                }
            }
            else
            {
                Log.LogMessage("Not generating output due to error(s) before configuration transformations.");
            }
        }

        private async Task<IList<JsonFile>> LoadJsonFiles()
        {
            var files = new List<JsonFile>(JsonFiles.Length);
            var errors = new List<BuildError>();
            foreach (var jsonFile in JsonFiles)
            {
                string identity;
                try
                {
                    identity = jsonFile.GetMetadata("Identity");
                    var fullPath = jsonFile.GetMetadata("FullPath");
                    var contents = await ReadContents(fullPath);
                    var file = JsonFileLoader.Parse(contents, identity);
                    files.Add(file);
                }
                catch (Exception e)
                {
                    Log.LogError("exception trying to load file", e);
                }
            }

            return files;
        }

        private async Task<string> ReadContents(string fullPath)
        {
            using (var stream = new StreamReader(fullPath))
            {
                return await stream.ReadToEndAsync();
            }
        }

        private (IList<JsonFile> configs, IList<JsonFile> artefacts) FilterJsonFiles(IEnumerable<JsonFile> jsonFiles)
        {
            var configs = new List<JsonFile>();
            var artefacts = new List<JsonFile>();
            foreach (var file in jsonFiles)
            {
                switch (file.Category)
                {
                    case ArtefactCategory.Config:
                        configs.Add(file);
                        break;

                    case ArtefactCategory.Dataset:
                    case ArtefactCategory.LinkedService:
                    case ArtefactCategory.Pipeline:
                        artefacts.Add(file);
                        break;

                    default:
                        throw new Exception("Unknown JSON file category.");
                }
            }

            return (configs, artefacts);
        }

        private IList<JsonFile> ApplyConfig(JsonFile config, IList<JsonFile> artefacts)
        {
            var artefactCopies = DeepClone(artefacts);

            var applier = new ConfigurationApplier();
            applier.ApplyTransforms(artefactCopies, config);

            OutputErrors(applier.Errors);

            return artefactCopies;
        }

        private void OutputErrors(IList<BuildError> errors)
        {
            foreach(var error in errors)
            {
                OutputError(error);
            }
        }

        private void OutputError(BuildError error)
        {
            var severity = ErrorCodes.All[error.Code].DefaultSeverity;
            switch (severity)
            {
                case ErrorSeverity.Information:
                    Log.LogMessage("ADFBuild", error.Code, "help keyword", error.FileName, error.LineNumber, error.LinePosition,
                        error.LineNumber, error.LinePosition, error.Message);
                    break;

                case ErrorSeverity.Warning:
                    Log.LogWarning("ADFBuild", error.Code, "help keyword", error.FileName, error.LineNumber, error.LinePosition,
                        error.LineNumber, error.LinePosition, error.Message);
                    break;

                case ErrorSeverity.Error:
                    Log.LogError("ADFBuild", error.Code, "help keyword", error.FileName, error.LineNumber, error.LinePosition,
                        error.LineNumber, error.LinePosition, error.Message);
                    break;
            }
        }

        private IList<JsonFile> DeepClone(IList<JsonFile> artefacts)
        {
            var copy = new List<JsonFile>(artefacts.Count);
            foreach(var artefact in artefacts)
            {
                var newArtefact = new JsonFile(artefact.Name,
                    artefact.Identity,
                    (JObject)artefact.Json.DeepClone(),
                    artefact.Category);
                copy.Add(newArtefact);
            }

            return copy;
        }

        private async Task SaveOutput(JsonFile config, IList<JsonFile> transformedArtefacts)
        {
            ResultSaver saver = new ResultSaver();
            await saver.SaveDocuments(transformedArtefacts, config, OutputPath);
        }
    }
}
