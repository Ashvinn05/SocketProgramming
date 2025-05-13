using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for tasks, including cancellation support.
/// </summary>
namespace SocketProgramApp.Utils
{
    /// <summary>
    /// Provides extension methods for tasks, including cancellation support.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Awaits the specified task, allowing it to be canceled using the provided cancellation token.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the task.</typeparam>
        /// <param name="task">The task to await.</param>
        /// <param name="cancellationToken">The cancellation token to signal cancellation.</param>
        /// <returns>The result of the task if it completes successfully.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the task is canceled.</exception>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            return await task;
        }
    }
}