using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Logger
{
    public class DBEventLogger : EventLoggerBase
    {
        #region Constants
        private const string TableCreationString = @"If not exists (select name from sysobjects where name = '{0}') 
                                                   CREATE TABLE {0} (time_of_occurance varchar(max), event_severity varchar(max), calling_member varchar(max), message varchar(max))";

        private const string TableDataInsertionString = "INSERT INTO {0} VALUES (@timeOfOccurance, @eventSeverity, @callingMember, @message)";
        #endregion

        #region Private fields
        private string ConnectionString;
        private string TableName;
        #endregion

        /// <summary>
        /// Constructs an object of type DBEventLogger.
        /// </summary>
        /// <param name="connectionString">SQL server connection string.</param>
        /// <param name="tableName">SQL table name.</param>
        /// <param name="errorHandlingCallback">Callback for errors.</param>
        /// <exception cref="ArgumentException"></exception>
        public DBEventLogger(string connectionString, string tableName, Action<Task> errorHandlingCallback) : base(errorHandlingCallback)
        {
            // Check if connection string is null or empty
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string is null or empty.");

            // Check if table name is null or empty
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name is null or empty.");

            // Check if handler is null
            if (errorHandlingCallback == null)
                throw new ArgumentException("Error handling callback is null.");

            ConnectionString = new DbConnectionStringBuilder().ConnectionString = connectionString;
            TableName = tableName;
        }

        #region Public methods
        /// <summary>
        /// Prepares the event logger.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>  
        /// <exception cref="SqlException"></exception>  
        /// <exception cref="ArgumentException"></exception>
        public override void Prepare()
        {
            if (!IsPrepared)
            {
                // Check if database table exists. If not, create one
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand query = new SqlCommand(string.Format(TableCreationString, TableName), connection))
                {
                    // Open connection
                    connection.Open();

                    // Run the query
                    query.ExecuteNonQuery();
                }

                IsPrepared = true;
            }
        }
        #endregion

        #region Private and protected methods
        /// <summary>
        /// Represents processing queue method.
        /// </summary>
        protected override void ProcessingQueue()
        {
            // Consume log entries
            while (!LogEntries.IsCompleted)
            {
                // Check if operation has been canceled
                if (TokenSource.Token.IsCancellationRequested)
                    break;

                // Try to get log entry from the queue
                if (LogEntries.TryTake(out LogEntry Entry, 50))
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    using (SqlCommand Query = new SqlCommand(string.Format(TableDataInsertionString, TableName), connection))
                    {
                        // Construct a query
                        Query.Parameters.AddWithValue("@timeOfOccurance", Entry.TimeOfOccurance.ToString("d/M/yyyy HH:mm:ss"));
                        Query.Parameters.AddWithValue("@eventSeverity", Entry.EventSeverity.ToString());
                        Query.Parameters.AddWithValue("@callingMember", Entry.CallingMember);
                        Query.Parameters.AddWithValue("@message", Entry.Message);

                        // Open connection
                        connection.Open();

                        // Run the query
                        Query.ExecuteNonQuery();
                    }
                }
            }
        }
        #endregion
    }
}
