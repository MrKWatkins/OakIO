namespace MrKWatkins.OakIO.Resources.Aky;

// Decode native packets without depending on a machine player.
// https://bitbucket.org/JulienNevo/arkostracker3/src/14d975bdf0787cde4a5fa9425abe793e4a3a3536/doc/export/AKY.md
internal static class AkyDecoder
{
    [Pure]
    internal static IEnumerable<AkyFrame> Decode(AkyFile file)
    {
        var registers = new byte[file.Header.ChipCount * 14];
        var lastEnvelope = Enumerable.Repeat(-1, file.Header.ChipCount).ToArray();
        var channels = new AkyChannelState[file.Header.ChannelCount];
        foreach (var pattern in file.Patterns)
        {
            var tracks = pattern.Tracks;
            for (var channel = 0; channel < channels.Length; ++channel)
            {
                channels[channel] = new AkyChannelState { Track = tracks[channel].Offset };
            }
            for (var frame = 0; frame < pattern.Duration; ++frame)
            {
                var retriggers = new bool[file.Header.ChipCount];
                var hardware = new bool[file.Header.ChipCount];
                for (var chip = 0; chip < file.Header.ChipCount; ++chip)
                {
                    registers[(chip * 14) + 7] = 63;
                }
                for (var channel = 0; channel < channels.Length; ++channel)
                {
                    var state = channels[channel];
                    if (state.Remaining == 0)
                    {
                        var block = new AkyTrackBlock(file, state.Track);
                        state.Remaining = block.Duration;
                        state.Position = block.RegisterBlockOffset;
                        state.Track += 3;
                        state.Initial = true;
                        state.Sid = null;
                    }
                    DecodeChannel(file, state, registers, channel, retriggers, hardware);
                    --state.Remaining;
                    state.Initial = false;
                }
                for (var chip = 0; chip < file.Header.ChipCount; ++chip)
                {
                    retriggers[chip] |= hardware[chip] && registers[(chip * 14) + 13] != lastEnvelope[chip];
                    if (retriggers[chip])
                    {
                        lastEnvelope[chip] = registers[(chip * 14) + 13];
                    }
                }
                yield return new AkyFrame((byte[])registers.Clone(), retriggers, channels.Select(channel => channel.Sid).ToArray());
            }
        }
    }

    private static void DecodeChannel(AkyFile file, AkyChannelState state, byte[] registers, int channel, bool[] retriggers, bool[] hardware)
    {
        var chip = channel / 3;
        var voice = channel % 3;
        var start = chip * 14;
        var volume = start + 8 + voice;
        var tone = start + (voice * 2);
        var code = ReadPacketCode(file, state);
        var kind = code & 3;
        var noise = false;
        if (state.Initial)
        {
            if (kind <= 1)
            {
                if ((code & 128) != 0)
                {
                    throw new InvalidDataException("AKY initial packet has reserved bits set.");
                }
                registers[volume] = (byte)((code >> 3) & 15);
                noise = (code & 4) != 0;
            }
            else
            {
                registers[volume] = 16;
                registers[start + 13] = (byte)(code >> 4);
                hardware[chip] = true;
                retriggers[chip] |= (code & 4) != 0;
                noise = (code & 8) != 0;
            }
            if (noise)
            {
                registers[start + 6] = ReadNoise(file, state);
            }
            if ((kind & 1) != 0)
            {
                SetWord(registers, tone, ReadWord(file, state) & 4095);
            }
            if (kind >= 2)
            {
                SetWord(registers, start + 11, ReadWord(file, state));
            }
        }
        else if (kind == 0)
        {
            if ((code & 4) != 0)
            {
                registers[volume] = (byte)((code >> 3) & 15);
            }
            else if ((code & 127) != 0)
            {
                throw new InvalidDataException("AKY packet has an invalid control command.");
            }
            noise = (code & 128) != 0;
            if (noise)
            {
                registers[start + 6] = ReadNoise(file, state);
            }
        }
        else if (kind == 1)
        {
            registers[volume] = (byte)((code >> 2) & 15);
            if ((code & 64) != 0)
            {
                registers[tone] = Read(file, state);
            }
            if ((code & 128) != 0)
            {
                var extra = Read(file, state);
                registers[tone + 1] = (byte)(extra & 15);
                noise = (extra & 128) != 0;
                if ((extra & 64) != 0)
                {
                    registers[start + 6] = ReadNoise(file, state);
                }
            }
        }
        else
        {
            registers[volume] = 16;
            hardware[chip] = true;
            if (kind == 2)
            {
                registers[start + 13] = (byte)(8 | ((code >> 2) & 7));
                if ((code & 128) != 0)
                {
                    registers[start + 11] = Read(file, state);
                }
                if ((code & 64) != 0)
                {
                    registers[start + 12] = Read(file, state);
                }
            }
            else
            {
                if ((code & 4) != 0)
                {
                    registers[start + 11] = Read(file, state);
                }
                if ((code & 8) != 0)
                {
                    registers[start + 12] = Read(file, state);
                }
                if ((code & 16) != 0)
                {
                    registers[tone] = Read(file, state);
                }
                if ((code & 32) != 0)
                {
                    registers[tone + 1] = (byte)(Read(file, state) & 15);
                }
                if ((code & 64) != 0)
                {
                    var shape = Read(file, state);
                    if (shape > 15)
                    {
                        throw new InvalidDataException("AKY envelope shape exceeds four bits.");
                    }
                    registers[start + 13] = shape;
                }
            }
            if ((code & (kind == 2 ? 32 : 128)) != 0)
            {
                var extra = Read(file, state);
                retriggers[chip] |= (extra & 1) != 0;
                noise = (extra & 2) != 0;
                if ((extra & 4) != 0)
                {
                    var value = extra >> 3;
                    if (value == 0)
                    {
                        throw new InvalidDataException("An enabled AKY noise period must be nonzero.");
                    }
                    registers[start + 6] = (byte)value;
                }
            }
        }
        if ((kind & 1) != 0)
        {
            registers[start + 7] &= (byte)~(1 << voice);
        }
        if (noise)
        {
            registers[start + 7] &= (byte)~(8 << voice);
        }
    }

