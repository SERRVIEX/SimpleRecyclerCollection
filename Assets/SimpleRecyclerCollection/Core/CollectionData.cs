namespace SimpleRecyclerCollection.Core
{
    using System.Collections.Generic;

    using UnityEngine.Events;

    public sealed class CollectionData<T>
    {
        private List<T> _data;
        public T this[int index] => _data[index];
        public int Count => _data.Count;

        public UnityEvent OnMarkedDirty = new UnityEvent();

        // Constructors

        public CollectionData() => _data = new List<T>();

        // Methods

        public void Contains(T item) => _data.Contains(item);

        public int IndexOf(T item) => _data.IndexOf(item);

        public void Add(T item)
        {
            _data.Add(item);

            OnMarkedDirty?.Invoke();
        }

        public void Add(T[] items)
        {
            for (int i = 0; i < items.Length; i++)
                _data.Add(items[i]);

            OnMarkedDirty?.Invoke();
        }

        public void Add(List<T> items) => Add(items.ToArray());

        public void Insert(int index, T item)
        {
            if (index < 0)
                _data.Insert(0, item);

            else if (index >= _data.Count)
            {
                Add(item);
                return;
            }
            else
                _data.Insert(index, item);

            OnMarkedDirty?.Invoke();
        }

        public void Insert(int index, T[] items)
        {
            if (index < 0)
            {
                for (int i = 0; i < items.Length; i++)
                    _data.Insert(i, items[i]);
            }
            else if (index >= _data.Count)
            {
                Add(items);
                return;
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                    _data.Insert(index + i, items[i]);
            }

            OnMarkedDirty?.Invoke();
        }

        public void Insert(int index, List<T> items) => Insert(index, items.ToArray());

        public void Remove(T item)
        {
            _data.Remove(item);

            OnMarkedDirty?.Invoke();
        }

        public void Remove(T[] items)
        {
            for (int i = 0; i < items.Length; i++)
                _data.Remove(items[i]);

            OnMarkedDirty?.Invoke();
        }

        public void Remove(List<T> items) => Remove(items.ToArray());

        public void RemoveAt(int index)
        {
            _data.RemoveAt(index);

            OnMarkedDirty?.Invoke();
        }

        public void Replace(T item)
        {
            _data.Clear();
            Add(item);

            OnMarkedDirty?.Invoke();
        }

        public void Replace(T[] items)
        {
            _data.Clear();
            Add(items);

            OnMarkedDirty?.Invoke();
        }

        public void Replace(List<T> items)
        {
            _data.Clear();
            Add(items);

            OnMarkedDirty?.Invoke();
        }

        public void Clear()
        {
            _data.Clear();

            OnMarkedDirty?.Invoke();
        }
    }
}