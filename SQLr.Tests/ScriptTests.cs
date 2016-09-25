using NUnit.Framework;
using SQLr;
using System.Collections.Generic;
using System.IO;

namespace SQLr.Tests
{
    [TestFixture]
    public class ScriptTests
    {
        [Test]
        public void Ordinal_has_max_value_when_initialized()
        {
            var script = new Script();

            Assert.That(script.Ordinal, Is.EqualTo(long.MaxValue));
        }

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

                Assert.That(script.GetText(), Is.EqualTo(scriptText));
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

        [Test]
        public void Get_Text_Returns_Null_When_First_Initialized()
        {
            var script = new Script();

            Assert.That(script.GetText(), Is.Null);
        }

        [Test]
        public void Get_Text_Returns_Mapped_Value_When_Passed_Mapping()
        {
            var script = new Script();
            script.Text = @"This text has a <<Variable>>";

            var expectedMessage = "Pass";
            var varMap = new Dictionary<string, string>() { { "Variable", expectedMessage } };

            Assert.That(script.GetText(varMap), Is.EqualTo("This text has a Pass"));
        }

        [Test]
        public void Get_Text_Throws_Error_If_A_Variable_Is_Missing()
        {
            var script = new Script();
            script.Text = @"This text has a <<Variable>> and also a <<MissingVariable>>";

            var varMap = new Dictionary<string, string>() { { "Variable", "var" } };

            Assert.That(() => { script.GetText(varMap); }, Throws.ArgumentException);
        }

        [Test]
        public void Get_Subsets_Returns_Null_When_First_Initialized()
        {
            var script = new Script();

            Assert.That(script.GetSubsets(), Is.Not.Null.And.Empty);
        }

        [Test]
        public void Get_Subsets_Returns_Mapped_Value_When_Passed_Mapping()
        {
            var script = new Script();
            script.Text = @"{{Subset=<<SubsetVariable>>}}";

            var expectedMessage = "SpecialSubset";
            var varMap = new Dictionary<string, string>() { { "SubsetVariable", expectedMessage } };

            Assert.That(script.GetSubsets(varMap), Has.Member(expectedMessage));
        }

        [Test]
        public void Get_Warning_Returns_Null_When_First_Initialized()
        {
            var script = new Script();

            Assert.That(script.GetWarning(), Is.Null);
        }

        [Test]
        public void Get_Warning_Returns_Mapped_Value_When_Passed_Mapping()
        {
            var script = new Script();
            script.Text = @"{{Warning=<<WarnVariable>>}}";

            var expectedMessage = "This is a warning";
            var varMap = new Dictionary<string, string>() { { "WarnVariable", expectedMessage } };

            Assert.That(script.GetWarning(varMap), Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Get_Database_Returns_Null_When_First_Initialized()
        {
            var script = new Script();

            Assert.That(script.GetDatabase(), Is.Null);
        }

        [Test]
        public void Get_Database_Returns_Mapped_Value_When_Passed_Mapping()
        {
            var script = new Script();
            script.Text = @"{{Database=<<DatabaseVariable>>}}";

            var expectedMessage = "NewDatabase";
            var varMap = new Dictionary<string, string>() { { "DatabaseVariable", expectedMessage } };

            Assert.That(script.GetDatabase(varMap), Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Get_Timeout_Returns_6000_When_First_Initialized()
        {
            var script = new Script();

            Assert.That(script.GetTimeout(), Is.EqualTo(6000));
        }

        [Test]
        public void Get_Timeout_Returns_Mapped_Value_When_Passed_Mapping()
        {
            var script = new Script();
            script.Text = @"{{Timeout=<<TimeoutVariable>>}}";

            var expectedMessage = "12345";
            var varMap = new Dictionary<string, string>() { { "TimeoutVariable", expectedMessage } };

            Assert.That(script.GetTimeout(varMap).ToString(), Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Get_Timeout_Returns_6000_When_Timeout_Value_Is_Invalid()
        {
            var script = new Script();
            script.Text = @"{{Timeout=Invalid23}}";

            Assert.That(script.GetTimeout(), Is.EqualTo(6000));
        }
    }
}