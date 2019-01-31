using Godot;
using System;

public class AnimationPlayerManager : AnimationPlayer
{
	public enum AnimationName: ushort
	{
		Idle_unarmed,
		Pistol_equip,
		Pistol_fire,
		Pistol_idle,
		Pistol_reload,
		Pistol_unequip,
		Rifle_equip,
		Rifle_fire,
		Rifle_idle,
		Rifle_reload,
		Rifle_unequip,
		Knife_equip,
		Knife_fire,
		Knife_idle,
		Knife_unequip,
	}

	public Action OnAnimationEnded = null;

	private static AnimationContext[] contexts;

	private AnimationName currentState = AnimationName.Idle_unarmed;


	private AnimationContext CurrentStateContext
	{
		get => contexts[(ushort)this.currentState];
	}

	public AnimationName CurrentState
	{
		get => this.currentState;
	}

	public override void _Ready()
	{
		InitAnimationContexts();
		Connect("animation_finished", this, nameof(this.AnimationEnded));
	}

	public bool SetAnimation(AnimationName name)
	{
		if (name == this.currentState)
		{
			GD.Print("AnimationPlayerManager -- WARNING: animation is already " + name.ToString());
			return true;
		}

		if (HasAnimation(name.ToString()))
		{ // If requested animation is loaded into Godot
			if (System.Array.IndexOf(this.CurrentStateContext.States, (ushort)name) >= 0)
			{ // if the requested animation is possible from the current state
				this.currentState = name;
				Play(name.ToString(), -1, this.CurrentStateContext.Speed);
				return true;
			}
			else
			{
				GD.Print("AnimationPlayerManager -- WARNING: Cannot change to " + 
					name + " from " + this.CurrentStateContext.Name);
			}
		}

		return false;
	}

	private static void InitAnimationContexts()
	{
		contexts = new AnimationContext[Enum.GetNames(typeof(AnimationName)).Length];
		AnimationName name;

		// unarmed
		name = AnimationName.Idle_unarmed;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Knife_equip, (ushort)AnimationName.Pistol_equip, 
				(ushort)AnimationName.Rifle_equip, (ushort)AnimationName.Idle_unarmed },
			Speed = 1,
		};

		// Pistol
		name = AnimationName.Pistol_equip;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Pistol_idle },
			Next = (ushort)AnimationName.Pistol_idle,
			Speed = 1.4f,
		};

		name = AnimationName.Pistol_fire;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Pistol_idle },
			Next = (ushort)AnimationName.Pistol_idle,
			Speed = 1.8f,
		};

		name = AnimationName.Pistol_idle;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Pistol_fire, (ushort)AnimationName.Pistol_reload, 
				(ushort)AnimationName.Pistol_unequip, (ushort)AnimationName.Pistol_idle },
			Speed = 1,
		};

		name = AnimationName.Pistol_reload;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Pistol_idle },
			Next = (ushort)AnimationName.Pistol_idle,
			Speed = 1,
		};

		name = AnimationName.Pistol_unequip;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Idle_unarmed },
			Next = (ushort)AnimationName.Idle_unarmed,
			Speed = 1.4f,
		};

		// Rifle
		name = AnimationName.Rifle_equip;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Rifle_idle },
			Next = (ushort)AnimationName.Rifle_idle,
			Speed = 2,
		};

		name = AnimationName.Rifle_fire;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Rifle_idle },
			Next = (ushort)AnimationName.Rifle_idle,
			Speed = 6,
		};

		name = AnimationName.Rifle_idle;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Rifle_fire, (ushort)AnimationName.Rifle_reload, 
				(ushort)AnimationName.Rifle_unequip, (ushort)AnimationName.Rifle_idle },
			Speed = 1,
		};

		name = AnimationName.Rifle_reload;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Rifle_idle },
			Next = (ushort)AnimationName.Rifle_idle,
			Speed = 1.45f,
		};

		name = AnimationName.Rifle_unequip;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Idle_unarmed },
			Next = (ushort)AnimationName.Idle_unarmed,
			Speed = 2,
		};

		// Knife
		name = AnimationName.Knife_equip;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Knife_idle },
			Next = (ushort)AnimationName.Knife_idle,
			Speed = 1,
		};

		name = AnimationName.Knife_fire;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Knife_idle },
			Next = (ushort)AnimationName.Knife_idle,
			Speed = 1.35f,
		};

		name = AnimationName.Knife_idle;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Knife_fire, (ushort)AnimationName.Knife_unequip, 
				(ushort)AnimationName.Knife_idle },
			Speed = 1,
		};

		name = AnimationName.Knife_unequip;
		contexts[(ushort)name] = new AnimationContext((ushort)name, name.ToString())
		{
			States = new ushort[] { (ushort)AnimationName.Idle_unarmed },
			Next = (ushort)AnimationName.Idle_unarmed,
			Speed = 1,
		};
	}

	private void AnimationEnded(AnimationName name)
	{
		AnimationName nextState = (AnimationName)this.CurrentStateContext.Next;
		if (this.CurrentStateContext.Next != this.CurrentStateContext.Id)
		{
			this.SetAnimation(nextState);
		}
	}

	/// <summary>
	/// This function is called by the animation player
	/// </summary>
	private void AnimPlayerCb()
	{
		if (this.OnAnimationEnded == null)
		{
			GD.Print("AnimationPlayerManager -- WARNING: No callback function for the animation to call!");
		}
		else
		{
			this.OnAnimationEnded();
		}
	}
}
