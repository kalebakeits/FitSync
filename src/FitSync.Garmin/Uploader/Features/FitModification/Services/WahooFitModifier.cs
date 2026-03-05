namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public class WahooFitModifier(ILogger<WahooFitModifier> logger) : IWahooFitModifier
{
    private readonly ILogger<WahooFitModifier> logger = logger;

    private const ushort GarminManufacturer = 1;
    private const ushort GarminEdge830Product = 2530;
    private const ushort WahooManufacturer = 32;
    private const ushort GlobalFileId = 0;
    private const ushort GlobalDeviceInfo = 23;
    private const byte FieldNumManufacturer = 2;
    private const byte FieldNumProduct = 4;

    public byte[] ModifyDeviceInfo(byte[] fitFileData)
    {
        this.logger.LogDebug("Patching Wahoo FIT file in-place - {Bytes} bytes.", fitFileData.Length);

        byte[] data = (byte[])fitFileData.Clone();
        int pos = 0;
        int modificationsCount = 0;

        while (pos < data.Length)
        {
            // Each chained FIT record starts with a file header
            if (pos + 1 >= data.Length)
                break;

            int headerSize = data[pos];
            if (headerSize < 12 || pos + headerSize > data.Length)
                break;

            // Header contains DataSize at bytes 4-7 (uint32 LE)
            uint dataSize = BitConverter.ToUInt32(data, pos + 4);
            int recordEnd = pos + headerSize + (int)dataSize + 2; // +2 for CRC

            // Clamp to actual data length
            if (recordEnd > data.Length)
                recordEnd = data.Length;

            // Walk messages within this chained record
            // localMesgDefs maps local message number → field layout (offset of manufacturer, offset of product, message size)
            Dictionary<byte, LocalMesgLayout> localMesgDefs = [];

            int msgPos = pos + headerSize;
            while (msgPos < recordEnd - 2) // -2 to skip trailing CRC
            {
                if (msgPos >= data.Length)
                    break;

                byte recordHeader = data[msgPos];
                bool isDefinition = (recordHeader & 0x40) != 0;
                bool isCompressedTimestamp = (recordHeader & 0x80) != 0;
                byte localNum = (byte)(isCompressedTimestamp ? (recordHeader & 0x60) >> 5 : recordHeader & 0x0F);

                if (isCompressedTimestamp)
                {
                    // Compressed timestamp data message — use existing definition
                    if (localMesgDefs.TryGetValue(localNum, out LocalMesgLayout? compLayout))
                        msgPos += 1 + compLayout.MesgSize;
                    else
                        break;
                    continue;
                }

                if (isDefinition)
                {
                    bool hasDeveloperFields = (recordHeader & 0x20) != 0;

                    // Definition: 1 byte header + 1 reserved + 1 arch + 2 global num + 1 field count + N*3 fields
                    if (msgPos + 5 >= data.Length)
                        break;

                    byte architecture = data[msgPos + 2]; // 0 = little-endian, 1 = big-endian
                    ushort globalNum = architecture == 0
                        ? BitConverter.ToUInt16(data, msgPos + 3)
                        : (ushort)((data[msgPos + 3] << 8) | data[msgPos + 4]);

                    byte fieldCount = data[msgPos + 5];
                    int defSize = 6 + fieldCount * 3;

                    if (msgPos + defSize > data.Length)
                        break;

                    if (globalNum == GlobalFileId || globalNum == GlobalDeviceInfo)
                    {
                        // FileId = head unit (always patch). DeviceInfo = only patch if Wahoo head unit (sensors stay).
                        int mesgSize = 0;
                        int? manufacturerOffset = null;
                        int? productOffset = null;

                        for (int f = 0; f < fieldCount; f++)
                        {
                            int fieldDefPos = msgPos + 6 + f * 3;
                            byte fieldNum = data[fieldDefPos];
                            byte fieldSize = data[fieldDefPos + 1];

                            if (fieldNum == FieldNumManufacturer)
                                manufacturerOffset = mesgSize;
                            else if (fieldNum == FieldNumProduct)
                                productOffset = mesgSize;

                            mesgSize += fieldSize;
                        }

                        localMesgDefs[localNum] = new LocalMesgLayout(mesgSize, manufacturerOffset, productOffset, architecture == 1, globalNum == GlobalFileId);
                    }
                    else
                    {
                        // Still need mesgSize to skip data messages
                        int mesgSize = 0;
                        for (int f = 0; f < fieldCount; f++)
                            mesgSize += data[msgPos + 6 + f * 3 + 1];

                        localMesgDefs[localNum] = new LocalMesgLayout(mesgSize, null, null, false);
                    }

                    int devFieldsSize = 0;
                    if (hasDeveloperFields && msgPos + defSize < data.Length)
                    {
                        byte devFieldCount = data[msgPos + defSize];
                        devFieldsSize = 1 + devFieldCount * 3;
                    }

                    msgPos += defSize + devFieldsSize;
                }
                else
                {
                    // Data message
                    if (!localMesgDefs.TryGetValue(localNum, out LocalMesgLayout? layout))
                        break;

                    int dataStart = msgPos + 1;

                    // Read current manufacturer value.
                    ushort currentMfr = 0;
                    if (layout.ManufacturerOffset.HasValue && dataStart + layout.ManufacturerOffset.Value + 2 <= data.Length)
                    {
                        int mfrOffset = dataStart + layout.ManufacturerOffset.Value;
                        currentMfr = layout.BigEndian
                            ? (ushort)((data[mfrOffset] << 8) | data[mfrOffset + 1])
                            : BitConverter.ToUInt16(data, mfrOffset);
                    }

                    // FileId: always patch (it's the head unit record).
                    // DeviceInfo: only patch if it's the Wahoo head unit (mfr=32) — leave sensors/HRMs alone.
                    bool shouldPatch = layout.ManufacturerOffset.HasValue
                        && (layout.IsFileId || currentMfr == WahooManufacturer)
                        && currentMfr != GarminManufacturer;

                    if (shouldPatch && layout.ManufacturerOffset.HasValue && dataStart + layout.ManufacturerOffset.Value + 2 <= data.Length)
                    {
                        int offset = dataStart + layout.ManufacturerOffset.Value;
                        this.logger.LogDebug("Patching manufacturer at offset {Offset}: {OldMfr} → {NewMfr}", offset, currentMfr, GarminManufacturer);
                        if (layout.BigEndian)
                        {
                            data[offset] = (byte)(GarminManufacturer >> 8);
                            data[offset + 1] = (byte)(GarminManufacturer & 0xFF);
                        }
                        else
                        {
                            data[offset] = (byte)(GarminManufacturer & 0xFF);
                            data[offset + 1] = (byte)(GarminManufacturer >> 8);
                        }
                        modificationsCount++;
                    }

                    if (shouldPatch && layout.ProductOffset.HasValue && dataStart + layout.ProductOffset.Value + 2 <= data.Length)
                    {
                        int offset = dataStart + layout.ProductOffset.Value;
                        if (layout.BigEndian)
                        {
                            data[offset] = (byte)(GarminEdge830Product >> 8);
                            data[offset + 1] = (byte)(GarminEdge830Product & 0xFF);
                        }
                        else
                        {
                            data[offset] = (byte)(GarminEdge830Product & 0xFF);
                            data[offset + 1] = (byte)(GarminEdge830Product >> 8);
                        }
                        modificationsCount++;
                    }

                    msgPos += 1 + layout.MesgSize;
                }
            }

            // Recalculate CRC for this chained record
            int crcStart = pos;
            int crcLength = headerSize + (int)dataSize;
            if (crcStart + crcLength + 2 <= data.Length)
            {
                byte[] recordBytes = new byte[crcLength];
                Array.Copy(data, crcStart, recordBytes, 0, crcLength);
                ushort crc = CRC.Calc16(recordBytes, crcLength);
                data[crcStart + crcLength] = (byte)(crc & 0xFF);
                data[crcStart + crcLength + 1] = (byte)(crc >> 8);
            }

            pos = recordEnd;
        }

        this.logger.LogInformation("Patched Wahoo FIT file: {Count} fields modified in-place.", modificationsCount);
        return data;
    }

    private record LocalMesgLayout(int MesgSize, int? ManufacturerOffset, int? ProductOffset, bool BigEndian, bool IsFileId = false);
}
