namespace SimpleRecyclerCollection
{
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.Assertions;

    using Core;

    public class RecyclerCollectionContext<TCellData, TCellView, TContext> : Collection
       where TCellData : class
       where TCellView : CellViewContext<TCellData, TContext>
    {
        public TContext Context { get; private set; }

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

        [SerializeField] private TCellView _cellPrefab;

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

        private List<ReusableCell> _reusableCells = new List<ReusableCell>();

        // Methods

        public override void Initialize()
        {
            if (IsInitialized)
            {
                Debug.Log($"Collection already initialized.");
                return;
            }

            if (Context == null)
            {
                Debug.Log($"Context can't be null.");
                return;
            }

            // Awake is called only if the object is active, so if it isn't active in
            // the hierarchy, force it to get the components.
            if (!gameObject.activeInHierarchy)
                Awake();

            Assert.IsTrue(_cellPrefab != null);

            _data = new CollectionData<TCellData>();
            _data.OnMarkedDirty.AddListener(() =>
            {
                UpdateContentVirtualSize();
                UpdatePosition();
            });

            Content.OnTransformsDimensionsChanged.AddListener(RebuildLayout);
            LayoutGroup.OnMarkedDirty.AddListener(RebuildLayout);

            _initialCellSize = _cellPrefab.RectTransform.rect.size;
            _currentCellSize = _initialCellSize;

            IsInitialized = true;

            RebuildLayout();
        }

        public void SetContext(TContext context)
        {
            Context = context;
        }

        protected sealed override void UpdatePool()
        {
            _numberOfCells = Mathf.Clamp(CalculateNumberOfCells(), 0, 512);

            // Create missing cells.
            if (_numberOfCells > _reusableCells.Count)
            {
                for (int i = _reusableCells.Count; i < _numberOfCells; i++)
                {
                    TCellView view = Instantiate(_cellPrefab, Content.RectTransform);
                    view.SetCollection(this);
                    view.SetContext(Context);
                    _reusableCells.Add(new ReusableCell(view));
                }
            }
            // Destroy unused reusable cells to free memory.
            else if (_numberOfCells < _reusableCells.Count)
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
        }

        protected sealed override void UpdateProperties()
        {
            // Get scrolling direction.
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
            float indexedPosition = (m_Position[MainAxis] - CachedPadding[MainAxis]) / cellSizeWithSpacing[MainAxis];
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

            for (int reusableCellIndex = 0, positionAlongMainAxis = -1, tupleIndex = 0; reusableCellIndex < _reusableCells.Count; reusableCellIndex++, tupleIndex++)
            {
                int index = targetIndex + reusableCellIndex;
                if (index % _currentTupleCount == 0)
                    positionAlongMainAxis++;

                ReusableCell reusableCell = _reusableCells[CircularIndex(index, _reusableCells.Count)];

                if (index < 0 || index >= _data.Count)
                {
                    reusableCell.ActiveSelf = false;
                    continue;
                }

                // Simple circular index.
                tupleIndex %= _currentTupleCount;

                TCellData data = _data[index];

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
            }

            CalculateNormalizedPosition();
        }

        /// <summary>
        /// Complex circular index.
        /// </summary>
        private int CircularIndex(int i, int size)
        {
            if (i < 0) 
                return size - 1 + (i + 1) % size;

            return (i + 1) % size;
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
                _currentCellSize[SecondAxis] = size;
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

        /// <summary>
        /// Update the content of all cells.
        /// </summary>
        public void Refresh()
        {
            for (int i = 0; i < _reusableCells.Count; i++)
            {
                ReusableCell reusableCell = _reusableCells[i];
                if (reusableCell.ActiveSelf)
                {
                    reusableCell.View.OnContentRefresh();
                    reusableCell.View.OnPositionRefresh();
                }
            }
        }

        public virtual void SnapTo(int index)
        {
            Velocity = 0;
            Vector2 cellSizeWithSpacing = _currentCellSize + LayoutGroup.Spacing;
            float targetPosition = Mathf.FloorToInt(index / (float)_currentTupleCount) * cellSizeWithSpacing[MainAxis];
            targetPosition = Mathf.Clamp(targetPosition + CachedPadding[MainAxis] - (Content.RectTransform.rect.size[MainAxis] - _currentCellSize[MainAxis]) / 2f, 0, MaxScrollPosition);
            m_Position[MainAxis] = targetPosition;
            UpdatePosition();
        }

        public virtual void SnapTo(TCellData item) => SnapTo(Data.IndexOf(item));

        public virtual void ScrollTo(int index, float duration = .25f)
        {
            Velocity = 0;

            float originPosition = m_Position[MainAxis];
            AutoScroller.Scroll(duration, t =>
            {
                Vector2 cellSizeWithSpacing = _currentCellSize + LayoutGroup.Spacing;
                float targetPosition = Mathf.FloorToInt(index / (float)_currentTupleCount) * cellSizeWithSpacing[MainAxis];
                targetPosition = Mathf.Clamp(targetPosition + CachedPadding[MainAxis] - (Content.RectTransform.rect.size[MainAxis] - _currentCellSize[MainAxis]) / 2f, 0, MaxScrollPosition);

                m_Position[MainAxis] = Mathf.Lerp(originPosition, targetPosition, t);
            });
        }

        public virtual void ScrollTo(TCellData item, float duration = .25f) => ScrollTo(Data.IndexOf(item), duration);

        public virtual void ScrollTo(float position, float duration)
        {
            float originPosition = m_Position[MainAxis];
            AutoScroller.Scroll(duration, t =>
            {
                m_Position[MainAxis] = Mathf.Lerp(originPosition, position, t);
            });
        }
    }
}