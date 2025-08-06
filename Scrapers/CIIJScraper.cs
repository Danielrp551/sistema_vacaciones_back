using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SISTEMA_VACACIONES.Helpers;
using SISTEMA_VACACIONES.Interfaces;
using SISTEMA_VACACIONES.Models;

namespace SISTEMA_VACACIONES.Scrapers
{
    public class CIIJScraper : IScraper
    {
        public string SourceName => "ICIJ Offshore Leaks";

        private static readonly Random _random = new Random();

        private readonly HttpClient _httpClient;

        public CIIJScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SearchResult>> ScrapeAsync(string entityName)
        {
            var results = new List<SearchResult>();
            int from = 0;
            bool hasMoreResults = true;
            int pageNumber = 1;

            while (hasMoreResults)
            {
                // Asignar un User-Agent aleatorio y otras cabeceras para simular una solicitud realista.
                string userAgent = UserAgentProvider.GetRandomUserAgent();
                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

                if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                }
                if (!_httpClient.DefaultRequestHeaders.Contains("Accept-Language"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                }
                if (!_httpClient.DefaultRequestHeaders.Contains("Referer"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Referer", "https://offshoreleaks.icij.org/");
                }

                // Construir la URL de búsqueda con el parámetro 'from'
                string relativeUrl = from == 0
                    ? $"/search?q={Uri.EscapeDataString(entityName)}&c=&j=&d="
                    : $"/search?q={Uri.EscapeDataString(entityName)}&c=&j=&d=&from={from}";

                var fullUri = new Uri(_httpClient.BaseAddress, relativeUrl);
                Console.WriteLine($"[Página {pageNumber}] Solicitando URL: {fullUri} con User-Agent: {userAgent}");

                // Realizar la petición GET
                var response = await _httpClient.GetAsync(relativeUrl);
                Console.WriteLine($"[Página {pageNumber}] Respuesta HTTP: {(int)response.StatusCode} {response.ReasonPhrase}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Página {pageNumber}] Error al acceder a ICIJ: {response.StatusCode}");
                    throw new Exception($"Error al acceder a ICIJ: {response.StatusCode}");
                }

                var html = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Página {pageNumber}] Longitud del HTML: {html.Length} caracteres.");

                if (string.IsNullOrEmpty(html))
                {
                    Console.WriteLine($"[Página {pageNumber}] Respuesta vacía recibida.");
                    break;
                }

                // Parsear el HTML sin guardarlo en disco
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Buscar la tabla de resultados por su clase
                var table = doc.DocumentNode.SelectSingleNode("//table[contains(@class, 'search__results__table')]");
                if (table == null)
                {
                    Console.WriteLine($"[Página {pageNumber}] Tabla de resultados no encontrada.");
                    break;
                }
                else
                {
                    Console.WriteLine($"[Página {pageNumber}] Tabla de resultados encontrada.");
                    var rows = table.SelectNodes(".//tbody/tr");
                    if (rows == null)
                    {
                        Console.WriteLine($"[Página {pageNumber}] No se encontraron filas en la tabla.");
                    }
                    else
                    {
                        Console.WriteLine($"[Página {pageNumber}] {rows.Count} filas encontradas.");
                        foreach (var row in rows)
                        {
                            var cells = row.SelectNodes(".//td");
                            if (cells != null && cells.Count >= 4)
                            {
                                // Extraer los datos
                                var nameNode = cells[0].SelectSingleNode(".//a");
                                var entity = nameNode != null ? nameNode.InnerText.Trim() : cells[0].InnerText.Trim();

                                var jurisdiction = cells[1].InnerText.Trim();
                                var linkedTo = cells[2].InnerText.Trim();

                                var dataFromNode = cells[3].SelectSingleNode(".//a");
                                var dataFrom = dataFromNode != null ? dataFromNode.GetAttributeValue("href", "").Trim() : "";

                                // Crear el objeto especializado SearchResultICIJ
                                results.Add(new SearchResultICIJ
                                {
                                    Source = SourceName,
                                    Entity = entity,
                                    Jurisdiction = jurisdiction,
                                    LinkedTo = linkedTo,
                                    DataFrom = dataFrom
                                });
                            }
                            else
                            {
                                Console.WriteLine($"[Página {pageNumber}] Fila con número de celdas insuficiente.");
                            }
                        }
                    }
                }

                // Comprobar si hay más resultados disponibles mediante la existencia del div 'more_results'
                var moreResultsDiv = doc.DocumentNode.SelectSingleNode("//div[@id='more_results']");
                if (moreResultsDiv != null)
                {
                    from += 100; // Incrementa 'from' para solicitar la siguiente página
                    pageNumber++;
                    Console.WriteLine($"[Página {pageNumber}] Más resultados disponibles. Incrementando 'from' a {from}.");

                    // Introducir un retraso aleatorio entre 1 y 3 segundos
                    int delaySeconds;
                    lock (_random)
                    {
                        delaySeconds = _random.Next(1, 3);
                    }
                    Console.WriteLine($"Esperando {delaySeconds} segundos antes de la siguiente solicitud...");
                    await Task.Delay(delaySeconds * 1000);
                }
                else
                {
                    hasMoreResults = false;
                    Console.WriteLine($"[Página {pageNumber}] No hay más resultados disponibles.");
                }
            }

            Console.WriteLine($"Total de resultados encontrados: {results.Count}");
            return results;
        }
    }
}