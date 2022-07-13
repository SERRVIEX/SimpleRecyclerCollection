namespace SimpleRecyclerCollection
{
    using UnityEngine;
    using UnityEngine.Events;

    public sealed class CollectionLayoutGroup : MonoBehaviour
    {
        public bool AutoTuples
        {
            get => _autoTuples; 
            set
            {
                if (value == _autoTuples) return;

                _autoTuples = value;
                OnMarkedDirty?.Invoke();
            }
        }

        [SerializeField] private bool _autoTuples = false;

        public int TupleCount
        {
            get => _tupleCount; 
            set
            {
                int clampedValue = Mathf.Clamp(value, 1, 8);
                if (_tupleCount == clampedValue) return;

                _tupleCount = clampedValue;
                OnMarkedDirty?.Invoke();
            }
        }

        [SerializeField, Range(1, 8)] private int _tupleCount = 1;

        public bool Expand
        {
            get => _expand; 
            set
            {
                if (value == _expand) return;

                _expand = value;
                OnMarkedDirty?.Invoke();
            }
        }

        [SerializeField] private bool _expand = false;

        public int Align
        {
            get => _align;
            set
            {
                if (value == _align)
                    return;

                _align = Mathf.Clamp(value, -1, 1);
                OnMarkedDirty?.Invoke();
            }
        }

        [SerializeField, Range(-1, 1)] private int _align;

        public RectOffset Padding
        {
            get => _padding; 
            set
            {
                _padding = value;
                OnMarkedDirty?.Invoke();
            }
        }

        [SerializeField] private RectOffset _padding;

        public Vector2 Spacing
        {
            get => _spacing; 
            set
            {
                if (value == _spacing) return;

                _spacing = new Vector2(Mathf.Clamp(value.x, 0, 250), Mathf.Clamp(value.y, 0, 250));
                OnMarkedDirty?.Invoke();
            }
        }

        [SerializeField] private Vector2 _spacing;

        public UnityEvent OnMarkedDirty = new UnityEvent();

        // Methods

        private void OnValidate()
        {
            if(Application.isPlaying)
                OnMarkedDirty?.Invoke();

            _spacing = new Vector2(Mathf.Clamp(_spacing.x, 0, 250), Mathf.Clamp(_spacing.y, 0, 250));
        }
    }
}