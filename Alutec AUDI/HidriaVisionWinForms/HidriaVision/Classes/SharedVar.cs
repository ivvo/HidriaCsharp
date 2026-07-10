namespace HidriaVision
{
    /// <summary>
    /// This class is used to make an variable which can be shared between different threads.
    /// </summary>
    /// <typeparam name="T">Type of variable.</typeparam>
    public class SharedVar<T>
    {
        #region Private fields
        private readonly object ObjLock;
        private T _Value;
        #endregion

        #region Properties
        /// <summary>
        /// Sets and gets the value atomically.
        /// </summary>
        public T Value
        {
            get
            {
                lock (ObjLock)
                    return _Value;
            }
            set
            {
                lock (ObjLock)
                    _Value = value;
            }
        }
        #endregion

        public SharedVar()
        {
            // Initialize the lock
            ObjLock = new object();
        }
    }
}
