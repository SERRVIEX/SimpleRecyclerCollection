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
        [SerializeField] private Button _addPrefab;

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

            _addPrefab.onClick.AddListener(() =>
            {
                int count = int.Parse(_inputCount.text);
                int index = int.Parse(_inputIndex.text);

                List<MyCellData> list = new List<MyCellData>();

                for (int i = 0; i < count; i++)
                {
                    MyCellData data = new MyCellData();
                    data.Title = $"Main Cell {Random.Range(9000, 9999)}";
                    list.Add(data);
                }

                _collection.Data.Insert(index, list);
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