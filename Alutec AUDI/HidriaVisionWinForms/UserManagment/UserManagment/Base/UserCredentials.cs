namespace UserManagment
{
    /// <summary>
    /// This structure represents user credentials
    /// </summary>
    public struct UserCredentials
    {
        #region Public fields
        public readonly string Username;
        public readonly Base64String Password;
        #endregion

        public UserCredentials(string username, string password, bool isBase64 = false)
        {
            Username = username;
            Password = new Base64String(password, isBase64);
        }
    }
}
