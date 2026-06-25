namespace FitSync.Garmin.Shared.GarminClient.Services;

using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using FitSync.Garmin.Shared.GarminClient.DTOs;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using OAuth;

public partial class GarminApiClient(ILogger<GarminApiClient> logger) : IGarminApiClient
{
    private readonly ILogger<GarminApiClient> logger = logger;

    private const string SsoEmbedUrl = "https://sso.garmin.com/sso/embed";
    private const string SsoSignInUrl = "https://sso.garmin.com/sso/signin";
    private const string OAuth1TokenUrl =
        "https://connectapi.garmin.com/oauth-service/oauth/preauthorized";
    private const string OAuth2TokenUrl =
        "https://connectapi.garmin.com/oauth-service/oauth/exchange/user/2.0";
    private const string UploadUrl = "https://connectapi.garmin.com/upload-service/upload";
    private const string WorkoutServiceUrl =
        "https://connectapi.garmin.com/workout-service/workout";
    private const string ScheduleServiceUrl =
        "https://connectapi.garmin.com/workout-service/schedule";
    private const string ConsumerCredentialsUrl =
        "https://thegarth.s3.amazonaws.com/oauth_consumer.json";
    private const string UserAgent = "GCM-iOS-5.7.2.1";
    private const string Origin = "https://sso.garmin.com";
    private const string OAuth1LoginUrlParam =
        "https://sso.garmin.com/sso/embed&accepts-mfa-tokens=true";

    private static readonly object CommonQueryParams = new
    {
        id = "gauth-widget",
        embedWidget = "true",
        gauthHost = SsoEmbedUrl,
        redirectAfterAccountCreationUrl = SsoEmbedUrl,
        redirectAfterAccountLoginUrl = SsoEmbedUrl,
        service = SsoEmbedUrl,
        source = SsoEmbedUrl,
    };

    public async Task<long> CreateWorkoutAsync(
        string workoutJson,
        string accessToken,
        CancellationToken ct
    )
    {
        this.logger.LogInformation("Creating Garmin workout.");
        this.logger.LogDebug("Garmin workout JSON: {Json}", workoutJson);

        IFlurlResponse response = await WorkoutServiceUrl
            .WithOAuthBearerToken(accessToken)
            .WithHeader("NK", "NT")
            .WithHeader("origin", Origin)
            .WithHeader("User-Agent", UserAgent)
            .WithHeader("Content-Type", "application/json")
            .AllowAnyHttpStatus()
            .PostStringAsync(workoutJson, cancellationToken: ct);

        if (response.StatusCode >= 400)
        {
            string errorBody = await response.GetStringAsync();
            this.logger.LogError(
                "Garmin CreateWorkout failed with {StatusCode}: {Body}",
                response.StatusCode,
                errorBody
            );
            throw new InvalidOperationException(
                $"Garmin CreateWorkout failed ({response.StatusCode}): {errorBody}"
            );
        }

        System.Text.Json.JsonElement created =
            await response.GetJsonAsync<System.Text.Json.JsonElement>();
        long workoutId = created.GetProperty("workoutId").GetInt64();

        this.logger.LogInformation("Created Garmin workout {WorkoutId}.", workoutId);
        return workoutId;
    }

    public async Task<long> ScheduleWorkoutAsync(
        long workoutId,
        DateOnly date,
        string accessToken,
        CancellationToken ct
    )
    {
        this.logger.LogInformation(
            "Scheduling Garmin workout {WorkoutId} on {Date}.",
            workoutId,
            date
        );

        IFlurlResponse scheduleResponse = await $"{ScheduleServiceUrl}/{workoutId}"
            .WithOAuthBearerToken(accessToken)
            .WithHeader("NK", "NT")
            .WithHeader("origin", Origin)
            .WithHeader("User-Agent", UserAgent)
            .PostJsonAsync(new { date = date.ToString("yyyy-MM-dd") }, cancellationToken: ct);

        System.Text.Json.JsonElement scheduleResult =
            await scheduleResponse.GetJsonAsync<System.Text.Json.JsonElement>();
        long workoutScheduleId = scheduleResult.GetProperty("workoutScheduleId").GetInt64();

        this.logger.LogInformation(
            "Scheduled Garmin workout {WorkoutId} on {Date} (scheduleId={ScheduleId}).",
            workoutId,
            date,
            workoutScheduleId
        );

        return workoutScheduleId;
    }

