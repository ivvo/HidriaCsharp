using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Logger
{
    public abstract class EventLoggerBase : IDisposable
    {
        #region Events
        public event EventHandler<LogEntryAddedEventArgs> LogEntryAddedEvent;
        #endregion

        #region Delegates
        protected Action<Task> ErrorHandlingCallback;
        #endregion

        #region Private and protected fields
        private volatile bool DisposedValue;
        private Task ProcessingQueueTask;
        protected BlockingCollection<LogEntry> LogEntries;
        protected CancellationTokenSource TokenSource;
        protected bool IsPrepared;
        #endregion

        public EventLoggerBase(Action<Task> errorHandlingCallback)
        {
            ErrorHandlingCallback = errorHandlingCallback;
            LogEntries = new BlockingCollection<LogEntry>();
            DisposedValue = false;
            IsPrepared = false;
        }

        #region public Methods
        /// <summary>
        /// Starts the event logger.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if logger is prepared
            if (!IsPrepared)
                throw new InvalidOperationException("Logger not prepared.");
           
            // Create a new task and start it
            if(ProcessingQueueTask == null || ProcessingQueueTask.IsCompleted)
            {
                TokenSource = new CancellationTokenSource();
                ProcessingQueueTask = new Task(ProcessingQueue, TaskCreationOptions.LongRunning);
                ProcessingQueueTask.ContinueWith(ErrorHandlingCallback, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
                ProcessingQueueTask.Start();
            }
        }

        /// <summary>
        /// Stops the event logger.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Stop()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Stop the task
            if(ProcessingQueueTask != null && !ProcessingQueueTask.IsCompleted)
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
        /// Adds new log entry to the processing queue.
        /// </summary>
        /// <param name="eventSeverity">Log entry severity.</param>
        /// <param name="message">Log entry message.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void AddEntry(LoggingLevel eventSeverity, string message, [CallerMemberName] string callingMember = "")
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Add log entry to the processing queue only if cancellation or complete adding hasn't been requested
            if (!TokenSource.IsCancellationRequested && !LogEntries.IsAddingCompleted)
                LogEntries.Add(new LogEntry(eventSeverity, callingMember, message));
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
        /// Prepares the event logger.
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
        /// Raises the log entry added event.
        /// </summary>
        /// <param name="logEntry">Log entry.</param>
        protected void OnLogEntryAddedEvent(LogEntry logEntry)
        {
            LogEntryAddedEvent?.Invoke(this, new LogEntryAddedEventArgs(logEntry));
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
                    // Stop the task and complete adding of the remaining log entries
                    if (ProcessingQueueTask != null && !ProcessingQueueTask.IsCompleted)
                    {
                        LogEntries.CompleteAdding();

                        if (!ProcessingQueueTask.IsFaulted)
                            ProcessingQueueTask.Wait();
                    }

                    // Dispose blocking collection
                    LogEntries.Dispose();

                    // Dispose token source
                    TokenSource.Dispose();
                }

                DisposedValue = true;
            }
        }
        #endregion
    }
}
