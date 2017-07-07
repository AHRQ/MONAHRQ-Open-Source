using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The Monahrq data domain service interface/contract.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        void Dispose();
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback">The callback.</param>
        void GetAll<T>(Action<List<T>, Exception> callback) where T : IEntity;
        /// <summary>
        /// Gets the enity by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="callback">The callback.</param>
        void GetEnityById<T>(object id, Action<object, Exception> callback) where T : IEntity;
        /// <summary>
        /// Refreshes the enity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void RefreshEnity<T>(T entity, Action<bool, Exception> callback) where T : IEntity;
        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void Save<T>(T entity, Action<object, Exception> callback) where T : IEntity;
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void Delete<T>(T entity, Action<bool, Exception> callback) where T : IEntity;
    }
}