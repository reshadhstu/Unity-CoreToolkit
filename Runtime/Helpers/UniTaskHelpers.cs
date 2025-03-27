using System;
using Cysharp.Threading.Tasks;

namespace CoreToolkit.Runtime.Helpers
{
    /// <summary>
    /// Provides helper methods for working with UniTask, including methods to await multiple tasks
    /// and capture their results or exceptions.
    /// </summary>
    public class UniTaskHelpers {
        /// <summary>
        /// Awaits two UniTasks and returns a tuple containing the results or exceptions for each task.
        /// Ensures that both tasks complete before returning, even if one or both fail.
        /// </summary>
        /// <typeparam name="T1">The result type of the first task.</typeparam>
        /// <typeparam name="T2">The result type of the second task.</typeparam>
        /// <param name="task1">The first task to await.</param>
        /// <param name="task2">The second task to await.</param>
        /// <returns>A tuple containing the results or exceptions to both tasks.</returns>
        public static async UniTask<(Result<T1> Task1Result, Result<T2> Task2Result)> WhenAllSettled<T1, T2>(UniTask<T1> task1, UniTask<T2> task2) {
            var task1Result = await WrapTaskResult(task1);
            var task2Result = await WrapTaskResult(task2);

            return (task1Result, task2Result);
        }

        static async UniTask<Result<T>> WrapTaskResult<T>(UniTask<T> task) {
            try {
                var result = await task;
                return new Result<T>(result, null);
            }
            catch (Exception ex) {
                return new Result<T>(default, ex);
            }
        }

        public struct Result<T> {
            public T Value { get; }
            public Exception Exception { get; }

            public bool IsSuccess => Exception == null;

            public Result(T value, Exception exception) {
                Value = value;
                Exception = exception;
            }
        }
    }
}