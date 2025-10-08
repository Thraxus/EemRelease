namespace Eem.Thraxus.Common.Interfaces
{
    internal interface IHaveSubscriptions
    {
        void SubscriptionHandler(bool close = false);
        void Subscribe();
        void UnSubscribe();
    }
}