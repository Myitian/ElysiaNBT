namespace ElysiaNBT;

public interface IInstance<TSelf> where TSelf : IInstance<TSelf>, allows ref struct
{
    static abstract TSelf Instance { get; }
}
