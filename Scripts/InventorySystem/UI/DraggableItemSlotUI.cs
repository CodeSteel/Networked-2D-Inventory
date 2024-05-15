using Game.UIWindowSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.InventorySystem
{
    public class DraggableItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TextMeshProUGUI ItemAmountText;
        public Image ItemIconImage;
        
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public Transform TargetParent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            // not the best but it works
            Transform targetTransform = transform.parent;
            UIWindowHandlerData.GetTopCanvas(ref targetTransform);

            TargetParent = transform.parent;
            transform.SetParent(targetTransform);
            transform.SetAsLastSibling();
            _canvasGroup.alpha = 0.8f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.SetParent(TargetParent);
            transform.localPosition = Vector3.zero;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}