using UnityEngine;


namespace XFABManager
{
    internal static class GameObjectExtensions
    {

        public static bool IsDestroy(this GameObject gameObject)
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

    }

}