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

        public static void SetCurrentUserProvider(Func<string> provider) {
            ConfigContainer.Instance.CurrentUserProvider = provider;
        }

        public static void SetConnectionStringProvider(Func<string> provider) {
            ConfigContainer.Instance.ConnectionStringProvider = provider;
        }
    }
}
