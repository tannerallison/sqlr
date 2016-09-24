using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SQLr
{
    internal class SqlQueryRunner
    {
        #region Constants, Statics & Fields

        private readonly SqlConnection _connection;

        private SqlCommand _currentCommand;

        #endregion Constants, Statics & Fields

        #region Constructors & Destructors

        public SqlQueryRunner(SqlConnection connection)
        {
            _connection = connection;
        }

        #endregion Constructors & Destructors

        #region Methods

        public void CancelCurrentScript()
        {
            _currentCommand?.Cancel();
        }

        public async Task ExecuteCommandAsync(string command, int? timeout = null, BackgroundWorker worker = null)
        {
            var queries = ParseScript(command);
            var transaction = _connection.BeginTransaction();

            foreach (KeyValuePair<int, string> query in queries)
            {
                if (worker != null && worker.CancellationPending)
                {
                    RollbackTransaction(transaction,
                        new Exception($"Query was cancelled on line: {query.Key}\r\n{query.Value}\r\n"));
                }

                try
                {
                    _currentCommand = new SqlCommand(query.Value, _connection, transaction);
                    if (timeout.HasValue)
                        _currentCommand.CommandTimeout = timeout.Value;

                    var asyncWaitHandle = _currentCommand.BeginExecuteNonQuery().AsyncWaitHandle;

                    asyncWaitHandle.WaitOne();

                    if (transaction.Connection == null)
                        throw new Exception($"Transaction was rolled back within the query that starts on line {query.Key}\r\n");
                }
                catch (SqlException ex)
                {
                    RollbackTransaction(transaction,
                        new Exception(
                            $"LINE: {query.Key + ex.LineNumber}\r\nERROR: {ex.Message}\r\n\r\n{query.Value}", ex));
                }
            }
            transaction.Commit();
        }

        public void ExecuteCommand(string command, int? timeout = null, BackgroundWorker worker = null)
        {
            var queries = ParseScript(command);

            var transaction = _connection.BeginTransaction("transaction");
            foreach (var query in queries)
            {
                if (worker != null && worker.CancellationPending)
                {
                    RollbackTransaction(transaction,
                        new Exception($"Query was cancelled on line: {query.Key}\r\n{query.Value}\r\n"));
                }

                try
                {
                    _currentCommand = new SqlCommand(query.Value, _connection, transaction);
                    if (timeout.HasValue)
                        _currentCommand.CommandTimeout = timeout.Value;

                    var asyncWaitHandle = _currentCommand.BeginExecuteNonQuery().AsyncWaitHandle;

                    asyncWaitHandle.WaitOne();

                    if (transaction.Connection == null)
                        throw new Exception($"Transaction was rolled back within the query that starts on line {query.Key}\r\n");
                }
                catch (SqlException ex)
                {
                    RollbackTransaction(transaction,
                        new Exception(
                            $"LINE: {query.Key + ex.LineNumber}\r\nERROR: {ex.Message}\r\n\r\n{query.Value}", ex));
                }
            }
            transaction.Commit();
        }

        public Dictionary<int, string> ParseScript(string script)
        {
            var commandSql = script.TrimEnd();

            if (commandSql.EndsWith("\rGO") || commandSql.EndsWith("\nGO"))
            {
                commandSql = commandSql.Substring(0, commandSql.Length - 2);
            }

            // Split the query by the GO text
            var queries = new Dictionary<int, string>();
            var lines = commandSql.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var query = new StringBuilder();
            var multComment = false;
            var debugSection = false;
            var lineNum = 0;
            var queryLine = 1;
            foreach (var lineItem in lines)
            {
                lineNum++;
                var line = lineItem.Trim();

                // Comment out all code in debug sections
                if (debugSection)
                    line = "-- " + line;
                if (line.ToLower().StartsWith(@"--#debug"))
                    // If it starts with "--#debug" and ends with "#enddebug" on one line, it will be commented out anyway
                    debugSection = true;
                if (line.ToLower().EndsWith(@"#enddebug"))
                    debugSection = false;

                // If there is a multi-line comment within a single line, just strip it out.
                line = Regex.Replace(line, @"/\*.*?\*/", "");

                // We don't want to match a "GO" if it's within a multi-line comment
                if (line.Contains("/*"))
                    multComment = true;
                if (line.Contains("*/"))
                    multComment = false;

                query.AppendLine(line);

                if (line.StartsWith("--") || string.IsNullOrEmpty(line) || debugSection || multComment ||
                    !line.ToUpper().Equals("GO"))
                    continue;

                var queryText =
                    Regex.Replace(query.ToString().Trim(), @"^GO\b", "", RegexOptions.Multiline | RegexOptions.IgnoreCase).Trim();

                if (!string.IsNullOrEmpty(queryText)) // Don't include empty queries
                    queries.Add(queryLine, queryText);
                queryLine = lineNum;
                query.Clear();
            }

            var lastQuery = query.ToString();
            if (!string.IsNullOrEmpty(lastQuery)) // Don't include empty queries
                queries.Add(queryLine, lastQuery);

            return queries;
        }

        private static void RollbackTransaction(SqlTransaction transaction, Exception e)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"ROLLBACK FAILED: {ex.Message}\r\nInner Exception: {e.Message}",
                    e);
            }
        }

        #endregion Methods
    }
}