using FishNet.Connection;
using FishNet.Object;
using Game.InteractionSystem;
using Game.UIWindowSystem;
using SteelBox;

namespace Game.InventorySystem
{
    public class InventoryContainerInteraction : NetworkBehaviour, IInteraction
    {
        public string InteractionText => "Container";
        public void Interact(int nobId, bool asServer)
        {
            if (asServer)
            {
                if (!nobId.TryGetNetworkObjectFromObjectId(out NetworkObject networkObject)) return;
                C_TRpcShowContainer(networkObject.Owner);
                InventoryHandlerData.S_AddListener(ObjectId, networkObject.Owner);
            }
        }

        [TargetRpc]
        private void C_TRpcShowContainer(NetworkConnection _)
        {
            UIWindowHandlerData.ShowWindow(UIWindowType.Container, ObjectId);
        }
    }
}