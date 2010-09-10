using System;
using System.Linq.Expressions;
using Norm;

namespace NoRMatic {

    // Partial class to contain all the behavior registration and flagging related members
    public abstract partial class NoRMaticModel<T> where T : NoRMaticModel<T> {

        // Soft Delete Properties
        public bool IsDeleted { get; internal set; }
        public DateTime DateDeleted { get; internal set; }

        // Versioning Properties
        public bool IsVersion { get; internal set; }
        public DateTime DateVersioned { get; internal set; }
        public ObjectId VersionOfId { get; internal set; }

        private static BehaviorContainer<T> Behaviors {
            get { return BehaviorContainer<T>.Instance; }
        }

        /// <summary>
        /// Extends the Where conditions of any query executed for a given collection.
        /// </summary>
        public static void AddQueryBehavior(Expression<Func<T, bool>> action) {
            Behaviors.Query.Add(action);
        }

        /// <summary>
        /// BeforeSaveBehaviors are functions called before any Save for the collection.  If any of these
        /// functions return 'true' the save will NOT proceed.  This can be useful if there are pre-save validations or other checks
        /// that need to be made that may stop the save.
        /// </summary>
        public static void AddBeforeSaveBehavior(Func<T, bool> action) {
            Behaviors.BeforeSave.Add(action);
        }

        /// <summary>
        /// Actions executed after a save and after a version is created if EnableVersioning is set.
        /// </summary>
        public static void AddAfterSaveBehavior(Action<T> action) {
            Behaviors.AfterSave.Add(action);
        }

        /// <summary>
        /// BeforeDeleteBehaviors are functions called before any Delete for the collection.  If any of these
        /// functions return 'true' the delete will not proceed (including soft deletes).
        /// </summary>
        public static void AddBeforeDeleteBehavior(Func<T, bool> action) {
            Behaviors.BeforeDelete.Add(action);
        }

        /// <summary>
        /// Actions executed after a delete.
        /// </summary>
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
    }
}