    [MustUseReturnValue]
    private static byte ReadPacketCode(AkyFile file, AkyChannelState state)
    {
        var controls = 0;
        while (true)
        {
            var code = Read(file, state);
            if (!state.Initial && code == 8)
            {
                state.Position = file.ReadPointer(state.Position);
            }
            else if (state.Initial && (code & 0xC3) == 0x80)
            {
                state.Sid = ReadSid(file, state, (byte)((code >> 2) & 15));
            }
            else if (!state.Initial && code == 16)
            {
                RequireSidVersion(file);
                var settings = Read(file, state);
                if (settings > 31 || (settings != 0 && (settings & 1) == 0))
                {
                    throw new InvalidDataException("AKY SID settings contain reserved bits.");
                }
                state.Sid = settings == 0 ? null : ReadSid(file, state, (byte)(settings >> 1));
            }
            else
            {
                return code;
            }
            if (++controls > file.Body.Length)
            {
                throw new InvalidDataException("AKY control sequence does not reach a frame.");
            }
        }
    }

    [MustUseReturnValue]
    private static AkySidState ReadSid(AkyFile file, AkyChannelState state, byte lowShelfVolume)
    {
        RequireSidVersion(file);
        // The exporter writes ST timer pairs as bytes, not endian-sensitive words.
        // https://bitbucket.org/JulienNevo/arkostracker3/src/14d975bdf0787cde4a5fa9425abe793e4a3a3536/source/export/aky/process/RegisterBlockEncoder.cpp
        return file.Header.SidProfile switch
        {
            AkySidProfile.Cpc => new AkyCpcSidState(lowShelfVolume, Read(file, state), Read(file, state)),
            AkySidProfile.AmstradPlus => new AkyAmstradPlusSidState(lowShelfVolume, ReadWord(file, state), ReadWord(file, state)),
            AkySidProfile.AtariSt => new AkyAtariStSidState(lowShelfVolume, Read(file, state), Read(file, state), Read(file, state), Read(file, state)),
            _ => throw new InvalidDataException("Unknown AKY SID profile.")
        };
    }

    private static void RequireSidVersion(AkyFile file)
    {
        if (file.Header.Version == 0)
        {
            throw new InvalidDataException("SID commands require AKY version one.");
        }
    }

    private static void SetWord(byte[] registers, int offset, int value)
    {
        registers[offset] = (byte)value;
        registers[offset + 1] = (byte)(value >> 8);
    }

    [MustUseReturnValue]
    private static byte Read(AkyFile file, AkyChannelState state) => file.ReadByte(state.Position++);

    [MustUseReturnValue]
    private static ushort ReadWord(AkyFile file, AkyChannelState state)
    {
        var result = file.ReadWord(state.Position);
        state.Position += 2;
        return result;
    }

    [MustUseReturnValue]
    private static byte ReadNoise(AkyFile file, AkyChannelState state)
    {
        var value = Read(file, state);
        if (value is 0 or > 31)
        {
            throw new InvalidDataException("AKY noise periods must be between one and 31.");
        }
        return value;
    }
}