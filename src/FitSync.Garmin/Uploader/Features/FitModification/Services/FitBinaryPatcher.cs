namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

/// <summary>
/// Patches manufacturer and product fields directly in the raw FIT binary data
/// without decode/re-encode. Walks the binary structure to find field offsets,
/// overwrites the target bytes, and recalculates CRC.
/// </summary>
public class FitBinaryPatcher(ILogger<FitBinaryPatcher> logger) : IFitModifier
{
    private readonly ILogger<FitBinaryPatcher> logger = logger;

    private const ushort GarminManufacturer = 1;
    private const ushort GarminEdge830Product = 2530;

    private const ushort FileIdMesgNum = 0;
    private const ushort DeviceInfoMesgNum = 23;

    private const byte FileIdManufacturerFieldNum = 1;
    private const byte FileIdProductFieldNum = 2;
    private const byte DeviceInfoManufacturerFieldNum = 2;
    private const byte DeviceInfoProductFieldNum = 4;

    private const byte CompressedHeaderMask = 0x80;
    private const byte CompressedLocalMesgNumMask = 0x60;
    private const byte MesgDefinitionMask = 0x40;
    private const byte DevDataMask = 0x20;
    private const byte LocalMesgNumMask = 0x0F;

    public byte[] ModifyDeviceInfo(byte[] fitFileData)
    {
        try
        {
            this.logger.LogDebug("Binary patching FIT file - {Bytes} bytes", fitFileData.Length);

            byte[] patched = (byte[])fitFileData.Clone();
            int totalPatches = 0;
            int position = 0;

            while (position < patched.Length)
            {
                int chainStart = position;
                int headerSize = patched[position];

                if (headerSize < 12 || position + headerSize > patched.Length)
                {
                    this.logger.LogWarning(
                        "Invalid header size {Size} at position {Position}",
                        headerSize,
                        position
                    );
                    break;
                }

                uint dataSize = BitConverter.ToUInt32(patched, position + 4);
                bool hasCrc = headerSize == 14;
                int chainEnd = position + headerSize + (int)dataSize;
                int dataStart = position + headerSize;
                int dataEnd = chainEnd;

                if (chainEnd + 2 > patched.Length && dataSize > 0)
                {
                    this.logger.LogWarning(
                        "Chain extends beyond file at position {Position}",
                        position
                    );
                    break;
                }

                int patches = PatchChainData(patched, dataStart, dataEnd);
                totalPatches += patches;

                if (dataSize > 0)
                {
                    RecalculateCrc(patched, chainStart, chainEnd + 2);
                }

                position = chainEnd + (dataSize > 0 ? 2 : 0);
            }

            this.logger.LogInformation(
                "Binary patched FIT file: {Patches} field patches applied. You are now a Garmin",
                totalPatches
            );

            return patched;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to binary patch FIT file, returning unchanged");
            return fitFileData;
        }
    }

    private int PatchChainData(byte[] data, int start, int end)
    {
        FieldLayout?[] localMesgDefs = new FieldLayout?[16];
        int position = start;
        int patches = 0;

        while (position < end)
        {
            byte recordHeader = data[position];

            if ((recordHeader & CompressedHeaderMask) == CompressedHeaderMask)
            {
                byte localMesgNum = (byte)((recordHeader & CompressedLocalMesgNumMask) >> 5);
                FieldLayout? layout = localMesgDefs[localMesgNum];
                if (layout == null)
                {
                    this.logger.LogWarning(
                        "Missing definition for compressed local mesg {Num} at {Pos}",
                        localMesgNum,
                        position
                    );
                    break;
                }

                int dataStart = position + 1;
                patches += PatchDataRecord(data, dataStart, layout);
                position = dataStart + layout.TotalFieldSize;
            }
            else if ((recordHeader & MesgDefinitionMask) == MesgDefinitionMask)
            {
                position++;
                byte reserved = data[position++];
                byte architecture = data[position++];
                bool isBigEndian = architecture == 1;

                ushort globalMesgNum = isBigEndian
                    ? (ushort)((data[position] << 8) | data[position + 1])
                    : BitConverter.ToUInt16(data, position);
                position += 2;

                byte numFields = data[position++];
                List<FieldDef> fields = new(numFields);

                for (int i = 0; i < numFields; i++)
                {
                    byte fieldNum = data[position++];
                    byte fieldSize = data[position++];
                    byte baseType = data[position++];
                    fields.Add(new FieldDef(fieldNum, fieldSize, baseType));
                }

                if ((recordHeader & DevDataMask) == DevDataMask)
                {
                    byte numDevFields = data[position++];
                    for (int i = 0; i < numDevFields; i++)
                    {
                        byte devFieldNum = data[position++];
                        byte devFieldSize = data[position++];
                        byte devIndex = data[position++];
                        fields.Add(new FieldDef(devFieldNum, devFieldSize, 0, true));
                    }
                }

                byte localNum = (byte)(recordHeader & LocalMesgNumMask);
                localMesgDefs[localNum] = new FieldLayout(globalMesgNum, isBigEndian, fields);
            }
            else
            {
                byte localMesgNum = (byte)(recordHeader & LocalMesgNumMask);
                FieldLayout? layout = localMesgDefs[localMesgNum];
                if (layout == null)
                {
                    this.logger.LogWarning(
                        "Missing definition for local mesg {Num} at {Pos}",
                        localMesgNum,
                        position
                    );
                    break;
                }

                int dataStart = position + 1;
                patches += PatchDataRecord(data, dataStart, layout);
                position = dataStart + layout.TotalFieldSize;
            }
        }

        return patches;
    }

