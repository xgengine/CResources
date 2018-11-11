using System;
using System.Collections;
//copy from Unity.ResourceManager 
namespace H3D.CResources
{
    /// <summary>
    /// Status values for IAsyncOperations
    /// </summary>
    public enum AsyncOperationStatus
    {
        None,
        Succeeded,
        Failed
    };


    /// <summary>
    /// Base interface of all async ops
    /// </summary>
    public interface IAsyncOperation : IEnumerator
    {
        /// <summary>
        /// returns the status of the operation
        /// </summary>
        /// <value><c>true</c> if is done; otherwise, <c>false</c>.</value>
        AsyncOperationStatus Status { get; }
        /// <summary>
        /// Release operation back to internal cache. This can be used to avoid garbage collection.
        /// </summary>
        bool Release();
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ResourceManagement.IAsyncOperation"/> is done.
        /// </summary>
        /// <value><c>true</c> if is done; otherwise, <c>false</c>.</value>
        bool IsDone { get; }

        /// <summary>
        /// Gets the percent complete of this operation.
        /// </summary>
        /// <value>The percent complete.</value>
        float PercentComplete { get; }

        /// <summary>
        /// Reset status and error
        /// </summary>
        void ResetStatus();
        /// <summary>
        /// Occurs when completed.
        /// </summary>
        event Action<IAsyncOperation> Completed;
        /// <summary>
        /// Gets the exception that caused this operation to change its status to Failure.
        /// </summary>
        /// <value>The exception.</value>
        Exception OperationException { get; }

		/// <summary>
		/// Gets the result.
		/// </summary>
		/// <value>The result.</value>
		object Result { get; }
	}

	
}
