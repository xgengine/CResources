using System;

namespace Build.Pipeline.Interfaces
{
    /// <summary>
    /// Base interface for all objects that can be stored in <see cref="IBuildContext"/>.
    /// </summary>
    public interface IContextObject { }


    /// <summary>
    /// Base interface for build data container system
    /// </summary>
    public interface IBuildContext
    {
        /// <summary>
        /// Checks the build context for existence of a data that is of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of data to check for existence.</typeparam>
        /// <returns><c>true</c> if the context contains specified type of data; otherwise, <c>false</c>.</returns>
        bool ContainsContextObject<T>() where T : IContextObject;

        /// <summary>
        /// Checks the build context for existence of a data that is of the specified type.
        /// </summary>
        /// <param name="type">Type of data to check for existence.</param>
        /// <returns><c>true</c> if the context contains specified type of data; otherwise, <c>false</c>.</returns>
        bool ContainsContextObject(Type type);

        /// <summary>
        /// Gets the data of the specified type contained in the build context.
        /// </summary>
        /// <typeparam name="T">Type of data to return.</typeparam>
        /// <returns>The type of data specified.</returns>
        T GetContextObject<T>() where T : IContextObject;

        /// <summary>
        /// Gets the data of the specified type contained in the build context.
        /// </summary>
        /// <param name="type">Type of data to return.</param>
        /// <returns>The type of data specified.</returns>
        IContextObject GetContextObject(Type type);

        /// <summary>
        /// Adds the data of the specified type to the build context.
        /// </summary>
        /// <typeparam name="T">Type of data to add.</typeparam>
        /// <param name="contextObject">Object holding the data to add.</param>
        void SetContextObject<T>(IContextObject contextObject) where T : IContextObject;

        /// <summary>
        /// Adds the data of the specified type to the build context.
        /// </summary>
        /// <param name="type">Type of data to add.</param>
        /// <param name="contextObject">Object holding the data to add.</param>
        void SetContextObject(Type type, IContextObject contextObject);

        /// <summary>
        /// Adds the data to the build context. Type will be inferred using Reflection.
        /// </summary>
        /// <param name="contextObject">Object holding the data to add.</param>
        void SetContextObject(IContextObject contextObject);

        /// <summary>
        /// Tries to get the data of the specified type contained in the build context.
        /// </summary>
        /// <typeparam name="T">Type of data to return.</typeparam>
        /// <param name="contextObject">The object holding the data to be returned if found.</param>
        /// <returns><c>true</c> if the context was able to returned the specified data; otherwise, <c>false</c>.</returns>
        bool TryGetContextObject<T>(out T contextObject) where T : IContextObject;

        /// <summary>
        /// Tries to get the data of the specified type contained in the build context.
        /// </summary>
        /// <param name="type">Type of data to return.</param>
        /// <param name="contextObject">The object holding the data to be returned if found.</param>
        /// <returns><c>true</c> if the context was able to returned the specified data; otherwise, <c>false</c>.</returns>
        bool TryGetContextObject(Type type, out IContextObject contextObject);
    }
}
