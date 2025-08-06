using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SISTEMA_VACACIONES.Helpers;
using SISTEMA_VACACIONES.Interfaces;
using SISTEMA_VACACIONES.Models;

namespace SISTEMA_VACACIONES.Scrapers
{
    public class OFACScraper : IScraper
    {
        public string SourceName => "OFAC Sanctions List";

        public async Task<List<SearchResult>> ScrapeAsync(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("El nombre de la entidad no puede estar vacío.", nameof(entityName));

            // Ejecutamos la lógica de Selenium en un Task para no bloquear el hilo asíncrono.
            return await Task.Run(() =>
            {
                var results = new List<SearchResult>();

                // Configurar ChromeOptions para ejecución "headless" y otras optimizaciones
                var options = new ChromeOptions();
                options.AddArgument("--headless");        // Modo sin interfaz gráfica.
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--window-size=1200x800");
                options.AddArgument($"--user-agent={UserAgentProvider.GetRandomUserAgent()}");

                using var driver = new ChromeDriver(options);

                try
                {
                    // Navegar a la página OFAC
                    driver.Navigate().GoToUrl("https://sanctionssearch.ofac.treas.gov/");

                    // Esperar a que el input de búsqueda esté disponible
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                    wait.Until(d => d.FindElement(By.Id("ctl00_MainContent_txtLastName")));

                    // Ubicar el input de búsqueda, limpiar y enviar la entidad
                    var searchInput = driver.FindElement(By.Id("ctl00_MainContent_txtLastName"));
                    searchInput.Clear();
                    searchInput.SendKeys(entityName);

                    // Ubicar y hacer clic en el botón de búsqueda
                    var searchButton = driver.FindElement(By.Name("ctl00$MainContent$btnSearch"));
                    searchButton.Click();

                    // Esperar a que los resultados se carguen
                    wait.Until(d => d.FindElement(By.Id("gvSearchResults")));

                    // Obtener la tabla de resultados y recorrer las filas
                    var tableBody = driver.FindElement(By.CssSelector("div#scrollResults div table#gvSearchResults tbody"));
                    var rows = tableBody.FindElements(By.TagName("tr"));

                    foreach (var row in rows)
                    {
                        var cells = row.FindElements(By.TagName("td"));
                        if (cells.Count >= 6)
                        {
                            // Extraer y limpiar cada dato relevante
                            var entityNameNode = cells[0].FindElement(By.TagName("a"));
                            var name = entityNameNode.Text.Trim();      // Nombre de la entidad.
                            var address = cells[1].Text.Trim();           // Dirección.
                            var type = cells[2].Text.Trim();              // Tipo.
                            var programs = cells[3].Text.Trim();          // Programas.
                            var list = cells[4].Text.Trim();              // Lista.
                            var score = cells[5].Text.Trim();             // Puntaje.

                            // Usar la clase especializada SearchResultOFAC para el resultado
                            results.Add(new SearchResultOFAC
                            {
                                Source = SourceName,
                                Name = name,
                                Address = address,
                                Type = type,
                                Programs = programs,
                                List = list,
                                Score = score
                            });
                        }
                    }

                    Console.WriteLine($"[{SourceName}] Resultados encontrados: {results.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{SourceName}] Error durante el scraping: {ex.Message}");
                }
                finally
                {
                    driver.Quit();
                }

                return results;
            });
        }

    }
}