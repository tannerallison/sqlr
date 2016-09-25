using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SQLr.Tests
{
    [TestFixture]
    public class ScriptDirectoryTests
    {
        private const int waitPeriod = 100;

        private static int _testNumber = 1234;

        private string _directory;

        [Test]
        public void Added_Script_File_Automatically_Drawn_Into_ScriptDirectory()
        {
            var scriptDirectory = new ScriptDirectory(_directory, false);

            var filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            Thread.Sleep(waitPeriod);

            Assert.That(scriptDirectory.Scripts.Count, Is.EqualTo(1));
        }

        [Test]
        public void Changed_Script_Files_Automatically_Get_Updated_In_ScriptDirectory()
        {
            var filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile1.sql");
            var warning = "A Warning Message";
            File.WriteAllText(filePath, $@"{{{{Warning={warning}}}}}");

            var scriptDirectory = new ScriptDirectory(_directory, false);

            Assume.That(scriptDirectory.Scripts.First().GetWarning(), Is.EqualTo(warning));

            var newWarning = "A New Warning Message";
            File.WriteAllText(filePath, $@"{{{{Warning={newWarning}}}}}");

            Thread.Sleep(waitPeriod);

            Assert.That(scriptDirectory.Scripts.First().GetWarning(), Is.EqualTo(newWarning));
        }

        [Test]
        public void Creating_ScriptDirectory_Draws_In_All_Script_Files()
        {
            var filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");
            filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");
            filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile3.sql");
            File.WriteAllText(filePath, "A Test File 3");

            var scriptDirectory = new ScriptDirectory(_directory, false);

            Assert.That(scriptDirectory.Scripts.Count, Is.EqualTo(3));
        }

        [Test]
        public void Creating_ScriptDirectory_With_SubDirectories_Draws_In_All_Files()
        {
            var subDirectoryA = Path.Combine(_directory, "SubDirectoryA");
            Directory.CreateDirectory(subDirectoryA);

            var subDirectoryB = Path.Combine(_directory, "SubDirectoryB");
            Directory.CreateDirectory(subDirectoryB);

            var filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            filePath = Path.Combine(subDirectoryA, $"_{++_testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");

            filePath = Path.Combine(subDirectoryB, $"_{++_testNumber}_TestFile3.sql");
            File.WriteAllText(filePath, "A Test File 3");

            var scriptDirectory = new ScriptDirectory(_directory, true);

            Assert.That(scriptDirectory.Scripts.Count, Is.EqualTo(3));

            Directory.Delete(subDirectoryA, true);
            Directory.Delete(subDirectoryB, true);
        }

        [Test]
        public void Files_With_Same_Name_In_Different_SubDirectories_Selected_By_Depth_Then_Alpha()
        {
            var subDirectoryA = Path.Combine(_directory, "SubDirectoryA");
            Directory.CreateDirectory(subDirectoryA);

            var subDirectoryB = Path.Combine(_directory, "SubDirectoryB");
            Directory.CreateDirectory(subDirectoryB);

            var filePath = Path.Combine(_directory, $"_{_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "Test File 1 - MainDirectory");

            filePath = Path.Combine(subDirectoryA, $"_{_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "Test File 1 - SubDirectoryA");

            _testNumber++;

            filePath = Path.Combine(subDirectoryA, $"_{_testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "Test File 2 - SubDirectoryA");

            filePath = Path.Combine(subDirectoryB, $"_{_testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "Test File 2 - SubDirectoryB");

            var scriptDirectory = new ScriptDirectory(_directory, true);

            Assert.That(scriptDirectory.Scripts.Count, Is.EqualTo(2), "Only two files should be included since they have the same name");

            Directory.Delete(subDirectoryA, true);
            Directory.Delete(subDirectoryB, true);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "ScriptDirectoryTestFolder");
            Directory.CreateDirectory(_directory);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(_directory, true);
        }

        [Test]
        public void Removed_Script_Files_Automatically_Removed_From_ScriptDirectory()
        {
            var filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");
            filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");

            var scriptDirectory = new ScriptDirectory(_directory, false);

            File.Delete(filePath);

            Thread.Sleep(waitPeriod);

            Assert.That(scriptDirectory.Scripts.Count, Is.EqualTo(1));
        }

        [Test]
        public void Renamed_Script_Files_Automatically_Get_Renamed_In_ScriptDirectory()
        {
            var filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");

            var scriptDirectory = new ScriptDirectory(_directory, false);

            var newNumber = ++_testNumber;
            var newName = "newTestName";
            var newFileName = Path.Combine(_directory, $"_{newNumber}_{newName}.sql");

            Thread.Sleep(25);

            File.Move(filePath, newFileName);

            Thread.Sleep(waitPeriod);

            Assert.That(scriptDirectory.Scripts.Count, Is.EqualTo(2));
            Assert.That(scriptDirectory.Scripts.FirstOrDefault(v => v.Name == newName), Is.Not.Null.And.Property("Ordinal").EqualTo(newNumber));
        }

        [SetUp]
        public void Setup()
        {
            var files = Directory.EnumerateFiles(_directory, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}