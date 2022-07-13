namespace SimpleRecyclerCollection.Example
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MyMainCellView : CellView<MyMainCellData>
    {
        public override RectTransform RectTransform => m_RectTransform;
        [SerializeField] protected RectTransform m_RectTransform;

        [SerializeField] protected Text Title;

        // Methods

        private void OnValidate()
        {
            if(m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();
        }

        public override void OnContentUpdate(int index, MyMainCellData data)
        {
            Title.text = data.Title;
        }

        public override void OnPositionUpdate(Vector3 localPosition) { }
    }
}