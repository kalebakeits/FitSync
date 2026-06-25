namespace FitSync.Wahoo.Shared.WahooClient.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Wahoo.Shared.AuthData;
using FitSync.Wahoo.Shared.Configuration;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using Microsoft.Extensions.Options;

public class WahooRequestFactory(
    IOptions<WahooClientOptions> options,
    IEncryptionService encryptionService
) : IWahooRequestFactory
{
    private readonly IOptions<WahooClientOptions> options = options;
    private readonly IEncryptionService encryptionService = encryptionService;

    public HttpRequestMessage BuildFetchWorkoutsRequest(Integration integration, string url)
    {
        WahooAuthData authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);
        HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authData.AccessToken
        );
        return request;
    }

    public HttpRequestMessage BuildPublishPlanRequest(
        Integration integration,
        WahooPlanDto plan,
        string externalId
    )
    {
        WahooAuthData authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);
        string url = $"{this.options.Value.BaseUrl.TrimEnd('/')}/v1/plans";
        byte[] planBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(plan));
        MultipartFormDataContent content = new();
        ByteArrayContent fileContent = new(planBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Add(fileContent, "plan[file]", "plan.json");
        content.Add(new StringContent(DateTime.UtcNow.ToString("O")), "plan[provider_updated_at]");
        content.Add(new StringContent(externalId), "plan[external_id]");
        HttpRequestMessage request = new(HttpMethod.Post, url) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authData.AccessToken
        );
        return request;
    }

    public HttpRequestMessage BuildUpdatePlanRequest(
        Integration integration,
        long planId,
        WahooPlanDto plan
    )
    {
        WahooAuthData authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);
        string url = $"{this.options.Value.BaseUrl.TrimEnd('/')}/v1/plans/{planId}";
        byte[] planBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(plan));
        MultipartFormDataContent content = new();
        ByteArrayContent fileContent = new(planBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Add(fileContent, "plan[file]", "plan.json");
        content.Add(new StringContent(DateTime.UtcNow.ToString("O")), "plan[provider_updated_at]");
        HttpRequestMessage request = new(HttpMethod.Put, url) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authData.AccessToken
        );
        return request;
    }

    public HttpRequestMessage BuildScheduleWorkoutRequest(
        Integration integration,
        long planId,
        string name,
        DateOnly scheduledDate,
        int durationMinutes
    )
    {
        WahooAuthData authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);
        string url = $"{this.options.Value.BaseUrl.TrimEnd('/')}/v1/workouts";
        string starts = scheduledDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToString("O");
        FormUrlEncodedContent content =
            new(
                new Dictionary<string, string>
                {
                    ["workout[name]"] = name,
                    ["workout[workout_token]"] = $"fitsync-{planId}",
                    ["workout[workout_type_id]"] = "0",
                    ["workout[starts]"] = starts,
                    ["workout[minutes]"] = durationMinutes.ToString(),
                    ["workout[plan_id]"] = planId.ToString(),
                }
            );
        HttpRequestMessage request = new(HttpMethod.Post, url) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authData.AccessToken
        );
        return request;
    }

    public HttpRequestMessage BuildRescheduleWorkoutRequest(
        Integration integration,
        long workoutId,
        DateOnly newDate
    )
    {
        WahooAuthData authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);
        string url = $"{this.options.Value.BaseUrl.TrimEnd('/')}/v1/workouts/{workoutId}";
        string starts = newDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToString("O");
        FormUrlEncodedContent content =
            new(new Dictionary<string, string> { ["workout[starts]"] = starts });
        HttpRequestMessage request = new(HttpMethod.Put, url) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authData.AccessToken
        );
        return request;
    }
}
