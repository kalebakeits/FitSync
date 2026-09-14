namespace FitSync.Shared.Features.ActivityIngest.Services;

using Dynastream.Fit;
using FitSync.Shared.Features.ActivityIngest.DTOs;
using Microsoft.Extensions.Logging;

public class FitSessionDecoder(ILogger<FitSessionDecoder> logger) : IFitSessionDecoder
{
    private readonly ILogger<FitSessionDecoder> logger = logger;

    public FitSessionStats? Decode(byte[] fitFileData)
    {
        this.logger.LogInformation(
            "Decoding FIT session stats from {ByteCount} bytes.",
            fitFileData.Length
        );

        if (fitFileData.Length == 0)
        {
            this.logger.LogWarning("Cannot decode FIT session stats from an empty file.");
            return null;
        }

        try
        {
            MemoryStream stream = new(fitFileData);
            Decode decoder = new();
            SessionMesg? session = null;

            decoder.MesgEvent += (sender, args) =>
            {
                if (Convert.ToInt32(args.mesg.Num) == Convert.ToInt32(MesgNum.Session))
                    session = new SessionMesg(args.mesg);
            };

            if (!decoder.IsFIT(stream) || !decoder.CheckIntegrity(stream))
            {
                this.logger.LogWarning("FIT file failed the format or integrity check.");
                return null;
            }

            stream.Position = 0;
            decoder.Read(stream);

            if (session is null)
            {
                this.logger.LogWarning("FIT file contains no session mesg.");
                return null;
            }

            FitSessionStats stats =
                new(
                    Sport: session.GetSport() is { } sport ? Convert.ToInt32(sport) : null,
                    DurationSeconds: session.GetTotalTimerTime() is { } timerTime
                        ? Convert.ToInt32(timerTime)
                        : session.GetTotalElapsedTime() is { } elapsedTime
                            ? Convert.ToInt32(elapsedTime)
                            : null,
                    DistanceMeters: session.GetTotalDistance() is { } distance
                        ? Convert.ToDouble(distance)
                        : null,
                    AvgHeartRate: session.GetAvgHeartRate() is { } heartRate
                        ? Convert.ToInt32(heartRate)
                        : null,
                    AvgPower: session.GetAvgPower() is { } power ? Convert.ToInt32(power) : null
                );

            this.logger.LogInformation(
                "Decoded FIT session: sport {Sport}, duration {DurationSeconds}s, distance {DistanceMeters}m, avg HR {AvgHeartRate}, avg power {AvgPower}.",
                stats.Sport,
                stats.DurationSeconds,
                stats.DistanceMeters,
                stats.AvgHeartRate,
                stats.AvgPower
            );

            return stats;
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Failed to decode FIT session stats.");
            return null;
        }
    }
}
