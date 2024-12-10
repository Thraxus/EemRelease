
namespace Eem.Thraxus.Common.Interfaces
{
    internal interface IInit<in TA, in TB>
    {
        void Init(TA itemA, TB itemB);
    }

    internal interface IInit<in TA>
    {
        void Init(TA itemA);
    }
}