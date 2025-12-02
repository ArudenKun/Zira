using ZLinq;

namespace Zira.Extensions;

public static class EnumExtensions
{
    public static IEnumerable<Enum> GetAllValues(this Enum @enum, bool orderByName = false) =>
        GetAllValues(@enum.GetType(), orderByName);

    public static IEnumerable<Enum> GetAllValues(this Type t, bool orderByName = false)
    {
        if (!t.IsEnum)
        {
            throw new ArgumentException($"{nameof(t)} must be an enum type");
        }

        var names = orderByName
            ? Enum.GetNames(t)
                .AsValueEnumerable()
                .OrderBy(e => e.ToString(), StringComparer.Ordinal)
                .ToArray()
            : Enum.GetNames(t).AsValueEnumerable().ToArray();

        foreach (var name in names)
        {
            yield return (Enum)Enum.Parse(t, name);
        }
    }
}
