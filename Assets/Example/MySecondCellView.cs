namespace SimpleRecyclerCollection.Example
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MySecondCellView : MyCellView
    {
        [SerializeField] private Image _thumbnail;

        // Methods

        public override void OnContentUpdate(int index, MyCellData data)
        {
            base.OnContentUpdate(index, data);

            MySecondCellData secondCellData = data as MySecondCellData;
            _thumbnail.sprite = secondCellData.Thumbnail;
        }

        public override void OnPositionUpdate(Vector3 localPosition) { }
    }
}