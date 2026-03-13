using Modules.PopupSystem.Components;

namespace Modules.PopupSystem.Events
{
    public struct PopupClosedEvent
    {
        public BasePopup Popup { get; }
        public PopupClosedEvent(BasePopup popup)
        {
            Popup = popup;
        }
    }

    public struct PopupShowedEvent
    {
        public BasePopup Popup { get; }
        public PopupShowedEvent(BasePopup popup)
        {
            Popup = popup;
        }
    }
}