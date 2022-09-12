namespace SimpleRecyclerCollection
{
    using UnityEngine;

    using SimpleRecyclerCollection.Core;

    public abstract class CellViewContext<TData, TContext> : MonoBehaviour
    {
        public abstract RectTransform RectTransform { get; }

        public Collection Collection { get; private set; }
        public TContext Context { get; private set; }

        // Methods

        public void SetCollection(Collection collection) => Collection = collection;
        public void SetContext(TContext context) => Context = context;
        
        public abstract void OnContentUpdate(int index, TData data);
        public abstract void OnPositionUpdate(Vector3 localPosition);

        public virtual void OnContentRefresh() { }
        public virtual void OnPositionRefresh() { }
    }
}