namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// A memory bank in a NEX file.
/// </summary>
public sealed class NexBank
{
    private readonly byte[] data;

    internal NexBank(int bankNumber, byte[] data)
    {
        BankNumber = bankNumber;
        this.data = data;
    }

    /// <summary>
    /// Gets the bank number.
    /// </summary>
    public int BankNumber { get; }

    /// <summary>
    /// Gets the raw bank data.
    /// </summary>
    public IReadOnlyList<byte> Data => data;

    internal ReadOnlyMemory<byte> DataMemory => data;

    /// <inheritdoc />
    [Pure]
    public override string ToString() => $"Bank {BankNumber}";
}