    private int PatchDataRecord(byte[] data, int dataStart, FieldLayout layout)
    {
        int patches = 0;

        if (layout.GlobalMesgNum == FileIdMesgNum)
        {
            patches += PatchField(
                data,
                dataStart,
                layout,
                FileIdManufacturerFieldNum,
                GarminManufacturer
            );
            patches += PatchField(
                data,
                dataStart,
                layout,
                FileIdProductFieldNum,
                GarminEdge830Product
            );
        }
        else if (layout.GlobalMesgNum == DeviceInfoMesgNum)
        {
            patches += PatchField(
                data,
                dataStart,
                layout,
                DeviceInfoManufacturerFieldNum,
                GarminManufacturer
            );
            patches += PatchField(
                data,
                dataStart,
                layout,
                DeviceInfoProductFieldNum,
                GarminEdge830Product
            );
        }

        return patches;
    }

    private int PatchField(
        byte[] data,
        int dataStart,
        FieldLayout layout,
        byte targetFieldNum,
        ushort value
    )
    {
        int offset = 0;
        foreach (FieldDef field in layout.Fields)
        {
            if (field.IsDev)
            {
                offset += field.Size;
                continue;
            }

            if (field.FieldNum == targetFieldNum && field.Size == 2)
            {
                if (layout.IsBigEndian)
                {
                    data[dataStart + offset] = (byte)(value >> 8);
                    data[dataStart + offset + 1] = (byte)(value & 0xFF);
                }
                else
                {
                    data[dataStart + offset] = (byte)(value & 0xFF);
                    data[dataStart + offset + 1] = (byte)(value >> 8);
                }
                return 1;
            }

            offset += field.Size;
        }

        return 0;
    }

    private void RecalculateCrc(byte[] data, int chainStart, int chainEndWithCrc)
    {
        ushort crc = 0;
        for (int i = chainStart; i < chainEndWithCrc - 2; i++)
        {
            crc = CrcGet16(crc, data[i]);
        }
        data[chainEndWithCrc - 2] = (byte)(crc & 0xFF);
        data[chainEndWithCrc - 1] = (byte)(crc >> 8);
    }

    private static ushort CrcGet16(ushort crc, byte dataByte)
    {
        ReadOnlySpan<ushort> crcTable =
        [
            0x0000,
            0xCC01,
            0xD801,
            0x1400,
            0xF001,
            0x3C00,
            0x2800,
            0xE401,
            0xA001,
            0x6C00,
            0x7800,
            0xB401,
            0x5000,
            0x9C01,
            0x8801,
            0x4400
        ];

        ushort tmp = crcTable[crc & 0xF];
        crc = (ushort)((crc >> 4) & 0x0FFF);
        crc = (ushort)(crc ^ tmp ^ crcTable[dataByte & 0xF]);

        tmp = crcTable[crc & 0xF];
        crc = (ushort)((crc >> 4) & 0x0FFF);
        crc = (ushort)(crc ^ tmp ^ crcTable[(dataByte >> 4) & 0xF]);

        return crc;
    }

    private sealed record FieldDef(byte FieldNum, byte Size, byte BaseType, bool IsDev = false);

    private sealed class FieldLayout(
        ushort globalMesgNum,
        bool isBigEndian,
        List<FitBinaryPatcher.FieldDef> fields
    )
    {
        public ushort GlobalMesgNum { get; } = globalMesgNum;
        public bool IsBigEndian { get; } = isBigEndian;
        public List<FieldDef> Fields { get; } = fields;
        public int TotalFieldSize => this.Fields.Sum(f => f.Size);
    }
}
