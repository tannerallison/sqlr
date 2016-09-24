using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace SQLr.Tests
{
    [TestFixture]
    public class ScriptTests
    {
        public class SettingTextUpdatesFields
        {
            private Script script;

            private string testVariable = "TestVar";
            private string testSubset = "TestSubset";
            private string warningMessage = "This is the message";
            private int timeout = 23;
            private string database = "TestDB";

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                script = new Script();

                string text = $"<<{testVariable}>> " +
                    $"{{{{Subset={testSubset}}}}} " +
                    $"{{{{Warning={warningMessage}}}}} " +
                    $"{{{{Timeout={timeout}}}}} " +
                    $"{{{{Database={database}}}}} ";

                script.Text = text;
            }

            [Test]
            public void Setting_Text_Updates_Variables()
            {
                Assert.That(script.Variables, Has.Member(testVariable));
            }

            [Test]
            public void Setting_Text_Updates_Subsets()
            {
                Assert.That(script.GetSubsets(), Has.Member(testSubset));
            }

            [Test]
            public void Setting_Text_Updates_Warning()
            {
                Assert.That(script.GetWarning(), Is.EqualTo(warningMessage));
            }

            [Test]
            public void Setting_Text_Updates_Timeout()
            {
                Assert.That(script.GetTimeout(), Is.EqualTo(timeout));
            }

            [Test]
            public void Setting_Text_Updates_Database()
            {
                Assert.That(script.GetDatabase(), Is.EqualTo(database));
            }
        }

        public class SettingFilePathUpdatesFields
        {
            private string _directory;
            private static long ord = 123;
            private string name = "FilePathTests";

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                _directory = Path.Combine(Path.GetTempPath(), "SettingFilePathUpdatesFieldTests");

                if (!Directory.Exists(_directory))
                    Directory.CreateDirectory(_directory);
            }

            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                Directory.Delete(_directory, true);
            }

            [Test]
            public void Setting_FilePath_Updates_Text()
            {
                string scriptText = "This is a test file";

                var filePath = Path.Combine(_directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(script.Text, Is.EqualTo(scriptText));
            }

            [Test]
            public void Setting_FilePath_Updates_Name()
            {
                string scriptText = "This is a test file";

                var filePath = Path.Combine(_directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(script.Name, Is.EqualTo(name));
            }

            [Test]
            public void Setting_FilePath_Updates_Ordinal()
            {
                string scriptText = "This is a test file";

                var filePath = Path.Combine(_directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(script.Ordinal, Is.EqualTo(ord));
            }

            [Test]
            public void Ordinal_Cannot_Be_Set_If_Script_Backed_By_File()
            {
                string scriptText = "This is a test file";

                var filePath = Path.Combine(_directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(() => { script.Ordinal = 1; }, Throws.InvalidOperationException);
            }

            [Test]
            public void Name_Cannot_Be_Set_If_Script_Backed_By_File()
            {
                string scriptText = "This is a test file";

                var filePath = Path.Combine(_directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(() => { script.Name = "NewName"; }, Throws.InvalidOperationException);
            }
        }
    }
}