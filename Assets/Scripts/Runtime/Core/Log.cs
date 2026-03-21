
using UnityEngine;

namespace CustomLogger
{
    public static class Log
    {
        private static ILogger _logger;

        public static void SetLogger(ILogger logger)
        {
            if (logger == null) return;
            _logger = logger;
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