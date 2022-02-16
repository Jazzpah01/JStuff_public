using System.Collections;

namespace JStuff.Threading
{
    /// <summary>
    /// The implementing class will expect some work to be done
    /// by the JobManager.
    /// </summary>
    public interface IConsumer
    {
        void ConsumeJob(object data);
        void JobFailed();
    }

    public interface IPrioritizedConsumer : IConsumer
    {
        int Priority { get; }
    }
}