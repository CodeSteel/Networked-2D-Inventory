using FishNet.Object;

namespace SteelBox
{
    public abstract class BaseNetworkBehaviour : NetworkBehaviour
    {
        private void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            RegisterEvents();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            RegisterEvents();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            UnregisterEvents();
        }
        
        public override void OnStopClient()
        {
            base.OnStopClient();
            UnregisterEvents();
        }

        protected abstract void RegisterEvents();
        protected abstract void UnregisterEvents();
    }
}