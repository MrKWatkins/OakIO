namespace MrKWatkins.OakIO.ZXSpectrum.Nex;

public sealed class NexBank
{
    internal NexBank(int bankNumber, byte[] data)
    {
        BankNumber = bankNumber;
        Data = data;
    }

    public int BankNumber { get; }

    public byte[] Data { get; }

    public override string ToString() => $"Bank {BankNumber}";
}