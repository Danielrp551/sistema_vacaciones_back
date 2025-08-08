using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Data;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Repository
{
    public class PersonaRepository : IPersonaRepository
    {
        private readonly ApplicationDBContext _context;

        public PersonaRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Persona> GetByDniAsync(string dni)
        {
            return await _context.Personas.FirstOrDefaultAsync(p => p.Dni == dni);
        }

        public async Task AddAsync(Persona entity)
        {
            await _context.Personas.AddAsync(entity);
        }

        public async Task UpdateAsync(Persona entity)
        {
            _context.Personas.Update(entity);
        }

        public async Task DeleteAsync(Persona entity)
        {
            _context.Personas.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Persona>> GetAllAsync()
        {
            return await _context.Personas.ToListAsync();
        }

        public async Task<Persona> GetByIdAsync(int id)
        {
            return await _context.Personas.FindAsync(id);
        }

        public async Task<string> GetNombreByIdAsync(string id)
        {
            return null;
        }
    }
}