namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

internal static class SnapshotBanks
{
    internal static void Validate(IReadOnlyList<ReadOnlyMemory<byte>> banks)
    {
        ArgumentNullException.ThrowIfNull(banks);
        if (banks.Count != 8 || banks.Any(bank => bank.Length != 16384))
        {
            throw new ArgumentException("A 128K snapshot requires eight physical banks of exactly 16384 bytes, in bank-number order.", nameof(banks));
        }
    }
}