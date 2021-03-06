using System;
using System.Collections.Concurrent;
using System.Linq;
using RaccoonBlog.Web.Infrastructure.Indexes;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace RaccoonBlog.Web.Infrastructure.Raven
{
    /// <summary>
    /// This class manages the state of objects that desire a document session. We aren't relying on an IoC container here
    /// because this is the sole case where we actually need to do injection.
    /// </summary>
    public class DocumentStoreHolder
    {
    	private static IDocumentStore documentStore;

    	public static IDocumentStore DocumentStore
    	{
    		get { return (documentStore ?? (documentStore = CreateDocumentStore())); }
    	}

    	private static IDocumentStore CreateDocumentStore()
        {
            var store = new DocumentStore
                {
					ConnectionStringName = "RavenDB"
                }.Initialize();

            IndexCreation.CreateIndexes(typeof(Tags_Count).Assembly, store);

            return store;
        }

        private static readonly ConcurrentDictionary<Type, Accessors> AccessorsCache = new ConcurrentDictionary<Type, Accessors>();


        private static Accessors CreateAccessorsForType(Type type)
        {
            var sessionProp =
                type.GetProperties().FirstOrDefault(
                    x => x.PropertyType == typeof(IDocumentSession) && x.CanRead && x.CanWrite);
            if (sessionProp == null)
                return null;

            return new Accessors
                       {
                           Set = (instance, session) => sessionProp.SetValue(instance, session, null),
                           Get = instance => (IDocumentSession)sessionProp.GetValue(instance, null)
                       };
        }


        public static void TryAddSession(object instance)
        {
            var accessors = AccessorsCache.GetOrAdd(instance.GetType(), CreateAccessorsForType);

            if (accessors == null)
                return;

            accessors.Set(instance, DocumentStore.OpenSession());
        }

        public static void TryComplete(object instance, bool succcessfully)
        {
            Accessors accesors;
            if (AccessorsCache.TryGetValue(instance.GetType(), out accesors) == false || accesors == null)
                return;

            using (var documentSession = accesors.Get(instance))
            {
                if (documentSession == null)
                    return;

                if (succcessfully)
                    documentSession.SaveChanges();
            }
        }
     
        private class Accessors
        {
            public Action<object, IDocumentSession> Set;
            public Func<object, IDocumentSession> Get;
        }
    }
}
