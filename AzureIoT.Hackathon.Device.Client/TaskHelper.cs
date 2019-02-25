namespace AzureIoT.Hackathon.Device.Client
{
    using System;
    using System.Threading.Tasks;

    public static class TaskHelper
    {
        public static void IgnoreFault(this Task task)
        {
            if (task.IsCompleted)
            {
                var ignored = task.Exception;
            }
            else
            {
                task.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public static void Fork(this Task thisTask)
        {
            if (thisTask == null)
            {
                throw new InvalidOperationException("Fork a null task!");
            }

            thisTask.ContinueWith(t => Console.WriteLine($"Task faulted: {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
