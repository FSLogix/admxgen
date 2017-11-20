using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace admxgen
{
    class InputParser
    {
        private enum EnumerationElementType { Decimal, @String };

        private struct ParseTypeResult
        {
            public object[] Elements;
            public PolicyPresentation Presentation;
        }

        private TextReader _reader;
        private const int MAX_ID_LENGTH = 96;
        private MD5 _md5 = MD5.Create();

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

        BooleanElement CreateBooleanElement(string policyId, string key, string valueName)
        {
            return new BooleanElement { id = policyId, key = key, valueName = valueName, trueValue = new Value { Item = new ValueDecimal { value = 1 } }, falseValue = new Value { Item = new ValueDecimal { value = 0 } } };
        }

        private DecimalElement CreateDecimalElement(string policyId, string key, string valueName, uint minValue, uint maxValue)
        {

            return new DecimalElement { id = policyId, key = key, valueName = valueName, minValue = minValue, maxValue = maxValue, required = true };
        }

        private object CreateEnumElement(string policyId, string key, string valueName, EnumerationElementType type, IDictionary<string, uint> valuesList)
        {
            var element = new EnumerationElement
            {
                id = policyId,
                key = key,
                valueName = valueName
            };

            var itemList = new List<EnumerationElementItem>();
            foreach (var val in valuesList)
            {
                if (type == EnumerationElementType.Decimal)
                {
                    var enumStringResourceId = GetResourceId("Enum", policyId, val.Key);
                    AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = enumStringResourceId, Value = val.Key });
                    itemList.Add(new EnumerationElementItem { displayName = GetStringRef(enumStringResourceId), value = new Value { Item = new ValueDecimal { value = val.Value } } });
                }
                else if (type == EnumerationElementType.String)
                {
                    var enumStringResourceId = GetResourceId("Enum", policyId, val.Key);
                    AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = enumStringResourceId, Value = val.Key });
                    itemList.Add(new EnumerationElementItem { displayName = GetStringRef(enumStringResourceId), value = new Value { Item = val.Key } });
                }
                else
                {
                    throw new ArgumentOutOfRangeException("type", "Unexpected enumeration element type");
                }
            }
            element.item = itemList.ToArray();

            return element;
        }

        TextElement CreateTextElement(string policyId, string key, string valueName)
        {
            return new TextElement { id = policyId, key = key, valueName = valueName, required = true };
        }

        private string GetRef(string type, string resourceId)
        {
            return $"$({type}.{resourceId})";
        }

        private string GetResourceId(params string[] ss)
        {
            byte[] data = _md5.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(ss)));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private string GetStringRef(string resourceId)
        {
            return GetRef("string", resourceId);
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
            string fullCatName = string.Empty;
            foreach (var cat in category.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
            {
                fullCatName += string.IsNullOrEmpty(fullCatName) ? cat : "\\" + cat;
                var categoryStringResourceId = GetResourceId("Cat", fullCatName);
                var categoryId = GetResourceId("Cat", fullCatName);

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
                    var enumerationElementType = (EnumerationElementType)Enum.Parse(typeof(EnumerationElementType), properties["Type"]);
                    var valuesList = new Dictionary<string, uint>();
                    foreach (var v in properties["Values"].Split(new[] { '|' }))
                    {
                        var vv = v.Split(new[] { ':' });
                        valuesList.Add(vv[0], vv.Length > 1 ? uint.Parse(vv[1]) : 0);
                    }
                    result.Elements = new object[] { CreateEnumElement(policyId, key, valueName, enumerationElementType, valuesList) };
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new DropdownList { refId = policyId } } };
                    break;
                case "checkBox":
                    result.Elements = new object[] { CreateBooleanElement(policyId, key, valueName) };
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new CheckBox { refId = policyId, Value = properties["Label"] } } };
                    break;
                case "textBox":
                    result.Elements = new object[] { CreateTextElement(policyId, key, valueName) };
                    string defaultValue;
                    properties.TryGetValue("Default", out defaultValue);
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new TextBox { refId = policyId, label = properties["Label"], defaultValue = defaultValue } } };
                    break;
                case "decimal":
                    string minValue;
                    properties.TryGetValue("MinValue", out minValue);
                    string maxValue;
                    properties.TryGetValue("MaxValue", out maxValue);
                    result.Elements = new object[] { CreateDecimalElement(policyId, key, valueName, uint.Parse(minValue), uint.Parse(maxValue)) };
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new DecimalTextBox { refId = policyId, Value = properties["Label"] } } };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("valueName", "Unexpected policy type");
            }
            return result;
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
