namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// A memory bank in a NEX file.
/// </summary>
public sealed class NexBank
{
    internal NexBank(int bankNumber, byte[] data)
    {
        BankNumber = bankNumber;
        Data = data;
    }

    /// <summary>
    /// Gets the bank number.
    /// </summary>
    public int BankNumber { get; }

    /// <summary>
    /// Gets the raw bank data.
    /// </summary>
    public byte[] Data { get; }

    /// <inheritdoc />
    public override string ToString() => $"Bank {BankNumber}";
}