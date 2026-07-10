using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace UserManagment.DataManagment
{
    public class DBDataManager : IDataManager
    {
        #region Constants
        private const string TableCreationString = @"IF NOT EXISTS (select name from sysobjects where name = '{0}') 
                                                     CREATE TABLE {0} (name varchar(max), surname varchar(max), username varchar(max), password varchar(max), tag_id varchar(max), role int)";

        private const string TableDataInsertionString = @"INSERT INTO {0} VALUES (@name, @surname, @username, @password, @tagId, @role)";

        private const string TableDataSelectionString = "SELECT * FROM {0}";

        private const string TableDataExistanceString = "SELECT COUNT(*) FROM {0} WHERE username = @username";

        private const string TableDataUpdateString = @"UPDATE {0} SET name = @name, surname = @surname, username = @username, password = @password, tag_id = @tagId, role = @role
                                                       WHERE username = @currentUsername";

        private const string TableDataDeleteString = "DELETE FROM {0} WHERE username = @username";
        #endregion

        #region Private fields
        private string ConnectionString;
        private string TableName;
        private bool IsInitialized;
        #endregion

        /// <summary>
        /// Constructs an object of type DBDataManager.
        /// </summary>
        /// <param name="connectionString">SQL server connection string.</param>
        /// <param name="tableName">SQL table name.</param>
        /// <exception cref="ArgumentException"></exception>
        public DBDataManager(string connectionString, string tableName)
        {
            // Check if connection string is null or empty
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string is null or empty.");

            // Check if table name is null or empty
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name is null or empty.");

            ConnectionString = new DbConnectionStringBuilder().ConnectionString = connectionString;
            TableName = tableName;
            IsInitialized = false;
        }

        /// <summary>
        /// Initializes the manager.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>  
        /// <exception cref="SqlException"></exception>  
        /// <exception cref="ArgumentException"></exception>
        public void Initialize()
        {
            if(!IsInitialized)
            {
                // Check if database table exists. If not, create one
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand Query = new SqlCommand(string.Format(TableCreationString, TableName), connection))
                {
                    // Open connection
                    connection.Open();

                    // Run the query
                    Query.ExecuteNonQuery();
                }

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Adds new entry to xml file.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if entry has been added.</returns>
        /// <exception cref="InvalidOperationException"></exception>  
        /// <exception cref="SqlException"></exception>  
        /// <exception cref="ArgumentException"></exception>
        public bool AddEntry(UserData userData)
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Database manager not initialized.");

            // Check if user already exists
            if (!EntryExists(userData))
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand query = new SqlCommand(string.Format(TableDataInsertionString, TableName), connection))
                {
                    // Construct a query
                    query.Parameters.AddWithValue("@name", userData.Name);
                    query.Parameters.AddWithValue("@surname", userData.Surname);
                    query.Parameters.AddWithValue("@username", userData.UserCred.Username);
                    query.Parameters.AddWithValue("@password", userData.UserCred.Password.Base64Str);
                    query.Parameters.AddWithValue("@tagId", userData.TagCred.HasValue ? userData.TagCred.Value.TagId.Base64Str : (object)(DBNull.Value));
                    query.Parameters.AddWithValue("@role", (int)userData.UserRole);

                    // Open connection
                    connection.Open();

                    // Execute query
                    query.ExecuteNonQuery();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Modifies existing entry.
        /// </summary>
        /// <param name="currentUserData">User data.</param>
        /// <param name="modifiedUserData">Modified user data.</param>
        /// <returns>True if entry has been modified.</returns>
        /// <exception cref="InvalidOperationException"></exception>  
        /// <exception cref="SqlException"></exception>  
        /// <exception cref="ArgumentException"></exception>
        public bool ModifyEntry(UserData currentUserData, UserData modifiedUserData)
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Database manager not initialized.");

            // Check if user exists. If the modified username is the same as current username, then we should check also that user with that username does not already exist
            if (EntryExists(currentUserData) && (currentUserData.UserCred.Username == modifiedUserData.UserCred.Username || !EntryExists(modifiedUserData)))
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand query = new SqlCommand(string.Format(TableDataUpdateString, TableName), connection))
                {
                    // Construct a query
                    query.Parameters.AddWithValue("@name", modifiedUserData.Name);
                    query.Parameters.AddWithValue("@surname", modifiedUserData.Surname);
                    query.Parameters.AddWithValue("@username", modifiedUserData.UserCred.Username);
                    query.Parameters.AddWithValue("@password", modifiedUserData.UserCred.Password.Base64Str);
                    query.Parameters.AddWithValue("@tagId", modifiedUserData.TagCred.HasValue ? modifiedUserData.TagCred.Value.TagId.Base64Str : (object)(DBNull.Value));
                    query.Parameters.AddWithValue("@role", (int)modifiedUserData.UserRole);
                    query.Parameters.AddWithValue("@currentUsername", currentUserData.UserCred.Username);

                    // Open connection
                    connection.Open();

                    // Execute query
                    query.ExecuteNonQuery();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes existing entry.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if entry has been removed.</returns>
        /// <exception cref="InvalidOperationException"></exception>  
        /// <exception cref="SqlException"></exception>  
        /// <exception cref="ArgumentException"></exception>
        public bool RemoveEntry(UserData userData)
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Database manager not initialized.");

            // Check if user exists
            if (EntryExists(userData))
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand query = new SqlCommand(string.Format(TableDataDeleteString, TableName), connection))
                {
                    // Construct a query
                    query.Parameters.AddWithValue("@username", userData.UserCred.Username);

                    // Open connection
                    connection.Open();

                    // Execute query
                    query.ExecuteNonQuery();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all entries.
        /// </summary>
        /// <returns>List of all entries.</returns>
        /// <exception cref="InvalidOperationException"></exception>  
        /// <exception cref="SqlException"></exception>  
        /// <exception cref="ArgumentException"></exception>
        public List<UserData> GetEntries()
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Database manager not initialized.");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand query = new SqlCommand(string.Format(TableDataSelectionString, TableName), connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(query))
            {
                DataTable DataRows = new DataTable();

                // Get all rows from the database
                adapter.Fill(DataRows);

                // Returns all users
                return DataRows.AsEnumerable()
                               .Select(x => new UserData((string)x["name"], (string)x["surname"], new UserCredentials((string)x["username"], (string)x["password"], true),
                                                         DBNull.Value.Equals(x["tag_id"]) ? (TagCredentials?)null : new TagCredentials((string)x["tag_id"], true),
                                                         (Role)x["role"]))
                               .ToList();
            }
        }

        #region Private methods
        /// <summary>
        /// Checks if the given entry exists.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if the entry exists.</returns>
        private bool EntryExists(UserData userData)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand query = new SqlCommand(string.Format(TableDataExistanceString, TableName), connection))
            {
                // Construct a query
                query.Parameters.AddWithValue("@username", userData.UserCred.Username);

                // Open connection
                connection.Open();

                // Execute query
                return (int)(query.ExecuteScalar()) > 0;
            }
        }
        #endregion
    }
}
