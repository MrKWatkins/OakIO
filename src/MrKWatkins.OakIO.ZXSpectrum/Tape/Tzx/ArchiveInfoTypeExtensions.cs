using System.ComponentModel;
using System.Reflection;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Extension methods for <see cref="ArchiveInfoType"/>.
/// </summary>
public static class ArchiveInfoTypeExtensions
{
    /// <summary>
    /// Gets the human-readable description of the specified <see cref="ArchiveInfoType"/>.
    /// </summary>
    /// <param name="type">The archive info type.</param>
    /// <returns>The description from the <see cref="DescriptionAttribute"/>, or the enum name if no attribute is present.</returns>
    [Pure]
    public static string ToDescription(this ArchiveInfoType type)
    {
        var name = type.ToString();
        var members = typeof(ArchiveInfoType).GetMember(name);
        if (members.Length == 0)
        {
            return name;
        }

        var attribute = members[0].GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? name;
    }
}