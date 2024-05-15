using UnityEditor;
using UnityEngine;

namespace Game.InventorySystem
{
    public abstract class BaseItemSo : ScriptableObject
    {
        public string Id;
        public int StackLimit = 1;

        public string Name;
        public Sprite Sprite;
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            Id = this.name;
            EditorUtility.SetDirty(this);
#endif
        }
    }
}