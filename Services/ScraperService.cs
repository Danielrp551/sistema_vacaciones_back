using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SISTEMA_VACACIONES.DTOs.Screening;
using SISTEMA_VACACIONES.Interfaces;

namespace SISTEMA_VACACIONES.Services
{
    public class ScraperService : IScraperService
    {
        private readonly IEnumerable<IScraper> _scrapers;
        private readonly ILogger<ScraperService> _logger;
        public ScraperService(IEnumerable<IScraper> scrapers, ILogger<ScraperService> logger)
        {
            _scrapers = scrapers;
            _logger = logger;   
        }

        public async Task<ScreeningResponseDto> ScreenEntityAsync(string entityName)
        {
            _logger.LogInformation("Iniciando proceso de scraping para la entidad: {EntityName}", entityName);

            try
            {
                var tasks = _scrapers.Select(scraper =>
                {
                    _logger.LogInformation("Ejecutando scraper: {SourceName}", scraper.SourceName);
                    return scraper.ScrapeAsync(entityName).ContinueWith(task => new
                    {
                        SourceName = scraper.SourceName,
                        Results = task.Result
                    });
                });

                var scraperResults = await Task.WhenAll(tasks);

                var sources = scraperResults.Select(sr => new SourceResultDto
                {
                    SourceName = sr.SourceName,
                    NumberOfHits = sr.Results.Count,
                    Results = sr.Results
                }).ToList();

                _logger.LogInformation("Scraping completado para la entidad: {EntityName}", entityName);

                return new ScreeningResponseDto
                {
                    Sources = sources
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el proceso de scraping para la entidad: {EntityName}", entityName);
                throw;
            }
        }
    }
}