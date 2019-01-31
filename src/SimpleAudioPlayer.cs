using Godot;

public class SimpleAudioPlayer : Spatial
{
	public enum SoundName : ushort
	{
		NONE,
		PISTOL_SHOT,
		RIFLE_SHOT,
		RELOAD
	}

	private AudioStream pistolShotAudio;
	private AudioStream rifleShotAudio;
	private AudioStream gunReloadAudio;

	private AudioStreamPlayer audioPlayer;

	public override void _Ready()
	{
		this.pistolShotAudio = ResourceLoader.Load("assets/sound/Revolver_Shot.wav") as AudioStream;
		this.rifleShotAudio = ResourceLoader.Load("assets/sound/Sniper_Shot.wav") as AudioStream;
		this.gunReloadAudio = ResourceLoader.Load("assets/sound/Rifle_Cock.wav") as AudioStream;

		this.audioPlayer = this.GetNode("Audio_Stream_Player") as AudioStreamPlayer;
		this.audioPlayer.Connect("finished", this, nameof(this.DestroySelf));
		this.audioPlayer.Stop();

		this.audioPlayer.VolumeDb = -32; // TODO remove (temporary for my own hearing)
	}

	public void PlaySound(SoundName name, Vector3 position) // TODO try with 3d audio position
	{
		if (this.pistolShotAudio == null || this.rifleShotAudio == null || this.gunReloadAudio == null)
		{
			GD.Print("Audio not set!");
			QueueFree();
			return;
		}

		if (name == SoundName.PISTOL_SHOT)
		{
			this.audioPlayer.Stream = this.pistolShotAudio;
		}
		else if (name == SoundName.RIFLE_SHOT)
		{
			this.audioPlayer.Stream = this.rifleShotAudio;
		}
		else if (name == SoundName.RELOAD)
		{
			this.audioPlayer.Stream = this.gunReloadAudio;
		}
		else
		{
			GD.Print("UNKNOWN STREAM");
			QueueFree();
			return;
		}

		this.audioPlayer.Play();
	}

	private void DestroySelf()
	{
		this.audioPlayer.Stop();
		QueueFree();
	}
}
