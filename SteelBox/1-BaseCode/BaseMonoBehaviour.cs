using UnityEngine;

namespace SteelBox
{
    public abstract class BaseMonoBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            RegisterEvents();
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        protected abstract void RegisterEvents();

        protected abstract void UnregisterEvents();
    }
}