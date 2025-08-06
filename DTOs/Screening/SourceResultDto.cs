using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SISTEMA_VACACIONES.Models;

namespace SISTEMA_VACACIONES.DTOs.Screening
{
    public class SourceResultDto
    {
        public string SourceName { get; set; }
        public int NumberOfHits { get; set; }
        public List<SearchResult> Results { get; set; }          
    }
}