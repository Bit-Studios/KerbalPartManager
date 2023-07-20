using UnityEngine;
using UnityEngine.UIElements;
using Logger = ShadowUtilityLIB.logging.Logger;

namespace BetterPartsManager.UI
{
    public class DragManipulator : IManipulator
    {
        private Logger logger = new Logger(BetterPartsManagerMod.ModId,"");
        public VisualElement _target;
        public Vector3 offset;
        public PickingMode Mode;
        public bool Dragging = false;
        public Vector2[] deadzone = new Vector2[0];
        public VisualElement target
        {
            get => _target;
            set
            {
                _target = value;

                _target.RegisterCallback<PointerDownEvent>(DragBegin);
                _target.RegisterCallback<PointerUpEvent>(DragEnd);
                _target.RegisterCallback<PointerMoveEvent>(PointerMove);
            }
        }

        public void DragBegin(PointerDownEvent evt)
        {
            logger.Debug($"x1 {deadzone[0].x} y1 {deadzone[0].y} x2 {deadzone[1].x} y2 {deadzone[1].y}");
            if (evt.localPosition.x >= deadzone[0].x && evt.localPosition.x <= deadzone[1].x && evt.localPosition.y >= deadzone[0].y && evt.localPosition.y <= deadzone[1].y)
            {
                logger.Debug($"drag is in deadzone");

            }
            else
            {
                Mode = target.pickingMode;
                target.pickingMode = PickingMode.Ignore;
                offset = evt.localPosition;
                Dragging = true;
                target.CapturePointer(evt.pointerId);
            }

        }

        public void DragEnd(IPointerEvent evt)
        {
            target.ReleasePointer(evt.pointerId);
            Dragging = false;
            target.pickingMode = Mode;
        }
        public void PointerMove(PointerMoveEvent evt)
        {
            if (Dragging)
            {
                Vector3 delta = evt.localPosition - (Vector3)offset;
                target.transform.position += delta;
            }
        }
    }
}
