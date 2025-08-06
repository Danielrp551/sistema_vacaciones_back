using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SISTEMA_VACACIONES.DTOs.Screening;

namespace SISTEMA_VACACIONES.Interfaces
{
    public interface IScraperService
    {
        Task<ScreeningResponseDto> ScreenEntityAsync(string entityName);      
    }
}