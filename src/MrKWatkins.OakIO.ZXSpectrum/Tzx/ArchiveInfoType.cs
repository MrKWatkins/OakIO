using System.ComponentModel;

namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

#pragma warning disable CA1028
public enum ArchiveInfoType : byte
{
    [Description("Full Title")]
    FullTitle = 0,

    [Description("Software House/Publisher")]
    SoftwareHouseOrPublisher = 1,

    [Description("Author(s)")]
    Authors = 2,

    [Description("Year of Publication")]
    YearOfPublication = 3,

    [Description("Language")]
    Language = 4,

    [Description("Game/Utility Type")]
    GameOrUtilityType = 5,

    [Description("Price")]
    Price = 6,

    [Description("Protection Scheme/Loader")]
    ProtectionSchemeOrLoader = 7,

    [Description("Origin")]
    Origin = 8,

    [Description("Comment(s)")]
    Comments = 0xFF
}
#pragma warning restore CA1028