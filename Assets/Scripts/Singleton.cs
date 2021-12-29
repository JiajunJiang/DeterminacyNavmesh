using UnityEngine;

namespace DefaultNamespace
{
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T Reset()
        {
            _instance = new T();
            return _instance;
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();
                return _instance;
            }
        }
    }
    
    /// <summary>
    /// 继承于MonoBehaviour的单例模式
    /// </summary>
    public class SingletonBehaviour<T> : MonoBehaviour where T : UnityEngine.MonoBehaviour
    {
        protected static T instance;

        public static T Reset()
        {
            instance = (T) FindObjectOfType(typeof(T));
            return instance;
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = (T) FindObjectOfType(typeof(T));

                return instance;
            }
        }
    }
}