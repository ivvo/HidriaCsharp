namespace UserManagment
{
    /// <summary>
    /// This structure represents tag credentials
    /// </summary>
    public struct TagCredentials
    {
        #region Public fields
        public readonly Base64String TagId;
        #endregion

        public TagCredentials(string tagId, bool isBase64 = false)
        {
            TagId = new Base64String(tagId, isBase64);
        }
    }
}
