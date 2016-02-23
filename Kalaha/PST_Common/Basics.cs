//
// Basics
//
// Some very basic helper functions, built as static member functions of class Basics.
//
// Additionally some sophisticated asynchronous lock features provided by Stephen Toub.
// (currently used when writing to files).
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PST_Common
{
    public class Basics
    {
        // --- Attributes of the class Basics ---

        /// <summary>A high number which is enough to be the highest number</summary>
        private const int _infinity = 1000000;

        // --- Methods of the class Basics ---

        /// <summary>
        /// A Random generator using the GUID. The advantage of using GUIDs instead of the default constructor
        /// is that you can generate thousands of random number within a very short timeframe. The default uses the system
        /// clock which causes issues if you want to create random numbers at nearly the "same" time.
        /// </summary>
        /// <param name="low">The "from" part of the range to generate the random number in</param>
        /// <param name="high">The "to" part of the range</param>
        /// <returns>A random number between "low" and "high", including these</returns>
        public static int GetRandomNumber(int low, int high)
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8),
                                       System.Globalization.NumberStyles.HexNumber));

            return rndNum.Next(low, high + 1);
        }

        /// <returns>A sufficiently high number</returns>
        public static int Infinity()
        {
            return _infinity;
        }

        /// <summary>
        /// Waits for the given number of milliseconds.
        /// </summary>
        /// <param name="milliSecs">The number of milliseconds to wait.</param>
        public static void Wait(int milliSecs)
        {
            // Set is never called, so we wait until the timeout occurs:
            using (EventWaitHandle tmpEvent = new ManualResetEvent(false))
            {
                tmpEvent.WaitOne(TimeSpan.FromMilliseconds(milliSecs));
            }
        }

    } // class Basics


    /// <summary>
    /// Class Singleton is a template that shows how to implement a thread-safe singleton.
    /// </summary>
    public sealed class Singleton
    {
        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile Singleton _inst;
        private static object _syncRoot = new Object();

        // --- Methods of the class ---

        /// <summary>The constructor of the class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private Singleton() { }

        /// <summary>
        /// If the single instance has not yet been created, yet, creates the instance.
        /// This approach ensures that only one instance is created and only when the instance is needed.
        /// This approach uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        /// This double-check locking approach solves the thread concurrency problems while avoiding an exclusive
        /// lock in every call to the Instance property method. It also allows you to delay instantiation
        /// until the object is first accessed.
        /// </summary>
        /// <returns>The single instance of the class</returns>
        public static Singleton Inst
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new Singleton();
                    }
                }

                return _inst;
            }
        }
    }
 
    /// <summary>
    /// Class AsyncLock provides a means to lock asynchronous calls like access to files, thereby
    /// keeping the execution in a reasonable order.
    /// </summary>
    public class AsyncLock
    {

        private readonly AsyncSemaphore _semaphore;
        private readonly Task<Releaser> _releaser;

        public AsyncLock()
        {
            _semaphore = new AsyncSemaphore(1);
            _releaser = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted ?
                _releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser(AsyncLock toRelease)
            { 
                _toRelease = toRelease;
            }

            public void Dispose()
            {
                if (_toRelease != null)
                    _toRelease._semaphore.Release();
            }
        }
    }



    public class AsyncSemaphore
    {

        private readonly static Task s_completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> m_waiters = new Queue<TaskCompletionSource<bool>>();
        private int m_currentCount;


        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
            m_currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (m_waiters)
            {
                if (m_currentCount > 0)
                {
                    --m_currentCount;
                    return s_completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    m_waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (m_waiters)
            {
                if (m_waiters.Count > 0)
                    toRelease = m_waiters.Dequeue();
                else
                    ++m_currentCount;
            }
            if (toRelease != null)
                toRelease.SetResult(true);
        }
    }
}
