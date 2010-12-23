﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Myre.Entities.Behaviours;
using System.Collections.ObjectModel;
using Myre;
using Myre.Extensions;
using Ninject;
using Ninject.Parameters;

namespace Myre.Entities
{
    public struct EntityVersion
    {
        public static readonly EntityVersion None = new EntityVersion(null, 0);

        public EntityDescription Creator { get; private set; }
        public uint Version { get; private set; }

        public EntityVersion(EntityDescription creator, uint version)
            : this()
        {
            Creator = creator;
            Version = version;
        }
    }

    /// <summary>
    /// A class which represents a collection of related properties and behaviours.
    /// </summary>
    public class Entity
        : IDisposableObject, IRecycleable
    {
        public sealed class InitialisationContext
        {
            private Entity entity;
            internal bool frozen;

            internal InitialisationContext(Entity entity)
            {
                this.entity = entity;
            }

            public Property<T> GetOrCreateProperty<T>(string name, T defaultValue = default(T), PropertyCopyBehaviour copyBehaviour = PropertyCopyBehaviour.None)
            {
                CheckFrozen();

                var property = entity.GetProperty<T>(name);
                if (property == null)
                {
                    property = new Property<T>(name, defaultValue, copyBehaviour);
                    entity.AddProperty(property);
                }

                return property;
            }

            public Property<T> GetProperty<T>(string name)
            {
                CheckFrozen();
                return entity.GetProperty<T>(name);
            }

            public T GetBehaviour<T>(string name = null)
                where T : Behaviour
            {
                CheckFrozen();
                return entity.GetBehaviour<T>(name);
            }

            public T[] GetBehaviours<T>()
                where T : Behaviour
            {
                CheckFrozen();
                return entity.GetBehaviours<T>();
            }

            private void CheckFrozen()
            {
                if (frozen)
                    throw new InvalidOperationException("Entity initialisation contexts can only be used during initialisation.");
            }
        }


        private Dictionary<string, IProperty> properties;
        private Dictionary<Type, Behaviour[]> behaviours;

        private List<IProperty> propertiesList;
        private List<Behaviour> behavioursList;

        private InitialisationContext initialisationContext;

        public EntityVersion Version { get; private set; }

        /// <summary>
        /// Gets the scene this entity belongs to.
        /// </summary>
        /// <value>The scene.</value>
        public Scene Scene { get; internal set; }

        /// <summary>
        /// Gets the behaviours this entity contains.
        /// </summary>
        /// <value>The behaviours.</value>
        public ReadOnlyCollection<Behaviour> Behaviours { get; private set; }

        /// <summary>
        /// Gets the properties this entity contains.
        /// </summary>
        /// <value>The properties.</value>
        public ReadOnlyCollection<IProperty> Properties { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        internal Entity(IEnumerable<IProperty> properties, IEnumerable<Behaviour> behaviours, EntityVersion version)
        {
            this.Version = version;

            // create public read-only collections
            this.propertiesList = new List<IProperty>(properties);
            this.behavioursList = new List<Behaviour>(behaviours);
            this.Properties = new ReadOnlyCollection<IProperty>(propertiesList);
            this.Behaviours = new ReadOnlyCollection<Behaviour>(behavioursList);

            // add properties
            this.properties = new Dictionary<string, IProperty>();
            foreach (var item in Properties)
                this.properties.Add(item.Name, item);

            // sort behaviours by their type
            var catagorised = new Dictionary<Type, List<Behaviour>>();
            foreach (var item in Behaviours)
            {
                CatagoriseBehaviour(catagorised, item);
                item.Owner = this;
            }

            // add behaviours
            this.behaviours = new Dictionary<Type, Behaviour[]>();
            foreach (var item in catagorised)
                this.behaviours.Add(item.Key, item.Value.ToArray());

            // create initialisation context
            this.initialisationContext = new InitialisationContext(this);

            Initialise();
        }

        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="behaviours">The behaviours.</param>
        public Entity(IKernel kernel, IEnumerable<PropertyData> propertyData, IEnumerable<BehaviourData> behaviourData)
            : this(kernel, propertyData, behaviourData, EntityVersion.None)
        {
        }

        internal Entity(IKernel kernel, IEnumerable<PropertyData> propertyData, IEnumerable<BehaviourData> behaviourData, EntityVersion version)
        {
            this.Version = version;
            
            // create properties
            this.properties = new Dictionary<string, IProperty>();
            foreach (var item in propertyData)
                AddProperty(kernel, item);
            
            // create behaviours
            var b = CreateBehaviours(kernel, behaviourData).ToArray();
            var catagorised = new Dictionary<Type, List<Behaviour>>();
            foreach (var item in b)
                CatagoriseBehaviour(catagorised, item);

            this.behaviours = new Dictionary<Type, Behaviour[]>();
            foreach (var item in catagorised)
                behaviours.Add(item.Key, item.Value.ToArray());

            // create public read-only collections
            this.Properties = new ReadOnlyCollection<IProperty>(properties.Values.ToArray());
            this.Behaviours = new ReadOnlyCollection<Behaviour>(b);
        }
         * */

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Entity"/> is reclaimed by garbage collection.
        /// </summary>
        ~Entity()
        {
            Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposeManagedResources"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            IsDisposed = true;
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        internal void Initialise()
        {
            initialisationContext.frozen = false;
            foreach (var item in Behaviours)
            {
                if (!item.IsReady)
                    item.Initialise(initialisationContext);
            }
            initialisationContext.frozen = true;
        }

        /// <summary>
        /// Shuts down this instance.
        /// </summary>
        internal void Shutdown()
        {
            foreach (var item in Behaviours)
            {
                if (item.IsReady)
                    item.Shutdown();
            }
        }

        /// <summary>
        /// Prepares this instance for re-use.
        /// </summary>
        public void Recycle()
        {
            if (Scene != null)
                Scene.Remove(this);

            if (Version.Creator != null)
                Version.Creator.Recycle(this);
        }

        internal void AddProperty(IProperty property)
        {
            properties.Add(property.Name, property);
            propertiesList.Add(property);
        }

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <param name="name">The name of the propery.</param>
        /// <returns>The property with the specified name and data type.</returns>
        public IProperty GetProperty(string name)
        {
            IProperty property = null;
            properties.TryGetValue(name, out property);
            return property;
        }

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <typeparam name="T">The data type this property contains.</typeparam>
        /// <param name="name">The name of the propery.</param>
        /// <returns>The property with the specified name and data type.</returns>
        public Property<T> GetProperty<T>(string name)
        {
            return GetProperty(name) as Property<T>;
        }

        /// <summary>
        /// Gets the behaviour of the specified type and name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Behaviour GetBehaviour(Type type, string name = null)
        {
            Behaviour[] array;
            if (behaviours.TryGetValue(type, out array))
            {
                foreach (var item in array)
                {
                    if (item.Name == name)
                        return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the behaviours of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Behaviour[] GetBehaviours(Type type)
        {
            Behaviour[] array = null;
            behaviours.TryGetValue(type, out array);

            return array;
        }

        /// <summary>
        /// Gets the behaviour of the specified type and name.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T GetBehaviour<T>(string name = null)
            where T : Behaviour
        {
            return GetBehaviour(typeof(T), name) as T;
        }

        /// <summary>
        /// Gets the behaviours of the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns></returns>
        public T[] GetBehaviours<T>()
            where T : Behaviour
        {
            return GetBehaviours(typeof(T)) as T[];
        }

        /*
        internal IProperty AddProperty(IKernel kernel, PropertyData propertyData)
        {
            var property = CreatePropertyInstance(kernel, propertyData);
            properties.Add(property.Name, property);

            return property;
        }

        private IProperty CreatePropertyInstance(IKernel kernel, PropertyData property)
        {
            Type type;
            if (!propertyTypes.TryGetValue(property.DataType, out type))
            {
                type = genericType.MakeGenericType(property.DataType);
                propertyTypes.Add(property.DataType, type);
            }

            var data = property.CreateValue(kernel);
            return type.CreateInstance(
                new Type[] { typeof(string), property.DataType, typeof(PropertyCopyBehaviour), typeof(bool) },
                new object[] { property.Name, data, property.CopyBehaviour, property.Serialise })
               as IProperty;
        }

        private IEnumerable<Behaviour> CreateBehaviours(IKernel kernel, IEnumerable<BehaviourData> behaviourData)
        {
            foreach (var item in behaviourData)
            {
                var name = new ConstructorArgument("name", item.Name);
                var settings = new ConstructorArgument("settings", item.Settings);
                var entity = new Parameter("entity", this, true);
                var behaviour = kernel.Get(item.Type, name, settings, entity) as Behaviour;
                behaviour.Settings = item.Settings;
                behaviour.Owner = this;

                yield return behaviour;
            }
        }
        */

        private void CatagoriseBehaviour(Dictionary<Type, List<Behaviour>> catagorised, Behaviour behaviour)
        {
            Type type = behaviour.GetType();

            List<Behaviour> behavioursOfType;
            if (!catagorised.TryGetValue(type, out behavioursOfType))
            {
                behavioursOfType = new List<Behaviour>();
                catagorised.Add(type, behavioursOfType);
            }

            behavioursOfType.Add(behaviour);
        }
    }
}