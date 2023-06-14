using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        private static readonly object Lock = new();

        private static bool applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting) return null;

                lock (Lock)
                {
                    if (instance == null)
                    {
                        instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong - there should never be more than 1 singleton!");
                            return instance;
                        }

                        if (instance == null)
                        {
                            Debug.LogError($"No instance of {typeof(T)} found!");
                            return null;
                        }
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        ///     When Unity quits, it destroys objects in a random order.
        ///     In principle, a Singleton is only destroyed when application quits.
        ///     If any script calls Instance after it have been destroyed,
        ///     it will create a buggy ghost object that will stay on the Editor scene
        ///     even after stopping playing the Application. Really bad!
        ///     So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
    }
}