using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace admxgen
{
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
                var parser = new InputParser(new StreamReader(args[0]));
                parser.Parse();

                parser.Definitions.revision = "1.0";
                parser.Definitions.schemaVersion = "1.0";
                parser.Definitions.policyNamespaces.target.prefix = "FSLogix";
                parser.Definitions.policyNamespaces.target.@namespace = "FSLogix.Policies";
                parser.Definitions.resources.minRequiredRevision = "1.0";
                parser.Definitions.resources.fallbackCulture = "en-US";
                parser.Definitions.supersededAdm = new List<FileReference>
                {
                    new FileReference { fileName = "fslogixODFC2.2.adm" },
                    new FileReference { fileName = "fslogixODFC2.5.adm" }
                }.ToArray();

                parser.Resources.revision = "1.0";
                parser.Resources.schemaVersion = "1.0";
                parser.Resources.displayName = "FSLogix";
                parser.Resources.description = "FSLogix Configuration";

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
