using NUnit.Framework;
using System.IO;

namespace admxgen
{
    class InputParserTests
    {
        //[TestCase]
        //public void ShouldParseCategory()
        //{
        //    var reader = new StringReader(
        //        "\"Category\",\"Display Name\",\"Class\",\"Type\",\"Explanation\",\"Registry Key\",\"Value Name\",\"Possible Values\",\"Supported On\"\r\n" +
        //        "\"Profile Containers\",\"Enabled\",\"Machine\",\"checkBox\",\"Controls whether or not the Profiles feature is active.\",\"Software\\Policies\\FSLogix\\ODFC\",\"Enabled\",\"\",\"FSLogix Office 365 Containers 2.5\"");
        //    var inputParser = new InputParser(reader);
        //    inputParser.Parse();
        //    Assert.That(inputParser.Policies.Count, Is.EqualTo(1));
        //    CollectionAssert.Contains(inputParser.Policies, new Policy { DisplayName = "Enabled", Category = new PolicyCategory { Name = "Profile Containers" } });
        //}
    }
}
