namespace SimpleRecyclerCollection.Example
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MyThirdCellView : MyMainCellView
    {
        [SerializeField] private Image _background;

        // Methods

        public override void OnContentUpdate(int index, MyMainCellData data)
        {
            base.OnContentUpdate(index, data);

            MyThirdCellData thirdCellData = data as MyThirdCellData;
            _background.color = thirdCellData.BackgroundColor;
        }

        public override void OnPositionUpdate(Vector3 localPosition) { }
    }
}