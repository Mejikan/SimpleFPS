using Godot;

public interface IBulletHittable
{
	void BulletHit(float damage, Vector3 origin);
}