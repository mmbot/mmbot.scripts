using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace mmbot.scriptcatalog.generator
{
    class Program
    {
        private const string ScriptLinkRoot = "https://github.com/mmbot/mmbot.scripts/raw/master/scripts/";
        private const string Repo = "http://github.com/mmbot/mmbot.scripts/blob/master/scripts/";
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException(@"Must be invoked with 2 arguments, relative script path and output file path. e.g. mmbot.scriptcatalog.generator.exe ..\scripts\ .\");
            }

            var relativeScriptPath = args[0];
            var outputPath = args[1];

            var scripts = Directory.GetFiles(relativeScriptPath, "*.csx", SearchOption.AllDirectories);

            if (!scripts.Any())
            {
                throw new ArgumentException("No scripts found, execute this script from the catalog directory and ensure the scripts folder exists");
            }
            var allMetadata = new List<Metadata>();

            foreach (var scriptFile in scripts)
            {
                var scriptName = Path.GetFileNameWithoutExtension(scriptFile);
                var scriptFolder = new DirectoryInfo(Path.GetDirectoryName(scriptFile)).Name;
                if (scriptFolder.ToLowerInvariant() == "scripts")
                {
                    scriptFolder = string.Empty;
                }
                else
                {
                    scriptFolder = scriptFolder.Replace(" ", "%20") + "/";
                }
                var scriptLink = string.Concat(ScriptLinkRoot, scriptFolder, scriptName, ".csx");
                var repoScriptLink = string.Concat(Repo, scriptFolder, scriptName, ".csx");

                Console.WriteLine("Parsing comments for " + scriptName);
                try
                {
                    var parseOptions = Roslyn.Compilers.CSharp.ParseOptions.Default.WithParseDocumentationComments(true);
                    var cancellationToken = System.Threading.CancellationToken.None;
                    var tree = Roslyn.Compilers.CSharp.SyntaxTree.ParseFile(scriptFile, parseOptions, cancellationToken);
                    var trees = new List<Roslyn.Compilers.CSharp.SyntaxTree>();
                    trees.Add(tree);
                    
                    var compilation = Roslyn.Compilers.CSharp.Compilation.Create("comments", null, trees);
                    var classSymbol = compilation.GlobalNamespace.GetMembers();
                    var doc = classSymbol[0].GetDocumentationComment(null, cancellationToken);
                    var comments = XDocument.Parse("<root>" + doc.FullXmlFragmentOpt + "</root>");


                    var metadata = new Metadata
                    {
                        Name = scriptName,
                        Description = ParseComment(comments.Root.Element("description").GetValueOrEmpty()).Trim(),
                        Configuration = ParseComment(comments.Root.Element("configuration").GetValueOrEmpty()).Trim(),
                        Commands = ParseComment(comments.Root.Element("commands").GetValueOrEmpty()).Trim(),
                        Notes = ParseComment(comments.Root.Element("notes").GetValueOrEmpty()).Trim(),
                        Author = ParseComment(comments.Root.Element("author").GetValueOrEmpty()).Trim(),
                        Link = scriptLink.Trim(),
                        RepoLink = repoScriptLink.Trim(),
                    };

                    allMetadata.Add(metadata);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to parse comments for {0} - {1}\r\n{2}", scriptFile, ex.Message, ex);
                    Console.WriteLine("Generating filler entry");

                    allMetadata.Add(new Metadata
                    {
                        Name = scriptName,
                        Description = string.Empty,
                        Author = string.Empty,
                        Commands = string.Empty,
                        Configuration = string.Empty,
                        Notes = string.Empty,
                        Link = scriptLink.Trim(),
                        RepoLink = repoScriptLink.Trim()
                    });

                }

            }

            WriteJson(allMetadata, outputPath);

            WriteYml(allMetadata, outputPath);

            Console.WriteLine("Completed cataloging, output has been saved to catalog.json and catalog.yml");

        }

        private static void WriteJson(List<Metadata> allMetadata, string outputPath)
        {
            var outputFile = Path.Combine(outputPath, "catalog.json");
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            File.WriteAllText(outputFile, JsonConvert.SerializeObject(allMetadata.OrderBy(m => m.Name).ToArray(), Formatting.Indented, jsonSerializerSettings));
            Console.WriteLine("Finished writing {0}", outputFile);
        }

        private static void WriteYml(IEnumerable<Metadata> allMetadata, string outputPath)
        {
            // build yaml file
            var yml = string.Join("\n", allMetadata.OrderBy(m => m.Name).Select(metadata =>
            {
                var sb = new StringBuilder();

                sb.AppendLine("- name: " + metadata.Name.Trim());

                sb.AppendLine("  description: " + metadata.Description.Trim());

                if (!string.IsNullOrWhiteSpace(metadata.Configuration))
                {
                    sb.AppendLine("  configuration: |");
                    foreach (var line in metadata.Configuration.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine("    " + line.Trim());
                    }
                }
                if (!string.IsNullOrWhiteSpace(metadata.Commands))
                {
                    sb.AppendLine("  commands: |");
                    foreach (var line in metadata.Commands.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine("    " + line.Trim());
                    }
                }
                if (!string.IsNullOrWhiteSpace(metadata.Notes))
                {
                    sb.AppendLine("  notes: >");
                    foreach (var line in metadata.Notes.Split(new[] {'\n'}))
                    {
                        sb.AppendLine("    " + line.Trim());
                    }
                }
                sb.AppendLine("  author: " + metadata.Author.Trim());
                sb.AppendLine("  permalink: " + metadata.Link.Trim());
                sb.AppendLine("  repolink: " + metadata.RepoLink.Trim());
                sb.AppendLine("");
                return sb.ToString();
            }).ToArray());


            // save as utf8 without BOM
            var utf8NoBomEncoding = new UTF8Encoding(false);

            var outputFile = Path.Combine(outputPath, "catalog.yml");
            File.WriteAllText(outputFile, yml.Replace("\r\n", "\n"), utf8NoBomEncoding);
            Console.WriteLine("Finished writing {0}", outputFile);
        }

        private static string ParseComment(string data)
        {
            if (data == null)
            {
                return string.Empty;
            }
            return string.Join("\n", data.Split(';').Select(s => s.Trim())).Trim();
        }

        public class Metadata
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Configuration { get; set; }
            public string Commands { get; set; }
            public string Notes { get; set; }
            public string Author { get; set; }
            public string Link { get; set; }
            public string RepoLink { get; set; }            
        }

    }

    public static class XLinqExtensions
    {
        public static string GetValueOrEmpty(this XElement element)
        {
            if (element == null)
            {
                return string.Empty;
            }

            return element.Value;
        }
    }
}
