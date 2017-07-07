using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Infrastructure.BaseDataLoader;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Infrastructure.Validation;
using NHibernate.Linq;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The default datset target mapper for Monahrq.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monahrq.DataSets.Model.ITargetMapper" />
    public class TargetMapper<T> : ITargetMapper
        where T: DatasetRecord, new()
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public T Target
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                var prop = ElementMaps[name].Property;
                return prop.GetValue(Target);
            }
            set
            {
                if (GetFromExisting(name, value)) return;
                ElementMappingModel<T> test;
                if (ElementMaps.TryGetValue(name, out test))
                {
                    test.ApplyValue(Target, value);
                }
            }
        }

        Lazy<IDictionary<string, IDictionary<object, object>>> LazyExistingValues
        {
            get;set;
        }
       
        private IDictionary<string, IDictionary<object, object>> ExistingValues { get {return LazyExistingValues.Value; }} 
        private bool GetFromExisting(string name, object value)
        {
            var valueList = GuardExistingValueList(name);
            var key = value ?? CrosswalkScopeBin.NullKey;
            object result;
            var exists = valueList.TryGetValue(key, out result);
            if (exists)
            {
                var elemEx = result as ElementMappingValueException;
                if (elemEx != null)
                {
                    Errors.Add(elemEx);
                }
                else
                {
                    ElementMaps[name].Property.SetValue(Target, result);
                }
            }
            return exists;
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified element.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public object this[Element element]
        {
            get
            {
                return this[element.Name];
            }
            set
            {
                this[element.Name] = value == DBNull.Value ? null : value;
            }
        }

        /// <summary>
        /// Gets or sets the element maps.
        /// </summary>
        /// <value>
        /// The element maps.
        /// </value>
        Dictionary<string, ElementMappingModel<T>> ElementMaps
        {
            get;
            set;
        }

        IEnumerable<IMappedFieldEntryViewModel> Crosswalks { get; set; }

        IDictionary<object, object> GuardExistingValueList(string name)
        {
            IDictionary<object, object> valueList;
           
            if(!ExistingValues.TryGetValue(name, out valueList))
            {
                ExistingValues.Add(name, valueList = new Dictionary<object, object>());
            }
            return valueList;
        }

        /// <summary>
        /// Gets or sets the target definition.
        /// </summary>
        /// <value>
        /// The target definition.
        /// </value>
        Target TargetDefinition { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetMapper{T}"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="crosswalks">The crosswalks.</param>
        public TargetMapper(Target target, IEnumerable<IMappedFieldEntryViewModel> crosswalks)
        {
            TargetDefinition = target;
            var existing = new Dictionary<string,IDictionary<object,object>>();
            LazyExistingValues = new Lazy<IDictionary<string,IDictionary<object,object>>>(()=> existing, true);

            Crosswalks = crosswalks ?? Enumerable.Empty<IMappedFieldEntryViewModel>();
            Reset();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Errors = new List<ElementMappingValueException>();
            if (ElementMaps == null)
            {
                BuildElementMaps(TargetDefinition);
            }
            Target = new T();
        }

        /// <summary>
        /// Builds the element maps.
        /// </summary>
        /// <param name="target">The target.</param>
        private void BuildElementMaps(Target target)
        {
            using (var session = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>().SessionFactory.OpenSession())
            {
                ElementMaps = new Dictionary<string, ElementMappingModel<T>>();
                var elementProps = session.Query<Element>().Where(e => e.Owner.Id == target.Id).ToList();
                //ElementMappingModel<T>.PropertyTypes.Clear();
                foreach (var elem in elementProps)
                {
                    var xwalk = Crosswalks.Where(x => string.Equals(elem.Name, x.Element.Name, StringComparison.OrdinalIgnoreCase)).SelectMany(x => x.CrosswalkModels);
                    var map = new ElementMappingModel<T>(elem, xwalk);
                    map.ValueInvalid += (o, e) => Errors.Add(e.Data);
                    map.ValueEvaluated += (o, e) => AddToExisting(elem.Name, e.Data);
                    ElementMaps.Add(elem.Name, map);
                }
            }
        }

        private void AddToExisting(string elementName, ElementMappingValueEvaluated mapping)
        {
            var dict = GuardExistingValueList(elementName);
            var key = mapping.Source ?? CrosswalkScopeBin.NullKey;
            dict[key] = mapping.Translation;
        }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public List<ElementMappingValueException> Errors { get; private set; }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        IEnumerable<ElementMappingValueException> ITargetMapper.Errors { get { return this.Errors; } }

        dynamic ITargetMapper.Target
        {
            get 
            {
                return this.Target;
            }
        }

        /// <summary>
        /// Creates the bulk importer.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="target">The target.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>a dynamic object of the that implements <see cref="Monahrq.DataSets.Model.ITargetMapper"/></returns>
        public dynamic CreateBulkImporter(IDbConnection connection, Target target = null, int? batchSize = null) 
        {
            return new BulkInsert<T, int>(connection, target, batchSize);
        }
    }

    /// <summary>
    /// The dataset target factory.
    /// </summary>
    public class TargetMapperFactory
    {
        /// <summary>
        /// Gets the validation engine.
        /// </summary>
        /// <value>
        /// The validation engine.
        /// </value>
        public InstanceValidator ValidationEngine { get; private set; }

        /// <summary>
        /// Gets or sets the target definition.
        /// </summary>
        /// <value>
        /// The target definition.
        /// </value>
        private Target TargetDefinition { get; set; }

        /// <summary>
        /// Gets or sets the typeto map.
        /// </summary>
        /// <value>
        /// The typeto map.
        /// </value>
        private Type TypetoMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetMapperFactory"/> class.
        /// </summary>
        /// <param name="targetDefinition">The target definition.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public TargetMapperFactory(Target targetDefinition, DatasetContext context = null)
        {
            TargetDefinition = targetDefinition;
            if(!TargetDefinition.IsCustom )
                TypetoMap = Type.GetType(TargetDefinition.ClrType);
            else if (TargetDefinition.IsCustom && context != null)
            {
                TypetoMap = context.TargetType;
            }
            else
            {
                throw new InvalidOperationException(string.Format("This target can't be mapped due to an invalid target type \"{0}\".", TargetDefinition.Name));
            }
                    
            ValidationEngine = new InstanceValidator(TypetoMap);
        }

        /// <summary>
        /// Creates the mapper.
        /// </summary>
        /// <param name="crosswalks">The crosswalks.</param>
        /// <returns></returns>
        public ITargetMapper CreateMapper(IEnumerable<IMappedFieldEntryViewModel> crosswalks)
        {
            crosswalks = crosswalks ?? Enumerable.Empty<IMappedFieldEntryViewModel>();
            var typeToConstruct = typeof(TargetMapper<>);
            typeToConstruct = typeToConstruct.MakeGenericType(TypetoMap);
            var xwalk = typeof(IEnumerable<IMappedFieldEntryViewModel>);
            var ctor = typeToConstruct.GetConstructor(new Type[] {typeof(Target), xwalk });
           // var args = new object[] { TargetDefinition, crosswalks };
            return ctor.Invoke(new object[]{TargetDefinition, crosswalks} ) as ITargetMapper;
        }

        /// <summary>
        /// Gets the dummy target.
        /// </summary>
        /// <value>
        /// The dummy target.
        /// </value>
        static Target DummyTarget
        {
            get
            {
                var wing = WingRepository.New(Guid.NewGuid().ToString());
                return TargetRepository.New(wing, Guid.Empty, Guid.NewGuid().ToString());
            }
        }
    }

}
