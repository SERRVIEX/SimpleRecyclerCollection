namespace SimpleRecyclerCollection.Core
{
    using System;

    using UnityEngine;

    public class AutoScroller
    {
        private Collection _collection;

        public bool Active { get; private set; }

        private Action<float> _onUpdate;

        private float _time = 0;
        public float _duration;

        // Constructors

        public AutoScroller(Collection collection) => _collection = collection;
        
        // Methods

        public void Scroll(float duration, Action<float> onUpdate)
        {
            Active = true;
            _onUpdate = onUpdate;

            if (!_collection.gameObject.activeInHierarchy && duration <= 0)
            {
                Complete();
                return;
            }

            _time = 0;
            _duration = duration;
        }

        public void Update()
        {
            if(Active)
            {
                _time += Time.unscaledDeltaTime;

                if (_time > _duration)
                    Complete();
                else
                    _onUpdate?.Invoke(_time / _duration);
            }
        }

        public void Complete()
        {
            Active = false;
            _onUpdate?.Invoke(1);
            _onUpdate = null;
        }

        public void Release() => Active = false;
    }
}