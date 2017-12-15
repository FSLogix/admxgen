using admxgen;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace admxgen_tests
{
    class AcceptanceTests
    {
        public void AssertFilesAreSame(string filename1, string filename2)
        {
            var contents1 = File.ReadAllLines(filename1);
            var contents2 = File.ReadAllLines(filename2);

            Assert.That(contents1.Length, Is.EqualTo(contents2.Length));

            for (int i = 0; i < contents1.Length; ++i)
            {
                Assert.That(contents1[i], Is.EqualTo(contents2[i]), "Files differ on line {0}", i);
            }
        }

        [TestCase("test-data\\checkBoxTest.csv", "test-data\\checkBoxTest-expected")]
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
            AssertFilesAreSame(expectedOutput + ".admx", "actual.admx");
            AssertFilesAreSame(expectedOutput + ".adml", "actual.adml");
        }
    }
}
