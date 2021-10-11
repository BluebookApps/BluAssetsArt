namespace NG
{
	public enum SSHOrientation
	{
		portrait, landscape, both
	}

	public struct SSH_IntVector2
	{
		public int width;
		public int height;

		public SSH_IntVector2(int w, int h)
		{
			width = w;
			height = h;
		}
	}
}