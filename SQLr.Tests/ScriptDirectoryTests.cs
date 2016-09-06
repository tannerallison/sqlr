using NUnit.Framework;
using System.IO;
using System.Threading;

namespace SQLr.Tests
{
	[TestFixture]
	public class ScriptDirectoryTests
	{
		[SetUp]
		public void SetUp()
		{
			_directory = Path.Combine(Path.GetTempPath(), "SQLrTesting");

			Directory.CreateDirectory(_directory);

			_filePath = Path.Combine(_directory, $"_{_testNumber++}_TestFile.sql");

			File.WriteAllText(_filePath, "<<VariableA>>\r\n{{Subset=SubA}}");

			_scriptDirectory = new ScriptDirectory(_directory, false);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_directory, true);
		}

		private static int _testNumber = 1234;
		private string _directory;
		private string _filePath;
		private ScriptDirectory _scriptDirectory;

		[Test]
		public void AddFileTest()
		{
			var newFilePath = Path.Combine(_directory, $"_{_testNumber++}_TestFile.sql");
			File.WriteAllText(newFilePath, "<<VariableA>>\r\n{{Subset=SubA}}");

			Thread.Sleep(100); // Give the file watcher time to see the change and update

			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(2));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(2));
			Assert.That(_scriptDirectory.Subsets, Has.Count.EqualTo(2));
		}

		[Test]
		public void DeleteFileTest()
		{
			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(1));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(1));

			File.Delete(_filePath);

			Thread.Sleep(100); // Give the file watcher time to see the change and update

			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(0));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(0));
		}

		[Test]
		public void RenameFileTest()
		{
			var newFilePath = Path.Combine(_directory, $"_{_testNumber++}_TestFile.sql");

			File.Move(_filePath, newFilePath);

			Thread.Sleep(100); // Give the file watcher time to see the change and update

			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(1));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(1));
		}

		[Test]
		public void Test()
		{
			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(1));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(1));
			Assert.That(_scriptDirectory.Subsets, Has.Count.EqualTo(1));
		}

		[Test]
		public void TestAddVariable()
		{
			File.AppendAllText(_filePath, "<<VariableB>>");

			Thread.Sleep(100); // Give the file watcher time to see the change and update

			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(1));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(2));
		}

		[Test]
		public void TestChangeVariable()
		{
			File.WriteAllText(_filePath, "<<VariableB>>\r\n{{Subset=SubA}}");

			Thread.Sleep(100); // Give the file watcher time to see the change and update

			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(1));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(1));
		}

		[Test]
		public void TestSameVariableTwice()
		{
			File.AppendAllText(_filePath, "<<VariableA>>");

			Thread.Sleep(100); // Give the file watcher time to see the change and update

			Assert.That(_scriptDirectory.Scripts, Has.Count.EqualTo(1));
			Assert.That(_scriptDirectory.Variables, Has.Count.EqualTo(1));
		}
	}
}