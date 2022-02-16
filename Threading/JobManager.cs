using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using UnityEngine.SceneManagement;

namespace JStuff.Threading
{
    /// <summary>
    /// A MonoBehavior class which adds multithreading to a project in a simple manageable way
    /// </summary>
    public class JobManager
    {
        // Serializeable Data
        [Min(1)] [SerializeField] private int maxJobs = 1000;
        [Min(1)] [SerializeField] private int threadAmount = 1;
        [SerializeField] private bool setThreads = false;
        [SerializeField] private List<DedicatedThreads> dedicatedThreads = null;
        [SerializeField] private bool dedicatedThreadsEnabled = false;
        [Min(1)] [SerializeField] private int numberOfPriorities = 1;
        [SerializeField] private bool advancedOptions = false;
        [SerializeField] private bool allowMultithreading = true;
        [SerializeField] private bool consumeAsStartCoroutine = false;

        [SerializeField] private OverloadProtocol overloadProtocol = 0;
        [SerializeField] private UpdateProtocol updateProtocol = 0;


        // Non serializable
        private Queue<(IConsumer, Func<object, object>, object)>[] jobQueue;
        private Queue<(IConsumer, object, bool)> consumeQueue;

        private readonly object consumeLock = new object();
        private readonly object jobLock = new object();

        private static Semaphore jobSemaphore;
        private static CountdownEvent pendingEvent;
        private static ManualResetEvent stallEvent;

        private static Thread[] threads;

        public int Pending => pendingEvent.CurrentCount - 1;

        public int NumberOfThreads => threads.Length;

        public int MaxJobs => maxJobs;

        public int ThreadAmount => threadAmount;

        public void Initialize(JobManagerData data)
        {
            maxJobs = data.maxJobs;
            threadAmount = data.threadAmount;
            setThreads = data.setThreads;
            dedicatedThreads = data.dedicatedThreads;
            dedicatedThreadsEnabled = data.dedicatedThreadsEnabled;
            numberOfPriorities = data.numberOfPriorities;
            advancedOptions = data.advancedOptions;
            allowMultithreading = data.allowMultithreading;
            consumeAsStartCoroutine = data.consumeAsStartCoroutine;
            overloadProtocol = data.overloadProtocol;
            updateProtocol = data.updateProtocol;


            // Sanitize serialized fields
            OnValidate();

            if (maxJobs < 1)
                throw new System.Exception("Can't have less than 1 of max jobs.");
            if (threadAmount < 1)
                throw new System.Exception("Can't have less than one amount of threads.");

            if (!setThreads)
                threadAmount = Mathf.Min(maxJobs, Environment.ProcessorCount - 1);

            if (dedicatedThreadsEnabled)
            {
                int availableThreads = threadAmount;
                foreach (DedicatedThreads d in dedicatedThreads)
                {
                    threadAmount -= d.numberOfThreads;
                }
                if (availableThreads < 0)
                {
                    throw new System.Exception("Number of dedicated threads exceeds amount of available threads.");
                }
            }

            // Set up JobManager
            ClearThreads();

            if (!Application.isPlaying)
                return;

            jobSemaphore = new Semaphore(0, maxJobs);
            jobQueue = new Queue<(IConsumer, Func<object, object>, object)>[numberOfPriorities];
            pendingEvent = new CountdownEvent(1);
            stallEvent = new ManualResetEvent(false);

            for (int i = 0; i < numberOfPriorities; i++)
            {
                jobQueue[i] = new Queue<(IConsumer, Func<object, object>, object)>();
            }

            consumeQueue = new Queue<(IConsumer, object, bool)>();

            threads = new Thread[threadAmount];
            int j = 0;

            if (dedicatedThreadsEnabled && dedicatedThreads != null)
            {
                foreach (DedicatedThreads dt in dedicatedThreads)
                {
                    for (int i = 0; i < dt.numberOfThreads; i++)
                    {
                        threads[j] = new Thread(new ParameterizedThreadStart(WorkerThread));
                        threads[j].IsBackground = true;
                        threads[j].Start(dt.priority);
                        j++;
                    }
                }
            }

            for (int i = j; i < threads.Length; i++)
            {
                threads[i] = new Thread(new ParameterizedThreadStart(WorkerThread));
                threads[i].IsBackground = true;
                threads[i].Start(0);
            }
        }

