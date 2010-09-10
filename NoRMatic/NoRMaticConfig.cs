using System;
using System.Configuration;
using System.Linq;

namespace NoRMatic {

    public static class NoRMaticConfig {

        public static string ConnectionString {
            get { return ConfigContainer.Instance.ConnectionStringProvider(); }
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
            ConfigContainer.Instance.CurrentUserProvider = provider;
        }

        /// <summary>
        /// By default, NoRMatic will look for a connection string called 'NoRMaticConnectionString' in the connection strings
        /// section of the config file.  However, an override can be applied here which will allow any requests for the current
        /// connection string to be retrieved via a provider function.  This allows for runtime flexibility on how the connection
        /// string is created.
        /// </summary>
        public static void SetConnectionStringProvider(Func<string> provider) {
            ConfigContainer.Instance.ConnectionStringProvider = provider;
        }
    }
}
