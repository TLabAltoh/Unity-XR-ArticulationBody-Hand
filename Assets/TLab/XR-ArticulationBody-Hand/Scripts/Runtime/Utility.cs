using UnityEngine;

namespace TLab.XR.ArticulationBodyHand
{
    public static class Utility
    {
        public static T RequireComponent<T>(GameObject self) where T : Component
        {
            var result = self.GetComponent<T>();

            if (result == null)
                result = self.AddComponent<T>();

            return result;
        }

        public static T RequireComponent<T>(Transform self) where T : Component
        {
            return RequireComponent<T>(self.gameObject);
        }
    }
}
