using UnityEngine;

public interface IProjectile
{
    bool AllowBounce { get; }
    void Launch(Vector2 position, Vector2 direction, float force, bool allowBounce);
    float FireCooldown { get; }
    bool IsSpecial { get; }

}