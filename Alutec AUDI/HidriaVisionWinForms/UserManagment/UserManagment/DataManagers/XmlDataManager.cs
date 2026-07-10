using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UserManagment.DataManagment
{
    public class XmlDataManager : IDataManager
    {
        #region Private fields
        private string FolderPath;
        private string FullFilePath;
        private bool IsInitialized;
        #endregion

        /// <summary>
        /// Constructs an object of type XmlDataManager.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="fileName">File name.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception> 
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception> 
        /// <exception cref="PathTooLongException"></exception> 
        public XmlDataManager(string folderPath, string fileName)
        {
            // Check if folder path is null or empty
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Folder path is null or empty.");

            // Check if file name is null or empty
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name is null or empty.");

            FolderPath = Path.GetFullPath(folderPath);
            FullFilePath = Path.GetFullPath(string.Format(@"{0}\{1}", FolderPath, fileName));
            IsInitialized = false;
        }

        /// <summary>
        /// Initializes the manager.
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void Initialize()
        {
            if (!IsInitialized)
            {
                // Create directory for users
                Directory.CreateDirectory(FolderPath);

                // Check if file exists
                if(!File.Exists(FullFilePath))
                {
                    // Create xml file in a directory
                    File.Create(FullFilePath).Close();

                    // Create xml
                    XDocument Document = new XDocument();

                    // Create root element
                    XElement RootElement = new XElement("Users");
                    Document.Add(RootElement);

                    // Save xml
                    Document.Save(FullFilePath);
                }

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Adds new entry to xml file.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if entry has been added.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public bool AddEntry(UserData userData)
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Xml manager not initialized.");

            // Check if user already exists
            if (!EntryExists(userData))
            {
                XDocument Document = XDocument.Load(FullFilePath);

                // Construct user element
                XElement User = new XElement("User", new XElement("Name", userData.Name), new XElement("Surname", userData.Surname)
                                             , new XElement("Username", userData.UserCred.Username), new XElement("Password", userData.UserCred.Password.Base64Str)
                                             , new XElement("TagId", userData.TagCred.HasValue ? userData.TagCred.Value.TagId.Base64Str : "NULL"), new XElement("Role", ((int)userData.UserRole)));

                // Append user element to xml file
                Document.Element("Users").Add(User);

                // Save the xml file
                Document.Save(FullFilePath);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Modifies existing entry.
        /// </summary>
        /// <param name="currentUserData">User data.</param>
        /// <param name="modifiedUserData">Modified user data.</param>
        /// <returns>True if entry has been modified.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public bool ModifyEntry(UserData currentUserData, UserData modifiedUserData)
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Xml manager not initialized.");

            // Check if user exists. If the modified username is the same as current username, then we should check also that user with that username does not already exist
            if (EntryExists(currentUserData) && (currentUserData.UserCred.Username == modifiedUserData.UserCred.Username || !EntryExists(modifiedUserData)))
            {
                XDocument Document = XDocument.Load(FullFilePath);

                // Get user with a given username
                XElement User = Document.Descendants("User")
                                        .Where(x => x.Element("Username").Value == currentUserData.UserCred.Username)
                                        .First();

                // Update user values
                User.Element("Name").Value = modifiedUserData.Name;
                User.Element("Surname").Value = modifiedUserData.Surname;
                User.Element("Username").Value = modifiedUserData.UserCred.Username;
                User.Element("Password").Value = modifiedUserData.UserCred.Password.Base64Str;
                User.Element("TagId").Value = modifiedUserData.TagCred.HasValue ? modifiedUserData.TagCred.Value.TagId.Base64Str : "NULL";
                User.Element("Role").Value = ((int)(modifiedUserData.UserRole)).ToString();

                // Save the xml file
                Document.Save(FullFilePath);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes existing entry.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if entry has been removed.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public bool RemoveEntry(UserData userData)
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Xml manager not initialized.");

            // Check if user exists
            if (EntryExists(userData))
            {
                XDocument Document = XDocument.Load(FullFilePath);

                // Removes the element with a given username
                Document.Descendants("User")
                        .Where(x => x.Element("Username").Value == userData.UserCred.Username)
                        .Remove();


                // Save the xml file
                Document.Save(FullFilePath);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all entries.
        /// </summary>
        /// <returns>List of all entries.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public List<UserData> GetEntries()
        {
            // Check if manager is initialized
            if (!IsInitialized)
                throw new InvalidOperationException("Xml manager not initialized.");

            XDocument Document = XDocument.Load(FullFilePath);

            // Return all users
            return Document.Descendants("User")
                           .Select(x => new UserData(x.Element("Name").Value, x.Element("Surname").Value,
                                                     new UserCredentials(x.Element("Username").Value, x.Element("Password").Value, true),
                                                     x.Element("TagId").Value == "NULL" ? (TagCredentials?)null: new TagCredentials(x.Element("TagId").Value, true),
                                                     (Role)int.Parse(x.Element("Role").Value)))
                           .ToList();
        }

        #region Private methods
        /// <summary>
        /// Checks if the given entry exists.
        /// </summary>
        /// <param name="userData">User data.</param>
        /// <returns>True if the entry exists.</returns>
        private bool EntryExists(UserData userData)
        {
            XDocument Document = XDocument.Load(FullFilePath);

            // Returns true if user with a given username exists
            return Document.Descendants("Users")
                           .Descendants("User")
                           .Any(x => x.Element("Username").Value == userData.UserCred.Username);
        }
        #endregion
    }
}
