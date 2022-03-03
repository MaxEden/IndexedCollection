using System;
using System.Collections.Generic;

namespace IndexedCollection
{
    public partial class IndexedCollection<TItem>
    {
        public class IndexByKey<TKey> : IIndex
        {
            private readonly Func<TItem, TKey>        _selector;
            private readonly IndexedCollection<TItem> _collection;

            private readonly Dictionary<TKey, Buffer<Entry>> _dict =
                new Dictionary<TKey, Buffer<Entry>>();

            private readonly Buffer<(Buffer<Entry> list, int index)> _backIndexes = new Buffer<(Buffer<Entry>, int)>();

            public IndexByKey(Func<TItem, TKey> selector, IndexedCollection<TItem> collection)
            {
                _selector = selector;
                _collection = collection;
            }

            void IIndex.Add(Entry entry)
            {
                var key = _selector(entry.Item);

                if(!_dict.TryGetValue(key, out var list))
                {
                    list = new Buffer<Entry>();
                    _dict[key] = list;
                }

                var innerIndex = list.Count;
                list.Add(entry);
                _backIndexes.SetAtAndResize(entry.Index, (list, innerIndex));
            }

            void IIndex.Remove(Entry item)
            {
                var tuple = _backIndexes.Array[item.Index];
                var list = tuple.list;
                var innerIndex = tuple.index;

                if(list == null) return;

                list.RemoveAndMixOrder(innerIndex);

                _backIndexes.SetAtAndResize(item.Index, default);
                if(list.Count > 0 && innerIndex < list.Count)
                {
                    _backIndexes.SetAtAndResize(list.Array[innerIndex].Index, tuple);
                }
            }

            public IEnumerable<Entry> Get(TKey key)
            {
                return _dict[key];
            }

            public bool TryGet(TKey key, out IEnumerable<Entry> entries)
            {
                entries = default;
                if(!_dict.TryGetValue(key, out var list)) return false;
                entries = list;
                return true;
            }

            public bool RemoveByKey(TKey key)
            {
                if(_dict.TryGetValue(key, out var values))
                {
                    var buffer = new Buffer<Entry>();
                    buffer.Refill(values);
                    foreach(var value in buffer)
                    {
                        _collection.Remove(value);
                    }

                    return true;
                }

                return false;
            }

            void IIndex.Rebuild()
            {
                var indexer = ((IIndex)this);
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