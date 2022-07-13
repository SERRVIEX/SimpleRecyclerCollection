namespace SimpleRecyclerCollection.Example
{
    using UnityEngine;
    using UnityEngine.UI;

    public class Configuration : MonoBehaviour
    {
        [SerializeField] private MyCollection _collection;

        [SerializeField] private Toggle _autoTuples;
        [SerializeField] private Slider _tupleCount;
        [SerializeField] private Toggle _expand;
        [SerializeField] private Slider _paddingLeft;
        [SerializeField] private Slider _paddingRight;
        [SerializeField] private Slider _paddingTop;
        [SerializeField] private Slider _paddingBottom;
        [SerializeField] private Slider _align;
        [SerializeField] private Slider _spacingX;
        [SerializeField] private Slider _spacingY;

        // Methods

        private void Start()
        {
            _collection.Initialize();

            _autoTuples.onValueChanged.AddListener(value =>
            {
                _tupleCount.interactable = !value;
                _expand.interactable = !value;

                _collection.LayoutGroup.AutoTuples = value;
            });

            _tupleCount.onValueChanged.AddListener(value =>
            {
                _collection.LayoutGroup.TupleCount = (int)value;
            });

            _expand.onValueChanged.AddListener(value =>
            {
                _autoTuples.interactable = !value;
                _collection.LayoutGroup.Expand = value;
            });

            _paddingLeft.onValueChanged.AddListener(value =>
            {
                RectOffset padding = _collection.LayoutGroup.Padding;
                padding.left = (int)value;
                _collection.LayoutGroup.Padding = padding;
            });

            _paddingRight.onValueChanged.AddListener(value =>
            {
                RectOffset padding = _collection.LayoutGroup.Padding;
                padding.right = (int)value;
                _collection.LayoutGroup.Padding = padding;
            });

            _paddingTop.onValueChanged.AddListener(value =>
            {
                RectOffset padding = _collection.LayoutGroup.Padding;
                padding.top = (int)value;
                _collection.LayoutGroup.Padding = padding;
            });

            _paddingBottom.onValueChanged.AddListener(value =>
            {
                RectOffset padding = _collection.LayoutGroup.Padding;
                padding.bottom = (int)value;
                _collection.LayoutGroup.Padding = padding;
            });

            _align.onValueChanged.AddListener(value =>
            {
                _collection.LayoutGroup.Align = (int)value;
            });

            _spacingX.onValueChanged.AddListener(value =>
            {
                Vector2 spacing = _collection.LayoutGroup.Spacing;
                spacing.x = value;
                _collection.LayoutGroup.Spacing = spacing;
            });

            _spacingY.onValueChanged.AddListener(value =>
            {
                Vector2 spacing = _collection.LayoutGroup.Spacing;
                spacing.y = value;
                _collection.LayoutGroup.Spacing = spacing;
            });
        }
    }
}