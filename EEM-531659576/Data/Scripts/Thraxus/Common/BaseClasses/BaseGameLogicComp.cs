using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace Eem.Thraxus.Common.BaseClasses
{
	public abstract class BaseGameLogicComp : MyGameLogicComponent
	{
		protected string EntityName = "PlaceholderName";
		protected long EntityId = 0L;
		protected long Ticks;

		public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
		{
			// Always return base.GetObjectBuilder(); after your code! 
			// Do all saving here, make sure to return the OB when done;
			return base.GetObjectBuilder(copy);
		}

		public override void UpdateBeforeSimulation()
		{
			Ticks++;
			TickTimer();
			base.UpdateBeforeSimulation();
		}

		protected abstract void TickTimer();
	}
}