using System.ComponentModel;
using System.Reflection;

namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public static class ArchiveInfoTypeExtensions
{
    [Pure]
    public static string ToDescription(this ArchiveInfoType type)
    {
        var name = type.ToString();
        var member = typeof(ArchiveInfoType).GetMember(name)[0];
        var attribute = member.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? name;
    }
}