using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

namespace SQLr
{
	public class ScriptListRunner
	{
		#region  Constants, Statics & Fields

		private readonly SqlConnection _connection;
		private List<Script> scripts;

		#endregion

		#region Constructors & Destructors

		public ScriptListRunner(SqlConnection connection, List<Script> scripts)
		{
			_connection = connection;
		}

		#endregion

		#region Methods

		public void ExecuteScripts(Dictionary<string, string> variableReplacements, BackgroundWorker worker = null)
		{
			var orderedScripts = scripts.OrderBy(b => b.Ordinal);

			foreach (var script in orderedScripts)
			{
				var database = script.Database(variableReplacements);
				if (_connection.Database != database)
					_connection.ChangeDatabase(database);

				var queryText = script.GetVariableReplacedQuery(variableReplacements);

				var sqlQueryRunner = new SqlQueryRunner(_connection);
				sqlQueryRunner.ExecuteCommand(queryText, script.Timeout, worker);
			}
		}

		#endregion
	}
}