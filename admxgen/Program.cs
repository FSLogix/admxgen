using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace admxgen
{
    public class TargetNamespace
    {
        public string Prefix { get; set; }
        public string Namespace { get; set; }
    }

    public class AdmxSettings
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public string MinRequiredRevision { get; set; }
        public TargetNamespace TargetNamespace { get; set; }
        public string SchemaVersion { get; set; }
        public string FallbackCulture { get; set; }
        public List<string> SupersededPolicyFiles { get; set; }
        public string File { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: admxgen [input_file] [output_filename]");
                    return 1;
                }

                Console.WriteLine($"Generating from {args[0]}");
                var admxSettings = JsonConvert.DeserializeObject<AdmxSettings>(File.ReadAllText(args[0]));

                var parser = new InputParser(new StreamReader(admxSettings.File));
                parser.Parse();

                parser.Definitions.revision = admxSettings.Revision;
                parser.Definitions.schemaVersion = admxSettings.SchemaVersion;
                parser.Definitions.policyNamespaces.target.prefix = admxSettings.TargetNamespace.Prefix;
                parser.Definitions.policyNamespaces.target.@namespace = admxSettings.TargetNamespace.Namespace;
                parser.Definitions.resources.minRequiredRevision = admxSettings.MinRequiredRevision;
                parser.Definitions.resources.fallbackCulture = admxSettings.FallbackCulture;
                parser.Definitions.supersededAdm = admxSettings.SupersededPolicyFiles.Select(s => new FileReference { fileName = s }).ToArray();

                parser.Resources.revision = admxSettings.Revision;
                parser.Resources.schemaVersion = admxSettings.SchemaVersion;
                parser.Resources.displayName = admxSettings.DisplayName;
                parser.Resources.description = admxSettings.Description;

                var xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8
                };

                Console.WriteLine($"Writing output {args[1]}.admx");
                using (var w = XmlWriter.Create(new StreamWriter($"{args[1]}.admx"), xmlWriterSettings))
                {
                    var ser = new XmlSerializer(parser.Definitions.GetType(), "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions");
                    ser.Serialize(w, parser.Definitions);
                }

                Console.WriteLine($"Writing output {args[1]}.adml");
                using (var w = XmlWriter.Create(new StreamWriter($"{args[1]}.adml"), xmlWriterSettings))
                {
                    var ser = new XmlSerializer(parser.Resources.GetType(), "http://www.microsoft.com/GroupPolicy/PolicyDefinitions");
                    ser.Serialize(w, parser.Resources);
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return e.HResult;
            }
        }
    }
}
