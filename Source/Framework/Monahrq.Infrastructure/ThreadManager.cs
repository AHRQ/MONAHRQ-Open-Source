using System.Collections.Concurrent;
using System.Threading;

namespace Monahrq.Infrastructure
{
    /// <summary>
    /// A simple thread manager
    /// </summary>
    public static class ThreadManager
    {
        /// <summary>
        /// The concurrent threads
        /// </summary>
        private static ConcurrentQueue<Thread> _concurrentThreads;
        /// <summary>
        /// Gets the concurrent threads.
        /// </summary>
        /// <value>
        /// The concurrent threads.
        /// </value>
        public static ConcurrentQueue<Thread> ConcurrentThreads
        {
            get
            {
                if (_concurrentThreads == null)
                {
                    _concurrentThreads = new ConcurrentQueue<Thread>();
                }
                return _concurrentThreads;
            }
        }

        /// <summary>
        /// Kills all.
        /// </summary>
        public static void KillAll()
        {
            Thread thread = null;
            foreach (var t in ConcurrentThreads)
            {
                ConcurrentThreads.TryDequeue(out thread);
                if (thread != null)
                {
                    thread.Abort();
                }
            }
        }
    }
}
