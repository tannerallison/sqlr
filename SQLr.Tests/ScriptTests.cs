using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace SQLr.Tests
{
	[TestFixture]
	public class ScriptTests
	{
		[SetUp]
		public void SetUp()
		{
			_directory = Path.Combine(Path.GetTempPath(), "SQLrTesting");

			Directory.CreateDirectory(_directory);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_directory, true);
		}

		private string _directory;

		[TestCase("VariableA")]
		[TestCase("12345")]
		[TestCase("Variable_123")]
		[TestCase("?#$?")]
		public void QueryStringReplaceTest(string variableName)
		{
			var filePath = Path.Combine(_directory, "_123_testFile.sql");

			File.WriteAllText(filePath, $"Test variableA: <<{variableName}>>");

			var script = new Script(filePath);

			var query = script.GetVariableReplacedQuery(new Dictionary<string, string> {{variableName, "Test1"}});

			Assert.That(query, Is.EqualTo("Test variableA: Test1"));
		}

		[TestCase("_123_TestCase.sql", 123)]
		[TestCase("_456_.sql", 456)]
		[TestCase("_0023123_TestCase.sql", 23123)]
		[TestCase("_0_TestCase.sql", 0)]
		[TestCase("_999999999999999_TestCase.sql", 999999999999999)]
		public void TestOrdinalValues(string fileName, long ordinalValue)
		{
			var filePath = Path.Combine(_directory, fileName);

			File.WriteAllText(filePath, "This is a test string");

			var script = new Script(filePath);

			Assert.That(script.Ordinal, Is.EqualTo(ordinalValue));
		}

		[TestCase("123_TestCase.sql")]
		[TestCase("_456TestCase.sql")]
		[TestCase("_0023123_TestCase.sqlbak")]
		[TestCase("0TestCase.sql")]
		[TestCase("_-12_TestCase.sql")]
		public void TestCreateWithBadFileNames(string fileName)
		{
			var badFilePath = Path.Combine(_directory, fileName);

			File.WriteAllText(badFilePath, "This is a test string");
			Script script;
			Assert.That(() => { script = new Script(badFilePath); }, Throws.ArgumentException);
		}
	}
}