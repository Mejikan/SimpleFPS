using Godot;
using System;

public class Player : KinematicBody
{
	private enum WeaponName : ushort
	{
		NONE,
		KNIFE,
		PISTOL,
		RIFLE,
	}

	private const float GRAVITY = -24.8f;
	private const int MAX_SPEED = 20;
	private const int JUMP_SPEED = 18;
	private const float ACCEL = 4.5f;
	private const int DEACCEL = 16;
	private const int MAX_SPRINT_SPEED = 30;
	private const int SPRINT_ACCEL = 18;
	private bool isSprinting = false;

	private Vector3 velocity = new Vector3();
	private Vector3 direction = new Vector3();

	private const int MAX_SLOPE_ANGLE = 40;

	private Camera camera;
	private Spatial rotationHelper;
	private SpotLight flashlight;

	private float MOUSE_SENSITIVITY = 0.05f;

	private AnimationPlayerManager animationManager;

	private float health = 100;

	// Weapon Stuff
	private WeaponName currentWeaponName = WeaponName.NONE;
	private IWeapon[] weapons;
	private bool changingWeapon = false;
	private WeaponName changingWeaponTarget = WeaponName.NONE;
	private bool reloadingWeapon = false;

	private Label uIStatusLabel;

	private PackedScene simpleAudioPlayerScene;

	private IWeapon currentWeapon
	{
		get => this.weapons[(ushort)this.currentWeaponName];
	}

	public AnimationPlayerManager AnimationManager
	{
		get => this.animationManager;
	}

	public Camera Camera
	{
		get => this.camera;
	}

	public override void _Ready()
	{
		this.rotationHelper = this.GetNode("Rotation_Helper") as Spatial;
		this.camera = this.rotationHelper.GetNode("Camera") as Camera;

		this.animationManager = this.rotationHelper.GetNode("Model/Animation_Player") as AnimationPlayerManager;
		this.animationManager.OnAnimationEnded = this.FireBullet;

		Input.SetMouseMode(Input.MouseMode.Captured);

		this.weapons = new IWeapon[Enum.GetNames(typeof(WeaponName)).Length];
		Spatial gunFirePoints = this.rotationHelper.GetNode("Gun_Fire_Points") as Spatial;
		this.weapons[(ushort)WeaponName.KNIFE] = gunFirePoints.GetNode("Knife_Point") as IWeapon;
		this.weapons[(ushort)WeaponName.PISTOL] = gunFirePoints.GetNode("Pistol_Point") as IWeapon;
		this.weapons[(ushort)WeaponName.RIFLE] = gunFirePoints.GetNode("Rifle_Point") as IWeapon;

		Vector3 gunAimPointPosition = (this.rotationHelper.GetNode("Gun_Aim_Point") as Spatial).GlobalTransform.origin;

		foreach (IWeapon weapon in this.weapons)
		{
			if (weapon != null)
			{
				weapon.Player = this;
				weapon.LookAt(gunAimPointPosition, new Vector3(0, 1, 0));
				weapon.RotateObjectLocal(new Vector3(0, 1, 0), Mathf.Deg2Rad(180));
			}
		}

		this.uIStatusLabel = this.GetNode("HUD/Panel/Gun_label") as Label;
		this.flashlight = this.rotationHelper.GetNode("Flashlight") as SpotLight;

		this.simpleAudioPlayerScene = ResourceLoader.Load("src/Simple_Audio_Player.tscn") as PackedScene;
	}

	public override void _PhysicsProcess(float delta)
	{
		this.ProcessInput(delta);
		this.ProcessMovement(delta);
		this.ProcessChangingWeapons(delta);
		this.ProcessReloading(delta);
		this.ProcessUI(delta);
	}

