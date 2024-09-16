using XRL.Core;

namespace XRL.World.Parts
{
	public class CycleColors : IPart
	{
		private int lastColor;
		private int nextFrame;

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool Render(RenderEvent E)
		{
			if (XRLCore.CurrentFrame >= nextFrame)
			{
				nextFrame = XRLCore.CurrentFrame + 4;
				if (nextFrame >= 62)
				{
					nextFrame -= 62;
				}
				if (++lastColor >= Crayons.BrightColors.Length)
				{
					lastColor = 0;
				}
			}
			E.DetailColor = Crayons.BrightColors[lastColor];

			return base.Render(E);
		}
	}
}