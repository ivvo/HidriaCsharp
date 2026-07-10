using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logger.DataLogger
{
    public class DBDataLogger<T> : DataLoggerBase<T> where T : struct
    {
        #region Private fields
        private string ConnectionString;
        private string Station;
        private string TableName;
        private int TypeNum;
        private int ProgramNum;
        #endregion

        /// <summary>
        /// Constructs object of type DBDataLogger.
        /// </summary>
        /// <param name="connectionString">SQL server connection string.</param>
        /// <param name="station">Station name.</param>
        /// <param name="typeNum">Part type number</param>
        /// <param name="programNum">Program number.</param>
        /// <param name="errorHandlingCallback">Callback for errors.</param>
        /// <exception cref="ArgumentException"></exception>
        public DBDataLogger(string connectionString, string station, int typeNum, int programNum, Action<Task> errorHandlingCallback) : base(errorHandlingCallback)
        {
            // Check if connection string is null or empty
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string is null or empty.");

            // Check if station name is null or empty
            if (string.IsNullOrEmpty(station))
                throw new ArgumentException("Station name is null or empty.");

            // Check if all values are positive
            if (typeNum < 0 || programNum < 0)
                throw new ArgumentException("Type number and program number must be a positive numbers.");

            // Check if handler is null
            if (errorHandlingCallback == null)
                throw new ArgumentException("Error handling callback is null.");

            ConnectionString = new DbConnectionStringBuilder().ConnectionString = connectionString;
            Station = station;
            TypeNum = typeNum;
            ProgramNum = programNum;
        }

        #region Public methods
        /// <summary>
        /// Prepares data logger.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>  
        /// <exception cref="SqlException"></exception>  
        /// <exception cref="ArgumentException"></exception>
        public override void Prepare()
        {
            string TableCreationString;

            // Construct table name
            TableName = string.Format("{0}_Type{1:D3}_P{2:D3}", Station, TypeNum, ProgramNum);

            // Construct table creation string
            TableCreationString = GenerateTableCreateString();

            // Check if database table exists. If not, create one
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand Query = new SqlCommand(TableCreationString, connection))
            {
                // Open connection
                connection.Open();

                // Run the query
                Query.ExecuteNonQuery();
            }

            IsPrepared = true;
        }
        #endregion

        #region Private and protected methods
        /// <summary>
        /// Represents processing queue method.
        /// </summary>
        protected override void ProcessingQueue()
        {
            // Consume data
            while (!Data.IsCompleted)
            {
                // Check if operation has been canceled
                if (TokenSource.Token.IsCancellationRequested)
                    break;

                // Try to get data from the queue
                if (base.Data.TryTake(out T Data, 50))
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    using (SqlCommand query = new SqlCommand(GenerateTableInsertString(Data), connection))
                    {
                        // Open connection
                        connection.Open();

                        // Run the query
                        query.ExecuteNonQuery();
                    }

                    // Fire the event when new data log entry is added
                    OnDataAddedEvent(Data);
                }
            }
        }

        /// <summary>
        /// Generates SQL table create string.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="structFields">Array of structure fields.</param>
        /// <returns>SQL table create string</returns>
        private string GenerateTableCreateString()
        {
            StringBuilder TableCreationString = new StringBuilder();
            FieldInfo[] StructFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Create first part where we need table name
            TableCreationString.AppendFormat("If not exists (select name from sysobjects where name = '{0}') CREATE TABLE {0} (", TableName);

            // Go through all fields
            for (int i = 0; i < StructFields.Length; i++)
            {
                Type structureFieldType = StructFields[i].FieldType;

                if (i == StructFields.Length - 1)
                    TableCreationString.AppendFormat("{0} {1})", StructFields[i].Name, GetSQLDataType(structureFieldType));
                else
                    TableCreationString.AppendFormat("{0} {1},", StructFields[i].Name, GetSQLDataType(structureFieldType));
            }

            return TableCreationString.ToString();
        }

        /// <summary>
        /// Generates SQL table insert string.
        /// </summary>
        /// <param name="result">Result structure.</param>
        /// <returns>SQL table insert string</returns>
        private string GenerateTableInsertString(T result)
        {
            StringBuilder TableInsertionString = new StringBuilder();
            FieldInfo[] StructFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Create first part where we need table name
            TableInsertionString.AppendFormat("INSERT INTO {0} VALUES (", TableName);

            // Go through all fields
            for (int i = 0; i < StructFields.Length; i++)
            {
                Type structureFieldType = StructFields[i].FieldType;

                if (i == StructFields.Length - 1)
                    TableInsertionString.AppendFormat(CultureInfo.InvariantCulture, "'{0}')", StructFields[i].GetValue(result));
                else
                    TableInsertionString.AppendFormat(CultureInfo.InvariantCulture, "'{0}',", StructFields[i].GetValue(result));
            }

            return TableInsertionString.ToString();
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Maps .NET types to database SQL types.
        /// </summary>
        /// <param name="structFieldType">Type of</param>
        /// <returns>SQL type which matches given .NET type</returns>
        private static string GetSQLDataType(Type structFieldType)
        {
            switch(Type.GetTypeCode(structFieldType))
            {
                case TypeCode.Boolean:
                    return "bit";
                case TypeCode.Byte:
                    return "tinyint";
                case TypeCode.SByte:
                case TypeCode.Int16:
                    return "smallint";
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return "int";
                case TypeCode.UInt32:
                case TypeCode.Int64:
                    return "bigint";
                case TypeCode.Single:
                    return "real";
                case TypeCode.Double:
                    return "float";
                case TypeCode.String:
                    return "varchar(max)";
                default:
                    throw new ArgumentException("Type not supported.");
            }
        }
        #endregion
    }
}
