using UnityEngine;


namespace XFABManager
{
    internal static class GameObjectExtensions
    {

        internal static bool IsDestroy(this GameObject gameObject)
        {
            try
            {
                if (gameObject == null || gameObject.transform == null)
                    return true;
            }
            catch (System.Exception)
            {
                return true;
            }
            return false;
        }

        internal static bool IsEmpty(this Transform transform)
        {
            try
            {
                if (transform == null || transform.gameObject == null)
                    return true;
            }
            catch (System.Exception)
            {
                return true;
            }
            return false;
        }

    }

}