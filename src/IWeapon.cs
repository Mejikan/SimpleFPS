using Godot;

public interface IWeapon
{
	ushort AmmoInWeapon { get; }
	ushort SpareAmmo { get; }
	bool CAN_RELOAD { get; }
	bool CAN_REFILL { get; }
	AnimationPlayerManager.AnimationName IDLE_ANIM { get; }
	AnimationPlayerManager.AnimationName FIRE_ANIM { get; }
	AnimationPlayerManager.AnimationName RELOAD_ANIM { get; }
	bool IsWeaponEnabled { get; }
	Player Player { get; set; }
	void LookAt(Vector3 target, Vector3 up);
	void RotateObjectLocal(Vector3 axis, float angle);
	void FireWeapon();
	bool EquipWeapon();
	bool UnequipWeapon();
	bool ReloadWeapon();
}