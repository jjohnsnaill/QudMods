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
			if (XRLCore.CurrentFrame >= nextFrame && XRLCore.CurrentFrame < nextFrame + 30)
			{
				nextFrame = XRLCore.CurrentFrame + 3;
				if (nextFrame >= 61)
				{
					nextFrame -= 61;
				}
				if (++lastColor >= 5)
				{
					lastColor = 0;
				}
			}
			switch (lastColor)
			{
				case 0: E.DetailColor = "r"; break;
				case 1: E.DetailColor = "R"; break;
				case 2: E.DetailColor = "o"; break;
				case 3: E.DetailColor = "O"; break;
				case 4: E.DetailColor = "W"; break;
			}

			return base.Render(E);
		}
	}
}