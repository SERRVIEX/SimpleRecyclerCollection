namespace SimpleRecyclerCollection.Core
{
    using System;

    using UnityEngine;

    [Serializable]
    public sealed class CellReference<TCellData, TCellView> where TCellView : CellView<TCellData>
    {
        [SerializeField] private string _baseType;

        [Serializable]
        public class Reference
        {
            public string AssemblyQualifiedName { get => _type; set => _type = value; }
            [SerializeField] public string _assemblyQualifiedName;

            public string Type { get => _type; set => _type = value; }
            [SerializeField] public string _type;

            public TCellView View { get => _view; set => _view = value; }
            [SerializeField] public TCellView _view;
        }

        public Reference[] References { get => _references; set => _references = value; }
        [SerializeField] public Reference[] _references;

        // Constructors

        public CellReference() => OnValidate();

        // Methods

        public void OnValidate() => _baseType = typeof(TCellData).AssemblyQualifiedName;
    }
}