using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NoRMatic {

    
    public static class Extensions {

        public static List<Expression<Func<T, bool>>> GetByType<T>(
            this Dictionary<Type, object> @this) {

            var list = new List<Expression<Func<T, bool>>>();

            foreach (var item in @this.Where(item => item.Key.IsAssignableFrom(typeof(T))))
                list.AddRange((List<Expression<Func<T, bool>>>)item.Value);

            return list;
        }

        public static List<Func<dynamic, bool>> GetByType(
            this Dictionary<Type, List<Func<dynamic, bool>>> @this, Type type) {

            var list = new List<Func<dynamic, bool>>();

            foreach (var item in @this.Where(item => item.Key.IsAssignableFrom(type)))
                list.AddRange(item.Value);

            return list;
        }

        public static List<Action<dynamic>> GetByType(
            this Dictionary<Type, List<Action<dynamic>>> @this, Type type) {

            var list = new List<Action<dynamic>>();

            foreach (var item in @this.Where(item => item.Key.IsAssignableFrom(type)))
                list.AddRange(item.Value);

            return list;
        }

        public static List<Type> GetTypesAssignableFrom(this Assembly[] @this, Type type) {

            var types = new List<Type>();
            foreach (var assembly in @this) {
                try { types.AddRange(assembly.GetTypes().Where(type.IsAssignableFrom)); } catch { }
            }
            return types;
        }
    }
}
