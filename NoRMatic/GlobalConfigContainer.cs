using System;
using System.Collections.Generic;
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

        public Func<string> CurrentUserProvider { get; set; }
        public Action<string> LogListener { get; set; }

        private Func<string> _connectionStringProvider;
        public Func<string> ConnectionStringProvider {
            get {
                if (_connectionStringProvider == null)
                    _connectionStringProvider = () => {
                        if (ConfigurationManager.ConnectionStrings["NoRMaticConnectionString"] == null)
                            throw new ApplicationException("No connection string provider was defined, and the default 'NoRMaticConnectionString' was not present in the configuration file.");
                        return ConfigurationManager.ConnectionStrings["NoRMaticConnectionString"].ConnectionString;
                    };

                return _connectionStringProvider;
            }
            set { _connectionStringProvider = value; }
        }

        private Dictionary<Type, List<Func<dynamic, bool>>> _beforeSave = new Dictionary<Type, List<Func<dynamic, bool>>>();
        public Dictionary<Type, List<Func<dynamic, bool>>> BeforeSave {
            get { return _beforeSave; }
            set { _beforeSave = value; }
        }

        private Dictionary<Type, List<Action<dynamic>>> _afterSave = new Dictionary<Type, List<Action<dynamic>>>();
        public Dictionary<Type, List<Action<dynamic>>> AfterSave {
            get { return _afterSave; }
            set { _afterSave = value; }
        }

        private Dictionary<Type, List<Func<dynamic, bool>>> _beforeDelete = new Dictionary<Type, List<Func<dynamic, bool>>>();
        public Dictionary<Type, List<Func<dynamic, bool>>> BeforeDelete {
            get { return _beforeDelete; }
            set { _beforeDelete = value; }
        }

        private Dictionary<Type, List<Action<dynamic>>> _afterDelete = new Dictionary<Type, List<Action<dynamic>>>();
        public Dictionary<Type, List<Action<dynamic>>> AfterDelete {
            get { return _afterDelete; }
            set { _afterDelete = value; }
        }

        public void DropBehaviors() {
            _beforeSave.Clear();
            _afterSave.Clear();
            _beforeDelete.Clear();
            _afterDelete.Clear();
        }
    }
}