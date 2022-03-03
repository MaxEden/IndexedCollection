using System;
using System.Collections;
using System.Collections.Generic;

namespace IndexedCollection
{
    public partial class IndexedCollection<TItem> : IEnumerable<IndexedCollection<TItem>.Entry>
    {
        private readonly Buffer<Entry>  _values   = new Buffer<Entry>();
        private readonly Buffer<IIndex> _indexers = new Buffer<IIndex>();

        public IndexedCollection() {}

        public Entry Add(TItem item)
        {
            int index = _values.Count;
            var entry = new Entry()
            {
                Item = item,
                Index = index
            };
            _values.Add(entry);
            foreach(var indexer in _indexers)
            {
                indexer.Add(entry);
            }

            return entry;
        }

        public void Remove(Entry entry)
        {
            var index = entry.Index;

            foreach(var indexer in _indexers)
            {
                indexer.Remove(entry);
            }

            _values.RemoveAndMixOrder(index);
            if(_values.Count > 0 && index < _values.Count)
            {
                _values.Array[index].Index = index;
            }
        }

        public void Rebuild(Entry entry)
        {
            Remove(entry);
            Add(entry.Item);
        }

        public IndexByKey<TKey> CreateIndexByKey<TKey>(Func<TItem, TKey> selector)
        {
            var index = new IndexByKey<TKey>(selector, this);
            _indexers.Add(index);
            ((IIndex)index).Rebuild();
            return index;
        }

        public IndexOneToOne<TKey> CreateIndexOneToOne<TKey>(Func<TItem, TKey> selector)
        {
            var index = new IndexOneToOne<TKey>(selector, this);
            _indexers.Add(index);
            ((IIndex)index).Rebuild();
            return index;
        }

        public IndexByType CreateIndexByType(Func<TItem, Type> selector)
        {
            var index = new IndexByType(selector, this);
            _indexers.Add(index);
            ((IIndex)index).Rebuild();
            return index;
        }

        public void Clear()
        {
            foreach(var indexer in _indexers)
            {
                indexer.Clear();
            }
        }

        public class Entry
        {
            public int   Index = -1;
            public TItem Item;
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}