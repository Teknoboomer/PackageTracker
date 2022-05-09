using Prism.Common;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;

namespace DialogServices
{
    internal class DialogParameters : IDialogParameters
    {
        private IDictionary<string, object> parameters = new Dictionary<string, object>();

        public int Count => parameters.Count;

        public IEnumerable<string> Keys => parameters.Keys;

        public void Add(string key, object value)
        {
            parameters.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return parameters.ContainsKey(key);
        }

        public T GetValue<T>(string key)
        {
            return (T)parameters[key];
        }

        public IEnumerable<T> GetValues<T>(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            return parameters.TryGetValue(key, out value);
        }
    }
}
