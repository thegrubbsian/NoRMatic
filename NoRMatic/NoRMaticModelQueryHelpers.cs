using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm;
using Norm.BSON;

namespace NoRMatic {

    // Partial class to contain all the proxy sugar methods for LINQ (FindOne, Exists, etc)
    public abstract partial class NoRMaticModel<T> where T : NoRMaticModel<T> {

        /// <summary>
        /// Returns all instances of this type from the database but query behaviors are respected.  This method
        /// will not return versions or soft deleted entities unless the related override argument is set to true.
        /// </summary>
        public static IEnumerable<T> All(bool includeDeleted = false, bool includeVersions = false) {

            if (ModelConfig.Query.Count == 0 && !ModelConfig.EnableSoftDelete)
                return GetMongoCollection().Find();

            var query = GetMongoCollection().AsQueryable();
            query = ModelConfig.Query.Aggregate(query, (c, b) => c.Where(b));

            if (ModelConfig.EnableSoftDelete && !includeDeleted)
                query = query.Where(x => !x.IsDeleted);

            if (ModelConfig.EnableVersioning && !includeVersions)
                query = query.Where(x => !x.IsVersion);

            WriteToLog(string.Format("ALL -- {0}", query));

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

            WriteToLog(string.Format("FIND -- {0}", query));

            return query;
        }

        /// <summary>
        /// Returns a single entity by it's Id from the database.
        /// </summary>
        public static T GetById(ObjectId id,
            bool includeDeleted = false, bool includeVersions = false) {

            var expando = new Expando();
            expando["_id"] = id;

            if (ModelConfig.EnableSoftDelete && !includeDeleted)
                expando["IsDeleted"] = false;

            if (ModelConfig.EnableVersioning && !includeVersions)
                expando["IsVersion"] = false;

            return GetMongoCollection().FindOne(expando);
        }


        /// <summary>
        /// Returns a single entity which matches the expression.  An exception will be thrown if more than one document
        /// matches the expression.
        /// </summary>
        public static T FindOne(Expression<Func<T, bool>> expression,
            bool includeDeleted = false, bool includeVersions = false) {
            return Find(expression, includeDeleted, includeVersions).SingleOrDefault();
        }

        /// <summary>
        /// Returns a boolean indicating whether a matching document exists in the collection.  All query behaviors
        /// will apply to the search.  Deleted and versioned documents can be included by using the related boolean flags.
        /// </summary>
        public static bool Exists(Expression<Func<T, bool>> expression,
            bool includeDeleted = false, bool includeVersions = false) {
            return Find(expression, includeDeleted, includeVersions).Count() > 0;
        }
    }
}
