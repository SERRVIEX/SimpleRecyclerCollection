namespace SimpleRecyclerCollection
{
    using UnityEngine;

    public abstract class CellView<TData> : MonoBehaviour
    {
        public abstract RectTransform RectTransform { get; }

        // Methods

        public abstract void OnContentUpdate(int index, TData data);
        public abstract void OnPositionUpdate(Vector3 localPosition);
    }
}