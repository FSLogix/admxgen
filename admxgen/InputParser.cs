using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;

namespace admxgen
{
    //class InputParser
    //{
    //    private TextReader _reader;

    //    public HashSet<PolicyCategory> Categories = new HashSet<PolicyCategory>();
    //    public List<Policy> Policies = new List<Policy>();

    //    public InputParser(TextReader reader)
    //    {
    //        this._reader = reader;
    //    }

    //    public void Parse()
    //    {
    //        using (var csvReader = new CsvReader(_reader, true))
    //        {
    //            var csvHeaders = csvReader.GetFieldHeaders();
    //            while (csvReader.ReadNextRecord())
    //            {
    //                ParseLine(csvReader);
    //            }
    //        }
    //    }

    //    private void ParseLine(CsvReader csvReader)
    //    {
    //        var category = csvReader["Category"];
    //        var displayName = csvReader["Display Name"];
    //        var _class = csvReader["Class"];
    //        var type = csvReader["Type"];
    //        var explanation = csvReader["Explanation"];
    //        var registryKey = csvReader["Registry Key"];
    //        var valueName = csvReader["Value Name"];
    //        //var possibleValues = csvReader["Possible Values"];
    //        //var supportedOn = csvReader["Supported On"];

    //        var categoryObj = new PolicyCategory { Name = category };
    //        Categories.Add(categoryObj);
    //        Policies.Add(new Policy {
    //            Category = categoryObj,
    //            Class = (PolicyClass) Enum.Parse(typeof(PolicyClass), _class),
    //            DisplayName = displayName,
    //            Explanation = explanation,
    //            RegistryKey = registryKey,
    //            Type = (PolicyType)Enum.Parse(typeof(PolicyType), type),
    //            ValueName = valueName
    //        });
    //    }
    //}

    ////enum PolicyClass { Machine }

    ////enum PolicyType { checkBox }

    ////class PolicyCategory
    ////{
    ////    public string Name { get; set; }

    ////    public override int GetHashCode()
    ////    {
    ////        return Name.GetHashCode();
    ////    }

    ////    public override bool Equals(object obj)
    ////    {
    ////        var c = obj as PolicyCategory;
    ////        if (c == null) return false;
    ////        return Name.Equals(c.Name);
    ////    }
    ////}

    ////class Policy
    ////{
    ////    public PolicyCategory Category { get; set; }
    ////    public PolicyClass Class { get; set; }
    ////    public string DisplayName { get; set; }
    ////    public string Explanation { get; set; }
    ////    public string RegistryKey { get; set; }
    ////    public PolicyType Type { get; set; }
    ////    public string ValueName { get; set; }

    ////    public override int GetHashCode()
    ////    {
    ////        return HashCodeHelper.CombineHashCodes(Category, Class, DisplayName, Explanation, RegistryKey, Type, ValueName);
    ////    }

    ////    public override bool Equals(object obj)
    ////    {
    ////        var p = obj as Policy;
    ////        if (p == null) return false;
    ////        return
    ////            Category.Equals(p.Category) &&
    ////            Class.Equals(p.Class) &&
    ////            DisplayName.Equals(p.DisplayName) &&
    ////            Explanation.Equals(p.Explanation) &&
    ////            RegistryKey.Equals(p.RegistryKey) &&
    ////            Type.Equals(p.Type) &&
    ////            ValueName.Equals(p.ValueName);
    ////    }
    ////}
}
