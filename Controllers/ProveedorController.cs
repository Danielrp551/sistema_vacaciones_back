using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SISTEMA_VACACIONES.Data;
using SISTEMA_VACACIONES.DTOs.Proveedor;
using SISTEMA_VACACIONES.Helpers;
using SISTEMA_VACACIONES.Interfaces;
using SISTEMA_VACACIONES.Mappers;

namespace SISTEMA_VACACIONES.Controllers
{
    [Route("api/proveedor")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IProveedorRepository _proveedorRepository;

        public ProveedorController(ApplicationDBContext context, IProveedorRepository proveedorRepository)
        {
            _context = context;
            _proveedorRepository = proveedorRepository;
        }     

        [HttpGet] 
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] ProveedorQueryObject queryObject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var proveedores = await _proveedorRepository.GetAllAsync(queryObject);

            var proveedoresDto = proveedores.Select(p => p.ToProveedorDto()).ToList();

            return Ok(proveedoresDto);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var proveedor = await _proveedorRepository.GetByIdAsync(id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return Ok(proveedor.ToProveedorDto());            
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateProveedorRequestDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var proveedor = createDto.ToProveedorFromCreateDto();

            await _proveedorRepository.CreateAsync(proveedor);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = proveedor.Id }, 
                proveedor.ToProveedorDto()
            );            
        }  

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UpdateProveedorRequestDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var proveedor = await _proveedorRepository.UpdateAsync(id, updateDto);
            if(proveedor == null)
                return NotFound();

            return Ok(proveedor.ToProveedorDto());            
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var proveedor = await _proveedorRepository.DeleteAsync(id);
            if(proveedor == null)
                return NotFound();

            return NoContent();
        }        
    }
}