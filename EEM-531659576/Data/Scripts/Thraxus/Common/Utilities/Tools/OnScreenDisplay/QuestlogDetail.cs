using System.Text;

namespace Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay
{
    public class QuestLogDetail
    {
        public StringBuilder NewQuest;
        public StringBuilder OldQuest;

        public QuestLogDetail(StringBuilder newQuest)
        {
            NewQuest = newQuest;
        }

        public void UpdateQuest(StringBuilder newQuest)
        {
            OldQuest = NewQuest;
            NewQuest = newQuest;
        }
    }
}