    public async Task RescheduleWorkoutAsync(
        long workoutScheduleId,
        DateOnly newDate,
        string accessToken,
        CancellationToken ct
    )
    {
        this.logger.LogInformation(
            "Rescheduling Garmin schedule entry {ScheduleId} to {Date}.",
            workoutScheduleId,
            newDate
        );

        IFlurlResponse response = await $"{ScheduleServiceUrl}/{workoutScheduleId}"
            .WithOAuthBearerToken(accessToken)
            .WithHeader("NK", "NT")
            .WithHeader("origin", Origin)
            .WithHeader("User-Agent", UserAgent)
            .AllowAnyHttpStatus()
            .PutJsonAsync(
                new { calendarDate = newDate.ToString("yyyy-MM-dd") },
                cancellationToken: ct
            );

        if (response.StatusCode >= 400)
        {
            string body = await response.GetStringAsync();
            throw new InvalidOperationException(
                $"Garmin RescheduleWorkout failed ({response.StatusCode}): {body}"
            );
        }

        this.logger.LogInformation(
            "Rescheduled Garmin schedule entry {ScheduleId} to {Date}.",
            workoutScheduleId,
            newDate
        );
    }

    public async Task<CookieJar> InitCookieJarAsync(CancellationToken ct)
    {
        this.logger.LogInformation("Initializing Garmin SSO cookie jar.");

        await SsoEmbedUrl
            .WithHeader("User-Agent", UserAgent)
            .WithHeader("origin", Origin)
            .SetQueryParams(CommonQueryParams)
            .WithCookies(out CookieJar jar)
            .GetStringAsync(cancellationToken: ct);

        return jar;
    }

    public async Task<string> GetCsrfTokenAsync(CookieJar jar, CancellationToken ct)
    {
        this.logger.LogInformation("Fetching Garmin CSRF token.");

        string rawBody = await SsoSignInUrl
            .WithHeader("User-Agent", UserAgent)
            .WithHeader("origin", Origin)
            .SetQueryParams(CommonQueryParams)
            .WithCookies(jar)
            .GetAsync(cancellationToken: ct)
            .ReceiveString();

        Match match = CsrfTokenRegex().Match(rawBody);
        if (!match.Success)
            throw new InvalidOperationException(
                $"Failed to find CSRF token in Garmin SSO response. Body length: {rawBody.Length}"
            );

        this.logger.LogInformation("CSRF token obtained.");
        return match.Groups["csrf"].Value;
    }

    public async Task<SendCredentialsResult> SendCredentialsAsync(
        string email,
        string password,
        string csrfToken,
        CookieJar jar,
        CancellationToken ct
    )
    {
        this.logger.LogInformation("Sending credentials to Garmin SSO.");

        bool wasRedirected = false;
        string redirectedTo = string.Empty;

        string rawResponseBody = await SsoSignInUrl
            .WithHeader("User-Agent", UserAgent)
            .WithHeader("origin", Origin)
            .WithHeader("NK", "NT")
            .SetQueryParams(CommonQueryParams)
            .WithCookies(jar)
            .OnRedirect(r =>
            {
                wasRedirected = true;
                redirectedTo = r.Redirect.Url;
            })
            .PostUrlEncodedAsync(
                new
                {
                    username = email,
                    password = password,
                    embed = "true",
                    _csrf = csrfToken
                },
                cancellationToken: ct
            )
            .ReceiveString();

        return new SendCredentialsResult(wasRedirected, redirectedTo, rawResponseBody);
    }

    public Task<ConsumerCredentials> GetConsumerCredentialsAsync(CancellationToken ct)
    {
        this.logger.LogInformation("Fetching Garmin OAuth consumer credentials.");
        return ConsumerCredentialsUrl.GetJsonAsync<ConsumerCredentials>(cancellationToken: ct);
    }

