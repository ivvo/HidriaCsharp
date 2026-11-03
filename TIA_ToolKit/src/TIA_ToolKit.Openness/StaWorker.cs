using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TIA_ToolKit
{
    /// <summary>
    /// En sam namenski STA thread, na katerem tečejo VSI Openness klici.
    /// GUI ne zamrzne, Openness objekti pa se vedno uporabljajo z istega threada.
    /// </summary>
    sealed class StaWorker : IDisposable
    {
        readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        readonly Thread _thread;

        public StaWorker()
        {
            _thread = new Thread(() =>
            {
                foreach (var work in _queue.GetConsumingEnumerable())
                    work();
            });
            _thread.IsBackground = true;
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Name = "OpennessWorker";
            _thread.Start();
        }

        public Task<T> Run<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            _queue.Add(() =>
            {
                try { tcs.SetResult(func()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }

        public Task Run(Action action)
        {
            return Run<object>(() => { action(); return null; });
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
        }
    }
}
