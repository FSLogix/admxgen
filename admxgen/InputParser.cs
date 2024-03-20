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
    public class InputParser
    {
        private enum ResourceType { Category, Definition, Policy, Display, Explain, Presentation, Enum };

        private enum EnumerationElementType { Decimal, @String };

        private struct ParseTypeResult
        {
            public object[] Elements;
            public PolicyPresentation Presentation;
        }

        private TextReader _reader;
        private const int MAX_ID_LENGTH = 96;
        private HashCalculator _hashCalculator = new HashCalculator();

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

        private object CreateEnumElement(string policyId, string key, string valueName, EnumerationElementType type, IDictionary<string, uint> valuesList)
        {
            var element = new EnumerationElement
            {
                id = policyId,
                key = key,
                valueName = valueName
            };

            int enumCounter = 0;
            var itemList = new List<EnumerationElementItem>();
            foreach (var val in valuesList)
            {
                var enumStringResourceId = GetResourceId(ResourceType.Enum, $"{policyId}_{enumCounter}");
                AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = enumStringResourceId, Value = val.Key });

                if (type == EnumerationElementType.Decimal)
                {
                    itemList.Add(new EnumerationElementItem { displayName = GetStringRef(enumStringResourceId), value = new Value { Item = new ValueDecimal { value = val.Value } } });
                }
                else if (type == EnumerationElementType.String)
                {
                    itemList.Add(new EnumerationElementItem { displayName = GetStringRef(enumStringResourceId), value = new Value { Item = val.Key } });
                }
                else
                {
                    throw new ArgumentOutOfRangeException("type", "Unexpected enumeration element type");
                }

                enumCounter++;
            }
            element.item = itemList.ToArray();

            return element;
        }

        private void EnsureValidId(string newCategoryId)
        {
            Regex rx = new Regex(@"^[a-zA-Z0-9_]*$");
            if (!rx.IsMatch(newCategoryId))
            {
                throw new ArgumentException($"Invalid identifier: {newCategoryId}");
            }
        }

        private string GetResourceId(ResourceType resourceType, string resourceId)
        {
            if (resourceType == ResourceType.Policy) { return resourceId; }

            return $"{resourceId}_{resourceType}";
        }

        private string GetPresentationRef(string resourceId)
        {
            return $"$(presentation.{resourceId})";
        }

        private string GetStringRef(string resourceId)
        {
            return $"$(string.{resourceId})";
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

        private string ParseCategory(string categoryId, string category)
        {
            string lastCategoryId = string.Empty;

            var splitCategoryId = categoryId.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var splitCategory = category.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            if (splitCategoryId.Length != splitCategory.Length)
            {
                throw new ArgumentException("CategoryId and Category are misaligned");
            }

            for (int i = 0; i < splitCategoryId.Length; i++)
            {
                var newCategoryId = splitCategoryId[i];
                EnsureValidId(newCategoryId);

                var categoryStringResourceId = GetResourceId(ResourceType.Category, newCategoryId);

                AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = categoryStringResourceId, Value = splitCategory[i] });
                var theNewCategory = new Category
                {
                    name = newCategoryId,
                    displayName = GetStringRef(categoryStringResourceId)
                };
                if (!string.IsNullOrEmpty(lastCategoryId))
                {
                    theNewCategory.parentCategory = new CategoryReference { @ref = lastCategoryId };
                }
                AddUniqueArrayItem(c => Definitions.categories.category = c, Definitions.categories.category, theNewCategory);

                lastCategoryId = newCategoryId;
            }

            return lastCategoryId;
        }

        private void ParseLine(CsvReader csvReader)
        {
            var categoryId = csvReader["CategoryId"];
            var category = csvReader["Category"];
            var policyId = csvReader["Id"];
            var displayName = csvReader["Display Name"];
            var @class = csvReader["Class"];
            var type = csvReader["Type"];
            var explanation = csvReader["Explanation"];
            var registryKey = csvReader["Registry Key"];
            var valueName = csvReader["Value Name"];
            var supportedOnId = csvReader["SupportedOnId"];
            var supportedOn = csvReader["Supported On"];
            var properties = csvReader["Properties"];

            try
            {
                EnsureValidId(policyId);
                EnsureValidId(supportedOnId);

                var displayResId = GetResourceId(ResourceType.Display, policyId);
                AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = displayResId, Value = displayName });
                var displayStringRef = GetStringRef(displayResId);

                var explainResId = GetResourceId(ResourceType.Explain, policyId);
                AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = explainResId, Value = explanation });
                var explainStringRef = GetStringRef(explainResId);

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
                var presentationStringRef = GetPresentationRef(parseTypeResults.Presentation.id);

                var policy = new PolicyDefinition
                {
                    name = policyId,
                    parentCategory = new CategoryReference { @ref = ParseCategory(categoryId, category) },
                    displayName = displayStringRef,
                    @class = (PolicyClass)Enum.Parse(typeof(PolicyClass), @class),
                    explainText = explainStringRef,
                    key = registryKey,
                    supportedOn = new SupportedOnReference { @ref = ParseSupportedOn(supportedOnId, supportedOn) },
                    elements = parseTypeResults.Elements,
                    presentation = presentationStringRef
                };
                if (parseTypeResults.Elements == null)
                {
                    policy.valueName = valueName;
                }
                AddUniqueArrayItem(c => Definitions.policies.policy = c, Definitions.policies.policy, policy);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing {category}\\{displayName}", ex);
            }
        }

        private string ParseSupportedOn(string supportedOnId, string supportedOn)
        {
            var id = GetResourceId(ResourceType.Definition, supportedOnId);
            AddUniqueArrayItem(c => Resources.resources.stringTable.@string = c, Resources.resources.stringTable.@string, new LocalizedString { id = id, Value = supportedOn });
            AddUniqueArrayItem(c => Definitions.supportedOn.definitions = c, Definitions.supportedOn.definitions, new SupportedOnDefinition { name = supportedOnId, displayName = GetStringRef(id) });

            return supportedOnId;
        }

        private ParseTypeResult ParseType(string policyId, string type, string key, string valueName, IDictionary<string, string> properties)
        {
            string defaultValue;
            var result = new ParseTypeResult();
            switch (type)
            {
                case "enum":
                    {
                        var enumerationElementType = (EnumerationElementType)Enum.Parse(typeof(EnumerationElementType), properties["Type"]);
                        var valuesList = new Dictionary<string, uint>();
                        foreach (var v in properties["Values"].Split(new[] { '|' }))
                        {
                            var vv = v.Split(new[] { ':' });
                            valuesList.Add(vv[0], vv.Length > 1 ? uint.Parse(vv[1]) : 0);
                        }
                        result.Elements = new object[] { CreateEnumElement(policyId, key, valueName, enumerationElementType, valuesList) };
                        result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new DropdownList { refId = policyId } } };
                    }
                    break;

                case "checkBox":
                case "enabledCheckBox":
                    result.Elements = new object[] {
                        new BooleanElement {
                            id = policyId,
                            key = key,
                            valueName = valueName,
                            trueValue = new Value { Item = new ValueDecimal { value = 1 } },
                            falseValue = new Value { Item = new ValueDecimal { value = 0 } }
                        }
                    };
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new CheckBox { refId = policyId, Value = properties["Label"] } } };
                    break;

                case "textBox":
                    result.Elements = new object[] {
                        new TextElement {
                            id = policyId,
                            key = key,
                            valueName = valueName,
                            required = true
                        }
                    };
                    properties.TryGetValue("Default", out defaultValue);
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new TextBox { refId = policyId, label = properties["Label"], defaultValue = defaultValue } } };
                    break;

                case "decimal":
                    string minValue;
                    properties.TryGetValue("MinValue", out minValue);
                    string maxValue;
                    properties.TryGetValue("MaxValue", out maxValue);
                    properties.TryGetValue("Default", out defaultValue);
                    result.Elements = new object[] {
                        new DecimalElement { 
                            id = policyId,
                            key = key,
                            valueName = valueName,
                            minValue = uint.Parse(minValue),
                            maxValue = uint.Parse(maxValue),
                            required = true
                        }
                    };
                    result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new DecimalTextBox { refId = policyId, Value = properties["Label"], defaultValue = uint.Parse(defaultValue) } } };
                    break;

                case "enabled":
                    result.Presentation = new PolicyPresentation { id = policyId };
                    break;

                case "boolean":
                case "enabledDropDown":
                    {
                        var enumerationElementType = EnumerationElementType.Decimal;
                        var valuesList = new Dictionary<string, uint>
                        {
                            { "Disabled", 0 },
                            { "Enabled", 1 }
                        };
                        result.Elements = new object[] { CreateEnumElement(policyId, key, valueName, enumerationElementType, valuesList) };
                        result.Presentation = new PolicyPresentation { id = policyId, Items = new object[] { new DropdownList { refId = policyId } } };
                    }
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
