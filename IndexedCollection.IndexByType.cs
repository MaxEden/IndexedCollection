using System;

namespace IndexedCollection
{
    public partial class IndexedCollection<TItem>
    {
        public class IndexByType : IndexByCriterion<Type, Type>
        {
            public IndexByType(Func<TItem, Type>        selector,
                               IndexedCollection<TItem> collection) : base(selector,
                (critKey, queryKey) => critKey.IsAssignableFrom(queryKey),
                collection) {}
        }
    }
}