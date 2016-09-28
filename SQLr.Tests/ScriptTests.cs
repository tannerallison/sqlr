// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr.Tests - ScriptTests.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr.Tests
{
    #region using

    using System.Collections.Generic;
    using System.IO;
    using NUnit.Framework;

    #endregion

    [TestFixture]
    public class ScriptTests
    {
        public class SettingTextUpdatesFields
        {
            private const string Database = "TestDB";
            private const string TestSubset = "TestSubset";
            private const string TestVariable = "TestVar";
            private const int Timeout = 23;
            private const string WarningMessage = "This is the message";
            private Script script;

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                script = new Script();

                var text = $"<<{TestVariable}>> " + $"{{{{Subset={TestSubset}}}}} "
                           + $"{{{{Warning={WarningMessage}}}}} " + $"{{{{Timeout={Timeout}}}}} "
                           + $"{{{{Database={Database}}}}} ";

                script.Text = text;
            }

            [Test]
            public void SettingTextUpdatesDatabase()
            {
                Assert.That(script.GetDatabase(), Is.EqualTo(Database));
            }

            [Test]
            public void SettingTextUpdatesSubsets()
            {
                Assert.That(script.GetSubsets(), Has.Member(TestSubset));
            }

            [Test]
            public void SettingTextUpdatesTimeout()
            {
                Assert.That(script.GetTimeout(), Is.EqualTo(Timeout));
            }

            [Test]
            public void SettingTextUpdatesVariables()
            {
                Assert.That(script.Variables, Has.Member(TestVariable));
            }

            [Test]
            public void SettingTextUpdatesWarning()
            {
                Assert.That(script.GetWarning(), Is.EqualTo(WarningMessage));
            }
        }

        public class SettingFilePathUpdatesFields
        {
            private static long ord = 123;
            private readonly string name = "FilePathTests";
            private string directory;

            [Test]
            public void NameCannotBeSetIfScriptBackedByFile()
            {
                var scriptText = "This is a test file";

                var filePath = Path.Combine(directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(() => { script.Name = "NewName"; }, Throws.InvalidOperationException);
            }

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                directory = Path.Combine(Path.GetTempPath(), "SettingFilePathUpdatesFieldTests");

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                Directory.Delete(directory, true);
            }

            [Test]
            public void OrdinalCannotBeSetIfScriptBackedByFile()
            {
                var scriptText = "This is a test file";

                var filePath = Path.Combine(directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(() => { script.Ordinal = 1; }, Throws.InvalidOperationException);
            }

            [Test]
            public void SettingFilePathUpdatesName()
            {
                var scriptText = "This is a test file";

                var filePath = Path.Combine(directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(script.Name, Is.EqualTo(name));
            }

            [Test]
            public void SettingFilePathUpdatesOrdinal()
            {
                var scriptText = "This is a test file";

                var filePath = Path.Combine(directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(script.Ordinal, Is.EqualTo(ord));
            }

            [Test]
            public void SettingFilePathUpdatesText()
            {
                var scriptText = "This is a test file";

                var filePath = Path.Combine(directory, $"_{++ord}_{name}.sql");
                File.WriteAllText(filePath, scriptText);
                var script = new Script();
                script.FilePath = filePath;

                Assert.That(script.GetText(), Is.EqualTo(scriptText));
            }
        }

        [Test]
        public void GetDatabaseReturnsMappedValueWhenPassedMapping()
        {
            var script = new Script();
            script.Text = @"{{Database=<<DatabaseVariable>>}}";

            var expectedMessage = "NewDatabase";
            var varMap = new Dictionary<string, string> { { "DatabaseVariable", expectedMessage } };

            Assert.That(script.GetDatabase(varMap), Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GetDatabaseReturnsNullWhenFirstInitialized()
        {
            var script = new Script();

            Assert.That(script.GetDatabase(), Is.Null);
        }

        [Test]
        public void GetSubsetsReturnsMappedValueWhenPassedMapping()
        {
            var script = new Script();
            script.Text = @"{{Subset=<<SubsetVariable>>}}";

            var expectedMessage = "SpecialSubset";
            var varMap = new Dictionary<string, string> { { "SubsetVariable", expectedMessage } };

            Assert.That(script.GetSubsets(varMap), Has.Member(expectedMessage));
        }

        [Test]
        public void GetSubsetsReturnsNullWhenFirstInitialized()
        {
            var script = new Script();

            Assert.That(script.GetSubsets(), Is.Not.Null.And.Empty);
        }

        [Test]
        public void GetTextReturnsMappedValueWhenPassedMapping()
        {
            var script = new Script();
            script.Text = @"This text has a <<Variable>>";

            var expectedMessage = "Pass";
            var varMap = new Dictionary<string, string> { { "Variable", expectedMessage } };

            Assert.That(script.GetText(varMap), Is.EqualTo("This text has a Pass"));
        }

        [Test]
        public void GetTextReturnsNullWhenFirstInitialized()
        {
            var script = new Script();

            Assert.That(script.GetText(), Is.Null);
        }

        [Test]
        public void GetTextThrowsErrorIfAVariableIsMissing()
        {
            var script = new Script();
            script.Text = @"This text has a <<Variable>> and also a <<MissingVariable>>";

            var varMap = new Dictionary<string, string> { { "Variable", "var" } };

            Assert.That(() => { script.GetText(varMap); }, Throws.ArgumentException);
        }

        [Test]
        public void GetTimeoutReturns6000WhenFirstInitialized()
        {
            var script = new Script();

            Assert.That(script.GetTimeout(), Is.EqualTo(6000));
        }

        [Test]
        public void GetTimeoutReturns6000WhenTimeoutValueIsInvalid()
        {
            var script = new Script();
            script.Text = @"{{Timeout=Invalid23}}";

            Assert.That(script.GetTimeout(), Is.EqualTo(6000));
        }

        [Test]
        public void GetTimeoutReturnsMappedValueWhenPassedMapping()
        {
            var script = new Script();
            script.Text = @"{{Timeout=<<TimeoutVariable>>}}";

            var expectedMessage = "12345";
            var varMap = new Dictionary<string, string> { { "TimeoutVariable", expectedMessage } };

            Assert.That(script.GetTimeout(varMap).ToString(), Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GetWarningReturnsMappedValueWhenPassedMapping()
        {
            var script = new Script();
            script.Text = @"{{Warning=<<WarnVariable>>}}";

            var expectedMessage = "This is a warning";
            var varMap = new Dictionary<string, string> { { "WarnVariable", expectedMessage } };

            Assert.That(script.GetWarning(varMap), Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GetWarningReturnsNullWhenFirstInitialized()
        {
            var script = new Script();

            Assert.That(script.GetWarning(), Is.Null);
        }

        [Test]
        public void OrdinalHasMaxValueWhenInitialized()
        {
            var script = new Script();

            Assert.That(script.Ordinal, Is.EqualTo(long.MaxValue));
        }
    }
}