namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

public interface IFitModifierFactory
{
    IFitModifier GetModifier(string source);
}
