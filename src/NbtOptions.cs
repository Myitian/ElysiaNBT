namespace ElysiaNBT;

public struct NbtOptions
{
    private int _maxDepth = 512;

    public int MaxDepth
    {
        readonly get => _maxDepth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _maxDepth = value;
        }
    }
    public bool HasRootName { get; set; }

    public NbtOptions()
    {
    }
}