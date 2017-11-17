using System.Collections.Generic;
using System.Xml.Serialization;

namespace admxgen
{
    [XmlType("policyDefinitions", Namespace = "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions")]
    public class PolicyDefinitions
    {
        [XmlAttribute("revision")]
        public string Revision;
        [XmlAttribute("schemaVersion")]
        public string SchemaVersion;
        [XmlArray("policyNamespaces")]
        public List<PolicyNamespace> PolicyNamespaces;
        [XmlElement("resources")]
        public Resources Resources;
        [XmlElement("supportedOn")]
        public SupportedOn SupportedOn;
        [XmlArray("categories")]
        public List<Category> Categories;
        [XmlArray("policies")]
        public List<Policy> Policies;
    }

    [XmlType("policyNamespace")]
    public class PolicyNamespace
    {
        [XmlAttribute("prefix")]
        public string Prefix;
        [XmlAttribute("namespace")]
        public string Namespace;
    }

    public class Resources
    {
        [XmlAttribute("minRequiredRevision")]
        public string MinRequiredRevision;
    }

    public class SupportedOn
    {
        [XmlArray("definitions")]
        public List<Definition> Definitions;
    }

    [XmlType("definition")]
    public class Definition
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("displayName")]
        public string DisplayName;
    }

    [XmlType("category")]
    public class Category
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("displayName")]
        public string DisplayName;
        [XmlElement("parentCategory", IsNullable = false)]
        public Reference ParentCategory;
    }

    public class Reference
    {
        [XmlAttribute("ref")]
        public string Ref;
    }

    [XmlType("policy")]
    public class Policy
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("class")]
        public string Class;
        [XmlAttribute("displayName")]
        public string DisplayName;
        [XmlAttribute("explainText")]
        public string ExplainText;
        [XmlAttribute("key")]
        public string Key;
        [XmlAttribute("presentation")]
        public string Presentation;
        [XmlElement("parentCategory")]
        public Reference ParentCategory;
        [XmlElement("supportedOn")]
        public Reference SupportedOn;
        [XmlElement("elements")]
        public Elements Elements;
    }

    public class Elements
    {
        [XmlElement(typeof(BooleanElement))]
        [XmlElement(typeof(TextElement))]
        [XmlElement(typeof(DecimalElement))]
        [XmlElement(typeof(EnumElement))]
        public List<Element> List;
    }

    public abstract class Element
    {
        [XmlAttribute("id")]
        public string Id;
        [XmlAttribute("key")]
        public string Key;
        [XmlAttribute("valueName")]
        public string ValueName;
    }

    [XmlType("boolean")]
    public class BooleanElement : Element
    {
        [XmlElement("trueValue")]
        public BooleanValue TrueValue = new BooleanValue { Decimal = new DecimalValue { Value = 1 } };
        [XmlElement("falseValue")]
        public BooleanValue FalseValue = new BooleanValue { Decimal = new DecimalValue { Value = 0 } };
    }

    public class BooleanValue
    {
        [XmlElement("decimal")]
        public DecimalValue Decimal;
    }

    [XmlType("text")]
    public class TextElement : Element
    {
        [XmlAttribute("required")]
        public bool Required;
    }

    [XmlType("decimal")]
    public class DecimalElement : Element
    {
        [XmlAttribute("required")]
        public bool Required;
        [XmlAttribute("maxValue")]
        public int MaxValue;
        [XmlAttribute("minValue")]
        public int MinValue;
    }

    [XmlType("enum")]
    public class EnumElement : Element
    {
        [XmlAttribute("required")]
        public bool Required;
        [XmlElement("item")]
        public List<EnumItem> Items;
    }

    public class EnumItem
    {
        [XmlAttribute("displayName")]
        public string DisplayName;
        [XmlElement("value")]
        public EnumValue Value;
    }

    public class EnumValue
    {
        [XmlElement("string", IsNullable = false)]
        public StringValue String;
        [XmlElement("decimal", IsNullable = false)]
        public DecimalValue Decimal;
    }

    public class StringValue
    {
        [XmlElement("string")]
        public string String;
    }

    public class DecimalValue
    {
        [XmlAttribute("value")]
        public int Value;
    }
}
