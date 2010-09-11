using System;
using System.Configuration;

namespace NoRMatic {

    internal sealed class GlobalConfigContainer {

        class Nested {
            static Nested() { } // Constructor so compiler doesn't mark beforefieldinit
            internal static readonly GlobalConfigContainer instance = new GlobalConfigContainer();
        }

        public static GlobalConfigContainer Instance {
            get { return Nested.instance; }
        }

        public string ConnectionStringName { get; set; }
        public Func<string> CurrentUserProvider { get; set; }

        private Func<string> _connectionStringProvider = () => ConfigurationManager.ConnectionStrings["NoRMaticConnectionString"].ConnectionString;
        public Func<string> ConnectionStringProvider {
            get { return _connectionStringProvider; }
            set { _connectionStringProvider = value; }
        }
    }
}