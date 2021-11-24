using System;

namespace ElevatorModels
{
    public class Button
    {
        public int ButtonPress { get; set; }

        public ButtonAction ButtonPressAction;
        public string Action
        {
            get { return ButtonPressAction.ToString(); }
            set
            {
                switch (value)
                {
                    case "U":
                        ButtonPressAction = ButtonAction.Up;
                        break;
                    case "D":
                        ButtonPressAction = ButtonAction.Down;
                        break;
                    default:
                        ButtonPressAction = ButtonAction.Internal;
                        break;
                }
            }
        }

        public enum ButtonAction
        {
            Up, 
            Down,
            Internal
        }

        public int CompareTo(Button other)
        {
            if (this.ButtonPress == other.ButtonPress)
                return 0;
            else
                return -1;
        }
    }
}
