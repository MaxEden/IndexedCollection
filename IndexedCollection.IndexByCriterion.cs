using System;
using System.Collections.Generic;

namespace IndexedCollection
{
    public partial class IndexedCollection<TItem>
    {
        public class IndexByCriterion<TQueryKey, TCriterionKey> : IIndex
        {
            private readonly Func<TItem, TCriterionKey>           _selector;
            private readonly Func<TCriterionKey, TQueryKey, bool> _criterion;
            private readonly IndexedCollection<TItem>             _collection;

            private readonly Dictionary<TCriterionKey, OEntry> _odict =
                new Dictionary<TCriterionKey, OEntry>();

            private readonly Dictionary<TQueryKey, Buffer<Entry>> _dict =
                new Dictionary<TQueryKey, Buffer<Entry>>();

            private readonly Buffer<(Buffer<Entry> list, int index)> _backIndexes = new Buffer<(Buffer<Entry>, int)>();

            class OEntry
            {
                public Buffer<Entry>     Entries = new Buffer<Entry>();
                public Buffer<TQueryKey> Keys    = new Buffer<TQueryKey>();
            }

            public IndexByCriterion(Func<TItem, TCriterionKey>           selector,
                                    Func<TCriterionKey, TQueryKey, bool> criterion,
                                    IndexedCollection<TItem>             collection)
            {
                _selector = selector;
                _collection = collection;
                _criterion = criterion;
            }

            public IEnumerable<Entry> Get(TQueryKey key)
            {
                if(_dict.TryGetValue(key, out var entries)) return entries;
                
                entries = new Buffer<Entry>();
                _dict.Add(key, entries);

                foreach(var entry in _collection._values)
                {
                    var criterionKey = _selector(entry.Item);
                    if(_criterion(criterionKey, key))
                    {
                        var innerIndex = entries.Count;
                        entries.Add(entry);
                        _backIndexes.SetAtAndResize(entry.Index, (entries, innerIndex));
                    }
                }

                foreach(var o in _odict)
                {
                    if(_criterion(o.Key, key))
                    {
                        o.Value.Keys.Add(key);
                    }
                }

                return entries;
            }

            public bool RemoveByKey(TQueryKey key)
            {
                if(!_dict.TryGetValue(key, out var values)) return false;
                
                var buffer = new Buffer<Entry>();
                buffer.Refill(values);
                foreach(var value in buffer)
                {
                    _collection.Remove(value);
                }

                return true;
            }

            void IIndex.Remove(Entry item)
            {
                var tuple = _backIndexes.Array[item.Index];
                var innerIndex = tuple.index;
                var list = tuple.list;
                if(list == null) return;

                list.RemoveAndMixOrder(innerIndex);

                _backIndexes.SetAtAndResize(item.Index, default);
                if(list.Count > 0 && innerIndex < list.Count)
                {
                    _backIndexes.SetAtAndResize(list.Array[innerIndex].Index, tuple);
                }
            }

            void IIndex.Add(Entry entry)
            {
                var criterionKey = _selector(entry.Item);
                if(!_odict.TryGetValue(criterionKey, out var oEntry))
                {
                    oEntry = new OEntry();
                    _odict.Add(criterionKey, oEntry);

                    foreach(var key in _dict.Keys)
                    {
                        if(_criterion(criterionKey, key))
                        {
                            oEntry.Keys.Add(key);
                        }
                    }
                }

                oEntry.Entries.Add(entry);
                _backIndexes.SetAtAndResize(entry.Index, default);
                foreach(var key in oEntry.Keys)
                {
                    var entries = _dict[key];
                    var innerIndex = entries.Count;
                    entries.Add(entry);
                    _backIndexes.SetAtAndResize(entry.Index, (entries, innerIndex));
                }
            }

            void IIndex.Rebuild()
            {
                var indexer = (IIndex)this;
                indexer.Clear();
                foreach(var value in _collection._values)
                {
                    indexer.Add(value);
                }
            }

            void IIndex.Clear()
            {
                _odict.Clear();
                _dict.Clear();
            }
        }
    }
}