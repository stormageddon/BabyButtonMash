using Godot;

public partial class LockButton : Button
{
	private bool _isLocked = false;
	private Texture2D _iconLocked;
	private Texture2D _iconUnlocked;

	public override void _Ready()
	{
		_iconLocked = GD.Load<Texture2D>("res://assets/locked.png");
		_iconUnlocked = GD.Load<Texture2D>("res://assets/unlocked.png");
		UpdateAppearance();
	}

	public override void _Pressed()
	{
		Node gameManager = GetTree().GetFirstNodeInGroup("game_manager");
		if (gameManager == null)
		{
			GD.PushWarning("LockButton: no node in group 'game_manager' found");
			return;
		}
		if (_isLocked)
			gameManager.Call("uninstall_key_blocker");
		else
			gameManager.Call("install_key_blocker");
		_isLocked = !_isLocked;
		UpdateAppearance();
	}

	private void UpdateAppearance()
	{
		if (_isLocked)
		{
			Icon = _iconLocked;
			SelfModulate = Colors.Red;
		}
		else
		{
			Icon = _iconUnlocked;
			SelfModulate = Colors.Green;
		}
	}
}
