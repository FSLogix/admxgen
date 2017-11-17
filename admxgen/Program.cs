using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace admxgen
{
    class Program
    {
        //static string Mash(string s)
        //{
        //    Regex rgx = new Regex("[^a-zA-Z0-9]");
        //    return rgx.Replace(s, "_");
        //}

        static void Main(string[] args)
        {
            var p = new PolicyDefinitions
            {
                Revision = "1.0",
                SchemaVersion = "1.0",
                PolicyNamespaces = new List<PolicyNamespace>
                {
                    new PolicyNamespace { Prefix = "Fslogix", Namespace = "Fslogix.Policies" }
                },
                Resources = new Resources { MinRequiredRevision = "1.0" },
                SupportedOn = new SupportedOn
                {
                    Definitions = new List<Definition>
                    {
                        new Definition { Name = "SupportedOn25", DisplayName = "$(string.String_BASE_SupportedOn_FSLogix_Profiles_2_5)" },
                        new Definition { Name = "SupportedOn26", DisplayName = "$(string.String_BASE_SupportedOn_FSLogix_Profiles_2_6)" }
                    }
                },
                Categories = new List<Category>
                {
                    new Category { Name = "Cat_FSLogix", DisplayName = "$(string.String_Cat_FSLogix)" },
                    new Category { Name = "Cat_PROF_Profile_Container", DisplayName = "$(string.String_PROF_Cat_Profile_Container)", ParentCategory = new Reference { Ref = "Cat_FSLogix" } }
                },
                Policies = new List<Policy>
                {
                    new Policy
                    {
                        Name = "Policy_PROF_Enabled",
                        Class = "Machine",
                        DisplayName = "$(string.String_CMN_Policy_Enabled)",
                        ExplainText = "$(string.String_PROF_Explain_Policy_Enabled)",
                        Key = "Software\\FSLogix\\Profiles",
                        Presentation = "$(presentation.Policy_PROF_Enabled)",
                        ParentCategory = new Reference { Ref = "Cat_PROF_Profile_Container" },
                        SupportedOn = new Reference { Ref = "SupportedOn25" },
                        Elements = new Elements
                        {
                            List = new List<Element>
                            {
                                new BooleanElement
                                {
                                    Id = "Policy_PROF_CheckBox_Element_Enabled",
                                    Key = "Software\\FSLogix\\Profiles",
                                    ValueName = "Enabled"
                                }
                            }
                        }
                    },
                    new Policy
                    {
                        Name = "Policy_PROF_VHD_Location",
                        Class = "Machine",
                        DisplayName = "$(string.String_Policy_VHD_Location)",
                        ExplainText = "$(string.String_Explain_Specifies_the_network)",
                        Key = "Software\\FSLogix\\Profiles",
                        Presentation = "$(presentation.Policy_VHD_Location)",
                        ParentCategory = new Reference { Ref = "Cat_PROF_Profile_Container" },
                        SupportedOn = new Reference { Ref = "SupportedOn25" },
                        Elements = new Elements
                        {
                            List = new List<Element>
                            {
                                new TextElement
                                {
                                    Id = "Policy_PROF_CheckBox_Element_Enabled",
                                    Key = "Software\\FSLogix\\Profiles",
                                    ValueName = "Enabled",
                                    Required = true
                                }
                            }
                        }
                    },
                    new Policy
                    {
                        Name = "Policy_PROF_Size_in_MBs",
                        Class = "Machine",
                        DisplayName = "$(string.String_Policy_VHD_Location)",
                        ExplainText = "$(string.String_Explain_Specifies_the_network)",
                        Key = "Software\\FSLogix\\Profiles",
                        Presentation = "$(presentation.Policy_VHD_Location)",
                        ParentCategory = new Reference { Ref = "Cat_PROF_Profile_Container" },
                        SupportedOn = new Reference { Ref = "SupportedOn25" },
                        Elements = new Elements
                        {
                            List = new List<Element>
                            {
                                new DecimalElement
                                {
                                    Id = "Policy_PROF_CheckBox_Element_Enabled",
                                    Key = "Software\\FSLogix\\Profiles",
                                    ValueName = "Enabled",
                                    Required = true,
                                    MaxValue = 10000,
                                    MinValue = 500
                                }
                            }
                        }
                    },
                    new Policy
                    {
                        Name = "Policy_PROF_Virtual_Disk_Type",
                        Class = "Machine",
                        DisplayName = "$(string.String_Policy_VHD_Location)",
                        ExplainText = "$(string.String_Explain_Specifies_the_network)",
                        Key = "Software\\FSLogix\\Profiles",
                        Presentation = "$(presentation.Policy_VHD_Location)",
                        ParentCategory = new Reference { Ref = "Cat_PROF_Profile_Container" },
                        SupportedOn = new Reference { Ref = "SupportedOn25" },
                        Elements = new Elements
                        {
                            List = new List<Element>
                            {
                                new EnumElement
                                {
                                    Id = "Policy_PROF_CheckBox_Element_Enabled",
                                    Key = "Software\\FSLogix\\Profiles",
                                    ValueName = "Enabled",
                                    Required = true,
                                    Items = new List<EnumItem>
                                    {
                                        new EnumItem { DisplayName = "string", Value = new EnumValue { String = new StringValue { String = "VHD" } } },
                                        new EnumItem { DisplayName = "string", Value = new EnumValue { String = new StringValue { String = "VHDX" } } }
                                    }
                                }
                            }
                        }
                    },
                    new Policy
                    {
                        Name = "Policy_PROF_Virtual_Disk_Type",
                        Class = "Machine",
                        DisplayName = "$(string.String_Policy_VHD_Location)",
                        ExplainText = "$(string.String_Explain_Specifies_the_network)",
                        Key = "Software\\FSLogix\\Profiles",
                        Presentation = "$(presentation.Policy_VHD_Location)",
                        ParentCategory = new Reference { Ref = "Cat_PROF_Profile_Container" },
                        SupportedOn = new Reference { Ref = "SupportedOn25" },
                        Elements = new Elements
                        {
                            List = new List<Element>
                            {
                                new EnumElement
                                {
                                    Id = "Policy_PROF_CheckBox_Element_Enabled",
                                    Key = "Software\\FSLogix\\Profiles",
                                    ValueName = "Enabled",
                                    Required = true,
                                    Items = new List<EnumItem>
                                    {
                                        new EnumItem { DisplayName = "string", Value = new EnumValue { Decimal = new DecimalValue { Value = 0 } } }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var xmlWriter = XmlWriter.Create(Console.Out, xmlWriterSettings))
            {
                var x = new XmlSerializer(p.GetType(), "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions");
                x.Serialize(xmlWriter, p);
            }

            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
