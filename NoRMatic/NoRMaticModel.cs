using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using Norm;
using Norm.Collections;

namespace NoRMatic {

    public abstract partial class NoRMaticModel<T> where T : NoRMaticModel<T> {

        public ObjectId Id { get; set; }
        public DateTime DateUpdated { get; set; }

        public List<ValidationResult> Errors {
            get { return Validate(); }
        }

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
            if (Validate().Count > 0) return;

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

        public T Clone() {
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            var obj = bf.Deserialize(ms);
            ms.Close();
            return (T)obj;
        }

        private List<ValidationResult> Validate() {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this, null, null), results, true);
            return results;
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

        private static IMongoCollection<T> GetCollection() {
            using (var db = Mongo.Create(NoRMaticConfig.ConnectionString))
                return db.GetCollection<T>();
        }
    }
}
