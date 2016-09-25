using NUnit.Framework;
using SQLr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLr.Tests
{
    [TestFixture]
    public class ConversionProjectTests
    {
        private string _directory;
        private string _subDirectoryA;
        private string _subDirectoryB;
        private static long _testNumber = 1234;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "ConversionProjectTestFolder");
            Directory.CreateDirectory(_directory);

            _subDirectoryA = Path.Combine(_directory, "SubDirectoryA");
            Directory.CreateDirectory(_subDirectoryA);

            _subDirectoryB = Path.Combine(_directory, "SubDirectoryB");
            Directory.CreateDirectory(_subDirectoryB);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(_directory, true);
        }

        [TearDown]
        public void TearDown()
        {
            var files = Directory.EnumerateFiles(_directory, "*.*", SearchOption.AllDirectories);
            foreach (var f in files)
                File.Delete(f);
        }

        [Test]
        public void Get_Scripts_Returns_All_Scripts_From_Single_ScriptDirectory()
        {
            var filePath = Path.Combine(_directory, $"_{++_testNumber}_TestFile1.sql");
            File.WriteAllText(filePath, "A Test File 1");

            var scriptDir = new ScriptDirectory(_directory, false);

            var proj = new ConversionProject();
            proj.ScriptDirectories.Add(scriptDir);

            var scripts = proj.GetScripts();
            Assert.That(scripts, Has.Count.EqualTo(1));
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == filePath), Is.Not.Null);
        }

        [Test]
        public void Get_Scripts_Returns_All_Scripts_From_Multiple_ScriptDirectory()
        {
            var filePathA = Path.Combine(_subDirectoryA, $"_{++_testNumber}_TestFile1.sql");
            File.WriteAllText(filePathA, "Test File 1 - SubDirectoryA");

            var filePathB = Path.Combine(_subDirectoryB, $"_{++_testNumber}_TestFile2.sql");
            File.WriteAllText(filePathB, "Test File 2 - SubDirectoryB");

            var scriptDirA = new ScriptDirectory(_subDirectoryA, false);
            var scriptDirB = new ScriptDirectory(_subDirectoryB, false);

            var proj = new ConversionProject();
            proj.ScriptDirectories.Add(scriptDirA);
            proj.ScriptDirectories.Add(scriptDirB);

            var scripts = proj.GetScripts();

            Assert.That(scripts, Has.Count.EqualTo(2));
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == filePathA), Is.Not.Null);
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == filePathB), Is.Not.Null);
        }

        [Test]
        public void Get_Scripts_Serves_Only_First_Script_If_Multiples_Exist_In_Different_Directories()
        {
            var duplicateFileName = $"_{++_testNumber}_TestFile1.sql";
            var duplicateFileDirectoryA = Path.Combine(_subDirectoryA, duplicateFileName);
            File.WriteAllText(duplicateFileDirectoryA, "Test File 1 - SubDirectoryA");

            var duplicateFileDirectoryB = Path.Combine(_subDirectoryB, duplicateFileName);
            File.WriteAllText(duplicateFileDirectoryB, "Test File 1 - SubDirectoryB");

            var filePathB = Path.Combine(_subDirectoryB, $"_{++_testNumber}_TestFile2.sql");
            File.WriteAllText(filePathB, "Test File 2 - SubDirectoryB");

            var scriptDirA = new ScriptDirectory(_subDirectoryA, false);
            var scriptDirB = new ScriptDirectory(_subDirectoryB, false);

            Assume.That(scriptDirA.Scripts.Count, Is.EqualTo(1));
            Assume.That(scriptDirB.Scripts.Count, Is.EqualTo(2));

            var proj = new ConversionProject();
            proj.ScriptDirectories.Add(scriptDirA);
            proj.ScriptDirectories.Add(scriptDirB);

            var scripts = proj.GetScripts();

            Assert.That(scripts, Has.Count.EqualTo(2), "Only two files should be returned since two of the three are duplicates");
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == duplicateFileDirectoryA), Is.Not.Null);
            Assert.That(scripts.FirstOrDefault(v => v.FilePath == filePathB), Is.Not.Null);
        }
    }
}