        public void Remove()
        {
            ClearThreads();
            ClearJobs();
        }

        /// <summary>
        /// Worker thread function to execute in the duration of the game.
        /// </summary>
        private void WorkerThread(object data)
        {
            int threadPriority = (int)data;

            while (true)
            {
                // Continually running worker threads. Will only work on jobs, 
                // when jobs are available.
                jobSemaphore.WaitOne();

                (IConsumer consumer, Func<object, object> func, object parameters) job =
                    (null, null, null);

                lock (jobLock)
                {
                    if (jobQueue[threadPriority].Count > 0)
                    {
                        job = jobQueue[threadPriority].Dequeue();
                    }
                    else
                    {
                        for (int i = 0; i < jobQueue.Length; i++)
                        {
                            if (jobQueue[i].Count > 0)
                                job = jobQueue[i].Dequeue();
                        }
                    }
                }

                if (job == (null, null, null))
                {
                    throw new System.Exception("No job was available while fetching job. Is jobSemaphore incremented correctly?");
                }

                // Execute job
                object retval = null;
                bool successful = true;

                try
                {
                    retval = job.func(job.parameters);
                }
                catch
                {
                    successful = false;
                }

                // Add result for main thread to handle
                lock (consumeLock)
                {
                    consumeQueue.Enqueue((job.consumer, retval, successful));
                }
                pendingEvent.Signal();
                stallEvent.Set();
            }
        }

        /// <summary>
        /// Add a job for the worker threads to execute.
        /// </summary>
        /// <param name="consumer">The class who needs the result of a job.</param>
        /// <param name="func">The function (WITHOUT SIDEEFFECTS) to be executed.</param>
        /// <param name="parameters">The parameters passed to the job function.</param>
        public void AddJob(IConsumer consumer, Func<object, object> func, object parameters)
        {
            if (!Application.isPlaying || !allowMultithreading)
            {
                // Execute job sequentially
                consumer.ConsumeJob(func(parameters));
            }
            else
            {
                if (Pending >= maxJobs)
                {
                    switch (overloadProtocol)
                    {
                        case OverloadProtocol.Stall:
                            stallEvent.WaitOne();
                            break;
                        case OverloadProtocol.DropAndCallJobFailed:
                            consumer.JobFailed();
                            return;
                        case OverloadProtocol.DropAndThrowException:
                            throw new System.Exception("Current number of jobs exceeds max jobs.");
                        case OverloadProtocol.ReplaceAJobAndCallJobFailed:
                            // Remove a least-priority job
                            jobSemaphore.WaitOne(); // Potentially bad performance
                            (IConsumer consumer, Func<object, object> f, object o) job = (null, null, null);
                            lock (jobLock)
                            {
                                for (int i = numberOfPriorities - 1; i >= 0; i--)
                                {
                                    if (jobQueue[i].Count > 0)
                                    {
                                        job = jobQueue[i].Dequeue();
                                        break;
                                    }
                                }
                            }
                            job.consumer.JobFailed();
                            pendingEvent.Signal();
                            stallEvent.Set();
                            break;
                        case OverloadProtocol.FinishJobs:
                            FinishJobs();
                            break;
                    }
                }

                // Add job to jobqueue, which will be handled by a worker thread.
                pendingEvent.AddCount();
                //Debug.Log(pendingEvent.CurrentCount);
                stallEvent.Reset();

                // Add a job and increase semaphore
                int priority = numberOfPriorities - 1;
                if (consumer is IPrioritizedConsumer)
                    priority = ((IPrioritizedConsumer)consumer).Priority;

                lock (jobLock)
                {
                    jobQueue[priority].Enqueue((consumer, func, parameters));
                }
                jobSemaphore.Release();
            }
        }

