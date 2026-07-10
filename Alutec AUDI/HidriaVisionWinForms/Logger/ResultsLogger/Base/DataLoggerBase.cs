using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.DataLogger
{
    public abstract class DataLoggerBase<T> : IDisposable where T : struct
    {
        #region Events
        public event EventHandler<DataAddedEventArgs<T>> DataAddedEvent;
        #endregion

        #region Delegates
        protected Action<Task> ErrorHandlingCallback;
        #endregion

        #region Private and protected fields
        private volatile bool DisposedValue;
        private Task ProcessingQueueTask;
        protected BlockingCollection<T> Data;
        protected CancellationTokenSource TokenSource;
        protected bool IsPrepared;
        #endregion

        public DataLoggerBase(Action<Task> errorHandlingCallback)
        {
            Type TypeArg;
            FieldInfo[] StructFields;

            ErrorHandlingCallback = errorHandlingCallback;
            Data = new BlockingCollection<T>();
            DisposedValue = false;
            IsPrepared = false;

            // Check if type is a structure
            TypeArg = typeof(T);

            if (!(TypeArg.IsValueType && !TypeArg.IsPrimitive && !TypeArg.IsEnum))
                throw new ArgumentException("Type is not a structure.");

            // Get all the fields from the structure
            StructFields = TypeArg.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Check that struct is not empty
            if (StructFields.Length == 0)
                throw new ArgumentException("Structure must have at least one field member.");

            // Go through all fields and check that they are of correct type
            foreach (FieldInfo structField in StructFields)
                if (!structField.FieldType.IsPrimitive && structField.FieldType != typeof(string))
                    throw new ArgumentException("One or more fields are not of a correct type.");
        }

        #region public Methods
        /// <summary>
        /// Starts the data logger.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if data logger is prepared
            if (!IsPrepared)
                throw new InvalidOperationException("Results logger not prepared.");

            // Create a new task and start it
            if (ProcessingQueueTask == null || ProcessingQueueTask.IsCompleted)
            {
                TokenSource = new CancellationTokenSource();
                ProcessingQueueTask = new Task(ProcessingQueue, TaskCreationOptions.LongRunning);
                ProcessingQueueTask.ContinueWith(ErrorHandlingCallback, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
                ProcessingQueueTask.Start();
            }
        }

        /// <summary>
        /// Stops the data logger.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Stop()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Stop the task
            if (ProcessingQueueTask != null && !ProcessingQueueTask.IsCompleted)
            {
                TokenSource.Cancel();

                if (!ProcessingQueueTask.IsFaulted)
                    ProcessingQueueTask.Wait();

                ProcessingQueueTask = null;

                // Dispose token source
                TokenSource.Dispose();
            }
        }

        /// <summary>
        /// Adds new data to the processing queue.
        /// </summary>
        /// <param name="data">Data structure.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void AddData(T data)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Add data to the processing queue only if cancellation or complete adding hasn't been requested
            if (!TokenSource.IsCancellationRequested && !this.Data.IsAddingCompleted)
                Data.Add(data);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Public abstract methods
        /// <summary>
        /// Prepares data logger.
        /// </summary>
        public abstract void Prepare();
        #endregion

        #region Protected abstract methods
        /// <summary>
        /// Represents processing queue method.
        /// </summary>
        protected abstract void ProcessingQueue();
        #endregion

        #region Private and protected methods
        /// <summary>
        /// Raises data added event.
        /// </summary>
        /// <param name="data">Data.</param>
        protected void OnDataAddedEvent(T data)
        {
            DataAddedEvent?.Invoke(this, new DataAddedEventArgs<T>(data));
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // Stop the task and complete adding of the remaining results
                    if (ProcessingQueueTask != null && !ProcessingQueueTask.IsCompleted)
                    {
                        Data.CompleteAdding();

                        if (!ProcessingQueueTask.IsFaulted)
                            ProcessingQueueTask.Wait();
                    }

                    // Dispose blocking collection
                    Data.Dispose();

                    // Dispose token source
                    TokenSource.Dispose();
                }

                DisposedValue = true;
            }
        }
        #endregion
    }
}
