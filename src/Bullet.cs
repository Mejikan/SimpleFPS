using Godot;
using System;

public class Bullet : Spatial
{
	/**
	* Why are we using a Area and not a RigidBody? The mean reason we’re not using a RigidBody is because 
	* we do not want the bullet to interact with other RigidBody nodes. By using an Area we are assuring 
	* that none of the other RigidBody nodes, including other bullets, will be effected.
	*
	* Another reason is simply because it is easier to detect collisions with a Area!
	*/

	public int BULLET_SPEED = 70;
	public int BULLET_DAMAGE = 15;

	private const int KILL_TIMER = 4;

	private float timer = 0;

	/// <summary>
	/// we need to track whether we’ve hit something or not is because queue_free does not immediately free the node
	/// </summary>
	private bool hitSomething = false;

	public override void _Ready()
	{
		this.GetNode("Area").Connect("body_entered", this, nameof(this.Collided));
	}

	public override void _PhysicsProcess(float delta)
	{
		Vector3 forwardDirection = GlobalTransform.basis.z.Normalized();
		GlobalTranslate(forwardDirection * BULLET_SPEED * delta);

		this.timer += delta;
		if (this.timer >= KILL_TIMER)
		{
			QueueFree();
		}
	}

	private void Collided(PhysicsBody body)
	{
		if (!this.hitSomething)
		{
			IBulletHittable hittable = body as IBulletHittable;
			if (hittable != null)
			{
				hittable.BulletHit(BULLET_DAMAGE, GlobalTransform.origin);
			}
		}

		this.hitSomething = true;
		QueueFree();
	}
}
