using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoRMatic {
    
    public static class Extensions {

        public static List<Expression<Func<dynamic, bool>>> GetByType(
            this Dictionary<Type, List<Expression<Func<dynamic, bool>>>> @this, Type type) {

            var list = new List<Expression<Func<dynamic, bool>>>();

            foreach (var item in @this.Where(item => item.Key.IsAssignableFrom(type)))
                list.AddRange(item.Value);

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
    }
}
