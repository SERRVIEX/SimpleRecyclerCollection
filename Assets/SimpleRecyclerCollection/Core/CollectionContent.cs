namespace SimpleRecyclerCollection.Core
{
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(CollectionLayoutGroup))]
    public class CollectionContent : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }

        [HideInInspector] public UnityEvent OnTransformsDimensionsChanged = new UnityEvent();

        // Methods

        private void Awake() => RectTransform = GetComponent<RectTransform>();

        private void OnValidate()
        {
            if(!Application.isPlaying)
                if (GetComponent<CollectionLayoutGroup>() == null)
                    gameObject.AddComponent<CollectionLayoutGroup>();
        }

        private void OnRectTransformDimensionsChange() => OnTransformsDimensionsChanged?.Invoke();
    }
}