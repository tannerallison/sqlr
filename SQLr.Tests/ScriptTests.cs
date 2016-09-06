using NUnit.Framework;
using System;
using System.IO;

namespace SQLr.Tests
{


	[TestFixture]
	public class ScriptTests
	{
		private string directory;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			directory = Path.GetDirectoryName(Path.GetTempFileName());
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			foreach (string file in Directory.EnumerateFiles(directory))
			{
				if (new FileInfo(file).CreationTime > DateTime.Now.AddHours(-2))
					File.Delete(file);
			}
		}

		[TestCase("_123_TestCase.sql", 123)]
		[TestCase("_456_.sql", 456)]
		[TestCase("_0023123_TestCase.sql", 23123)]
		[TestCase("_0_TestCase.sql", 0)]
		[TestCase("_999999999999999_TestCase.sql", 999999999999999)]
		public void TestOrdinalValues(string fileName, long ordinalValue)
		{
			var filePath = Path.Combine(directory, fileName);

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
			var badFilePath = Path.Combine(directory, fileName);

			File.WriteAllText(badFilePath, "This is a test string");
			Script script;
			Assert.That(() =>
			{
				script = new Script(badFilePath);
			}, Throws.ArgumentException);
		}
		
	}
}
