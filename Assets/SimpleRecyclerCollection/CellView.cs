namespace SimpleRecyclerCollection
{
    using UnityEngine;

    using SimpleRecyclerCollection.Core;

    public abstract class CellView<TData> : MonoBehaviour
    {
        public abstract RectTransform RectTransform { get; }
        public Collection Collection { get; private set; }

        // Methods

        public void Initialize(Collection collection) => Collection = collection;
        
        public abstract void OnContentUpdate(int index, TData data);
        public abstract void OnPositionUpdate(Vector3 localPosition);

        public virtual void OnContentRefresh() { }
        public virtual void OnPositionRefresh() { }
    }
}