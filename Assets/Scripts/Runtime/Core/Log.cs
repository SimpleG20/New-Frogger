
using UnityEngine;

namespace CustomLogger
{
    public static class Log
    {
        private static ILogger _logger;

        [RuntimeInitializeOnLoadMethod]
        private static void SetLogger()
        {
            _logger = new EditorLogger();
        }

        public static void log(object message)
        {
            _logger?.log(message);
        }
    }

    public interface ILogger
    {
        void log(object message);
    }

    public class EditorLogger : ILogger
    {
        public void log(object message)
        {
            #if UNITY_EDITOR
            Debug.Log(message);
            #endif
        }
    }
}