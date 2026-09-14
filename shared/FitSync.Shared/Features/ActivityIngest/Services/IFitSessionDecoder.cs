namespace FitSync.Shared.Features.ActivityIngest.Services;

using FitSync.Shared.Features.ActivityIngest.DTOs;

public interface IFitSessionDecoder
{
    FitSessionStats? Decode(byte[] fitFileData);
}
