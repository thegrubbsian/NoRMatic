using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;

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

        private Dictionary<Type, object> _query = new Dictionary<Type, object>();
        public Dictionary<Type, object> Query {
            get { return _query; }
            set { _query = value; }
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
            _query.Clear();
            _beforeSave.Clear();
            _afterSave.Clear();
            _beforeDelete.Clear();
            _afterDelete.Clear();
        }
    }
}