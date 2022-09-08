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

            //Assert.That(contents1.Length, Is.EqualTo(contents2.Length));

            for (int i = 0; i < contents1.Length; ++i)
            {
                Assert.That(contents1[i], Is.EqualTo(contents2[i]), "Files differ on line {0}", i);
            }
        }

        [TestCase("checkBoxTest.json", "checkBoxTest")]
        [TestCase("textBoxTest.json", "textBoxTest")]
        [TestCase("enumTest.json", "enumTest")]
        [TestCase("decimalTest.json", "decimalTest")]
        [TestCase("enabledTest.json", "enabledTest")]
        public void InputFileGeneratesCorrectOutputFile(string input, string outputPrefix)
        {
            var testdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(Path.Combine(testdir, "test-data"));

            if (File.Exists(outputPrefix + "-actual.admx")) File.Delete(outputPrefix + "-actual.admx");
            if (File.Exists(outputPrefix + "-actual.adml")) File.Delete(outputPrefix + "-actual.adml");

            var outputStringBuilder = new StringBuilder();
            var exe = Path.Combine(testdir, "admxgen.exe");
            var args = string.Format("{0} {1}-actual", input, outputPrefix);
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

            // Copy the actual generated file to the output file if there is no expected output file (and fail the test)
            if (CopyIf(outputPrefix + "-expected.admx", outputPrefix + "-actual.admx") ||
                CopyIf(outputPrefix + "-expected.adml", outputPrefix + "-actual.adml"))
            {
                Assert.Fail("Expected output files missing");
            }

            // Compare
            AssertFilesAreSame(outputPrefix + "-expected.admx", outputPrefix + "-actual.admx");
            AssertFilesAreSame(outputPrefix + "-expected.adml", outputPrefix + "-actual.adml");
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
