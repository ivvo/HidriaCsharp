using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.ImageLogger
{
    public abstract class ImageLoggerBase : IDisposable
    {
        #region Events
        public event EventHandler<ImageLogAddedEventArgs> ImageLogAddedEvent;
        #endregion

        #region Delegates
        protected Action<Task> ErrorHandlingCallback;
        #endregion

        #region Private and protected fields
        private volatile bool DisposedValue;
        private Task ProcessingQueueTask;
        protected BlockingCollection<ImageLogEntry> ImageLogEntries;
        protected CancellationTokenSource TokenSource;
        protected object ObjLock;
        protected bool IsPrepared;
        protected int _MaxNumberOfImages;
        #endregion

        #region Properties
        /// <summary>
        /// Sets or gets the maximum number of images.
        /// </summary>
        public int MaxNumberOfImages
        {
            get
            {
                lock (ObjLock)
                    return _MaxNumberOfImages;
            }
            set
            {
                lock (ObjLock)
                    _MaxNumberOfImages = value;
            }
        }
        #endregion

        public ImageLoggerBase(Action<Task> errorHandlingCallback)
        {
            ErrorHandlingCallback = errorHandlingCallback;
            ImageLogEntries = new BlockingCollection<ImageLogEntry>();
            DisposedValue = false;
            IsPrepared = false;
            ObjLock = new object();
            _MaxNumberOfImages = 100;
        }

        #region Public methods
        /// <summary>
        /// Starts the image logger.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if image logger is prepared
            if (!IsPrepared)
                throw new InvalidOperationException("Image logger not prepared.");

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
        /// Stops the image logger.
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

                if(!ProcessingQueueTask.IsFaulted)
                    ProcessingQueueTask.Wait();

                ProcessingQueueTask = null;

                // Dispose token source
                TokenSource.Dispose();
            }
        }

        /// <summary>
        /// Adds new image log entry to the processing queue.
        /// </summary>
        /// <param name="imageLog">Image log entry.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void AddEntry(ImageLogEntry imageLog)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Add image log entry to the processing queue only if cancellation or complete adding hasn't been requested
            if (!TokenSource.IsCancellationRequested && !this.ImageLogEntries.IsAddingCompleted)
                ImageLogEntries.Add(imageLog);
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
        /// Prepares image logger.
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
        /// Raises image log added event.
        /// </summary>
        /// <param name="imageLog">Image log entry.</param>
        protected void OnImageLogAddedEvent(ImageLogEntry imageLog)
        {
            ImageLogAddedEvent?.Invoke(this, new ImageLogAddedEventArgs(imageLog));
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
                        ImageLogEntries.CompleteAdding();

                        if (!ProcessingQueueTask.IsFaulted)
                            ProcessingQueueTask.Wait();
                    }

                    // Dispose blocking collection
                    ImageLogEntries.Dispose();

                    // Dispose token source
                    TokenSource.Dispose();
                }

                DisposedValue = true;
            }
        }
        #endregion
    }
}
