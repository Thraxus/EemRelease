using Eem.Thraxus.Common.Generics;

namespace Eem.Thraxus.Models
{
    public class ActionQueues
    {
        public readonly ActionQueue BeforeSimActionQueue = new ActionQueue();
        public readonly ActionQueue AfterSimActionQueue = new ActionQueue();
    }
}