using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tom.LevelEditor.Editor.Utility
{
    [System.Serializable]
    public class LevelEditorMessageSystem
    {
        private const string kPrefix = "\u27a4";

        public static LevelEditorMessageSystem instance = new LevelEditorMessageSystem();
        [SerializeField] private Message _currentMessage;

        private double _nextMessageClearTime = 0;

        public void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (_currentMessage.duration != 0)
            {
                bool lastRichTextState = EditorStyles.boldLabel.richText;

                EditorStyles.boldLabel.richText = true;
                string message = CreateStatusMessageString();

                GUILayout.Label($"{kPrefix} {message}", EditorStyles.boldLabel);
                EditorStyles.boldLabel.richText = lastRichTextState;
            }
            else
                GUILayout.Label(kPrefix, EditorStyles.boldLabel);
            GUILayout.EndVertical();
        }

        public void OnEnable()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        public void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        public static void Push(string message, float duration, MessageType messageType)
            => instance.PushMessage(message, duration, messageType);

        public static void Clear()
            => instance.ClearMessages();

        public void PushMessage(string message, float duration, MessageType messageType)
        {
            _currentMessage = new Message()
            {
                message = message,
                duration = duration,
                type = messageType
            };
            _nextMessageClearTime = EditorApplication.timeSinceStartup + duration;
        }

        public void ClearMessages()
        {
            _currentMessage = default;
        }

        private void Update()
        {
            if (_currentMessage.duration == 0) return;

            if (EditorApplication.timeSinceStartup > _nextMessageClearTime)
                ClearMessages();
        }

        private string CreateStatusMessageString()
        {
            switch (_currentMessage.type)
            {
                case MessageType.Warning:
                    return $"<color=#ffc800>{_currentMessage.message}</color>";
                case MessageType.Error:
                    return $"<color=#e02626>{_currentMessage.message}</color>";
                default:
                    return _currentMessage.message;
            }
        }

        public enum MessageType
        {
            Info,
            Warning,
            Error
        }

        [System.Serializable]
        public struct Message
        {
            public string message;
            public float duration;
            public MessageType type;
        }
    }
}