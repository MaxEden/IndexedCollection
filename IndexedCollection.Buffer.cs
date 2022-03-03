using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IndexedCollection
{
    public partial class IndexedCollection<TItem>
    {
        public class Buffer<T> : IEnumerable<T>
        {
            public int Count;
            public T[] Array;

            public Buffer()
            {
                Array = new T[4];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                Count = 0;
                Resize();
            }
            
            public void Refill(Buffer<T> buffer)
            {
                Count = buffer.Count;
                Resize();
                System.Array.Copy(buffer.Array, 0, Array, 0, Count);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(T item)
            {
                Count++;
                Resize();
                Array[Count - 1] = item;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetAtAndResize(int index, T value)
            {
                if(Count <= index)
                {
                    Count = index + 1;
                    Resize();
                }

                Array[index] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool RemoveAndMixOrder(int index)
            {
                if(index >= 0 && index < Count)
                {
                    Count--;
                    Array[index] = Array[Count];
                    Array[Count] = default;

                    return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Resize()
            {
                if(Array.Length < Count)
                {
                    var newArray = new T[Count * 2];
                    System.Array.Copy(Array, 0, newArray, 0, Array.Length);
                    Array = newArray;
                }

                for(int i = Count; i < Array.Length; i++)
                {
                    Array[i] = default;
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public struct Enumerator : IEnumerator<T>
            {
                private Buffer<T> _buffer;
                private int       _index;
                private T         _current;

                internal Enumerator(Buffer<T> buffer)
                {
                    _buffer = buffer;
                    _index = 0;
                    _current = default(T);
                }

                public void Dispose()
                {
                    _index = -1;
                    _current = default(T);
                }

                public bool MoveNext()
                {
                    if(_index >= _buffer.Count) return false;
                    _current = _buffer.Array[_index];
                    ++_index;
                    return true;
                }

                public void Reset()
                {
                    _index = 0;
                    _current = default(T);
                }

                public T           Current => _current;
                object IEnumerator.Current => _current;
            }
        }
    }
}