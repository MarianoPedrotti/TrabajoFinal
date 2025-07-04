using System.Text.Json;

namespace TrabajoPractico.Service
{
    public class CriptoYaService
    {
        private readonly HttpClient _httpClient;

        public CriptoYaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> ObtenerCotizacion(string cryptoCode)
        {
            try
            {
                string url = $"https://criptoya.com/api/satoshitango/{cryptoCode}/ars";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Error al obtener la cotización: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);

                return json.RootElement.GetProperty("totalAsk").GetDecimal();
            }
            catch (HttpRequestException httpEx)
            {
                throw new ApplicationException("Error de red al acceder a la API de CriptoYa.", httpEx);
            }
            catch (JsonException jsonEx)
            {
                throw new ApplicationException("Error al parsear la respuesta de CriptoYa.", jsonEx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error desconocido al obtener la cotización.", ex);
            }
        }
    }
    }
