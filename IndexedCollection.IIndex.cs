namespace IndexedCollection
{
    public partial class IndexedCollection<TItem>
    {
        private interface IIndex
        {
            void Remove(Entry item);
            void Add(Entry    entry);
            void Rebuild();
            void Clear();
        }
    }
}