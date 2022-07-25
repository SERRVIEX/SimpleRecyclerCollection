namespace SimpleRecyclerCollection.Core
{
    using System;

    using UnityEngine;

    [Serializable]
    public sealed class CellReference<TCellData, TCellView> where TCellView : CellView<TCellData>
    {
        [SerializeField] private string _baseDataType;

        [Serializable]
        public class Reference
        {
            public string DataTypeAssemblyQualifiedName { get => _dataTypeAssemblyQualifiedName; set => _dataTypeAssemblyQualifiedName = value; }
            [SerializeField] private string _dataTypeAssemblyQualifiedName;

            public string DataType { get => _dataType; set => _dataType = value; }
            [SerializeField] private string _dataType;

            public TCellView View { get => _view; set => _view = value; }
            [SerializeField] private TCellView _view;
        }

        public Reference[] References { get => _references; set => _references = value; }
        [SerializeField] private Reference[] _references;

        // Constructors

        public CellReference() => OnValidate();

        // Methods

        public void OnValidate() => _baseDataType = typeof(TCellData).AssemblyQualifiedName;
    }
}