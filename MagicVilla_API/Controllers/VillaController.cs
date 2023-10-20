﻿using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        private readonly ApplicationDbContext _db;
        public VillaController(ILogger<VillaController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        //public IEnumerable<VillaDTO> GetVillas()
        [ProducesResponseType(StatusCodes.Status200OK)]
        //public async ActionResult<IEnumerable<VillaDTO>> GetVillas()
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            _logger.LogInformation("Obtener las Villas"); // línea nueva
            //return VillaStore.villaLista;
            //return Ok(VillaStore.villaLista);
            /*return Ok(_db.Villas.ToList());*/ // traemos todas las villas
            return Ok(await _db.Villas.ToListAsync());
        }

        //[HttpGet("id:int")]
        // Documentación de los estados
        //[ProducesResponseType(200)]
        [HttpGet("id:int", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public VillaDTO GetVilla(int id)
        //public ActionResult<VillaDTO> GetVilla(int id)
        public async Task<ActionResult<VillaDTO>> GetVilla(int id)
        {
            if (id == 0)
            {
                _logger.LogError("Error al traer vill con Id " + id); // linea nueva
                return BadRequest(); //  código de estado 400
            }
            //var villa = VillaStore.villaLista.FirstOrDefault(v => v.Id == id);
            //var villa = _db.Villas.FirstOrDefault(v => v.Id == id);
            var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id); // asincrono

            if (villa == null)
            {
                return NotFound(); // 404
            }

            return Ok(villa);
            //return Ok(VillaStore.villaLista.FirstOrDefault(v=> v.Id == id));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // cada vez que se agrega un nuevo registro
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public ActionResult<VillaDTO> CrearVilla([FromBody] VillaDTO villaDTO)
        //public ActionResult<VillaDTO> CrearVilla([FromBody] VillaCreateDTO villaDTO)
        public async Task<ActionResult<VillaDTO>> CrearVilla([FromBody] VillaCreateDTO villaDTO)
        {
            if (!ModelState.IsValid) // si el modelo es válido, si no se cumplen las dataanotaction
            {
                return BadRequest(ModelState);
            }
            // validación personalizada, comprobar que no ingresen nombres repetidas
            // if (VillaStore.villaLista.FirstOrDefault(v => v.Nombre.ToLower() == villaDTO.Nombre.ToLower()) != null)
            //if(_db.Villas.FirstOrDefault(v => v.Nombre.ToLower() == villaDTO.Nombre.ToLower()) != null)
            if (await _db.Villas.FirstOrDefaultAsync(v => v.Nombre.ToLower() == villaDTO.Nombre.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "La Villa con ese nombre ya existe"); //("nombre validación", "Mensaje a mostrar")
                return BadRequest(ModelState);
            }


            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }

            //if (villaDTO.Id > 0)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

            //villaDTO.Id = VillaStore.villaLista.OrderByDescending(v => v.Id).First().Id + 1;
            //VillaStore.villaLista.Add(villaDTO);

            Villa modelo = new()
            {
                //Id = villaDTO.Id,
                Nombre = villaDTO.Nombre,
                Detalle = villaDTO.Detalle,
                ImagenUrl = villaDTO.ImagenUrl,
                Ocupantes = villaDTO.Ocupantes,
                Tarifa = villaDTO.Tarifa,
                MetrosCuadrados = villaDTO.MetrosCuadrados,
                Amenidad = villaDTO.Amenidad,
            };

            // agregar registro a la base de datos
            //_db.Villas.Add(modelo);
            await _db.Villas.AddAsync(modelo); // asincrono
            await _db.SaveChangesAsync();

            //return Ok(villaDTO);
            //return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
            return CreatedAtRoute("GetVilla", new { id = modelo.Id }, modelo);
        }

        [HttpDelete("{id:int}")]
        // se utiliza IActionResult la iterfaz no se necesita del modelo, porque debe retornar un NoContent
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id) 
        {
            if(id == 0)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaLista.FirstOrDefault(v=> v.Id == id);
            //var villa = _db.Villas.FirstOrDefault(v => v.Id == id);
            var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if (villa == null) 
            {
                return NotFound();
            }

            //VillaStore.villaLista.Remove(villa);
            _db.Villas.Remove(villa); // Remove no tiene método asíncrono
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO) 
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaDTO)
        {
            if(villaDTO == null || id != villaDTO.Id)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaLista.FirstOrDefault(v => v.Id == id);
            //villa.Nombre = villaDTO.Nombre;
            //villa.Ocupantes = villaDTO.Ocupantes;
            //villa.MetrosCuadrados = villaDTO.MetrosCuadrados;

            Villa modelo = new()
            {
                Id = villaDTO.Id,
                Nombre = villaDTO.Nombre,
                Detalle = villaDTO.Detalle,
                ImagenUrl = villaDTO.ImagenUrl,
                Ocupantes = villaDTO.Ocupantes,
                Tarifa = villaDTO.Tarifa,
                MetrosCuadrados = villaDTO.MetrosCuadrados,
                Amenidad = villaDTO.Amenidad,
            };

            //_db.Update(modelo);
            _db.Villas.Update(modelo); // Update no tiene método asíncrono
            await _db.SaveChangesAsync() ;

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaLista.FirstOrDefault(v => v.Id == id);
            //var villa = _db.Villas.FirstOrDefault(v => v.Id == id);
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

            //VillaDTO villaDTO = new VillaDTO { }
            // antes de realizar la acción/cambio, que se almacene temporalmente en villaDTO
            VillaUpdateDTO villaDTO = new()
            {
                Id = villa.Id,
                Nombre = villa.Nombre,
                Detalle = villa.Detalle,
                ImagenUrl = villa.ImagenUrl,
                Ocupantes = villa.Ocupantes,
                Tarifa = villa.Tarifa,
                MetrosCuadrados = villa.MetrosCuadrados,
                Amenidad = villa.Amenidad,
            };

            if(villa == null) return BadRequest();
            
            //patchDTO.ApplyTo(villa, ModelState);
            patchDTO.ApplyTo(villaDTO, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // este modelo es el que se va a actualizar
            // el db set que hace referencia a Villas solo se le puede enviar con el mismo modelo algo actualizado
            Villa modelo = new()
            {
                Id = villaDTO.Id,
                Nombre = villaDTO.Nombre,
                Detalle = villaDTO.Detalle,
                ImagenUrl = villaDTO.ImagenUrl,
                Ocupantes = villaDTO.Ocupantes,
                Tarifa = villaDTO.Tarifa,
                MetrosCuadrados = villaDTO.MetrosCuadrados,
                Amenidad = villaDTO.Amenidad,
            };

            //
            _db.Villas.Update(modelo); // No es método asíncrono
            await _db.SaveChangesAsync();

            return NoContent();
        }

    }
}