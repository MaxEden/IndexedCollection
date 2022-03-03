using System;
using System.Collections.Generic;

namespace IndexedCollection
{
    public partial class IndexedCollection<TItem>
    {
        public class IndexOneToOne<TKey> : IIndex
        {
            private readonly Func<TItem, TKey>        _selector;
            private readonly IndexedCollection<TItem> _collection;

            private readonly Dictionary<TKey, Entry> _dict =
                new Dictionary<TKey, Entry>();

            private readonly Buffer<(TKey key, bool set)> _backIndexes = new Buffer<(TKey, bool)>();

            public IndexOneToOne(Func<TItem, TKey> selector, IndexedCollection<TItem> collection)
            {
                _selector = selector;
                _collection = collection;
            }

            void IIndex.Add(Entry entry)
            {
                var key = _selector(entry.Item);

                _dict.Add(key, entry);
                _backIndexes.SetAtAndResize(entry.Index, (key, true));
            }

            void IIndex.Remove(Entry item)
            {
                var tuple = _backIndexes.Array[item.Index];
                _dict.Remove(tuple.key);
                _backIndexes.SetAtAndResize(item.Index, (default, false));
            }

            public Entry Get(TKey key)
            {
                return _dict[key];
            }

            public bool TryGet(TKey key, out Entry entry)
            {
                return _dict.TryGetValue(key, out entry);
            }

            public bool RemoveByKey(TKey key)
            {
                if(_dict.TryGetValue(key, out var entry))
                {
                    _collection.Remove(entry);
                    return true;
                }
                return false;
            }

            void IIndex.Rebuild()
            {
                var indexer = (IIndex)this;
                indexer.Clear();
                foreach(var entry in _collection._values)
                {
                    indexer.Add(entry);
                }
            }

            void IIndex.Clear()
            {
                _dict.Clear();
                _backIndexes.Clear();
            }
        }
    }
}