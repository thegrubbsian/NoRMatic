using System;
using System.Linq.Expressions;
using Norm;
using Norm.Collections;
using Norm.Protocol.Messages;

namespace NoRMatic {

    // Partial class to contain all the database helper methods (GetCollection, AddIndex, etc)
    public abstract partial class NoRMaticModel<T> where T : NoRMaticModel<T> {

        /// <summary>
        /// Returns a raw NoRM collection which allows direct access to all of the underlying NoRM methods.
        /// </summary>
        public static IMongoCollection<T> GetMongoCollection() {

            var conString = ModelConfig.ConnectionStringProvider != null ?
                ModelConfig.ConnectionStringProvider() : NoRMaticConfig.ConnectionString;

            using (var db = Mongo.Create(conString)) {
                return db.GetCollection<T>();
            }
        }

        /// <summary>
        /// Creates an index for the collection of the given type.  Optional parameters allow for specifying a name for the
        /// index, setting the index as unique, and dictating the index ordering.
        /// </summary>
        public static void AddIndex(Expression<Func<T, object>> indexKey, 
            string indexName = null, bool isUnique = false, IndexOption direction = IndexOption.Ascending) {

            if (string.IsNullOrEmpty(indexName))
                indexName = typeof (T).Name + "_Index_" + ObjectId.NewObjectId();

            GetMongoCollection().CreateIndex(indexKey, indexName, isUnique, direction);
        }

        /// <summary>
        /// Sets a model specific connection string provider.  This provider will override any global provider
        /// that is registered through NoRMaticConfig.SetConnectionStringProvider().
        /// </summary>
        public static void SetConnectionStringProvider(Func<string> provider) {
            ModelConfig.ConnectionStringProvider = provider;
        }
    }
}
