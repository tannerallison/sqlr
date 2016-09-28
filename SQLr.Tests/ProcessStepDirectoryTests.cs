// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr.Tests - ProcessStepDirectoryTests.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
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
    using System.Threading;
    using NUnit.Framework;

    #endregion

    [TestFixture]
    public class ProcessStepDirectoryTests
    {
        private const int WaitPeriod = 100;

        private static int testNumber = 1234;

        private string directory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "ScriptDirectoryTestFolder");
            Directory.CreateDirectory(directory);
        }

        [SetUp]
        public void Setup()
        {
            var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
                File.Delete(file);
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
        public void AddedScriptFileAutomaticallyDrawnIntoScriptDirectory()
        {
            var scriptDirectory = new ProcessStepDirectory(directory, "*.sql", false);

            var filePath = Path.Combine(directory, $"_{++testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            Thread.Sleep(WaitPeriod);

            Assert.That(scriptDirectory.Steps.Count, Is.EqualTo(1));
        }

        [Test]
        public void ChangedScriptFilesAutomaticallyGetUpdatedInScriptDirectory()
        {
            var filePath = Path.Combine(directory, $"_{++testNumber}_TestFile1.sql");
            var warning = "A Warning Message";
            File.WriteAllText(filePath, $@"{{{{Warning={warning}}}}}");

            var scriptDirectory = new ProcessStepDirectory(directory, "*.sql", false);

            Assume.That(scriptDirectory.Steps.OfType<Script>().First().GetWarning(), Is.EqualTo(warning));

            var newWarning = "A New Warning Message";
            File.WriteAllText(filePath, $@"{{{{Warning={newWarning}}}}}");

            Thread.Sleep(WaitPeriod);

            Assert.That(scriptDirectory.Steps.OfType<Script>().First().GetWarning(), Is.EqualTo(newWarning));
        }

        [Test]
        public void CreatingScriptDirectoryDrawsInAllScriptFiles()
        {
            var filePath = Path.Combine(directory, $"_{++testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");
            filePath = Path.Combine(directory, $"_{++testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");
            filePath = Path.Combine(directory, $"_{++testNumber}_TestFile3.sql");
            File.WriteAllText(filePath, "A Test File 3");

            var scriptDirectory = new ProcessStepDirectory(directory, "*.sql", false);

            Assert.That(scriptDirectory.Steps.Count, Is.EqualTo(3));
        }

        [Test]
        public void CreatingScriptDirectoryWithSubDirectoriesDrawsInAllFiles()
        {
            var subDirectoryA = Path.Combine(directory, "SubDirectoryA");
            Directory.CreateDirectory(subDirectoryA);

            var subDirectoryB = Path.Combine(directory, "SubDirectoryB");
            Directory.CreateDirectory(subDirectoryB);

            var filePath = Path.Combine(directory, $"_{++testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            filePath = Path.Combine(subDirectoryA, $"_{++testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");

            filePath = Path.Combine(subDirectoryB, $"_{++testNumber}_TestFile3.sql");
            File.WriteAllText(filePath, "A Test File 3");

            var scriptDirectory = new ProcessStepDirectory(directory, "*.sql", true);

            Assert.That(scriptDirectory.Steps.Count, Is.EqualTo(3));

            Directory.Delete(subDirectoryA, true);
            Directory.Delete(subDirectoryB, true);
        }

        [Test]
        public void FilesWithSameNameInDifferentSubDirectoriesSelectedByDepthThenAlpha()
        {
            var subDirectoryA = Path.Combine(directory, "SubDirectoryA");
            Directory.CreateDirectory(subDirectoryA);

            var subDirectoryB = Path.Combine(directory, "SubDirectoryB");
            Directory.CreateDirectory(subDirectoryB);

            var filePath = Path.Combine(directory, $"_{testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "Test File 1 - MainDirectory");

            filePath = Path.Combine(subDirectoryA, $"_{testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "Test File 1 - SubDirectoryA");

            testNumber++;

            filePath = Path.Combine(subDirectoryA, $"_{testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "Test File 2 - SubDirectoryA");

            filePath = Path.Combine(subDirectoryB, $"_{testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "Test File 2 - SubDirectoryB");

            var scriptDirectory = new ProcessStepDirectory(directory, "*.sql", true);

            Assert.That(
                scriptDirectory.Steps.Count,
                Is.EqualTo(2),
                "Only two files should be included since they have the same name");

            Directory.Delete(subDirectoryA, true);
            Directory.Delete(subDirectoryB, true);
        }

        [Test]
        public void RemovedScriptFilesAutomaticallyRemovedFromScriptDirectory()
        {
            var filePath = Path.Combine(directory, $"_{++testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");
            filePath = Path.Combine(directory, $"_{++testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");

            var scriptDirectory = new ProcessStepDirectory(directory, "*.sql", false);

            File.Delete(filePath);

            Thread.Sleep(WaitPeriod);

            Assert.That(scriptDirectory.Steps.Count, Is.EqualTo(1));
        }

        [Test]
        public void RenamedScriptFilesAutomaticallyGetRenamedInScriptDirectory()
        {
            var filePath = Path.Combine(directory, $"_{++testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            filePath = Path.Combine(directory, $"_{++testNumber}_TestFile2.sql");
            File.WriteAllText(filePath, "A Test File 2");

            var scriptDirectory = new ProcessStepDirectory(directory, "*.sql", false);

            var newNumber = ++testNumber;
            var newName = "newTestName";
            var newFileName = Path.Combine(directory, $"_{newNumber}_{newName}.sql");

            Thread.Sleep(25);

            File.Move(filePath, newFileName);

            Thread.Sleep(WaitPeriod);

            Assert.That(
                scriptDirectory.Steps.Count,
                Is.EqualTo(2),
                "Contents: {0}",
                scriptDirectory.Steps.Aggregate(string.Empty, (c, n) => c + n.FilePath + "; "));
            Assert.That(
                scriptDirectory.Steps.FirstOrDefault(v => v.Name == newName),
                Is.Not.Null.And.Property("Ordinal").EqualTo(newNumber));
        }
    }
}