        /// <summary>
        /// Every frame, see if a job has been finished. If so, give the result to the consumer.
        /// This is done on main thread.
        /// </summary>
        public void Update()
        {
            if (updateProtocol == UpdateProtocol.ConsumeAll)
            {
                ConsumeAll();
                return;
            }

            // If a job has been executed, send result to appropriate consumer
            (IConsumer consumer, object retval, bool successful) job = (null, null, false);
            bool hasJob = false;
            lock (consumeLock)
            {
                if (consumeQueue.Count > 0)
                {
                    job = consumeQueue.Dequeue();
                    hasJob = true;
                }
            }
            if (hasJob)
            {
                if (job.successful)
                {
                    job.consumer.ConsumeJob(job.retval);
                }
                else
                {
                    job.consumer.JobFailed();
                }
            }
        }

        private static void ClearThreads()
        {
            if (threads != null && threads.Length > 0)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Abort();
                }
            }
            threads = null;
        }

        private void FinishJobs()
        {
            // Main thread sleeps untill all jobs are done.
            pendingEvent.Signal();
            pendingEvent.Wait();
            pendingEvent.Reset(1);
            return;
        }

        private void FinishJobs(int ofPriority)
        {
            throw new System.Exception("Not implemented exception.");
        }

        private void ConsumeAll()
        {
            // CONSUME ALL
            lock (consumeLock)
            {
                while (consumeQueue.Count > 0)
                {
                    // If a job has been executed, send result to appropriate consumer
                    (IConsumer consumer, object retval, bool successful) job = (null, null, false);
                    bool hasJob = false;
                    job = consumeQueue.Dequeue();
                    hasJob = true;
                    if (job.successful)
                    {
                        job.consumer.ConsumeJob(job.retval);
                    }
                    else
                    {
                        job.consumer.JobFailed();
                    }
                }
            }
        }

        private void ClearJobs(Func<int, bool> toClear)
        {
            lock (jobLock)
            {
                for (int i = 0; i < numberOfPriorities; i++)
                {
                    if (toClear(i))
                    {
                        jobQueue[i] = new Queue<(IConsumer, Func<object, object>, object)>();
                    }
                }
            }
        }
        private void ClearJobs()
        {
            lock (jobLock)
            {
                for (int i = 0; i < numberOfPriorities; i++)
                {
                    jobQueue[i] = new Queue<(IConsumer, Func<object, object>, object)>();
                }
            }
        }

        // Annoying guistuff
        private void OnValidate()
        {
            if (numberOfPriorities <= 1)
            {
                dedicatedThreads = null;
            }

            if (maxJobs < threadAmount)
            {
                maxJobs = threadAmount;
            }

            if (maxJobs == threadAmount && overloadProtocol == OverloadProtocol.ReplaceAJobAndCallJobFailed)
            {
                maxJobs = threadAmount + 1;
                Debug.LogError("Max jobs must be larger than amount of threads, if ReplaceAJobAndCallJobFailed is the protocol.");
            }

            if (dedicatedThreads != null)
            {
                while (dedicatedThreads.Count > numberOfPriorities)
                {
                    dedicatedThreads.RemoveAt(dedicatedThreads.Count - 1);
                }

                for (int i = 0; i < dedicatedThreads.Count; i++)
                {
                    if (setThreads && dedicatedThreads[i].numberOfThreads > threadAmount)
                    {
                        Debug.LogError("Can't have number of dedicated threads exceed maximum number of threads.");
                    }

                    if (dedicatedThreads[i].priority > numberOfPriorities - 1)
                    {
                        dedicatedThreads[i].priority = numberOfPriorities - 1;
                    }
                    if (dedicatedThreads[i].numberOfThreads < 1)
                    {
                        dedicatedThreads[i].numberOfThreads = 1;
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void SetTestData(int maxJobs, int threadAmount, OverloadProtocol protocol = OverloadProtocol.Stall)
        {
            this.maxJobs = maxJobs;
            this.threadAmount = threadAmount;
            this.setThreads = true;
            this.dedicatedThreadsEnabled = false;
            this.overloadProtocol = protocol;
        }
#endif
    }
}