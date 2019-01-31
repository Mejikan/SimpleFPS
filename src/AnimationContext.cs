public class AnimationContext
{
	public ushort Id;

	public string Name;

	/// <summary>
	/// The animation states as numbers.
	/// </summary>
	/// <value>Each element is a number representing a state (should use an enum for this).</value>
	public ushort[] States = {};

	/// <summary>
	/// The Id of the next animation state to go to.
	/// </summary>
	public ushort Next;

	/// <summary>
	/// The speed of the animation.
	/// </summary>
	public float Speed = 0f;

	public AnimationContext(ushort id, string name)
	{
		this.Id = id;
		this.Name = name;
		this.Next = this.Id;
	}

	// override object.Equals
	public override bool Equals(object obj)
	{	
		AnimationContext context = obj as AnimationContext;
		if (context == null)
		{
			return false;
		}
		
		if (context.Id != this.Id)
		{
			return false;
		}
		
		return true;
	}
	
	// override object.GetHashCode
	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}