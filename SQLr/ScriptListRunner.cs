// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - ScriptListRunner.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    // public class ScriptListRunner
    // {
    // #region Constants, Statics & Fields

    // private readonly SqlConnection _connection;
    // private List<Script> scripts;

    // #endregion Constants, Statics & Fields

    // #region Constructors & Destructors

    // public ScriptListRunner(SqlConnection connection, List<Script> scripts)
    // {
    // _connection = connection;
    // }

    // #endregion Constructors & Destructors

    // #region Methods

    // public void ExecuteScripts(Dictionary<string, string> variableReplacements, BackgroundWorker worker = null)
    // {
    // var orderedScripts = scripts.OrderBy(b => b.Ordinal);

    // foreach (var script in orderedScripts)
    // {
    // var database = script.Database(variableReplacements);
    // if (_connection.Database != database)
    // _connection.ChangeDatabase(database);

    // var queryText = script.GetVariableReplacedQuery(variableReplacements);

    // var sqlQueryRunner = new SqlQueryRunner(_connection);
    // sqlQueryRunner.ExecuteCommand(queryText, script.Timeout, worker);
    // }
    // }

    // #endregion Methods
    // }
}