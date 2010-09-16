using System;
using System.Linq.Expressions;
using Norm;

namespace NoRMatic {

    // Partial class to contain all the behavior registration and flagging related members
    public abstract partial class NoRMaticModel<T> where T : NoRMaticModel<T> {

        // Soft Delete
        /// <summary>
        /// This flag is set when a model is deleted for which EnableSoftDelete has been set.
        /// </summary>
        public bool IsDeleted { get; internal set; }

        /// <summary>
        /// This is the date the model was deleted when EnableSoftDelete has been set.  If EnableSoftDelete
        /// is not set this value will be null.
        /// </summary>
        public DateTime DateDeleted { get; internal set; }

        // Versioning
        /// <summary>
        /// This flag is set for documents in the collection which are previous versions of a source
        /// document.
        /// </summary>
        public bool IsVersion { get; internal set; }
        
        /// <summary>
        /// For models which have EnableVersioning set, this is the Id of the source document.  If 
        /// EnableVersioning is not set this value will be null.
        /// </summary>
        public ObjectId VersionOfId { get; internal set; }

        // User Auditing
        /// <summary>
        /// If EnableUserAuditing is set for the model, this will be the value supplied by
        /// the CurrentUserProvider when Save() is called on the model.  If EnableUserAuditing is not set
        /// this value will be null.
        /// </summary>
        public string UpdatedBy { get; internal set; }

        /// <summary>
        /// Extends the Where conditions of any query executed for a given collection.
        /// </summary>
        public static void AddQueryBehavior(Expression<Func<T, bool>> action) {
            ModelConfig.Query.Add(action);
        }

        /// <summary>
        /// BeforeSaveBehaviors are functions called before any Save for the collection.  If any of these
        /// functions return 'true' the save will NOT proceed.  This can be useful if there are pre-save validations or other checks
        /// that need to be made that may stop the save.
        /// </summary>
        public static void AddBeforeSaveBehavior(Func<T, bool> action) {
            ModelConfig.BeforeSave.Add(action);
        }

        /// <summary>
        /// Actions executed after a save and after a version is created if EnableVersioning is set.
        /// </summary>
        public static void AddAfterSaveBehavior(Action<T> action) {
            ModelConfig.AfterSave.Add(action);
        }

        /// <summary>
        /// BeforeDeleteBehaviors are functions called before any Delete for the collection.  If any of these
        /// functions return 'true' the delete will not proceed (including soft deletes).
        /// </summary>
        public static void AddBeforeDeleteBehavior(Func<T, bool> action) {
            ModelConfig.BeforeDelete.Add(action);
        }

        /// <summary>
        /// Actions executed after a delete.
        /// </summary>
        public static void AddAfterDeleteBehavior(Action<T> action) {
            ModelConfig.AfterDelete.Add(action);
        }

        /// <summary>
        /// Drops all registered behaviors and resets EnableSoftDelete and EnableVersioning to false.
        /// </summary>
        public static void DropBehaviors() {
            ModelConfig.DropBehaviors();
        }

        /// <summary>
        /// Enables soft delete behavior which causes Delete actions to be logical not physical.  When a soft delete enabled entity
        /// is deleted, it's IsDeleted and DateDeleted properties are set and all subsequent changes to the entity
        /// are disallowed.
        /// </summary>
        public static void EnableSoftDelete() {
            ModelConfig.EnableSoftDelete = true;
        }

        /// <summary>
        /// Enables versioning behavior which causes all Save actions to also save a new version of the entity in the collection.
        /// Versions automatically have their IsVersion and DateVersioned properties to be set.  Versions recieve a new Id but
        /// point back to their original document via the VersionOfId property.  Versions cannot be changed but will be deleted
        /// when their originating document is deleted unless soft delete is enabled.
        /// </summary>
        public static void EnableVersioning() {
            ModelConfig.EnableVersioning = true;
        }

        /// <summary>
        /// Enables user auditing behavior which cuases all Save actions to set the UpdatedBy property based on the given CurrentUserProvider
        /// function.  If CurrentUserProvider is not supplied, then an empty string will be used for the value.  If used in conjunction with
        /// EnableVersioning, each version will also be marked with the user who created the version with the VersionMadeBy property.
        /// </summary>
        public static void EnableUserAuditing() {
            ModelConfig.EnableUserAuditing = true;
        }

        /// <summary>
        /// Disables the soft delete behavior of the model.
        /// </summary>
        public static void DisableSoftDelete() {
            ModelConfig.EnableSoftDelete = false;
        }

        /// <summary>
        /// Disables the versioning behavior of the model.
        /// </summary>
        public static void DisableVersioning() {
            ModelConfig.EnableVersioning = false;
        }

        /// <summary>
        /// Disables the user auditing behavior of the model.
        /// </summary>
        public static void DisableUserAuditing() {
            ModelConfig.EnableUserAuditing = false;
        }
    }
}
