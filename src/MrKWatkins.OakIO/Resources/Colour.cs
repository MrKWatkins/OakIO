namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// A colour with eight-bit red, green, blue and alpha components.
/// </summary>
/// <param name="Red">The red component.</param>
/// <param name="Green">The green component.</param>
/// <param name="Blue">The blue component.</param>
/// <param name="Alpha">The alpha component; 255 is opaque and zero is transparent.</param>
public readonly record struct Colour(byte Red, byte Green, byte Blue, byte Alpha = 255);