	public override void _Input(InputEvent evt)
	{
		if (evt is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
		{
			InputEventMouseMotion evtMouseMotion = evt as InputEventMouseMotion;
			this.rotationHelper.RotateX(Mathf.Deg2Rad(evtMouseMotion.Relative.y * MOUSE_SENSITIVITY));
			this.RotateY(Mathf.Deg2Rad(evtMouseMotion.Relative.x * MOUSE_SENSITIVITY * -1)); // -1 rotates in same direction

			Vector3 cameraRotation = this.rotationHelper.RotationDegrees;
			cameraRotation.x = Mathf.Clamp(cameraRotation.x, -70, 70); // prevent rotating player upside down
			this.rotationHelper.RotationDegrees = cameraRotation;
		}
	}

	public void CreateSound(SimpleAudioPlayer.SoundName name, Vector3 position)
	{
		SimpleAudioPlayer audioPlayer = this.simpleAudioPlayerScene.Instance() as SimpleAudioPlayer;
		// ASSUMING child of root is the root node our player is under!
		Node root = (GetTree().Root.GetChildren())[0] as Node;
		root.AddChild(audioPlayer);

		audioPlayer.PlaySound(name, position);
	}

	public void FireBullet()
	{
		if (this.changingWeapon)
		{
			return;
		}

		this.currentWeapon.FireWeapon();
	}

	private void ProcessInput(float delta)
	{
		// Walking
		this.direction = new Vector3();

		Transform camXForm = this.camera.GetGlobalTransform();

		Vector2 inputMovementVector = new Vector2();

		if (Input.IsActionPressed("movement_forward"))
		{
			inputMovementVector.y += 1;
		}
		if (Input.IsActionPressed("movement_backward"))
		{
			inputMovementVector.y -= 1;
		}
		if (Input.IsActionPressed("movement_left"))
		{
			inputMovementVector.x -= 1;
		}
		if (Input.IsActionPressed("movement_right"))
		{
			inputMovementVector.x += 1;
		}

		inputMovementVector = inputMovementVector.Normalized();

		// Move direction relative to the camera
		this.direction += (-1) * camXForm.basis.z.Normalized() * inputMovementVector.y;
		this.direction += camXForm.basis.x.Normalized() * inputMovementVector.x;

		// Jumping
		if (IsOnFloor())
		{
			if (Input.IsActionJustPressed("movement_jump"))
			{
				this.velocity.y = JUMP_SPEED;
			}
		}

		// Sprinting
		if (Input.IsActionPressed("movement_sprint"))
		{
			this.isSprinting = true;
		}
		else
		{
			this.isSprinting = false;
		}

		// Flashlight
		if (Input.IsActionJustPressed("flashlight"))
		{
			if (this.flashlight.IsVisibleInTree())
			{
				this.flashlight.Hide();
			}
			else
			{
				this.flashlight.Show();
			}
		}

		// Capturing/Freeing the cursor
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (Input.GetMouseMode() == Input.MouseMode.Visible)
			{
				Input.SetMouseMode(Input.MouseMode.Captured);
			}
			else
			{
				Input.SetMouseMode(Input.MouseMode.Visible);
			}
		}

		// Changing weapons
		ushort weaponEquipTarget = (ushort)this.currentWeaponName;

		if (Input.IsKeyPressed((int)KeyList.Key1))
		{
			weaponEquipTarget = 0;
		}
		if (Input.IsKeyPressed((int)KeyList.Key2))
		{
			weaponEquipTarget = 1;
		}
		if (Input.IsKeyPressed((int)KeyList.Key3))
		{
			weaponEquipTarget = 2;
		}
		if (Input.IsKeyPressed((int)KeyList.Key4))
		{
			weaponEquipTarget = 3;
		}

		if (Input.IsActionJustPressed("shift_weapon_positive"))
		{
			weaponEquipTarget += 1;
		}
		if (Input.IsActionJustPressed("shift_weapon_negative"))
		{
			weaponEquipTarget -= 1;
		}

		weaponEquipTarget = (ushort)Mathf.Clamp(weaponEquipTarget, 0, this.weapons.Length - 1);

		if (!this.changingWeapon)
		{
			if (!this.reloadingWeapon)
			{
				if (weaponEquipTarget != (ushort)this.currentWeaponName)
				{
					this.changingWeaponTarget = (WeaponName)weaponEquipTarget;
					this.changingWeapon = true;
				}
			}
		}

		// Firing weapons
		if (Input.IsActionPressed("fire"))
		{
			if (!this.reloadingWeapon)
			{
				if (!this.changingWeapon)
				{
					if (this.currentWeapon != null)
					{
						if (this.currentWeapon.AmmoInWeapon > 0)
						{
							if (this.animationManager.CurrentState == currentWeapon.IDLE_ANIM)
							{
								this.animationManager.SetAnimation(currentWeapon.FIRE_ANIM);
							}
						}
						else
						{
							this.reloadingWeapon = true;
						}
					}
				}
			}
		}

		// Reloading weapons
		if (!this.reloadingWeapon)
		{
			if (!this.changingWeapon)
			{
				if (Input.IsActionJustPressed("reload"))
				{
					if (this.currentWeapon != null)
					{
						if (this.currentWeapon.CAN_RELOAD)
						{
							bool isReloading = false;
							foreach (IWeapon weapon in this.weapons)
							{ // TODO: why iterate over every weapon and not just check current?
								if (weapon != null)
								{
									if (this.animationManager.CurrentState == weapon.RELOAD_ANIM)
									{
										isReloading = true;
									}
								}
							}
							if (!isReloading)
							{
								this.reloadingWeapon = true;
							}
						}
					}
				}
			}
		}
	}

