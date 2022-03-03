# Indexed Collection   
Indexed collection is an extended version of dictionary collection for cases where Dictionary is not enough and full fledged database is too much.    
Indexed collection allows you to create one-to-one and one-to-many indexes with a o(1) retrival and forcibly or lazyly rebuild indexes.
 
Available indexes:
| Type               | Description                                                                            |
| ------------------ | ---------------------------------------------------------------------------------------| 
| Index By Key       | Groups all values by key retrieved by a selector                                       |
| Index One To One   | Creates dictionary where every value has a unique calculated key retrieved by selector |
| Index By Type      | Lazily creates indexes for the whole type hierarchy of values                          |

```c#
class Dispatcher
{
	private readonly IndexedCollection<Subscription> _untargetedSubscriptions = new();
	
	private IndexedCollection<Subscription>.IndexByKey<IListener> _untargetedByListener;
	private IndexedCollection<Subscription>.IndexByType           _untargetedByForType;

    public Dispatcher()
    {
        _untargetedByListener = _untargetedSubscriptions.CreateIndexByKey(p => p.ListenerContext.Listener);
        _untargetedByForType = _untargetedSubscriptions.CreateIndexByType(p => p.ForType);
    }
	
	public void AddSubscription(Subscription subscription)
    {
        _untargetedSubscriptions.Add(subscription);
    }
		
	public void InjectSubscriptions(IEntity entity)
    {
        foreach(var entry in _untargetedByForType.Get(entity.GetType()))
        {
            AddSubscriptions(entity, entry.Item);
        }
    }
	
	internal void RemoveListener(IListener listener)
    {
        _untargetedByListener.RemoveByKey(listener);
    }
}

