using UnityEngine;
using TigerForge;

namespace SpatialNotes
{
    public class Cursor_Main : MonoBehaviour
    {
        public Texture2D cursorNormal;
        public Texture2D cursorQuestionMark;
        public Texture2D cursorDrag;
        public Texture2D cursorExit;
        

        void Start()
        {
            SetCursor(cursorNormal);
            EventManager.StartListening("CURSOR_REFRESH", ListenToCursorEvent);
        }

        

        void ListenToCursorEvent()
        {
            string cursorName = EventManager.GetString("CURSOR_NAME");
            Texture2D cursor = cursorNormal;

            switch(cursorName)
            {
                case "NORMAL":
                    cursor = cursorNormal;
                    break;
                case "QUESTION":
                    cursor = cursorQuestionMark;
                    break;
                case "DRAG":
                    cursor = cursorDrag;
                    break;
                case "EXIT":
                    cursor = cursorExit;
                    break;
            }

            if (cursor != null && cursorName != "CIRCLE")
                SetCursor(cursor);
        }

        void SetCursor(Texture2D cursor)
        {
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        }

        void OnDestroy()
        {
            // Reset cursor to default when script is disabled or GameObject is destroyed
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
