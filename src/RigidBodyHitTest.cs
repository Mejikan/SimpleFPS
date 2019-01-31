using Godot;

public class RigidBodyHitTest : RigidBody, IBulletHittable
{
	public void BulletHit(float damage, Vector3 origin)
	{
		Vector3 direction = GlobalTransform.origin - origin;
		direction = direction.Normalized();

		ApplyImpulse(origin, direction * damage);
	}
}