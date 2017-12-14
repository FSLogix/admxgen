using admxgen;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace admxgen_tests
{
    class AcceptanceTests
    {
        public bool FilesAreSame(string filename1, string filename2)
        {
            var contents1 = File.ReadAllText(filename1);
            var contents2 = File.ReadAllText(filename2);
            return contents1.Equals(contents2);
        }

        [TestCase("test-data\\t1.csv", "test-data\\t1-expected")]
        public void InputFileGeneratesCorrectOutputFile(string input, string expectedOutput)
        {
            var testdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(testdir);

            if (File.Exists("actual.admx")) File.Delete("actual.admx");
            if (File.Exists("actual.adml")) File.Delete("actual.adml");

            var exe = "admxgen.exe";
            var args = string.Format("{0} actual", input);
            var p = Process.Start(new ProcessStartInfo{ Arguments = args, FileName = exe });
            p.WaitForExit();
            Assert.That(p.ExitCode, Is.EqualTo(0));

            // Compare
            Assert.That(FilesAreSame(expectedOutput + ".admx", "actual.admx"));
            Assert.That(FilesAreSame(expectedOutput + ".adml", "actual.adml"));
        }
    }
}
