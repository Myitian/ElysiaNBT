using System.Text;

namespace ElysiaNBT
{
    public static class DebugUtilities
    {
        public static string AsString(this Type type)
        {
            return type.AsString(new()).ToString();
        }
        public static StringBuilder AsString(this Type type, StringBuilder sb)
        {
            if (type.Namespace is not null)
                sb.Append(type.Namespace).Append('.');
            ReadOnlySpan<char> name = type.Name;
            int i = name.IndexOf('`');
            if (i >= 0)
                name = name[..i];
            sb.Append(name);
            ReadOnlySpan<Type> gArgs = type.GetGenericArguments();
            if (gArgs.Length > 0)
            {
                sb.Append('<');
                for (i = 0; i < gArgs.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    gArgs[i].AsString(sb);
                }
                sb.Append('>');
            }
            if (type.IsArray)
                sb.Append('[').Append(',', type.GetArrayRank()).Append(']');
            return sb;
        }
    }
}
