using System;
using System.Linq;
using System.Linq.Expressions;

namespace NoRMatic {

    // Partial class to contain all the proxy sugar methods for LINQ (FindOne, Exists, etc)
    public abstract partial class NoRMaticModel<T> where T : NoRMaticModel<T> {

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
