namespace SimpleRecyclerCollection.Example
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MyCellView : CellView<MyCellData>
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

        public override void OnContentUpdate(int index, MyCellData data)
        {
            Title.text = data.Title;
        }

        public override void OnPositionUpdate(Vector3 localPosition) { }
    }
}