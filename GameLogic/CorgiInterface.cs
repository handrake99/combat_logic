namespace IdleCs.GameLogic
{
    public interface ICorgiInterface<T> where T : class
    {
        T GetSpec();
    }
}