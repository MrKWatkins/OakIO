using System.ComponentModel;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

#pragma warning disable CA1028
/// <summary>
/// The type of information stored in an archive info entry.
/// </summary>
public enum ArchiveInfoType : byte
{
    /// <summary>
    /// The full title of the software.
    /// </summary>
    [Description("Full Title")]
    FullTitle = 0,

    /// <summary>
    /// The software house or publisher.
    /// </summary>
    [Description("Software House/Publisher")]
    SoftwareHouseOrPublisher = 1,

    /// <summary>
    /// The author or authors.
    /// </summary>
    [Description("Author(s)")]
    Authors = 2,

    /// <summary>
    /// The year of publication.
    /// </summary>
    [Description("Year of Publication")]
    YearOfPublication = 3,

    /// <summary>
    /// The language of the software.
    /// </summary>
    [Description("Language")]
    Language = 4,

    /// <summary>
    /// The game or utility type.
    /// </summary>
    [Description("Game/Utility Type")]
    GameOrUtilityType = 5,

    /// <summary>
    /// The price of the software.
    /// </summary>
    [Description("Price")]
    Price = 6,

    /// <summary>
    /// The protection scheme or loader used.
    /// </summary>
    [Description("Protection Scheme/Loader")]
    ProtectionSchemeOrLoader = 7,

    /// <summary>
    /// The origin of the tape.
    /// </summary>
    [Description("Origin")]
    Origin = 8,

    /// <summary>
    /// Comments about the tape.
    /// </summary>
    [Description("Comment(s)")]
    Comments = 0xFF
}
#pragma warning restore CA1028