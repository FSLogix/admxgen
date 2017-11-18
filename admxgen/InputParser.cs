using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace admxgen
{
    class InputParser
    {
        private TextReader _reader;
        private const int MAX_ID_LENGTH = 96;

        public PolicyDefinitions Definitions { get; } = new PolicyDefinitions
        {
            policyNamespaces = new PolicyNamespaces
            {
                target = new PolicyNamespaceAssociation()
            },
            resources = new LocalizationResourceReference(),
            supportedOn = new SupportedOnTable
            {
                definitions = new SupportedOnDefinition[0]
            },
            categories = new CategoryList(),
            policies = new PolicyList()
        };

        public PolicyDefinitionResources Resources { get; } = new PolicyDefinitionResources
        {
            resources = new Localization
            {
                stringTable = new LocalizationStringTable
                {
                    @string = new LocalizedString[0]
                },
                presentationTable = new LocalizationPresentationTable()
                {
                    presentation = new PolicyPresentation[0]
                }
            }
        };

        public InputParser(TextReader reader)
        {
            this._reader = reader;
        }

        private void AddUniqueArrayItem<T>(Action<T[]> setCollection, T[] collection, T item)
        {
            if (collection == null)
            {
                collection = new T[0];
            }
            var mutableCollection = new List<T>(collection);
            if (!mutableCollection.Any(i => i.Equals(item)))
            {
                mutableCollection.Add(item);
            }
            setCollection(mutableCollection.ToArray());
        }

        BooleanElement GetBooleanElement(string policyId, string key, string valueName)
        {
            return new BooleanElement { id = policyId, key = key, valueName = valueName, trueValue = new Value { Item = new ValueDecimal { value = 1 } }, falseValue = new Value { Item = new ValueDecimal { value = 0 } } };
        }

        private DecimalElement GetDecimalElement(string policyId, string key, string valueName, uint minValue, uint maxValue)
        {

            return new DecimalElement { id = policyId, key = key, valueName = valueName, minValue = minValue, maxValue = maxValue, required = true };
        }

        private string GetRef(string type, string resourceId)
        {
            return $"$({type}.{resourceId})";
        }

        private string GetResourceId(params string[] ss)
        {
            var result = string.Empty;
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            foreach (var s in ss)
            {
                result += rgx.Replace(s, "_") + "_";
            }
            return result.Length > MAX_ID_LENGTH ? result.Substring(0, MAX_ID_LENGTH) : result;
        }

        private string GetStringRef(string resourceId)
        {
            return GetRef("string", resourceId);
        }

        TextElement GetTextElement(string policyId, string key, string valueName)
        {
            return new TextElement { id = policyId, key = key, valueName = valueName, required = true };
        }

        public void Parse()
        {
            using (var csvReader = new CsvReader(_reader, true))
            {
                var csvHeaders = csvReader.GetFieldHeaders();
                while (csvReader.ReadNextRecord())
                {
                    ParseLine(csvReader);
                }
            }
        }

        private string ParseCategory(string category)
        {
            string lastCategoryId = string.Empty;
            foreach (var cat in category.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var categoryStringResourceId = GetResourceId("Cat", cat);
                var categoryId = GetResourceId("Cat", cat);

                AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = categoryStringResourceId, Value = cat });
                var theNewCategory = new Category
                {
                    name = categoryId,
                    displayName = GetStringRef(categoryStringResourceId)
                };
                if (!string.IsNullOrEmpty(lastCategoryId))
                {
                    theNewCategory.parentCategory = new CategoryReference { @ref = lastCategoryId };
                }
                AddUniqueArrayItem(c => Definitions.categories.category = c, Definitions.categories.category, theNewCategory);

                lastCategoryId = categoryId;
            }

            return lastCategoryId;
        }

        private void ParseLine(CsvReader csvReader)
        {
            var category = csvReader["Category"];
            var displayName = csvReader["Display Name"];
            var @class = csvReader["Class"];
            var type = csvReader["Type"];
            var explanation = csvReader["Explanation"];
            var registryKey = csvReader["Registry Key"];
            var valueName = csvReader["Value Name"];
            var supportedOn = csvReader["Supported On"];
            var properties = csvReader["Properties"];

            try
            {
                var displayNameStringResourceId = GetResourceId("DisplayName", displayName);
                AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = displayNameStringResourceId, Value = displayName });
                var displayNameStringRef = GetStringRef(displayNameStringResourceId);

                var explainTextStringResourceId = GetResourceId("Explain", explanation);
                AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = explainTextStringResourceId, Value = explanation });
                var explainTextStringRef = GetStringRef(explainTextStringResourceId);

                var policyId = GetResourceId("Policy", category, displayName);

                var propertiesDictionary = properties.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(part => part.Split('='))
                   .ToDictionary(split => split[0], split => split[1]);

                // If no label provided, use the policy display name
                if (!propertiesDictionary.Any(d => d.Key == "Label"))
                {
                    propertiesDictionary.Add("Label", displayName);
                }

                var parseTypeResults = ParseType(policyId, type, registryKey, valueName, propertiesDictionary);
                AddUniqueArrayItem(c => Resources.resources.presentationTable.presentation = c, Resources.resources.presentationTable.presentation, parseTypeResults.Presentation);
                var presentationStringRef = GetRef("presentation", parseTypeResults.Presentation.id);

                var policy = new PolicyDefinition
                {
                    name = policyId,
                    parentCategory = new CategoryReference { @ref = ParseCategory(category) },
                    displayName = displayNameStringRef,
                    @class = (PolicyClass)Enum.Parse(typeof(PolicyClass), @class),
                    explainText = explainTextStringRef,
                    key = registryKey,
                    supportedOn = new SupportedOnReference { @ref = ParseSupportedOn(supportedOn) },
                    elements = parseTypeResults.Elements,
                    presentation = presentationStringRef
                };
                AddUniqueArrayItem(c => Definitions.policies.policy = c, Definitions.policies.policy, policy);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing {category}\\{displayName}", ex);
            }
        }

        private string ParseSupportedOn(string supportedOn)
        {
            var supportedOnStringResourceId = GetResourceId("SupportedOn", supportedOn);
            var supportedOnId = GetResourceId("SupportedOn", supportedOn);

            AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = supportedOnStringResourceId, Value = supportedOn });
            AddUniqueArrayItem(c => Definitions.supportedOn.definitions = c, Definitions.supportedOn.definitions, new SupportedOnDefinition { name = supportedOnId, displayName = GetStringRef(supportedOnStringResourceId) });

            return supportedOnId;
        }

        private ParseTypeResult ParseType(string policyId, string type, string key, string valueName, IDictionary<string,string> properties)
        {
            var result = new ParseTypeResult();
            switch (type)
            {
                case "enum":
                    Console.WriteLine("WARN: unexpected policy type 'enum'");
                    result.Elements = new List<object> { GetBooleanElement(policyId, key, valueName) }.ToArray();
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new List<object> { new CheckBox { refId = policyId, Value = properties["Label"] } }.ToArray() };
                    break;
                case "checkBox":
                    result.Elements = new List<object>{ GetBooleanElement(policyId, key, valueName) }.ToArray();
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new List<object> { new CheckBox { refId = policyId, Value = properties["Label"] } }.ToArray() };
                    break;
                case "textBox":
                    result.Elements = new List<object> { GetTextElement(policyId, key, valueName) }.ToArray();
                    string defaultValue;
                    properties.TryGetValue("Default", out defaultValue);
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new List<object> { new TextBox { refId = policyId, label = properties["Label"], defaultValue = defaultValue } }.ToArray() };
                    break;
                case "decimal":
                    string minValue;
                    properties.TryGetValue("MinValue", out minValue);
                    string maxValue;
                    properties.TryGetValue("MinValue", out maxValue);
                    result.Elements = new List<object> { GetDecimalElement(policyId, key, valueName, uint.Parse(minValue), uint.Parse(maxValue)) }.ToArray();
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new List<object> { new DecimalTextBox { refId = policyId, Value = properties["Label"] } }.ToArray() };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("valueName", "Unexpected policy type");
            }
            return result;
        }

        private struct ParseTypeResult
        {
            public object[] Elements;
            public PolicyPresentation Presentation;
        }
    }
}

public partial class Category
{
    public override bool Equals(object obj)
    {
        var category = obj as Category;
        return this.name.Equals(category.name);
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }
}

public partial class LocalizedString
{
    public override bool Equals(object obj)
    {
        var @string = obj as LocalizedString;
        return this.id.Equals(@string.id);
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
}

public partial class SupportedOnDefinition
{
    public override bool Equals(object obj)
    {
        var supportedOnDefinition = obj as SupportedOnDefinition;
        return this.name.Equals(supportedOnDefinition.name);
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }
}
