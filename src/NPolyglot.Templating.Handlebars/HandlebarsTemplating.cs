using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NPolyglot.Templating.Handlebars
{
    public class HandlebarsTemplating : Task
    {
        [Required]
        public ITaskItem[] DslCodeFiles { get; set; }

        [Required]
        public ITaskItem[] Templates { get; set; }

        [Required]
        public string OutputDir { get; set; }

        [Output]
        public ITaskItem[] GeneratedOutput { get; set; }

        public override bool Execute()
        {
            try
            {
                // precompile templates
                var templates = new Dictionary<string, Func<object, string>>();
                foreach (var t in Templates)
                {
                    var fullPath = Path.GetFullPath(t.ItemSpec);
                    var filename = Path.GetFileName(t.ItemSpec);
                    var templateFunc = HandlebarsDotNet.Handlebars.Compile(File.ReadAllText(fullPath));
                    templates.Add(filename, templateFunc);
                }

                var outputResult = new List<ITaskItem>();

                // run matchig template for generated objects
                foreach (var f in DslCodeFiles.Where(IsValidFile))
                {
                    var templateKey = f.GetMetadata("Template");
                    var objContent = f.GetMetadata("Object");

                    var outputFilename = Path.GetFileNameWithoutExtension(f.ItemSpec) + ".cs";
                    var outputPath = Path.Combine(OutputDir, outputFilename);

                    // wrap and deserialize object to a dynamic
                    var wrap = "{\"root\":" + objContent + "}";
                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(wrap, new ExpandoObjectConverter());

                    // process the template
                    var generated = templates[templateKey](obj.root);
                    File.WriteAllText(outputPath, generated);
                    outputResult.Add(new TaskItem(outputPath));
                }

                GeneratedOutput = outputResult.ToArray();

                return true;
            }
            catch (Exception e)
            {
                Log.LogError("Failed to process DSL templates: {0}", e);
                return false;
            }
        }

        private bool IsValidFile(ITaskItem item) =>
            item.MetadataNames.Cast<string>().Contains("Object") &&
            item.MetadataNames.Cast<string>().Contains("Template");
    }
}
