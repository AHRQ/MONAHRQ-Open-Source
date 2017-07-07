using Monahrq.Infrastructure.Entities.Domain;
using NHibernate;
using System;
using System.ComponentModel;
using Castle.DynamicProxy;
using IInterceptor = Castle.DynamicProxy.IInterceptor;

namespace Monahrq.Infrastructure.Data.Interceptors
{
    public class DataBindingInterceptor : EmptyInterceptor
    {
        public ISessionFactory SessionFactory { set; get; }

        public override object Instantiate(string clazz, EntityMode entityMode, object id)
        {
            if (entityMode == EntityMode.Poco)
            {
                Type type = Type.GetType(clazz);
                if (type != null)
                {
                    var instance = DataBindingFactory.Create(type);
                    SessionFactory.GetClassMetadata(clazz).SetIdentifier(instance, id, entityMode);
                    return instance;
                }
            }
            return base.Instantiate(clazz, entityMode, id);
        }

        public override string GetEntityName(object entity)
        {
            var markerInterface = entity as DataBindingFactory.IMarkerInterface;
            if (markerInterface != null)
                return markerInterface.TypeName;
            return base.GetEntityName(entity);
        }
    }


    public static class DataBindingFactory
    {
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public static T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        public static object Create(Type type)
        {
            return _proxyGenerator.CreateClassProxy(type, new Type[]
			{
				typeof (INotifyPropertyChanged),
                typeof(IMarkerInterface),
				typeof (IEntity<int>)
			}, new NotifyPropertyChangedInterceptor(type.FullName));
        }

        public interface IMarkerInterface
        {
            string TypeName { get; }
        }

        public class NotifyPropertyChangedInterceptor : IInterceptor
        {
            private readonly string typeName;
            private PropertyChangedEventHandler _subscribers = delegate { };

            public NotifyPropertyChangedInterceptor(string typeName)
            {
                this.typeName = typeName;
            }

            public void Intercept(IInvocation invocation)
            {
                if (invocation.Method.DeclaringType == typeof(IMarkerInterface))
                {
                    invocation.ReturnValue = typeName;
                    return;
                }
                if (invocation.Method.DeclaringType == typeof(INotifyPropertyChanged))
                {
                    var propertyChangedEventHandler = (PropertyChangedEventHandler)invocation.Arguments[0];
                    if (invocation.Method.Name.StartsWith("add_"))
                    {
                        _subscribers += propertyChangedEventHandler;
                    }
                    else
                    {
                        _subscribers -= propertyChangedEventHandler;
                    }
                    return;
                }

                invocation.Proceed();

                if (invocation.Method.Name.StartsWith("set_"))
                {
                    var propertyName = invocation.Method.Name.Substring(4);
                    _subscribers(invocation.InvocationTarget, new PropertyChangedEventArgs(propertyName));
                }
            }
        }


        public static ISessionFactory ExposeSessionFactory(this ISessionFactory sessionFactory,
                Action<ISessionFactory> factoryAction)
        {
            factoryAction(sessionFactory);
            return sessionFactory;
        }
    }



}
