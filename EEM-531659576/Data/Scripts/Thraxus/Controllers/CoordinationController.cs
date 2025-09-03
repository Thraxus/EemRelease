using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Models;

namespace Eem.Thraxus.Controllers
{
    public class CoordinationController : BaseLoggingClass
    {
        public readonly ActionQueues ActionQueues = new ActionQueues();
        public BotController BotController;
        public FactionController FactionController;
        public DamageController DamageController;

        public CoordinationController()
        {
            BotController = new BotController(this);
            DamageController = new DamageController(this);
            FactionController = new FactionController(this);
        }

        public void EarlyInit()
        {
            BotController.OnWriteToLog += WriteGeneral;
            DamageController.OnWriteToLog += WriteGeneral;
            FactionController.OnWriteToLog += WriteGeneral;
            BotController.Init();
        }

        public void LateInit()
        {
            FactionController.Init();
            DamageController.Init();
        }

        public void BeforeSimUpdate()
        {
            FactionController.Update();
            ActionQueues.BeforeSimActionQueue.Execute();
        }

        public void BeforeSimUpdate10()
        {
            BotController.Update10();
        }

        public void AfterSimUpdate()
        {
            ActionQueues.AfterSimActionQueue.Execute();
        }

        public override void Close()
        {
            BotController.OnWriteToLog -= WriteGeneral;
            BotController.Close();
            DamageController.OnWriteToLog -= WriteGeneral;
            DamageController.Close();
            FactionController.OnWriteToLog -= WriteGeneral;
            FactionController.Close();
            base.Close();
        }
    }
}
