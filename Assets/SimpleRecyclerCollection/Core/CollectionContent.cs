namespace SimpleRecyclerCollection.Core
{
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(CollectionLayoutGroup))]
    public sealed class CollectionContent : MonoBehaviour
    {
        public RectTransform RectTransform => _rectTransform;
        [SerializeField] private RectTransform _rectTransform;

        [HideInInspector] public UnityEvent OnTransformsDimensionsChanged = new UnityEvent();

        // Methods

        private void OnValidate()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (!Application.isPlaying)
                if (GetComponent<CollectionLayoutGroup>() == null)
                    gameObject.AddComponent<CollectionLayoutGroup>();
        }

        private void OnRectTransformDimensionsChange() => OnTransformsDimensionsChanged?.Invoke();
    }
}