    public async Task<(string token, string secret)> GetOAuth1TokenAsync(
        string ticket,
        ConsumerCredentials credentials,
        CancellationToken ct
    )
    {
        this.logger.LogInformation("Fetching OAuth1 token.");

        OAuthRequest oauthClient = OAuthRequest.ForRequestToken(
            credentials.ConsumerKey,
            credentials.ConsumerSecret
        );
        oauthClient.RequestUrl =
            $"{OAuth1TokenUrl}?ticket={ticket}&login-url={OAuth1LoginUrlParam}";

        string response = await oauthClient
            .RequestUrl.WithHeader("User-Agent", UserAgent)
            .WithHeader("Authorization", oauthClient.GetAuthorizationHeader())
            .GetStringAsync(cancellationToken: ct);

        System.Collections.Specialized.NameValueCollection queryParams =
            HttpUtility.ParseQueryString(response);
        string oauthToken =
            queryParams.Get("oauth_token")
            ?? throw new InvalidOperationException(
                $"oauth_token missing in OAuth1 response: {response}"
            );
        string oauthTokenSecret =
            queryParams.Get("oauth_token_secret")
            ?? throw new InvalidOperationException(
                $"oauth_token_secret missing in OAuth1 response: {response}"
            );

        this.logger.LogInformation("OAuth1 token obtained.");
        return (oauthToken, oauthTokenSecret);
    }

    public async Task<GarminOAuth2Token> GetOAuth2TokenAsync(
        string oauth1Token,
        string oauth1Secret,
        ConsumerCredentials credentials,
        CancellationToken ct
    )
    {
        this.logger.LogInformation("Exchanging OAuth1 for OAuth2 token.");

        OAuthRequest oauthClient = OAuthRequest.ForProtectedResource(
            "POST",
            credentials.ConsumerKey,
            credentials.ConsumerSecret,
            oauth1Token,
            oauth1Secret
        );
        oauthClient.RequestUrl = OAuth2TokenUrl;

        GarminOAuth2Token token = await oauthClient
            .RequestUrl.WithHeader("User-Agent", UserAgent)
            .WithHeader("Authorization", oauthClient.GetAuthorizationHeader())
            .WithHeader("Content-Type", "application/x-www-form-urlencoded")
            .PostUrlEncodedAsync(new object(), cancellationToken: ct)
            .ReceiveJson<GarminOAuth2Token>();

        this.logger.LogInformation(
            "OAuth2 token obtained. Expires in {ExpiresIn}s.",
            token.ExpiresIn
        );
        return token;
    }

    public async Task<UploadResult> UploadActivityAsync(
        byte[] fitFileData,
        string accessToken,
        CancellationToken ct
    )
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"fit_{Guid.NewGuid()}.fit");
        await File.WriteAllBytesAsync(tempFile, fitFileData, ct);

        try
        {
            this.logger.LogInformation(
                "Uploading activity to Garmin ({Bytes} bytes).",
                fitFileData.Length
            );

            using IFlurlClient uploadClient = new FlurlClient();
            using IFlurlResponse response = await uploadClient
                .Request($"{UploadUrl}/.fit")
                .WithOAuthBearerToken(accessToken)
                .WithHeader("NK", "NT")
                .WithHeader("origin", Origin)
                .WithHeader("User-Agent", UserAgent)
                .AllowHttpStatus("2xx,409")
                .PostMultipartAsync(
                    data =>
                        data.AddFile(
                            "\"file\"",
                            path: tempFile,
                            contentType: "application/octet-stream",
                            fileName: $"\"{Path.GetFileName(tempFile)}\""
                        ),
                    cancellationToken: ct
                );

            int statusCode = response.StatusCode;

            if (statusCode == (int)HttpStatusCode.Conflict)
            {
                this.logger.LogInformation(
                    "Activity already uploaded (409 Conflict — treated as success)."
                );
                return UploadResult.Succeeded();
            }

            if (statusCode >= 200 && statusCode <= 299)
            {
                this.logger.LogInformation(
                    "Activity uploaded successfully (HTTP {StatusCode}).",
                    statusCode
                );
                return UploadResult.Succeeded();
            }

            this.logger.LogError("Unexpected upload status code {StatusCode}.", statusCode);
            return UploadResult.Failed(
                $"Unexpected status code: {statusCode}",
                (HttpStatusCode)statusCode
            );
        }
        catch (FlurlHttpException ex)
        {
            this.logger.LogError(
                ex,
                "HTTP error uploading to Garmin: {StatusCode}.",
                ex.StatusCode
            );
            return UploadResult.Failed(
                ex.Message,
                ex.StatusCode.HasValue ? (HttpStatusCode)ex.StatusCode.Value : null
            );
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [GeneratedRegex(@"name=""_csrf""\s+value=""(?<csrf>.+?)""")]
    private static partial Regex CsrfTokenRegex();
}
