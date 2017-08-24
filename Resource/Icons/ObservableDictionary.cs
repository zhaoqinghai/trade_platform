using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resource.Icons
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableDictionary()
        {
            _dict = new Dictionary<TKey, TValue>();
        }

        public void Set(IDictionary<TKey, TValue> dictionary)
        {
            this.Clear();
            foreach (var item in dictionary)
            {
                this.Add(item);
            }
        }

        #region Properties
        private IDictionary<TKey, TValue> _dict
        {
            get;
            set;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_dict.ContainsKey(key))
                {
                    return _dict[key];
                }
                return default(TValue);
            }
            set
            {
                TValue oldItem = default(TValue);
                if (_dict.Keys.Contains(key))
                {
                    oldItem = _dict[key];
                }
                _dict[key] = value;
                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                CheckReentrancy();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem));
            }
        }


        public ICollection<TKey> Keys => _dict?.Keys;

        public ICollection<TValue> Values => _dict?.Values;

        public int Count => _dict?.Count ?? 0;

        public bool IsReadOnly => _dict.IsReadOnly;
        #endregion

        #region Methods
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception>
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException();
            }
        }

        public void Add(TKey key, TValue value)
        {
            CheckReentrancy();
            if (_dict.ContainsKey(key))
                throw new InvalidOperationException();
            this[key] = value;
        }

        public bool Remove(TKey key)
        {
            CheckReentrancy();
            if (!_dict.ContainsKey(key))
                throw new InvalidOperationException();
            var item = _dict[key];
            var temp = _dict.Remove(key);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return temp;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            CheckReentrancy();
            if (_dict.ContainsKey(item.Key))
                throw new InvalidOperationException();
            _dict.Add(item);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            CheckReentrancy();
            _dict.CopyTo(array, arrayIndex);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, array, arrayIndex));
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            CheckReentrancy();
            if (!_dict.ContainsKey(item.Key))
                throw new InvalidOperationException();
            var temp = _dict.Remove(item);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return temp;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_dict).GetEnumerator();
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public void Clear()
        {
            CheckReentrancy();
            _dict.Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private class SimpleMonitor : IDisposable
        {
            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }

            public bool Busy { get { return _busyCount > 0; } }

            int _busyCount;
        }

        #region Private Fields

        private const string CountString = "Count";

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";

        private SimpleMonitor _monitor = new SimpleMonitor();

        #endregion Private Fields
    }
}
