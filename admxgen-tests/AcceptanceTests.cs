using admxgen;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace admxgen_tests
{
    class AcceptanceTests
    {
        public void AssertFilesAreSame(string expectedDataFilename, string actualDataFilename)
        {
            var contents1 = File.ReadAllLines(expectedDataFilename);
            var contents2 = File.ReadAllLines(actualDataFilename);

            Assert.That(contents1.Length, Is.EqualTo(contents2.Length));

            for (int i = 0; i < contents1.Length; ++i)
            {
                Assert.That(contents1[i], Is.EqualTo(contents2[i]), "Files differ on line {0}", i);
            }
        }

        [TestCase("test-data\\checkBoxTest.csv", "test-data\\checkBoxTest-expected")]
        [TestCase("test-data\\textBoxTest.csv", "test-data\\textBoxTest-expected")]
        [TestCase("test-data\\enumTest.csv", "test-data\\enumTest-expected")]
        [TestCase("test-data\\decimalTest.csv", "test-data\\decimalTest-expected")]
        public void InputFileGeneratesCorrectOutputFile(string input, string expectedOutput)
        {
            var testdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(testdir);

            if (File.Exists("actual.admx")) File.Delete("actual.admx");
            if (File.Exists("actual.adml")) File.Delete("actual.adml");

            var outputStringBuilder = new StringBuilder();
            var exe = "admxgen.exe";
            var args = string.Format("{0} actual", input);
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = args,
                    FileName = exe,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            p.OutputDataReceived += (sender, eventArgs) => outputStringBuilder.Append(eventArgs.Data);
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();
            Assert.That(p.ExitCode, Is.EqualTo(0), outputStringBuilder.ToString());

            if (CopyIf(expectedOutput + ".admx", "actual.admx") ||
                CopyIf(expectedOutput + ".adml", "actual.adml"))
            {
                Assert.Fail("Expected output files missing");
            }

            // Compare
            AssertFilesAreSame(expectedOutput + ".admx", "actual.admx");
            AssertFilesAreSame(expectedOutput + ".adml", "actual.adml");
        }

        private static bool CopyIf(string expectedOutputFilename, string actualFilename)
        {
            if (!File.Exists(expectedOutputFilename))
            {
                File.Copy(actualFilename, expectedOutputFilename);
                return true;
            }
            return false;
        }
    }
}
