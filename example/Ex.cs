namespace Example;

file class Ex { public int A = 1; }
partial class Ex2
{
    public static object GetEx() => new Ex() { A = 11 };
}