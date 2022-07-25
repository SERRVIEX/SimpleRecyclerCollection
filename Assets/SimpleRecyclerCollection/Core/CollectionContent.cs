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

        private bool _markedDirty;

        // Methods

        private void OnValidate()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (!Application.isPlaying)
                if (GetComponent<CollectionLayoutGroup>() == null)
                    gameObject.AddComponent<CollectionLayoutGroup>();
        }

        private void Update()
        {
            if(_markedDirty)
            {
                _markedDirty = false;
                OnTransformsDimensionsChanged?.Invoke();
            }
        }

        private void OnRectTransformDimensionsChange() => _markedDirty = true;
    }
}