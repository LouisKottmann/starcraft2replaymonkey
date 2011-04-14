using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheUpsideDownLemur
{
    public static class IoC
    {
        static Dictionary<Type, object> _registeredTypes = new Dictionary<Type, object>();

        public static void AddMonkey<T>(T toRegister)
        {
            _registeredTypes.Add(typeof(T), toRegister);
        }

        public static T FindMonkey<T>()
        {
            return (T)_registeredTypes[typeof(T)];
        }
    }
}