	private void ProcessMovement(float delta)
	{
		this.direction.y = 0;
		this.direction = this.direction.Normalized();

		this.velocity.y += delta * GRAVITY;

		Vector3 hvel = this.velocity;
		hvel.y = 0; // null any movement on the y-axis

		Vector3 target = this.direction;
		if (this.isSprinting)
		{
			target *= MAX_SPRINT_SPEED;
		}
		else
		{
			target *= MAX_SPEED;
		}

		float acceleration;
		if (this.direction.Dot(hvel) > 0)
		{
			if (this.isSprinting)
			{
				acceleration = SPRINT_ACCEL;
			}
			else
			{
				acceleration = ACCEL;
			}
		}
		else
		{
			acceleration = DEACCEL;
		}
		hvel = hvel.LinearInterpolate(target, acceleration * delta);
		this.velocity.x = hvel.x;
		this.velocity.z = hvel.z;
		this.velocity = MoveAndSlide(this.velocity, new Vector3(0, 1, 0), 0.05f, 4, Mathf.Deg2Rad(MAX_SLOPE_ANGLE));
	}

	private void ProcessChangingWeapons(float delta)
	{
		if (this.changingWeapon)
		{
			bool weaponUnequipped = false;

			if (this.currentWeapon == null)
			{
				weaponUnequipped = true;
			}
			else
			{
				if (this.currentWeapon.IsWeaponEnabled)
				{
					weaponUnequipped = this.currentWeapon.UnequipWeapon();
				}
				else
				{
					weaponUnequipped = true;
				}
			}

			if (weaponUnequipped)
			{
				bool weaponEquipped = false;
				IWeapon weaponToEquip = this.weapons[(ushort)this.changingWeaponTarget];

				if (weaponToEquip == null)
				{
					weaponEquipped = true;
				}
				else
				{
					if (!weaponToEquip.IsWeaponEnabled)
					{
						weaponEquipped = weaponToEquip.EquipWeapon();
					}
					else
					{
						weaponEquipped = true;
					}
				}

				if (weaponEquipped)
				{
					this.changingWeapon = false;
					this.currentWeaponName = this.changingWeaponTarget;
					this.changingWeaponTarget = WeaponName.NONE;
				}
			}
		}
	}

	private void ProcessReloading(float delta)
	{
		if (this.reloadingWeapon)
		{
			if (this.currentWeapon != null)
			{
				this.currentWeapon.ReloadWeapon();
			}
			this.reloadingWeapon = false;
		}
	}

	private void ProcessUI(float delta)
	{
		if (this.currentWeaponName == WeaponName.NONE || this.currentWeaponName == WeaponName.KNIFE)
		{
			this.uIStatusLabel.Text = String.Format("HEALTH: {0}", this.health);
		}
		else
		{
			this.uIStatusLabel.Text = String.Format("HEALTH: {0}\nAMMO: {1}/{2}", 
				this.health, this.currentWeapon.AmmoInWeapon, this.currentWeapon.SpareAmmo);
		}
	}
}
