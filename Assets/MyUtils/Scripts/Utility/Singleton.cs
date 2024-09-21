using UnityEngine;


namespace Utilities
{
    public class Singleton<T>:MonoBehaviour where T : MonoBehaviour
    {
        static T Instance;
        public static T instance
        {
            get
            {
                if(Instance == null)
                    Instance=GameObject.FindObjectOfType<T>();
                return Instance;
            }
            protected set
            {
                Instance= value;
            }
        }
        
    }
}
