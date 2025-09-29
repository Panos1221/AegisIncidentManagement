using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IncidentManagement.Application.Services.Vessels;

public class AisStreamBackgroundService : BackgroundService
{
    private readonly ILogger<AisStreamBackgroundService> _logger;
    private readonly AisStreamConfig _config;
    private readonly IShipStore _store;

    public AisStreamBackgroundService(
        ILogger<AisStreamBackgroundService> logger,
        IOptions<AisStreamConfig> config,
        IShipStore store)
    {
        _logger = logger;
        _config = config.Value;
        _store = store;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check if AIS Stream is enabled
        if (!_config.Enabled)
        {
            _logger.LogInformation("AIS Stream service is disabled in configuration. Service will not start.");
            return;
        }

        var retryCount = 0;
        const int maxRetries = 5;
        var baseDelay = TimeSpan.FromSeconds(5);

        while (!stoppingToken.IsCancellationRequested && retryCount < maxRetries)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to AIS Stream (attempt {RetryCount}/{MaxRetries})...", retryCount + 1, maxRetries);
                
                using var client = new ClientWebSocket();
                await client.ConnectAsync(new Uri("wss://stream.aisstream.io/v0/stream"), stoppingToken);
                _logger.LogInformation("Connected to AIS Stream successfully");

                var subMessage = new
                {
                    APIKey = _config.ApiKey,
                    BoundingBoxes = _config.BoundingBoxes
                };
                var json = JsonSerializer.Serialize(subMessage);
                _logger.LogInformation("Sending subscription message: {Message}", json);
                await client.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, stoppingToken);
                _logger.LogInformation("Subscription message sent successfully. Waiting for messages...");
                
                // Reset retry count on successful connection
                retryCount = 0;
                
                var buffer = new byte[8192];
                while (!stoppingToken.IsCancellationRequested && client.State == WebSocketState.Open)
                {
                    try
                    {
                        var result = await client.ReceiveAsync(buffer, stoppingToken);
                        
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogWarning("WebSocket connection closed by remote party. Reason: {CloseStatus}, Description: {CloseStatusDescription}", 
                                result.CloseStatus, result.CloseStatusDescription);
                            break;
                        }
                        
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            try
                            {
                                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                using var doc = JsonDocument.Parse(message);

                                _logger.LogInformation("Received message from AIS Stream: {Response}", message);      

                                if (doc.RootElement.TryGetProperty("Message", out var msg)
                                    && msg.TryGetProperty("PositionReport", out var pos))
                                {
                                    var ship = new Ship
                                    {
                                        Mmsi = msg.GetProperty("UserID").GetString() ?? "",
                                        Latitude = pos.GetProperty("Latitude").GetDouble(),
                                        Longitude = pos.GetProperty("Longitude").GetDouble(),
                                        Speed = pos.TryGetProperty("Sog", out var sog) ? sog.GetDouble() : null,
                                        LastUpdate = DateTime.UtcNow
                                    };

                                    _store.UpdateShip(ship);
                                }
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogWarning(ex, "Failed to parse AIS message JSON. Message will be skipped.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Unexpected error while processing AIS message. Message will be skipped.");
                            }
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("AIS Stream service is shutting down due to cancellation request.");
                        break;
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogWarning(ex, "WebSocket error occurred while receiving data: {Message}", ex.Message);
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AIS Stream service is shutting down due to cancellation request.");
                break;
            }
            catch (WebSocketException ex)
            {
                retryCount++;
                var delay = TimeSpan.FromSeconds(baseDelay.TotalSeconds * Math.Pow(2, retryCount - 1));
                
                _logger.LogWarning(ex, "WebSocket connection failed (attempt {RetryCount}/{MaxRetries}): {Message}. " +
                    "Will retry in {DelaySeconds} seconds.", retryCount, maxRetries, ex.Message, delay.TotalSeconds);
                
                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Maximum retry attempts ({MaxRetries}) reached for AIS Stream connection. Service will stop.", maxRetries);
                    break;
                }
                
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("AIS Stream service is shutting down during retry delay.");
                    break;
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                var delay = TimeSpan.FromSeconds(baseDelay.TotalSeconds * Math.Pow(2, retryCount - 1));
                
                _logger.LogError(ex, "Unexpected error in AIS Stream service (attempt {RetryCount}/{MaxRetries}): {Message}. " +
                    "Will retry in {DelaySeconds} seconds.", retryCount, maxRetries, ex.Message, delay.TotalSeconds);
                
                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Maximum retry attempts ({MaxRetries}) reached for AIS Stream service. Service will stop.", maxRetries);
                    break;
                }
                
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("AIS Stream service is shutting down during retry delay.");
                    break;
                }
            }
        }
        
        _logger.LogInformation("AIS Stream background service has stopped.");
    }
}
