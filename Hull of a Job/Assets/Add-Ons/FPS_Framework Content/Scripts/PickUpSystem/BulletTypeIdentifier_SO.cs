using UnityEngine;

namespace cowsins
{
    [CreateAssetMenu(fileName = "newBulletsInventoryItem", menuName = "COWSINS/FPS ENGINE/New Bullet Identifier", order = 1)]
    public class BulletTypeIdentifier_SO : Item_SO
    {
        public enum BulletType { Custom, Universal }

        public BulletType bulletType;

        public static bool IsCompatibleWith(BulletTypeIdentifier_SO a, BulletTypeIdentifier_SO b)
        {
            bool IsUniversal(BulletTypeIdentifier_SO id) =>
                id == null || id.bulletType == BulletType.Universal;

            // If both are universal -> valid.
            if (IsUniversal(a) && IsUniversal(b))
                return true;

            // If only one is universal -> Not valid.
            if (IsUniversal(a) || IsUniversal(b))
                return false;

            // Both are non-universal -> They must match exactly.
            return a == b;
        }
    }
}
