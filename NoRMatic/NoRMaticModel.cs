using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Norm;
using Norm.Collections;

namespace NoRMatic {

    public abstract class NoRMaticModel<T> where T : NoRMaticModel<T> {

        public ObjectId Id { get; set; }
        public DateTime DateUpdated { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; internal set; }
        public DateTime DateDeleted { get; internal set; }

        // Versioning Properties
        public bool IsVersion { get; private set; }
        public DateTime DateVersioned { get; set; }
        public ObjectId VersionOfId { get; set; }

        public static IEnumerable<T> All() {

            if (Behaviors.Query.Count == 0)
                return GetCollection().Find();

            var query = GetCollection().AsQueryable();
            query = Behaviors.Query.Aggregate(query, (c, b) => c.Where(b));
            return query.ToList();
        }

        public static IEnumerable<T> Find(Expression<Func<T, bool>> expression) {

            var query = GetCollection().AsQueryable().Where(expression);

            if (Behaviors.Query.Count > 0)
                query = Behaviors.Query.Aggregate(query, (c, b) => c.Where(b));

            return query.ToList();
        }

        public static T GetById(ObjectId id) {
            return GetCollection().FindOne(new { Id = id });
        }

        public void Save() {

            if (Behaviors.EnableSoftDelete && IsDeleted) return;
            if (Behaviors.EnableVersioning && IsVersion) return;

            if (Behaviors.BeforeSave.Count > 0)
                if (Behaviors.BeforeSave.Any(x => !x((T)this))) return;

            DateUpdated = DateTime.Now;
            GetCollection().Save((T)this);

            Behaviors.AfterSave.ForEach(x => x((T)this));

            if (Behaviors.EnableVersioning) SaveVersion();
        }

        public void Delete() {

            if (Behaviors.BeforeDelete.Count > 0)
                if (Behaviors.BeforeDelete.Any(x => !x((T)this))) return;

            if (Behaviors.EnableSoftDelete) {
                SoftDelete();
            } else {
                if (Behaviors.EnableVersioning) DeleteVersions();
                GetCollection().Delete((T)this);
            }

            Behaviors.AfterDelete.ForEach(x => x((T)this));
        }

        private void SoftDelete() {
            IsDeleted = true;
            DateDeleted = DateTime.Now;
            GetCollection().Save((T)this);
        }

        private void SaveVersion() {
            var clone = Clone();
            clone.Id = null;
            clone.IsVersion = true;
            clone.DateVersioned = DateTime.Now;
            clone.VersionOfId = Id;
            clone.Save();
        }

        private void DeleteVersions() {
            var versions = GetCollection().Find(new { VersionOfId = Id }).ToList();
            for (var i = 0; i < versions.Count(); i++)
                GetCollection().Delete(versions[i]);
        }

        public T Clone() {
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            var obj = bf.Deserialize(ms);
            ms.Close();
            return (T)obj;
        }

        private static IMongoCollection<T> GetCollection() {
            using (var db = Mongo.Create(NoRMaticConfig.ConnectionString))
                return db.GetCollection<T>();
        }

        #region Behavior Registration

        private static BehaviorContainer<T> Behaviors {
            get { return BehaviorContainer<T>.Instance; }
        }

        public static void AddQueryBehavior(Expression<Func<T, bool>> action) {
            Behaviors.Query.Add(action);
        }

        public static void AddBeforeSaveBehavior(Func<T, bool> action) {
            Behaviors.BeforeSave.Add(action);
        }

        public static void AddAfterSaveBehavior(Action<T> action) {
            Behaviors.AfterSave.Add(action);
        }

        public static void AddBeforeDeleteBehavior(Func<T, bool> action) {
            Behaviors.BeforeDelete.Add(action);
        }

        public static void AddAfterDeleteBehavior(Action<T> action) {
            Behaviors.AfterDelete.Add(action);
        }

        /// <summary>
        /// Drops all registered behaviors and resets EnableSoftDelete and EnableVersioning to false.
        /// </summary>
        public static void DropBehaviors() {
            Behaviors.DropBehaviors();
        }

        /// <summary>
        /// Enables soft delete behavior which causes Delete actions to be logical not physical.  When a soft delete enabled entity
        /// is deleted, it's IsDeleted and DateDeleted properties are set and all subsequent changes to the entity
        /// are disallowed.
        /// </summary>
        public static void EnableSoftDelete() {
            Behaviors.EnableSoftDelete = true;
        }

        /// <summary>
        /// Enables versioning behavior which causes all Save actions to also save a new version of the entity in the collection.
        /// Versions automatically have their IsVersion and DateVersioned properties to be set.  Versions recieve a new Id but
        /// point back to their original document via the VersionOfId property.  Versions cannot be changed but will be deleted
        /// when their originating document is deleted unless soft delete is enabled.
        /// </summary>
        public static void EnableVersioning() {
            Behaviors.EnableVersioning = true;
        }

        public static void DisableSoftDelete() {
            Behaviors.EnableSoftDelete = false;
        }

        public static void DisableVersioning() {
            Behaviors.EnableVersioning = false;
        }

        #endregion
    }
}
