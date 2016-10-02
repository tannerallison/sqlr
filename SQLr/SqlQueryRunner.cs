// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - SqlQueryRunner.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-10-01</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.SqlClient;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    #endregion

    internal class SqlQueryRunner
    {
        private readonly SqlConnection connection;

        private SqlCommand currentCommand;

        public SqlQueryRunner(SqlConnection connection) { this.connection = connection; }

        public void CancelCurrentScript() { currentCommand?.Cancel(); }

        public void ExecuteCommand(string command, int? timeout = null, BackgroundWorker worker = null)
        {
            var queries = ParseScript(command);

            var transaction = connection.BeginTransaction("transaction");
            foreach (var query in queries)
            {
                if ((worker != null) && worker.CancellationPending)
                {
                    RollbackTransaction(
                        transaction,
                        new Exception($"Query was cancelled on line: {query.Key}\r\n{query.Value}\r\n"));
                }

                try
                {
                    currentCommand = new SqlCommand(query.Value, connection, transaction);
                    if (timeout.HasValue)
                        currentCommand.CommandTimeout = timeout.Value;

                    var asyncWaitHandle = currentCommand.BeginExecuteNonQuery().AsyncWaitHandle;

                    asyncWaitHandle.WaitOne();

                    if (transaction.Connection == null)
                    {
                        throw new Exception(
                                  $"Transaction was rolled back within the query that starts on line {query.Key}\r\n");
                    }
                }
                catch (SqlException ex)
                {
                    RollbackTransaction(
                        transaction,
                        new Exception(
                            $"LINE: {query.Key + ex.LineNumber}\r\nERROR: {ex.Message}\r\n\r\n{query.Value}",
                            ex));
                }
            }

            transaction.Commit();
        }

        public async Task ExecuteCommandAsync(string command, int? timeout = null, BackgroundWorker worker = null)
        {
            var queries = ParseScript(command);
            var transaction = connection.BeginTransaction();

            foreach (var query in queries)
            {
                if ((worker != null) && worker.CancellationPending)
                {
                    RollbackTransaction(
                        transaction,
                        new Exception($"Query was cancelled on line: {query.Key}\r\n{query.Value}\r\n"));
                }

                try
                {
                    currentCommand = new SqlCommand(query.Value, connection, transaction);
                    if (timeout.HasValue)
                        currentCommand.CommandTimeout = timeout.Value;

                    var asyncWaitHandle = currentCommand.BeginExecuteNonQuery().AsyncWaitHandle;

                    asyncWaitHandle.WaitOne();

                    if (transaction.Connection == null)
                    {
                        throw new Exception(
                                  $"Transaction was rolled back within the query that starts on line {query.Key}\r\n");
                    }
                }
                catch (SqlException ex)
                {
                    RollbackTransaction(
                        transaction,
                        new Exception(
                            $"LINE: {query.Key + ex.LineNumber}\r\nERROR: {ex.Message}\r\n\r\n{query.Value}",
                            ex));
                }
            }

            transaction.Commit();
        }

        public Dictionary<int, string> ParseScript(string script)
        {
            var commandSql = script.TrimEnd();

            if (commandSql.EndsWith("\rGO") || commandSql.EndsWith("\nGO"))
                commandSql = commandSql.Substring(0, commandSql.Length - 2);

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
                line = Regex.Replace(line, @"/\*.*?\*/", string.Empty);

                // We don't want to match a "GO" if it's within a multi-line comment
                if (line.Contains("/*"))
                    multComment = true;
                if (line.Contains("*/"))
                    multComment = false;

                query.AppendLine(line);

                if (line.StartsWith("--") || string.IsNullOrEmpty(line) || debugSection || multComment
                    || !line.ToUpper().Equals("GO"))
                    continue;

                var queryText =
                    Regex.Replace(
                        query.ToString().Trim(),
                        @"^GO\b",
                        string.Empty,
                        RegexOptions.Multiline | RegexOptions.IgnoreCase).Trim();

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
                throw new Exception($"ROLLBACK FAILED: {ex.Message}\r\nInner Exception: {e.Message}", e);
            }
        }
    }
}