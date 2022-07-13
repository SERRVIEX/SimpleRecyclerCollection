namespace SimpleRecyclerCollection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.Assertions;

    using Core;

    public partial class RecyclerCollection<TCellData, TCellView> : Collection
       where TCellData : class, new()
       where TCellView : CellView<TCellData>
    {
        public CollectionData<TCellData> Data
        {
            get
            {
                if (!Application.isPlaying)
                    return null;

                if (!IsInitialized)
                {
                    Debug.Log($"The collection has not been initialized yet.");
                    return null;
                }

                return _data;
            }
        }

        private CollectionData<TCellData> _data;

        [SerializeField] private CellReference<TCellData, TCellView> _cellReferences = new CellReference<TCellData, TCellView>();
        private Dictionary<Type, TCellView> _cellViewPrefabsDistributed = new Dictionary<Type, TCellView>();
        private TCellView _cellDefaultPrefab;

        /// <summary>
        /// Create additional cells in cache (cellCount * tupleCount).
        /// </summary>
        public int CachedCellCount
        {
            get => _cachedCellCount;
            set
            {
                _cachedCellCount = Mathf.Clamp(value, 0, 128);
                RebuildLayout();
            }
        }

        [SerializeField] private int _cachedCellCount;

        /// <summary>
        /// Auto-calculated or fixed value from the LayoutGroup.
        /// </summary>
        private int _currentTupleCount;
        private float _currentTuplePadding;

        private Vector2 _initialCellSize;
        private Vector2 _currentCellSize;

        /// <summary>
        /// Number of cells that can be visible in content rect.
        /// </summary>
        private int _numberOfCells;

        private class ReusableCell
        {
            public int Index;
            public TCellData Data;
            public TCellView View;

            public bool ActiveSelf
            {
                get => View.gameObject.activeSelf;
                set => View.gameObject.SetActive(value);
            }

            // Constructors

            public ReusableCell(TCellView view)
            {
                Index = -1;
                View = view;
                ActiveSelf = false;
            }
        }

        /// <summary>
        /// All available reusable cells.
        /// </summary>
        private List<ReusableCell> _reusableCells = new List<ReusableCell>();

        /// <summary>
        /// Views that can potentially be used in a reusable cell through swap.
        /// </summary>
        private List<TCellView> _reusableCellViews = new List<TCellView>();

        // Methods

        public override void Initialize()
        {
            for (int i = 0; i < _cellReferences.References.Length; i++)
            {
                var reference = _cellReferences.References[i];

                Assert.IsTrue(reference.View != null);

                Type cellDataType = Type.GetType(reference.Type);
                _cellViewPrefabsDistributed.Add(cellDataType, reference.View);

                if(reference.View.GetType() == typeof(TCellView))
                    _cellDefaultPrefab = reference.View;
            }

            IsInitialized = true;

            _data = new CollectionData<TCellData>();
            _data.OnMarkedDirty.AddListener(() =>
            {
                UpdateContentVirtualSize();
                UpdatePosition();
            });

            Content.OnTransformsDimensionsChanged.AddListener(RebuildLayout);
            LayoutGroup.OnMarkedDirty.AddListener(RebuildLayout);

            _initialCellSize = _cellDefaultPrefab.RectTransform.rect.size;
            _currentCellSize = _initialCellSize;

            RebuildLayout();
        }

        protected override void OnValidate()
        {
            if (_cellReferences == null)
                _cellReferences = new CellReference<TCellData, TCellView>();
        }

        protected sealed override void UpdatePool()
        {
            _numberOfCells = Mathf.Clamp(CalculateNumberOfCells(), 0, 512);

            // Destroy unused reusable cells to free memory.
            if (_numberOfCells < _reusableCells.Count)
            {
                int count = _reusableCells.Count - _numberOfCells;

                for (int i = _reusableCells.Count - 1; i >= 0; i--)
                {
                    Destroy(_reusableCells[i].View.gameObject);
                    _reusableCells.RemoveAt(i);

                    count--;
                    if (count == 0)
                        break;
                }
            }

            while (_reusableCellViews.Count > 64)
            {
                Destroy(_reusableCellViews[0].gameObject);
                _reusableCellViews.RemoveAt(0);
            }
        }

        protected sealed override void UpdateProperties()
        {
            MainAxis = (int)Direction;
            SecondAxis = MainAxis == 0 ? 1 : 0;

            CalculateCellSize();
            CalculateNumberOfTuples();
            CalculatePadding();

            CellRatio = (_currentCellSize[MainAxis] + LayoutGroup.Spacing[MainAxis]) / Content.RectTransform.rect.size[MainAxis];

            CalculateAlign();
        }

        protected sealed override void UpdateContentVirtualSize()
        {
            // Counting the number of cells in one tuple.
            int value = Mathf.CeilToInt(_data.Count / (float)_currentTupleCount);
            // Calculate the total cell size of one tuple.
            float size = value * _currentCellSize[MainAxis];
            // Calculate the total space size of one tuple.
            size += (value - 1) * LayoutGroup.Spacing[MainAxis];
            // Padding must be taken into account.
            size += Direction == ScrollDirection.Horizontal ? LayoutGroup.Padding.horizontal : LayoutGroup.Padding.vertical;

            ContentVirtualSize = size;
            MaxScrollPosition = Mathf.Max(0, size - Content.RectTransform.rect.size[MainAxis]);
        }

        protected sealed override void UpdatePosition()
        {
            Vector2 contentSize = Content.RectTransform.rect.size;
            // Get cell size given space.
            Vector2 cellSizeWithSpacing = _currentCellSize + LayoutGroup.Spacing;
            // Convert position to expected floating point index.
            float indexedPosition = (Position[MainAxis] - CachedPadding[MainAxis]) / cellSizeWithSpacing[MainAxis];
            // Convert floating point index to integer.
            int targetIndex = Mathf.FloorToInt(indexedPosition);
            // From this position, the positions of all cells will be calculated.
            var targetPosition = (targetIndex - indexedPosition) * CellRatio;
            // Keep correct index for different tuples.
            // This means that grid is also supported.
            targetIndex *= _currentTupleCount;

            // Cached values.
            Vector3 localPosition = Vector3.zero;
            float invertedContentSize = contentSize[MainAxis] * (1 - Content.RectTransform.pivot[MainAxis]);
            // Force to ignore where the main axis pivot point of the content is.
            float centerOfContent = contentSize[SecondAxis] * (.5f - Content.RectTransform.pivot[SecondAxis]);

            for (int reusableCellIndex = 0, positionAlongMainAxis = -1, tupleIndex = 0; reusableCellIndex < _numberOfCells; reusableCellIndex++)
            {
                int index = targetIndex + reusableCellIndex;
                if (index % _currentTupleCount == 0)
                    positionAlongMainAxis++;

                ReusableCell reusableCell = reusableCellIndex < _reusableCells.Count ? _reusableCells[reusableCellIndex] : null;

                if (index < 0 || index >= _data.Count || targetPosition > 1)
                {
                    if (reusableCell != null)
                        reusableCell.ActiveSelf = false;
                    continue;
                }

                TCellData data = _data[index];

                if (reusableCell == null)
                    reusableCell = CreateReusableCell(FindCellViewPrefabType(data));

                // Don't try to swap or create a cell view if there is only one type of cell view prefab.
                if (_cellViewPrefabsDistributed.Count > 1)
                    SwapOrCreateCellView(ref reusableCell, data);

                if (reusableCell.Index != index || reusableCell.Data != data || !reusableCell.ActiveSelf)
                {
                    reusableCell.Index = index;
                    reusableCell.Data = data;
                    reusableCell.ActiveSelf = true;
                    reusableCell.View.OnContentUpdate(index, data);
                }

                // Calculate the main axis position in the content.
                float mainAxisPositionInTheContent = -(contentSize[MainAxis] * (targetPosition + CellRatio * positionAlongMainAxis) - invertedContentSize);
                // Force to ignore where the main axis pivot point of the cell is.
                float centerOfCell = (_currentCellSize[MainAxis] * (.5f - reusableCell.View.RectTransform.pivot[MainAxis])) - _currentCellSize[MainAxis] / 2;
                // Calculate the second axis position in the content.
                float secondAxisPositionInTheContent = (_currentCellSize[SecondAxis] + LayoutGroup.Spacing[SecondAxis]) * tupleIndex + CachedAlign;

                localPosition[MainAxis] = mainAxisPositionInTheContent + centerOfCell + _currentTuplePadding;
                localPosition[SecondAxis] = secondAxisPositionInTheContent + centerOfContent + CachedPadding[SecondAxis];

                // Inverse position for horizontal scrlling.
                if (Direction == ScrollDirection.Horizontal)
                    localPosition = -localPosition;

                reusableCell.View.RectTransform.sizeDelta = _currentCellSize;
                reusableCell.View.RectTransform.localPosition = localPosition;
                reusableCell.View.OnPositionUpdate(localPosition);

                // Process tuples.
                tupleIndex++;
                if (tupleIndex == _currentTupleCount)
                    tupleIndex = 0;
            }

            CalculateNormalizedPosition();
        }

        protected sealed override int CalculateNumberOfCells()
        {
            CalculateNumberOfTuples();
            int value = CalculateNumberOfCellsForAxis(m_Direction);
            return value * _currentTupleCount;
        }

        protected sealed override int CalculateNumberOfCellsForAxis(ScrollDirection direction)
        {
            int axis = (int)direction;

            Vector2 contentSize = Content.RectTransform.rect.size;
            Vector2 cellSize = _currentCellSize;

            // Calculate the number of cells without taking into account the space between them.
            int maxPosibleCellCount = Mathf.CeilToInt(contentSize[axis] / cellSize[axis]);

            // Calculate the height of all cells taking into account the space between them.
            float calculatedSize = maxPosibleCellCount * cellSize[axis] + (maxPosibleCellCount - 1) * LayoutGroup.Spacing[axis];

            // Get extra value that goes beyond content.
            float extraSize = calculatedSize - contentSize[axis];
            float requiredSpaceForCell = cellSize[axis] + LayoutGroup.Spacing[axis];

            // Remove extra cells.
            maxPosibleCellCount -= Mathf.FloorToInt(extraSize / requiredSpaceForCell);

            maxPosibleCellCount += _currentTupleCount;
            maxPosibleCellCount += _cachedCellCount;

            return maxPosibleCellCount;
        }

        private void CalculateNumberOfTuples()
        {
            if (!LayoutGroup.AutoTuples)
            {
                _currentTupleCount = LayoutGroup.TupleCount;
                return;
            }

            Vector2 contentSize = Content.RectTransform.rect.size;

            if (Direction == ScrollDirection.Horizontal)
                contentSize[SecondAxis] -= LayoutGroup.Padding.horizontal;
            else
                contentSize[SecondAxis] -= LayoutGroup.Padding.vertical;

            Vector2 cellSize = _currentCellSize;

            // Calculate the number of cells without taking into account the space between them.
            int maxPosibleCellCount = Mathf.CeilToInt(contentSize[SecondAxis] / cellSize[SecondAxis]);

            // Calculate the height of all cells taking into account the space between them.
            float calculatedSize = maxPosibleCellCount * cellSize[SecondAxis] + (maxPosibleCellCount - 1) * LayoutGroup.Spacing[SecondAxis];

            // Get extra value that goes beyond content.
            float extraSize = calculatedSize - contentSize[SecondAxis];
            float requiredSpaceForCell = cellSize[SecondAxis] + LayoutGroup.Spacing[SecondAxis];

            // Remove extra cells.
            maxPosibleCellCount -= Mathf.CeilToInt(extraSize / requiredSpaceForCell);

            _currentTupleCount = Mathf.Clamp(maxPosibleCellCount, 1, 8);
            _currentTuplePadding = _currentCellSize[MainAxis] * (_cachedCellCount / 2f);
        }

        private void CalculateCellSize()
        {
            if (LayoutGroup.AutoTuples)
            {
                _currentCellSize = _initialCellSize;
                return;
            }

            if (LayoutGroup.Expand)
            {
                Vector2 contentSize = Content.RectTransform.rect.size;
                float padding = Direction == ScrollDirection.Horizontal ? LayoutGroup.Padding.vertical : LayoutGroup.Padding.horizontal;
                contentSize[SecondAxis] -= padding;
                float size = contentSize[SecondAxis] - (LayoutGroup.Spacing[SecondAxis] * (LayoutGroup.TupleCount - 1));
                size /= LayoutGroup.TupleCount;
                _currentCellSize[SecondAxis] = size - padding / LayoutGroup.TupleCount;
                return;
            }

            _currentCellSize = _initialCellSize;
        }

        private void CalculatePadding()
        {
            if (Direction == ScrollDirection.Horizontal)
            {
                CachedPadding[MainAxis] = LayoutGroup.Padding.left + _currentTuplePadding;
                CachedPadding[SecondAxis] = LayoutGroup.Padding.bottom - LayoutGroup.Padding.top;
            }
            else
            {
                CachedPadding[SecondAxis] = LayoutGroup.Padding.left - LayoutGroup.Padding.right;
                CachedPadding[MainAxis] = LayoutGroup.Padding.top + _currentTuplePadding;
            }
        }

        private void CalculateAlign()
        {
            Vector2 contentSize = Content.RectTransform.rect.size;
            Vector2 cellSizeWithSpace = _currentCellSize + LayoutGroup.Spacing;

            int align = LayoutGroup.Expand ? 0 : -LayoutGroup.Align;

            // Calculate the total size of the second axis given the number of tuples.
            float secondAxisSize = cellSizeWithSpace[SecondAxis] * _currentTupleCount;
            // First, align all the tuples to the content center.
            // This will make it easier for us to align to a given value.
            CachedAlign = (cellSizeWithSpace[SecondAxis] - secondAxisSize) / 2 - LayoutGroup.Spacing[SecondAxis] / 2 * align;
            CachedAlign -= align * (Content.RectTransform.rect.size[SecondAxis] - secondAxisSize);
            CachedAlign = CachedAlign + contentSize[SecondAxis] / 2 * align - secondAxisSize / 2 * align;
        }

        private ReusableCell CreateReusableCell(Type viewType)
        {
            TCellView view = CreateCellView(viewType);
            ReusableCell reusableCell = new ReusableCell(view);
            _reusableCells.Add(reusableCell);
            return reusableCell;
        }

        private Type FindCellViewPrefabType(TCellData cellData)
        {
            Type cellDataType = cellData.GetType();

            if (_cellViewPrefabsDistributed.TryGetValue(cellDataType, out TCellView cellView))
                return cellView.GetType();

            Debug.LogError($"Cell view prefab not found for the {cellDataType}.");

            return _cellDefaultPrefab.GetType();
        }

        private TCellView CreateCellView(Type viewType)
        {
            TCellView view = Instantiate(FindCellViewPrefab(viewType), Content.RectTransform);
            view.RectTransform.sizeDelta = _currentCellSize;
            return view;
        }

        private void SwapOrCreateCellView(ref ReusableCell target, TCellData data)
        {
            Type viewType = FindCellViewPrefabType(data);

            if (target.View.GetType() == viewType)
                return;

            target.View.gameObject.SetActive(false);
            _reusableCellViews.Add(target.View);

            while (_reusableCellViews.Count > 64)
            {
                Destroy(_reusableCellViews[0].gameObject);
                _reusableCellViews.RemoveAt(0);
            }

            for (int i = 0; i < _reusableCellViews.Count; i++)
            {
                if (_reusableCellViews[i].GetType() == viewType)
                {
                    TCellView view = _reusableCellViews[i];
                    _reusableCellViews.RemoveAt(i);

                    target.View = view;
                    return;
                }
            }

            target.View = CreateCellView(viewType);
            target.View.gameObject.SetActive(false);
        }

        protected TCellView FindCellViewPrefab(Type viewType)
        {
            for (int i = 0; i < _cellReferences.References.Length; i++)
                if (_cellReferences.References[i].View.GetType() == viewType)
                    return _cellReferences.References[i].View;

            return null;
        }

        public virtual void SnapTo(int index)
        {
            Velocity = 0;
            Vector2 cellSizeWithSpacing = _currentCellSize + LayoutGroup.Spacing;
            float targetPosition = Mathf.FloorToInt(index / (float)_currentTupleCount) * cellSizeWithSpacing[MainAxis];
            targetPosition = Mathf.Clamp(targetPosition + CachedPadding[MainAxis] - (Content.RectTransform.rect.size[MainAxis] - _currentCellSize[MainAxis]) / 2f, 0, MaxScrollPosition);
            Position[MainAxis] = targetPosition;
            UpdatePosition();
        }

        public virtual void SnapTo(TCellData item) => SnapTo(Data.IndexOf(item));

        public virtual void ScrollTo(int index, float duration = .25f) => StartCoroutine(ScrollToImpl(index, duration));

        public virtual void ScrollTo(TCellData item, float duration = .25f) => ScrollTo(Data.IndexOf(item), duration);

        protected virtual IEnumerator ScrollToImpl(int index, float duration)
        {
            Velocity = 0;
            float originPosition = Position[MainAxis];
            Vector2 cellSizeWithSpacing = _currentCellSize + LayoutGroup.Spacing;
            float targetPosition = Mathf.FloorToInt(index / (float)_currentTupleCount) * cellSizeWithSpacing[MainAxis];
            targetPosition = Mathf.Clamp(targetPosition + CachedPadding[MainAxis] - (Content.RectTransform.rect.size[MainAxis] - _currentCellSize[MainAxis]) / 2f, 0, MaxScrollPosition);

            float time = 0;
            while (time < duration)
            {
                Position[MainAxis] = Mathf.Lerp(originPosition, targetPosition, time / duration);
                UpdatePosition();
                time += Time.deltaTime;
                yield return null;
            }

            Position[MainAxis] = targetPosition;
            UpdatePosition();
        }
    }
}