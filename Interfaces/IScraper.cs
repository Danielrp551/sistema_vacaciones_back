using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SISTEMA_VACACIONES.Models;

namespace SISTEMA_VACACIONES.Interfaces
{
    public interface IScraper
    {
        string SourceName { get; }
        Task<List<SearchResult>> ScrapeAsync(string entityName);
    }
}