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
}