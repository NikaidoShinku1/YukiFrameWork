using UnityEngine;


namespace XFABManager
{
    internal static class GameObjectExtensions
    {

        internal static bool IsDestroy(this GameObject gameObject)
        {
            if (gameObject == null || gameObject.ToString() == "null")
                return true;

            return false; 
        }

        internal static bool IsEmpty(this Transform transform)
        {
            if (transform == null || transform.ToString() == "null")
                return true;

            return false;
        }

    }

}