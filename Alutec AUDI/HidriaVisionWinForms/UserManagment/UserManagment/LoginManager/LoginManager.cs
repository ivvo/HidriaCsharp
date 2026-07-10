using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System;

namespace UserManagment.LoginManagment
{
    public class LoginManager
    {
        #region Events
        public event EventHandler LoginTimeoutEvent;
        public event EventHandler<RemainingMinutesLeftEventArgs> RemainingMinutesLeftEvent;
        #endregion

        #region Private fields
        private IDataManager DataManagment;
        private object ObjLock;
        private UserData? _CurrentUser;
        private Timer LoginTimeoutTimer;
        private int LoginTimeoutMinutes;
        private bool TimerStopped;
        #endregion

        #region Properties
        /// <summary>
        /// Gets current logon user
        /// </summary>
        public UserData? CurrentUser
        {
            get
            {
                lock (ObjLock)
                    return _CurrentUser;
            }
        }
        #endregion

        /// <summary>
        /// Constructs the object of type LoginManager.
        /// </summary>
        /// <param name="dataManagment">DataManagment object.</param>
        public LoginManager(IDataManager dataManagment)
        {
            DataManagment = dataManagment;
            ObjLock = new object();
            LoginTimeoutTimer = new Timer();
            _CurrentUser = null;
            LoginTimeoutMinutes = 0;
            TimerStopped = false;

            // Set timer interval to one minute
            LoginTimeoutTimer.Interval = 60 * 1000;

            // We will restart timer manually
            LoginTimeoutTimer.AutoReset = false;

            // Set timer elapsed callback
            LoginTimeoutTimer.Elapsed += TimerElapsedCallback;
        }

        #region Public methods
        /// <summary>
        /// Logins the user with username and password
        /// </summary>
        /// <param name="userCredentials">User credentials.</param>
        /// <param name="loginTimeoutMinutes">Login timeout in minutes.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>True if login was successfull.</returns>
        public bool Login(UserCredentials userCredentials, int loginTimeoutMinutes)
        {
            // Check user credentials
            if (string.IsNullOrEmpty(userCredentials.Username) || string.IsNullOrEmpty(userCredentials.Password.Base64Str))
                throw new ArgumentException("Invalid user credentials provided.");

            // Check login timeout time
            if(loginTimeoutMinutes <= 0)
                throw new ArgumentException("Login timeout must be greater than 0.");

            // Lock
            lock (ObjLock)
            {
                // Get all users
                IEnumerable<UserData> Users = DataManagment.GetEntries();

                // Try to get user with a given username and password
                UserData? User = Users.Cast<UserData?>().FirstOrDefault(x => x.Value.UserCred.Username == userCredentials.Username && x.Value.UserCred.Password.Base64Str == userCredentials.Password.Base64Str);

                // Check if user exists
                if (User != null)
                {
                    // Stop the timer
                    LoginTimeoutTimer.Stop();

                    // Set the login timeout
                    LoginTimeoutMinutes = loginTimeoutMinutes;

                    // Restart the timer
                    LoginTimeoutTimer.Start();
                    TimerStopped = false;

                    // Set current user
                    _CurrentUser = User;

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Logins the user with tag id.
        /// </summary>
        /// <param name="userTagCredentials">User tag credentials.</param>
        /// <param name="loginTimeoutMinutes">Login timeout in minutes.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>True if login was successfull.</returns>
        public bool Login(TagCredentials userTagCredentials, int loginTimeoutMinutes)
        {
            // Check tag credentials
            if(string.IsNullOrEmpty(userTagCredentials.TagId.Base64Str))
                throw new ArgumentException("Invalid user tag credentials provided.");

            // Check login timeout time
            if (loginTimeoutMinutes <= 0)
                throw new ArgumentException("Login timeout must be greater than 0.");

            // Lock
            lock (ObjLock)
            {
                // Get all users
                IEnumerable<UserData> Users = DataManagment.GetEntries();

                // Try to get user with a given tag id
                UserData? User = Users.Cast<UserData?>().FirstOrDefault(x => x.Value.TagCred.HasValue && x.Value.TagCred.Value.TagId.Base64Str == userTagCredentials.TagId.Base64Str);

                // Check if user exists
                if (User != null)
                {
                    // Stop the timer
                    LoginTimeoutTimer.Stop();

                    // Set the login timeout
                    LoginTimeoutMinutes = loginTimeoutMinutes;

                    // Restart the timer
                    LoginTimeoutTimer.Start();
                    TimerStopped = false;

                    // Set current user
                    _CurrentUser = User;

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Logsout the current user.
        /// </summary>
        public void Logout()
        {
            // Lock
            lock(ObjLock)
            {
                // Stop the timer
                LoginTimeoutTimer.Stop();
                TimerStopped = true;

                // No user is logged in
                _CurrentUser = null;
            }
        }
        #endregion

        #region Private events
        private void TimerElapsedCallback(object sender, ElapsedEventArgs e)
        {
            bool FireLoginTimeoutEvent = false;
            bool FireRemainingMinutesLeftEvent = false;

            lock(ObjLock)
            {
                // Check if timer is running
                if (!TimerStopped)
                {
                    // Decrease login timeout minutes
                    LoginTimeoutMinutes--;

                    // Check if the timer timeout occured
                    if (LoginTimeoutMinutes == 0)
                    {
                        // No user is logged in
                        _CurrentUser = null;

                        // Set flag for event
                        FireLoginTimeoutEvent = true;
                    }
                    else
                    {
                        // Restart the timer again
                        LoginTimeoutTimer.Start();

                        // Set flag for event
                        FireRemainingMinutesLeftEvent = true;
                    }
                }
            }

            if(FireLoginTimeoutEvent)
                // Raise the event for automatic logout
                LoginTimeoutEvent?.Invoke(this, EventArgs.Empty);

            if(FireRemainingMinutesLeftEvent)
                // Raise the event for the remaining minutes left
                RemainingMinutesLeftEvent?.Invoke(this, new RemainingMinutesLeftEventArgs(LoginTimeoutMinutes));
        }
        #endregion
    }
}