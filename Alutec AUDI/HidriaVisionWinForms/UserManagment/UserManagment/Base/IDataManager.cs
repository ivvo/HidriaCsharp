using System.Collections.Generic;

namespace UserManagment
{
    /// <summary>
    /// This interface defines methods of data managment
    /// </summary>
    public interface IDataManager
    {
        /// <summary>
        /// Initializes data manager.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds new entry.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if entry has been added.</returns>
        bool AddEntry(UserData userData);

        /// <summary>
        /// Modifies existing entry.
        /// </summary>
        /// <param name="currentUserData">Current user data.</param>
        /// <param name="modifiedUserData">Modified user data</param>
        /// <returns>True if entry has been modified.</returns>
        bool ModifyEntry(UserData currentUserData, UserData modifiedUserData);

        /// <summary>
        /// Removes entry.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if entry has been removed.</returns>
        bool RemoveEntry(UserData userData);

        /// <summary>
        /// Gets all entries.
        /// </summary>
        /// <returns>List of all entries.</returns>
        List<UserData> GetEntries();
    }
}
