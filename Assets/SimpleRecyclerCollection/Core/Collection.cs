namespace SimpleRecyclerCollection.Core
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Assertions;
    using UnityEngine.EventSystems;

    public abstract class Collection : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool IsInitialized { get; protected set; }

        public RectTransform RectTransform { get; private set; }

        public RectTransform Viewport => m_Viewport;
        [SerializeField] protected RectTransform m_Viewport;

        public CollectionContent Content => m_Content;
        [SerializeField] protected CollectionContent m_Content;

        public CollectionLayoutGroup LayoutGroup { get; private set; }

        public ScrollDirection Direction
        {
            get => m_Direction; set
            {
                m_Direction = value;
                RebuildLayout();
            }
        }

        [SerializeField] protected ScrollDirection m_Direction = ScrollDirection.Vertical;

        public bool Inertia { get => m_Inertia; set => m_Inertia = value; }
        [SerializeField] protected bool m_Inertia = true;

        public float DecelerationRate { get => m_DecelerationRate; set => m_DecelerationRate = value; }
        [SerializeField] protected float m_DecelerationRate = .135f;

        public UnityEvent<float> OnValueChanged { get => m_OnValueChanged; set => m_OnValueChanged = value; }
        [SerializeField] protected UnityEvent<float> m_OnValueChanged = new UnityEvent<float>();

        [HideInInspector] public float Velocity;

        protected AutoScroller AutoScroller { get; private set; }

        public float NormalizedPosition
        {
            get => _normalizedPosition;
            set => SetNormalizedPosition(value);
        }

        private float _normalizedPosition;

        public float Position => m_Position[MainAxis];

        /// <summary>
        /// Can be X or Y.
        /// </summary>
        protected int MainAxis;

        /// <summary>
        /// If the MainAxis is X then the SecondAxis will be Y and vice versa.
        /// </summary>
        protected int SecondAxis;

        /// <summary>
        /// The size of all cells and the space between them along with the padding on the main axis.
        /// </summary>
        protected float ContentVirtualSize;

        protected float MaxScrollPosition;

        /// <summary>
        /// Offset calculated on layout group update.
        /// </summary>
        protected Vector2 CachedPadding;

        /// <summary>
        /// Offset calculated on layout group update.
        /// </summary>
        protected float CachedAlign;

        /// <summary>
        /// Cell size / content size.
        /// </summary>
        protected float CellRatio;

        private bool _isDragging;

        protected Vector2 m_Position;
        private Vector2 _previousPosition;
        private Vector2 _positionHelper;
        private Vector2 _pointerStartPosition = Vector2.zero;

        // Methods

        protected override void Awake()
        {
            base.Awake();

            RectTransform = GetComponent<RectTransform>();

            Assert.IsFalse(Content == null);
            LayoutGroup = Content.GetComponent<CollectionLayoutGroup>();

            AutoScroller = new AutoScroller(this);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            RebuildLayout();
        }
#endif

        public abstract void Initialize();

        public void RebuildLayout()
        {
            if (!IsInitialized)
                return;

            UpdateProperties();
            UpdateContentVirtualSize();
            UpdatePool();
            UpdatePosition();
        }

        protected abstract void UpdatePool();
        protected abstract void UpdateProperties();
        protected abstract void UpdateContentVirtualSize();
        protected abstract void UpdatePosition();
        protected abstract int CalculateNumberOfCells();
        protected abstract int CalculateNumberOfCellsForAxis(ScrollDirection direction);

        private void SetNormalizedPosition(float value)
        {
            // Don't allow to calculate in editor mode.
            if (!Application.isPlaying)
                return;

            if (!IsInitialized)
            {
                Debug.Log($"The collection has not been initialized yet.");
                return;
            }

            if (ContentVirtualSize == 0)
            {
                _normalizedPosition = 0;
                m_Position[MainAxis] = 0;
                return;
            }

            _normalizedPosition = value;
            m_Position[MainAxis] = ContentVirtualSize * _normalizedPosition;

            UpdatePosition();
        }

        protected void CalculateNormalizedPosition()
        {
            // Don't allow to calculate in editor mode.
            if (!Application.isPlaying)
            {
                _normalizedPosition = 0;
                return;
            }

            if (!IsInitialized)
            {
                Debug.Log($"The collection has not been initialized yet.");
                return;
            }

            if (ContentVirtualSize < Content.RectTransform.rect.size[MainAxis])
            {
                _normalizedPosition = m_Position[MainAxis] < 0 ? 0 : 1;
                return;
            }

            _normalizedPosition = m_Position[MainAxis] / MaxScrollPosition;
        }

        protected float CalculateOffset(float position)
        {
            if (position < 0)
                return -position;
            

            if (Content.RectTransform.rect.size[MainAxis] > ContentVirtualSize)
                return -position;
            
            if (position > MaxScrollPosition)
                return MaxScrollPosition - position;
            
            return 0;
        }

        protected float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!IsInitialized)
                return;

            Velocity = 0;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInitialized)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            _isDragging = true;
            _positionHelper = m_Position;

            _pointerStartPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Content.RectTransform, eventData.position, eventData.pressEventCamera, out _pointerStartPosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsInitialized)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            if (AutoScroller.Active)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Content.RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pointerPosition);

            Vector2 pointerDelta = pointerPosition - _pointerStartPosition;
            if (Direction == ScrollDirection.Horizontal)
                pointerDelta = -pointerDelta;

            m_Position = _positionHelper + pointerDelta;
            float offset = CalculateOffset(m_Position[MainAxis]);
            m_Position[MainAxis] += offset;

            UpdatePosition();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsInitialized)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _isDragging = false;
            _positionHelper = m_Position;
        }

        protected virtual void LateUpdate()
        {
            if (!IsInitialized)
                return;

            if (AutoScroller.Active)
            {
                float offset = CalculateOffset(m_Position[MainAxis]);
                if (offset != 0)
                    AutoScroller.Release();
                else
                    AutoScroller.Update();

                m_Position[MainAxis] = Mathf.Clamp(m_Position[MainAxis], 0, MaxScrollPosition);
                UpdatePosition();

                _previousPosition = m_Position;
                return;
            }

            {
                float deltaTime = Time.unscaledDeltaTime;
                float offset = CalculateOffset(m_Position[MainAxis]);

                if (!_isDragging && (offset != 0 || Velocity != 0))
                {
                    Vector2 position = m_Position;

                    // Else move content according to velocity with deceleration applied.
                    if (Inertia)
                    {
                        Velocity *= Mathf.Pow(DecelerationRate, deltaTime);
                        if (Mathf.Abs(Velocity) < 1)
                            Velocity = 0;

                        position[MainAxis] += Velocity * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                        Velocity = 0;

                    if (Velocity != 0)
                    {
                        offset = CalculateOffset(position[MainAxis]);
                        position[MainAxis] += offset;

                        m_Position = position;
                        UpdatePosition();
                    }
                }

                if (_isDragging && Inertia)
                {
                    float newVelocity = (m_Position[MainAxis] - _previousPosition[MainAxis]) / deltaTime;
                    Velocity = Mathf.Lerp(Velocity, newVelocity, deltaTime * 10);
                }

                _previousPosition = m_Position;
            }
        }
    }
}