using System;
using System.Collections.Generic;

namespace UserManagment
{
    public class UserManager
    {
        #region Private fields
        private IDataManager DataManagment;
        #endregion

        /// <summary>
        /// Constructs the object of type UserManager.
        /// </summary>
        /// <param name="dataManagment">DataManagment object.</param>
        public UserManager(IDataManager dataManagment)
        {
            DataManagment = dataManagment;
        }

        #region Public methods
        /// <summary>
        /// Adds new user.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if the user has been added.</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool AddUser(UserData userData)
        {
            // Check user data
            if (!UserDataValidation(userData))
                throw new ArgumentException("Invalid user data provided.");

            // Add new user entry
            return DataManagment.AddEntry(userData);
        }

        /// <summary>
        /// Modify existing user data.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <param name="modifiedUserData">Modified user data.</param>
        /// <returns>True if the user has been modified.</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool ModifyUser(UserData userData, UserData modifiedUserData)
        {
            // Check user data
            if (!UserDataValidation(userData))
                throw new ArgumentException("Invalid user data provided.");

            // Modify user entry
            return DataManagment.ModifyEntry(userData, modifiedUserData);
        }

        /// <summary>
        /// Deletes existing user.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if the user has been deleted.</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool DeleteUser(UserData userData)
        {
            // Check user data
            if (!UserDataValidation(userData))
                throw new ArgumentException("Invalid user data provided.");

            // Remove user entry
            return DataManagment.RemoveEntry(userData);
        }

        public List<UserData> GetAllUsers()
        {
            // Return all entries
            return DataManagment.GetEntries();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Validates user data.
        /// </summary>
        /// <param name="user">User data.</param>
        /// <returns>True if user data are valid.</returns>
        private bool UserDataValidation(UserData user)
        {
            // Validates user parameters
            return (!string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Surname) && !string.IsNullOrEmpty(user.UserCred.Username) && 
                   (!user.TagCred.HasValue || !string.IsNullOrEmpty(user.TagCred.Value.TagId.Base64Str)) && !string.IsNullOrEmpty(user.UserCred.Password.Base64Str) && 
                   (user.UserRole >= Role.Normal && user.UserRole <= Role.GodLike));
        }
        #endregion
    }
}
