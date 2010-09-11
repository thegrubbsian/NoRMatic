using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NoRMatic {

    internal sealed class ModelConfigContainer<T> {

        class Nested {
            static Nested() { } // Constructor so compiler doesn't mark beforefieldinit
            internal static readonly ModelConfigContainer<T> instance = new ModelConfigContainer<T>();
        }

        public static ModelConfigContainer<T> Instance {
            get { return Nested.instance; }
        }

        public Func<string> ConnectionStringProvider { get; set; }
        public bool EnableVersioning { get; set; }
        public bool EnableSoftDelete { get; set; }
        public bool EnableUserAuditing { get; set; }

        private List<Expression<Func<T, bool>>> _query = new List<Expression<Func<T, bool>>>();
        public List<Expression<Func<T, bool>>> Query {
            get { return _query; }
            set { _query = value; }
        }

        private List<Func<T, bool>> _beforeSave = new List<Func<T, bool>>();
        public List<Func<T, bool>> BeforeSave {
            get { return _beforeSave; }
            set { _beforeSave = value; }
        }

        private List<Action<T>> _afterSave = new List<Action<T>>();
        public List<Action<T>> AfterSave {
            get { return _afterSave; }
            set { _afterSave = value; }
        }

        private List<Func<T, bool>> _beforeDelete = new List<Func<T, bool>>();
        public List<Func<T, bool>> BeforeDelete {
            get { return _beforeDelete; }
            set { _beforeDelete = value; }
        }

        private List<Action<T>> _afterDelete = new List<Action<T>>();
        public List<Action<T>> AfterDelete {
            get { return _afterDelete; }
            set { _afterDelete = value; }
        }

        public void DropBehaviors() {
            _query.Clear();
            _beforeSave.Clear();
            _afterSave.Clear();
            _beforeDelete.Clear();
            _afterDelete.Clear();
            EnableSoftDelete = false;
            EnableVersioning = false;
            EnableUserAuditing = false;
        }
    }
}
