﻿using RepoDb.Attributes;
using RepoDb.Exceptions;
using RepoDb.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RepoDb
{
    /// <summary>
    /// A class that is used to set a class property to be a primary property. This is an alternative class to <see cref="PrimaryAttribute"/> object.
    /// </summary>
    public static class PrimaryMapper
    {
        #region Privates

        private static readonly ConcurrentDictionary<int, ClassProperty> m_maps = new ConcurrentDictionary<int, ClassProperty>();

        #endregion

        #region Methods

        /*
         * Add
         */

        /// <summary>
        /// Adds a primary property mapping into an entity type (via expression).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="expression">The expression to be parsed.</param>
        public static void Add<TEntity>(Expression<Func<TEntity, object>> expression)
            where TEntity : class =>
            Add<TEntity>(expression, false);

        /// <summary>
        /// Adds a primary property mapping into an entity type (via expression).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="expression">The expression to be parsed.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add<TEntity>(Expression<Func<TEntity, object>> expression,
            bool force)
            where TEntity : class
        {
            // Validates
            ThrowNullReferenceException(expression, "Expression");

            // Get the property
            var property = ExpressionExtension.GetProperty<TEntity>(expression);

            // Get the class property
            var classProperty = GetClassProperty<TEntity>(property?.Name);
            if (classProperty == null)
            {
                throw new PropertyNotFoundException($"Property '{property.Name}' is not found at type '{typeof(TEntity).FullName}'.");
            }

            // Add to the mapping
            Add<TEntity>(classProperty, force);
        }

        /// <summary>
        /// Adds a primary property mapping into an entity type (via property name).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="propertyName">The name of the class property to be mapped.</param>
        public static void Add<TEntity>(string propertyName)
            where TEntity : class =>
            Add<TEntity>(propertyName, false);

        /// <summary>
        /// Adds a primary property mapping into an entity type (via property name).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="propertyName">The name of the class property to be mapped.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add<TEntity>(string propertyName,
            bool force)
            where TEntity : class
        {
            // Validates
            ThrowNullReferenceException(propertyName, "PropertyName");

            // Get the class property
            var classProperty = GetClassProperty<TEntity>(propertyName);
            if (classProperty == null)
            {
                throw new PropertyNotFoundException($"Property '{propertyName}' is not found at type '{typeof(TEntity).FullName}'.");
            }

            // Add to the mapping
            Add<TEntity>(classProperty, force);
        }

        /// <summary>
        /// Adds a primary property mapping into an entity type (via <see cref="Field"/> object).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="field">The instance of <see cref="Field"/> to be mapped.</param>
        public static void Add<TEntity>(Field field)
            where TEntity : class =>
            Add<TEntity>(field, false);

        /// <summary>
        /// Adds a primary property mapping into an entity type (via <see cref="Field"/> object).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="field">The instance of <see cref="Field"/> to be mapped.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add<TEntity>(Field field,
            bool force)
            where TEntity : class
        {
            // Validates
            ThrowNullReferenceException(field, "Field");

            // Get the class property
            var classProperty = GetClassProperty<TEntity>(field.Name);
            if (classProperty == null)
            {
                throw new PropertyNotFoundException($"Property '{field.Name}' is not found at type '{typeof(TEntity).FullName}'.");
            }

            // Add to the mapping
            Add<TEntity>(classProperty, force);
        }

        /// <summary>
        /// Adds a primary property mapping into a <see cref="ClassProperty"/> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="classProperty">The instance of <see cref="ClassProperty"/> to be mapped.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        internal static void Add<TEntity>(ClassProperty classProperty,
            bool force)
            where TEntity : class =>
            Add(typeof(TEntity), classProperty, force);

        /// <summary>
        /// Adds a primary property mapping into a <see cref="ClassProperty"/> object.
        /// </summary>
        /// <param name="entityType">The type of the data entity.</param>
        /// <param name="classProperty">The instance of <see cref="ClassProperty"/> to be mapped.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        internal static void Add(Type entityType,
            ClassProperty classProperty,
            bool force)
        {
            // Validate
            ThrowNullReferenceException(entityType, "EntityType");
            ThrowNullReferenceException(classProperty, "ClassProperty");

            // Variables
            var key = GenerateHashCode(entityType);
            var value = (ClassProperty)null;

            // Try get the cache
            if (m_maps.TryGetValue(key, out value))
            {
                if (force)
                {
                    // Update the existing one
                    m_maps.TryUpdate(key, classProperty, value);
                }
                else
                {
                    // Throws an exception
                    throw new MappingExistsException($"The primary property mapping to type '{classProperty.PropertyInfo.DeclaringType.FullName}' already exists.");
                }
            }
            else
            {
                // Add the mapping
                m_maps.TryAdd(key, classProperty);
            }
        }

        /*
         * Get
         */

        /// <summary>
        /// Gets the instance of <see cref="ClassProperty"/> that is mapped as primary key.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <returns>An instance of the mapped <see cref="ClassProperty"/> object.</returns>
        public static ClassProperty Get<TEntity>()
            where TEntity : class =>
            Get(typeof(TEntity));

        /// <summary>
        /// Gets the instance of <see cref="ClassProperty"/> that is mapped as primary key.
        /// </summary>
        /// <param name="entityType">The target type.</param>
        /// <returns>An instance of the mapped <see cref="ClassProperty"/> object.</returns>
        public static ClassProperty Get(Type entityType)
        {
            // Validate
            ThrowNullReferenceException(entityType, "Type");

            // Variables
            var key = GenerateHashCode(entityType);
            var value = (ClassProperty)null;

            // Try get the value
            m_maps.TryGetValue(key, out value);

            // Return the value
            return value;
        }

        /*
         * Remove
         */

        /// <summary>
        /// Removes the exising mapped primary property of the data entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        public static void Remove<TEntity>()
            where TEntity : class =>
            Remove(typeof(TEntity));

        /// <summary>
        /// Removes the exising mapped primary property of the data entity.
        /// </summary>
        /// <param name="entityType">The target type.</param>
        public static void Remove(Type entityType)
        {
            // Validate
            ThrowNullReferenceException(entityType, "Type");

            // Variables
            var key = GenerateHashCode(entityType);
            var value = (ClassProperty)null;

            // Try get the value
            m_maps.TryRemove(key, out value);
        }

        /*
         * Clear
         */

        /// <summary>
        /// Clears all the existing cached primary properties.
        /// </summary>
        public static void Clear()
        {
            m_maps.Clear();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Generates a hashcode for caching.
        /// </summary>
        /// <param name="type">The type of the data entity.</param>
        /// <returns>The generated hashcode.</returns>
        private static int GenerateHashCode(Type type)
        {
            return TypeExtension.GenerateHashCode(type);
        }

        /// <summary>
        /// Gets the instance of <see cref="ClassProperty"/> object from of the data entity based on name.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="propertyName">The property name.</param>
        /// <returns>An instance of <see cref="ClassProperty"/> object.</returns>
        private static ClassProperty GetClassProperty<TEntity>(string propertyName)
            where TEntity : class
        {
            var properties = PropertyCache.Get<TEntity>();
            return properties.FirstOrDefault(
                p => string.Equals(p.PropertyInfo.Name, propertyName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Validates the target object presence.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object to be checked.</param>
        /// <param name="argument">The name of the argument.</param>
        private static void ThrowNullReferenceException<T>(T obj,
            string argument)
        {
            if (obj == null)
            {
                throw new NullReferenceException($"The argument '{argument}' cannot be null.");
            }
        }

        #endregion
    }
}
