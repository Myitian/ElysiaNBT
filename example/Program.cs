using ElysiaNBT;
using ElysiaNBT.Serialization;
using System.Numerics;

namespace Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            object obj = NbtSerializer.DeserializeBinary<object>(File.OpenRead(Console.ReadLine()));
            string s = NbtSerializer.SerializeString(obj, StringNbtOptions.Minimal);
            Console.WriteLine(s);
            File.WriteAllText(Console.ReadLine(), s);
        }
    }

    class Ex
    {
        public AA Val { get; set; }
        [NbtEntryName("$ %")]
        private Vector2 VV = new Vector2(114.514f, 1919.810f);
    }

    enum AA
    {
        A,
        B,
        C
    }
}
