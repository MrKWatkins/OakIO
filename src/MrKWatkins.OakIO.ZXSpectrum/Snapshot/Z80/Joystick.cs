namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// The type of joystick emulated in a Z80 snapshot.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public enum Joystick
{
    /// <summary>
    /// Cursor joystick (Protek/AGF).
    /// </summary>
    Cursor = 0,

    /// <summary>
    /// Kempston joystick.
    /// </summary>
    Kempston = 1,

    /// <summary>
    /// Sinclair 2 left joystick (keys 1-5).
    /// </summary>
    Sinclair2Left = 2,

    /// <summary>
    /// Sinclair 2 right joystick (keys 6-0).
    /// </summary>
    Sinclair2Right = 3
}