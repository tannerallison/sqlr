// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr.Tests - ConversionProjectTests.cs
// <Author></Author>
// <CreatedDate>2016-09-24</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr.Tests
{
    #region using

    using System;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    #endregion

    [TestFixture]
    public class ConversionProjectTests
    {
        private static long testNumber = 1234;
        private string directory;
        private string subDirectoryA;
        private string subDirectoryB;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "ConversionProjectTestFolder");
            Directory.CreateDirectory(directory);

            subDirectoryA = Path.Combine(directory, "SubDirectoryA");
            Directory.CreateDirectory(subDirectoryA);

            subDirectoryB = Path.Combine(directory, "SubDirectoryB");
            Directory.CreateDirectory(subDirectoryB);
        }

        [TearDown]
        public void TearDown()
        {
            var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
            foreach (var f in files)
                File.Delete(f);
        }

        [OneTimeTearDown]
        [Timeout(5000)]
        public void OneTimeTearDown()
        {
            while (Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        [Test]
        public void Get_Scripts_Returns_All_Scripts_From_Multiple_Script_Directory()
        {
            var filePathA = Path.Combine(subDirectoryA, $"_{++testNumber}_TestFile1.sql");
            File.WriteAllText(filePathA, "Test File 1 - SubDirectoryA");

            var filePathB = Path.Combine(subDirectoryB, $"_{++testNumber}_TestFile2.sql");
            File.WriteAllText(filePathB, "Test File 2 - SubDirectoryB");

            var scriptDirA = new ProcessStepDirectory(subDirectoryA, "*.sql", false);
            var scriptDirB = new ProcessStepDirectory(subDirectoryB, "*.sql", false);

            var proj = new ConversionProject();
            proj.ScriptDirectories.Add(scriptDirA);
            proj.ScriptDirectories.Add(scriptDirB);

            var scripts = proj.GetScripts();

            Assert.That(scripts, Has.Count.EqualTo(2));
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == filePathA), Is.Not.Null);
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == filePathB), Is.Not.Null);
        }

        [Test]
        public void Get_Scripts_Returns_All_Scripts_From_Single_Script_Directory()
        {
            var filePath = Path.Combine(directory, $"_{++testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            var scriptDir = new ProcessStepDirectory(directory, "*.sql", false);

            var proj = new ConversionProject();
            proj.ScriptDirectories.Add(scriptDir);

            var scripts = proj.GetScripts();
            Assert.That(scripts, Has.Count.EqualTo(1));
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == filePath), Is.Not.Null);
        }

        [Test]
        public void Get_Scripts_Serves_Only_Last_Script_If_Multiples_Exist_In_Different_Directories()
        {
            var duplicateName = "TestFile1";
            var duplicateFileName = $"_{++testNumber}_{duplicateName}.sql";
            var duplicateFileDirectoryA = Path.Combine(subDirectoryA, duplicateFileName);
            File.WriteAllText(duplicateFileDirectoryA, "Test File 1 - SubDirectoryA");

            var duplicateFileDirectoryB = Path.Combine(subDirectoryB, duplicateFileName);
            File.WriteAllText(duplicateFileDirectoryB, "Test File 1 - SubDirectoryB");

            var filePathB = Path.Combine(subDirectoryB, $"_{++testNumber}_TestFile2.sql");
            File.WriteAllText(filePathB, "Test File 2 - SubDirectoryB");

            var scriptDirA = new ProcessStepDirectory(subDirectoryA, "*.sql", false);
            var scriptDirB = new ProcessStepDirectory(subDirectoryB, "*.sql", false);

            Assert.That(scriptDirA.Steps.Count, Is.EqualTo(1), "ScriptDirA should have one file");
            Assert.That(scriptDirB.Steps.Count, Is.EqualTo(2), "ScriptDirB should have two file");

            var proj = new ConversionProject();
            proj.ScriptDirectories.Add(scriptDirA);
            proj.ScriptDirectories.Add(scriptDirB);

            var scripts = proj.GetScripts();

            Assert.That(
                scripts,
                Has.Count.EqualTo(2),
                "Only two files should be returned since two of the three are duplicates");

            var scriptA = scripts.First(v => v.Name == duplicateName);
            Assert.That(
                scriptA.FilePath,
                Is.EqualTo(duplicateFileDirectoryB),
                "The duplicate file should be the copy found in the last script directory");
        }
    }
}