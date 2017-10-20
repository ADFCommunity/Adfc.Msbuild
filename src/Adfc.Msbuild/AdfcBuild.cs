using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

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

            try
            {
                try
                {
                    ExecuteAsync().Wait();
                }
                catch (AggregateException exception)
                {
                    if (exception.InnerExceptions.Count == 1)
                    {
                        var edi = ExceptionDispatchInfo.Capture(exception.InnerException);
                        edi.Throw();
                    }

                    throw;
                }
            }
            catch (Exception exception)
            {
                Log.LogError("Internal AdfcBuild error. Please check that your project is " +
                    "referencing the latest version of the Adfc.Msbuild Nuget package. If this " +
                    "error persists, please log an issue at https://github.com/ADFCommunity/Home/issues/new. " +
                    "Exception details:\n" + exception);
            }

            return !Log.HasLoggedErrors;
        }

        private async Task ExecuteAsync()
        {
            var jsonFiles = await LoadJsonFiles();
            await ValidateJsonFilesAsync(jsonFiles);
            if (!Log.HasLoggedErrors)
            {
                var (configs, artefacts) = FilterJsonFiles(jsonFiles);
                if (configs.Count == 0)
                {
                    configs.Add(new JsonFile("NoConfig", null, new JObject(), ArtefactCategory.Config));
                }
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
                string identity = jsonFile.GetMetadata("Identity");
                try
                {
                    var fullPath = jsonFile.GetMetadata("FullPath");
                    using (var reader = new StreamReader(fullPath))
                    {
                        var loader = new JsonFileLoader();
                        await loader.LoadAsync(identity, reader);
                        if (loader.JsonFile != null)
                        {
                            files.Add(loader.JsonFile);
                        }
                        OutputErrors(loader.Errors);
                    }
                }
                catch (Exception e)
                {
                    Log.LogError($"exception trying to load file {identity}: {e}");
                }
            }

            return files;
        }

        private async Task ValidateJsonFilesAsync(IList<JsonFile> jsonFiles)
        {
            using (var httpClient = new HttpClient(new CachingDelegatingHandler(new OfflineDelegatingHandler(new HttpClientHandler()))))
            {
                foreach (var jsonFile in jsonFiles)
                {
                    var validator = new JsonSchemaValidation(httpClient);
                    var error = await validator.ValidateAsync(jsonFile);
                    if (error != null)
                    {
                        OutputError(error);
                    }
                }
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
                        var error = new BuildError()
                        {
                            Code = ErrorCodes.Adfc0003.Code,
                            FileName = file.Identity,
                            Message = "Unable to determine Data Factory artefact type. You can specify the JSON schema to explicityle set the type."
                        };
                        OutputError(error);
                        break;
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

        private void OutputErrors(IEnumerable<BuildError> errors)
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
                    Log.LogMessage("ADFBuild", error.Code, "help keyword", error.FileName, error.LineNumber ?? 0, error.LinePosition ?? 0,
                        error.LineNumber ?? 0, error.LinePosition ?? 0, error.Message);
                    break;

                case ErrorSeverity.Warning:
                    Log.LogWarning("ADFBuild", error.Code, "help keyword", error.FileName, error.LineNumber ?? 0, error.LinePosition ?? 0,
                        error.LineNumber ?? 0, error.LinePosition ?? 0, message: error.Message);
                    break;

                case ErrorSeverity.Error:
                    Log.LogError("ADFBuild", error.Code, "help keyword", error.FileName, error.LineNumber ?? 0, error.LinePosition ?? 0,
                        error.LineNumber ?? 0, error.LinePosition ?? 0, error.Message, (object[])null);
                    break;

                default:
                    Log.LogError("Internal error. Unknown error severity " + severity);
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
