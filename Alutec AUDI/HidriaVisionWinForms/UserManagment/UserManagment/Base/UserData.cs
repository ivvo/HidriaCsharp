namespace UserManagment
{
    /// <summary>
    /// This structure represents a user data
    /// </summary>
    public struct UserData
    {
        #region Public fields
        public readonly string Name;
        public readonly string Surname;
        public readonly UserCredentials UserCred;
        public readonly TagCredentials? TagCred;
        public readonly Role UserRole;
        #endregion

        public UserData(string name, string surname, UserCredentials userCred, TagCredentials? tagCred, Role userRole)
        {
            Name = name;
            Surname = surname;
            UserCred = userCred;
            TagCred = tagCred;
            UserRole = userRole;
        }
    }
}
