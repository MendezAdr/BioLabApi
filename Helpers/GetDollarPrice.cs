using System.Text.Json.Serialization;
using System.Net.Http.Json;
namespace BioLabApi.Helpers;

public class GetDollarPrice
{
    public DolarResponse? CurrentRate { get; private set; }
    public bool IsSuccess { get; private set; } = false;
    public string ErrorMessage { get; private set; } = string.Empty;

    // Método que usará el worker para actualizar los datos
    public void UpdateRate(DolarResponse rate)
    {
        CurrentRate = rate;
        IsSuccess = true;
        ErrorMessage = string.Empty;
    }

    // Método para registrar si la API falló
    public void SetError(string message)
    {
        IsSuccess = false;
        ErrorMessage = message;
    }
}

public class DolarResponse
{
    [JsonPropertyName("promedio")]
    public decimal Promedio { get; set; }

    [JsonPropertyName("fechaActualizacion")]
    public DateTime FechaActualizacion { get; set; }
}



public class DollarUpdateWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GetDollarPrice _dollarHelper;
    private readonly ILogger<DollarUpdateWorker> _logger;

    public DollarUpdateWorker(
        IHttpClientFactory httpClientFactory, 
        GetDollarPrice dollarHelper,
        ILogger<DollarUpdateWorker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _dollarHelper = dollarHelper;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // El bucle se ejecutará mientras la aplicación esté viva
        while (!stoppingToken.IsCancellationRequested)
        {
            await FetchDollarPriceAsync(stoppingToken);

            // Calcular cuánto falta para las 6:05 PM del reloj del servidor
            var now = DateTime.Now;
            var nextUpdate = new DateTime(now.Year, now.Month, now.Day, 18, 5, 0);

            // Si ya pasaron las 6:05 PM de hoy, programar para mañana
            if (now > nextUpdate)
            {
                nextUpdate = nextUpdate.AddDays(1);
            }

            var delay = nextUpdate - now;
            _logger.LogInformation($"Siguiente actualización programada en: {delay.TotalHours} horas.");

            // Pausar el worker hasta que llegue la hora calculada
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task FetchDollarPriceAsync(CancellationToken stoppingToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Realizar la petición GET
            var response = await client.GetFromJsonAsync<DolarResponse>(
                "https://ve.dolarapi.com/v1/dolares/oficial", 
                stoppingToken);

            if (response != null)
            {
                _dollarHelper.UpdateRate(response);
                _logger.LogInformation($"Tasa actualizada con éxito: {response.Promedio}");
            }
            else
            {
                _dollarHelper.SetError("La respuesta de la API fue nula.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la tasa del dólar");
            _dollarHelper.SetError("No se pudo conectar con la API del dólar.");
        }
    }
}