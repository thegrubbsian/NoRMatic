using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Norm;
using Norm.Attributes;
using Norm.Collections;

namespace NoRMatic {

    public abstract partial class NoRMaticModel<T> where T : NoRMaticModel<T> {

        private static GlobalConfigContainer GlobalConfig {
            get { return GlobalConfigContainer.Instance; }
        }

        private static ModelConfigContainer<T> ModelConfig {
            get { return ModelConfigContainer<T>.Instance; }
        }

        public ObjectId Id { get; internal set; }
        public DateTime DateUpdated { get; internal set; }

        /// <summary>
        /// Returns any validation errors for the current state of the entity.  Validation uses the System.ComponentModel.DataAnnotations
        /// attributes along with NoRMatic's [ValidateChild] attribute which allows deep validation of custom nested types.
        /// </summary>
        [MongoIgnore]
        public List<ValidationResult> Errors {
            get { return Validate(); }
        }

        /// <summary>
        /// Returns all instances of this type from the database but query behaviors are respected.  This method
        /// will not return versions or soft deleted entities.
        /// NOTE: Use Find() to return deleted or versioned items if the EnableSoftDelete or EnableVersioning behaviors are set.
        /// </summary>
        public static IEnumerable<T> All() {

            if (ModelConfig.Query.Count == 0 && !ModelConfig.EnableSoftDelete)
                return GetMongoCollection().Find();

            var query = GetMongoCollection().AsQueryable();
            query = ModelConfig.Query.Aggregate(query, (c, b) => c.Where(b));

            if (ModelConfig.EnableVersioning)
                query = query.Where(x => !x.IsVersion);

            if (ModelConfig.EnableSoftDelete)
                query = query.Where(x => !x.IsDeleted);

            return query.ToList();
        }

        /// <summary>
        /// Returns an enumerable list of entities matching the given expression.  If there are any registered query behaviors
        /// for this type they will become constraints on this query.  By default, Find() will not return versions or soft deleted
        /// entities, this behavior can be overridden by passing true to the 'includeDeleted' or 'includeVersions' arguments.
        /// </summary>
        public static IQueryable<T> Find(Expression<Func<T, bool>> expression,
            bool includeDeleted = false, bool includeVersions = false) {

            var query = GetMongoCollection().AsQueryable().Where(expression);
            query = ModelConfig.Query.Aggregate(query, (c, b) => c.Where(b));

            if (ModelConfig.EnableSoftDelete && !includeDeleted)
                query = query.Where(x => !x.IsDeleted);

            if (ModelConfig.EnableVersioning && !includeVersions)
                query = query.Where(x => !x.IsVersion);

            return query;
        }

        /// <summary>
        /// Returns a single entity by it's Id from the database.
        /// </summary>
        public static T GetById(ObjectId id,
            bool includeDeleted = false, bool includeVersions = false) {

            //return GetMongoCollection().FindOne(new {Id = id});

            return Find(x => x.Id == id, includeDeleted, includeVersions).SingleOrDefault();
        }

        /// <summary>
        /// Deletes all documents from a collection.  NOTE: This does NOT respect the EnableSoftDelete behavior, if
        /// called DeleteAll() will permanently remove all documents from the collection.
        /// </summary>
        public static void DeleteAll() {
            using (var db = Mongo.Create(NoRMaticConfig.ConnectionString)) {
                // Try/Catch is to avoid error when DeleteAll() is called on a non-existant collection
                try { db.Database.DropCollection(typeof(T).Name); } catch { }
            }
        }

        /// <summary>
        /// Creates or saves the entity to the database.  If the EnableVersioning behavior is set then a version
        /// will be created for each save.  NOTE: Versions are created regardless of whether changes exist or not.
        /// </summary>
        public virtual void Save() {

            if (ModelConfig.EnableSoftDelete && IsDeleted) return;
            if (ModelConfig.EnableVersioning && IsVersion) return;

            if (!DoBeforeBehaviors(
                GlobalConfig.BeforeSave.GetByType(GetType()), 
                ModelConfig.BeforeSave)) return;

            if (Validate().Count > 0) return;

            DateUpdated = DateTime.Now;

            if (ModelConfig.EnableUserAuditing && GlobalConfig.CurrentUserProvider != null)
                UpdatedBy = GlobalConfig.CurrentUserProvider();

            GetMongoCollection().Save((T)this);

            if (ModelConfig.EnableVersioning) SaveVersion();

            DoAfterBehaviors(
                GlobalConfig.AfterSave.GetByType(GetType()), 
                ModelConfig.AfterSave);
        }

        /// <summary>
        /// Deletes or sets IsDeleted flag for the entity depending on whether or not the EnableSoftDelete
        /// behavior is set for this type.
        /// </summary>
        public virtual void Delete() {

            if (!DoBeforeBehaviors(
                GlobalConfig.BeforeDelete.GetByType(GetType()), 
                ModelConfig.BeforeDelete)) return;

            if (ModelConfig.EnableSoftDelete) {
                SoftDelete();
            } else {
                if (ModelConfig.EnableVersioning) DeleteVersions();
                GetMongoCollection().Delete((T)this);
            }

            DoAfterBehaviors(
                GlobalConfig.AfterDelete.GetByType(GetType()),
                ModelConfig.AfterDelete);
        }

        /// <summary>
        /// Returns all previous versions of the entity if the EnableVersioning flag is set for this type.
        /// </summary>
        public virtual IEnumerable<T> GetVersions() {
            return GetMongoCollection().Find(new { IsVersion = true, VersionOfId = Id })
                .OrderByDescending(x => x.DateVersioned);
        }

        private List<ValidationResult> Validate() {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this, null, null), results, true);
            return results;
        }

        private void SoftDelete() {
            IsDeleted = true;
            DateDeleted = DateTime.Now;
            GetMongoCollection().Save((T)this);
        }

        private void SaveVersion() {
            var clone = Clone();
            clone.IsVersion = true;
            clone.DateVersioned = DateTime.Now;
            clone.VersionOfId = Id;
            GetMongoCollection().Save(clone);
        }

        private T Clone() {
            var obj = GetMongoCollection().FindOne(new { Id });
            obj.Id = null;
            return obj;
        }

        private void DeleteVersions() {
            var versions = GetMongoCollection().Find(new { VersionOfId = Id }).ToList();
            for (var i = 0; i < versions.Count(); i++)
                GetMongoCollection().Delete(versions[i]);
        }

        private bool DoBeforeBehaviors(
            IEnumerable<Func<dynamic, bool>> global, IEnumerable<Func<T, bool>> model) {

            var all = true;

            if (global.Any(x => x(this) == false)) all = false;
            if (model.Any(x => x((T)this) == false)) all = false;

            return all;
        }

        private void DoAfterBehaviors(
            List<Action<dynamic>> global, List<Action<T>> model) {

            global.ForEach(x => x(this));
            model.ForEach(x => x((T)this));
        }
    }
}
