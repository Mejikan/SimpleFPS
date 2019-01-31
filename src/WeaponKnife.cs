using Godot;
using static AnimationPlayerManager;

public class WeaponKnife : Spatial, IWeapon
{
	private const int DAMAGE = 40;
	private ushort ammoInWeapon = 1;
	private ushort spareAmmo = 1;
	private const ushort AMMO_PER_MAG = 1;

	private const bool _CAN_RELOAD = false;
	private const bool _CAN_REFILL = false;

	private const AnimationName _IDLE_ANIM = AnimationName.Knife_idle;
	private const AnimationName _FIRE_ANIM = AnimationName.Knife_fire;
	private const AnimationName _RELOAD_ANIM = AnimationName.Idle_unarmed;

	private bool isWeaponEnabled = false;

	private Player player;

	public ushort AmmoInWeapon
	{
		get => this.ammoInWeapon;
	}
	public ushort SpareAmmo
	{
		get => this.spareAmmo;
	}

	public bool CAN_RELOAD
	{
		get => _CAN_RELOAD;
	}
	public bool CAN_REFILL
	{
		get => _CAN_REFILL;
	}

	public AnimationName IDLE_ANIM
	{
		get => _IDLE_ANIM;
	}
	public AnimationName FIRE_ANIM
	{
		get => _FIRE_ANIM;
	}
	public AnimationName RELOAD_ANIM
	{
		get => _RELOAD_ANIM;
	}

	public bool IsWeaponEnabled
	{
		get => this.isWeaponEnabled;
	}

	public Player Player
	{
		get => this.player;
		set { this.player = value; }
	}

	public void FireWeapon()
	{
		Area area = GetNode("Area") as Area;
		Godot.Array bodies = area.GetOverlappingBodies();
		
		foreach (object body in bodies)
		{
			Player player = body as Player;
			if (player != this.player && this.player != null)
			{ // prevent player from stabbing themselves
				IBulletHittable hittable = body as IBulletHittable;
				if (hittable != null)
				{
					hittable.BulletHit(DAMAGE, area.GlobalTransform.origin);
				}
			}
		}
	}

	public bool EquipWeapon()
	{
		if (this.player.AnimationManager.CurrentState == IDLE_ANIM)
		{
			this.isWeaponEnabled = true;
			return true;
		}

		if (this.player.AnimationManager.CurrentState == AnimationName.Idle_unarmed)
		{
			this.player.AnimationManager.SetAnimation(AnimationName.Knife_equip);
		}

		return false;
	}

	public bool UnequipWeapon()
	{
		if (this.player.AnimationManager.CurrentState == IDLE_ANIM)
		{
			this.player.AnimationManager.SetAnimation(AnimationName.Knife_unequip);
		}

		if (this.player.AnimationManager.CurrentState == AnimationName.Idle_unarmed)
		{
			this.isWeaponEnabled = false;
			return true;
		}

		return false;
	}

	public bool ReloadWeapon()
	{
		return false;
	}
}
