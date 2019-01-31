using Godot;
using static AnimationPlayerManager;

public class WeaponPistol : Spatial, IWeapon
{
	private const int DAMAGE = 15;
	private ushort ammoInWeapon = 10;
	private ushort spareAmmo = 20;
	private const ushort AMMO_PER_MAG = 10;

	private const bool _CAN_RELOAD = true;
	private const bool _CAN_REFILL = true;

	private const AnimationName _IDLE_ANIM = AnimationName.Pistol_idle;
	private const AnimationName _FIRE_ANIM = AnimationName.Pistol_fire;
	private const AnimationName _RELOAD_ANIM = AnimationName.Pistol_reload;

	private bool isWeaponEnabled = false;

	private PackedScene bulletScene;

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

	public override void _Ready()
	{
		this.bulletScene = ResourceLoader.Load("src/Bullet_Scene.tscn") as PackedScene;
	}

	public void FireWeapon()
	{
		Bullet bullet = this.bulletScene.Instance() as Bullet;
		// ASSUMING child of root is the root node our player is under!
		Node root = (GetTree().Root.GetChildren())[0] as Node;
		root.AddChild(bullet);

		bullet.GlobalTransform = this.GlobalTransform;
		bullet.Scale = new Vector3(4, 4, 4); // make bullet larger
		bullet.BULLET_DAMAGE = DAMAGE;

		// ammo count checking is performed by player script not weapon script
		this.ammoInWeapon -= 1;

		this.player.CreateSound(SimpleAudioPlayer.SoundName.PISTOL_SHOT, this.GlobalTransform.origin);
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
			this.player.AnimationManager.SetAnimation(AnimationName.Pistol_equip);
		}

		return false;
	}

	public bool UnequipWeapon()
	{
		if (this.player.AnimationManager.CurrentState == IDLE_ANIM)
		{
			if (this.player.AnimationManager.CurrentState != AnimationName.Pistol_unequip)
			{
				this.player.AnimationManager.SetAnimation(AnimationName.Pistol_unequip);
			}
		}

		if (this.player.AnimationManager.CurrentState == AnimationName.Idle_unarmed)
		{
			this.isWeaponEnabled = false;
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool ReloadWeapon()
	{
		bool canReload = false;

		if (this.player.AnimationManager.CurrentState == IDLE_ANIM)
		{
			canReload = true;
		}

		if (this.spareAmmo <= 0 || this.ammoInWeapon == AMMO_PER_MAG)
		{
			canReload = false;
		}

		if (canReload)
		{
			ushort ammoNeeded = (ushort)(AMMO_PER_MAG - this.ammoInWeapon);

			if (this.spareAmmo >= ammoNeeded)
			{
				this.spareAmmo -= ammoNeeded;
				this.ammoInWeapon = AMMO_PER_MAG;
			}
			else
			{
				this.ammoInWeapon += this.spareAmmo;
				this.spareAmmo = 0;
			}

			this.player.AnimationManager.SetAnimation(RELOAD_ANIM);
			this.player.CreateSound(SimpleAudioPlayer.SoundName.RELOAD, this.player.Camera.GlobalTransform.origin);

			return true;
		}

		return false;
	}
}
