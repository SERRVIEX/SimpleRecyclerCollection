namespace SimpleRecyclerCollection.Example
{
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;

    public class DataController : MonoBehaviour
    {
        [SerializeField] private MyCollection _collection;

        [SerializeField] private Sprite[] _sprites;

        [SerializeField] private InputField _inputCount;
        [SerializeField] private InputField _inputIndex;
        [SerializeField] private Button _addType1;
        [SerializeField] private Button _addType2;
        [SerializeField] private Button _addType3;

        [SerializeField] private InputField _toIndex;
        [SerializeField] private Button _snapTo;
        [SerializeField] private Button _scrollTo;

        [SerializeField] private Button _clearAll;

        // Methods

        private void Start()
        {
            _inputCount.text = 10.ToString();
            _inputIndex.text = 0.ToString();
            _toIndex.text = 0.ToString();

            _addType1.onClick.AddListener(() =>
            {
                int count = int.Parse(_inputCount.text);
                int index = int.Parse(_inputIndex.text);

                List<MyMainCellData> list = new List<MyMainCellData>();

                for (int i = 0; i < count; i++)
                {
                    MyMainCellData data = new MyMainCellData();
                    data.Title = $"Main Cell {Random.Range(9000, 9999)}";
                    list.Add(data);
                }

                _collection.Data.Insert(index, list);
            });

            _addType2.onClick.AddListener(() =>
            {
                int count = int.Parse(_inputCount.text);
                int index = int.Parse(_inputIndex.text);
                MySecondCellData[] array = new MySecondCellData[count];

                for (int i = 0; i < count; i++)
                {
                    MySecondCellData data = new MySecondCellData();
                    data.Title = $"Cat {Random.Range(9000, 9999)}";
                    data.Thumbnail = _sprites[Random.Range(0, _sprites.Length)];
                    array[i] = data;
                }

                _collection.Data.Insert(index, array);
            });

            _addType3.onClick.AddListener(() =>
            {
                int count = int.Parse(_inputCount.text);
                int index = int.Parse(_inputIndex.text);

                for (int i = 0; i < count; i++)
                {
                    MyThirdCellData data = new MyThirdCellData();
                    Color color = Random.ColorHSV();
                    string hex = ColorUtility.ToHtmlStringRGBA(color);
                    data.Title = hex;
                    data.BackgroundColor = color;
                    _collection.Data.Insert(index, data);
                } 
            });

            _snapTo.onClick.AddListener(() =>
            {
                int index = int.Parse(_toIndex.text);
                _collection.SnapTo(index);
            });

            _scrollTo.onClick.AddListener(() =>
            {
                int index = int.Parse(_toIndex.text);
                _collection.ScrollTo(index);
            });

            _clearAll.onClick.AddListener(() =>
            {
                _collection.Data.Clear();
            });
        }
    }
}