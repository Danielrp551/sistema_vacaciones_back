using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISTEMA_VACACIONES.Models
{
    public class SearchResultOFAC : SearchResult
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public string Programs { get; set; }
        public string List { get; set; }
        public string Score { get; set; }
    }
}