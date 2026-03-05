namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

public class FitModifierFactory(
    IFitModifier sdkModifier,
    IWahooFitModifier wahooModifier
) : IFitModifierFactory
{
    private readonly IFitModifier sdkModifier = sdkModifier;
    private readonly IWahooFitModifier wahooModifier = wahooModifier;

    public IFitModifier GetModifier(string source) => source == "Wahoo"
        ? this.wahooModifier
        : this.sdkModifier;
}
