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
    public class WorldBankScraper : IScraper
    {
        public string SourceName => "World Bank Sanctioned Firms";

        public async Task<List<SearchResult>> ScrapeAsync(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("El nombre de la entidad no puede estar vacío.", nameof(entityName));

            // Ejecutamos la lógica de Selenium en un Task para no bloquear el hilo asíncrono.
            return await Task.Run(() =>
            {
                var results = new List<SearchResult>();

                // Configurar opciones de Chrome
                var options = new ChromeOptions();
                options.AddArgument("--headless"); // Ejecutar en modo headless (sin UI)
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument($"--user-agent={UserAgentProvider.GetRandomUserAgent()}");

                using var driver = new ChromeDriver(options);

                try
                {
                    // Navegar a la página de World Bank Sanctioned Firms.
                    driver.Navigate().GoToUrl("https://projects.worldbank.org/en/projects-operations/procurement/debarred-firms");

                    // Se espera que la tabla principal se cargue; se espera que el div contenedor esté presente.
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    wait.Until(d => d.FindElement(By.CssSelector("div.k-grid-content.k-auto-scrollable")));

                    Console.WriteLine("La tabla principal está lista. Ahora ingresaremos el texto en el input de búsqueda.");

                    // Esperar a que el input de búsqueda esté disponible.
                    wait.Until(d => d.FindElement(By.Id("category")));

                    var searchInput = driver.FindElement(By.Id("category"));
                    searchInput.Clear();
                    searchInput.SendKeys(entityName);

                    // Espera a que se cargue la tabla luego de ingresar la búsqueda.
                    wait.Until(d => d.FindElement(By.CssSelector("div.k-grid-content.k-auto-scrollable table[role='grid'] tbody[role='rowgroup']")));

                    // Ubicar el cuerpo de la tabla.
                    var tableBody = driver.FindElement(By.CssSelector("div.k-grid-content.k-auto-scrollable table[role='grid'] tbody[role='rowgroup']"));

                    // Seleccionar filas visibles (sin display:none)
                    var rows = tableBody.FindElements(By.XPath(".//tr[@role='row' and not(contains(@style,'display: none'))]"));

                    foreach (var row in rows)
                    {
                        var cells = row.FindElements(By.TagName("td"));
                        if (cells.Count >= 7)
                        {
                            var firmName = cells[0].Text.Trim();
                            var address = cells[2].Text.Trim();
                            var country = cells[3].Text.Trim();
                            var fromDate = cells[4].Text.Trim();
                            var toDate = cells[5].Text.Trim();
                            var grounds = cells[6].Text.Trim();

                            // Crear y agregar el objeto especializado SearchResultWorldBank.
                            results.Add(new SearchResultWorldBank
                            {
                                Source = SourceName,
                                FirmName = firmName,
                                Address = address,
                                Country = country,
                                fromDate = fromDate,
                                ToDate = toDate,
                                Grounds = grounds,
                            });
                        }
                        else
                        {
                            Console.WriteLine("Fila con número insuficiente de celdas encontrada.");
                        }
                    }

                    Console.WriteLine($"[{SourceName}] Total de resultados encontrados: {results.Count}");
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