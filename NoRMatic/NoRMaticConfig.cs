using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoRMatic {

    public static class NoRMaticConfig {

        public static string ConnectionString {
            get { return GlobalConfigContainer.Instance.ConnectionStringProvider(); }
        }

        public static void Initialize() {
            
            var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(x => typeof(INoRMaticInitializer).IsAssignableFrom(x) && 
                    x != typeof(INoRMaticInitializer));

            foreach (var type in types) {
                try {
                    var initializer = Activator.CreateInstance(type);
                    ((INoRMaticInitializer) initializer).Setup();
                } catch { }
            }
        }

        /// <summary>
        /// In order for user auditing to work, an anonymous function will need to be supplied as the current user provider.
        /// This function will simply return a string for the current user.  This provider pattern allows for the method
        /// which looks up the current user to be changed at runtime.
        /// </summary>
        public static void SetCurrentUserProvider(Func<string> provider) {
            GlobalConfigContainer.Instance.CurrentUserProvider = provider;
        }

        /// <summary>
        /// By default, NoRMatic will look for a connection string called 'NoRMaticConnectionString' in the connection strings
        /// section of the config file.  However, an override can be applied here which will allow any requests for the current
        /// connection string to be retrieved via a provider function.  This allows for runtime flexibility on how the connection
        /// string is created.
        /// </summary>
        public static void SetConnectionStringProvider(Func<string> provider) {
            GlobalConfigContainer.Instance.ConnectionStringProvider = provider;
        }

        /// <summary>
        /// Extends the Where conditions of any query executed for types implementing the given abstract type;
        /// </summary>
        public static void AddQueryBehavior<T>(Expression<Func<T , bool>> action) {
            
            if (!GlobalConfigContainer.Instance.Query.ContainsKey(typeof(T)))
                GlobalConfigContainer.Instance.Query.Add(typeof(T), new List<Expression<Func<T, bool>>>());

            ((List<Expression<Func<T, bool>>>)GlobalConfigContainer.Instance.Query[typeof(T)]).Add(action);
        }

        /// <summary>
        /// BeforeSaveBehaviors are functions called before any Save for entities implementing a given abstract marker.  If any of these
        /// functions return 'true' the save will NOT proceed.  This can be useful if there are pre-save validations or other checks
        /// that need to be made that may stop the save.
        /// </summary>
        public static void AddBeforeSaveAbstractBehavior<T>(Func<dynamic, bool> action) {

            if (!GlobalConfigContainer.Instance.BeforeSave.ContainsKey(typeof(T)))
                GlobalConfigContainer.Instance.BeforeSave.Add(typeof(T), new List<Func<dynamic, bool>>());

            GlobalConfigContainer.Instance.BeforeSave[typeof(T)].Add(action);
        }

        /// <summary>
        /// Actions executed after a save and after a version is created if EnableVersioning is set.
        /// </summary>
        public static void AddAfterSaveAbstractBehavior<T>(Action<dynamic> action) {

            if (!GlobalConfigContainer.Instance.AfterSave.ContainsKey(typeof(T)))
                GlobalConfigContainer.Instance.AfterSave.Add(typeof(T), new List<Action<dynamic>>());

            GlobalConfigContainer.Instance.AfterSave[typeof(T)].Add(action);
        }

        /// <summary>
        /// BeforeDeleteBehaviors are functions called before any Delete for the collection.  If any of these
        /// functions return 'true' the delete will not proceed (including soft deletes).
        /// </summary>
        public static void AddBeforeDeleteAbstractBehavior<T>(Func<dynamic, bool> action) {

            if (!GlobalConfigContainer.Instance.BeforeDelete.ContainsKey(typeof(T)))
                GlobalConfigContainer.Instance.BeforeDelete.Add(typeof(T), new List<Func<dynamic, bool>>());
            
            GlobalConfigContainer.Instance.BeforeDelete[typeof(T)].Add(action);
        }

        /// <summary>
        /// Actions executed after a delete.
        /// </summary>
        public static void AddAfterDeleteAbstractBehavior<T>(Action<dynamic> action) {

            if (!GlobalConfigContainer.Instance.AfterDelete.ContainsKey(typeof(T)))
                GlobalConfigContainer.Instance.AfterDelete.Add(typeof(T), new List<Action<dynamic>>());

            GlobalConfigContainer.Instance.AfterDelete[typeof(T)].Add(action);
        }

        /// <summary>
        /// Removes all registered abstract behaviors.
        /// </summary>
        public static void DropAbstractBehaviors() {
            GlobalConfigContainer.Instance.DropBehaviors();
        }
    }
}
