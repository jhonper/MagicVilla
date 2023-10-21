using AutoMapper;
using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.DTO;
using MagicVilla_API.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumeroVillaController : ControllerBase
    {
        private readonly ILogger<NumeroVillaController> _logger;
        //private readonly ApplicationDbContext _db;
        private readonly IVillaRepositorio _villaRepo;
        private readonly INumeroVillaRepositorio _numeroRepo;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        // Constructor
        //public VillaController(ILogger<VillaController> logger, ApplicationDbContext db, IMapper mapper)
        public NumeroVillaController(ILogger<NumeroVillaController> logger, IVillaRepositorio villaRepo, 
                                                        INumeroVillaRepositorio numeroRepo, IMapper mapper)
        {
            _logger = logger;
            //_db = db;
            _villaRepo = villaRepo;
            _numeroRepo = numeroRepo;
            _mapper = mapper;
            _response = new(); // new APIResponse();
        }

        [HttpGet]
        //public IEnumerable<VillaDTO> GetVillas()
        [ProducesResponseType(StatusCodes.Status200OK)]
        //public async ActionResult<IEnumerable<VillaDTO>> GetVillas()
        //public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        public async Task<ActionResult<APIResponse>> GetNumeroVillas()
        {
            try
            {
                _logger.LogInformation("Obtenener números Villas"); // línea nueva
                                                              //return VillaStore.villaLista;
                                                              //return Ok(VillaStore.villaLista);
                /*return Ok(_db.Villas.ToList());*/ // traemos todas las villas

                //IEnumerable<Villa> villaList = await _db.Villas.ToListAsync(); // obtengo las villas 
                IEnumerable<NumeroVilla> numeroVillaList = await _numeroRepo.ObtenerTodos();

                _response.Resultado = _mapper.Map<IEnumerable<NumeroVillaDTO>>(numeroVillaList); // linea nueva
                _response.statusCode = HttpStatusCode.OK; // respuesta que se basa en APIResponse

                //return Ok(await _db.Villas.ToListAsync()); // antes de automapper
                return Ok(_mapper.Map<IEnumerable<NumeroVillaDTO>>(numeroVillaList));
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }     
            return _response;
        }

        //[HttpGet("id:int")]
        // Documentación de los estados
        //[ProducesResponseType(200)]
        [HttpGet("id:int", Name = "GetNumeroVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public VillaDTO GetVilla(int id)
        //public ActionResult<VillaDTO> GetVilla(int id)
        //public async Task<ActionResult<VillaDTO>> GetVilla(int id)
        public async Task<ActionResult<APIResponse>> GetNumeroVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer número villa con Id " + id); // linea nueva
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false; 
                    return BadRequest(_response);
                    /*return BadRequest();*/ //  código de estado 400
                }

                //var villa = VillaStore.villaLista.FirstOrDefault(v => v.Id == id);
                //var villa = _db.Villas.FirstOrDefault(v => v.Id == id);
                //var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);*/ // asincrono
                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);

                if (numeroVilla == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    return NotFound(_response); // 404
                }

                _response.Resultado = _mapper.Map<NumeroVillaDTO>(numeroVilla);
                _response.statusCode = HttpStatusCode.OK;

                //return Ok(villa); // antes de automapper
                //return Ok(VillaStore.villaLista.FirstOrDefault(v=> v.Id == id));
                //return Ok(_mapper.Map<VillaDTO>(villa));
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // cada vez que se agrega un nuevo registro
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public ActionResult<VillaDTO> CrearVilla([FromBody] VillaDTO villaDTO)
        //public ActionResult<VillaDTO> CrearVilla([FromBody] VillaCreateDTO villaDTO)
        //public async Task<ActionResult<VillaDTO>> CrearVilla([FromBody] VillaCreateDTO villaDTO)
        //public async Task<ActionResult<VillaDTO>> CrearVilla([FromBody] VillaCreateDTO createDTO)
        public async Task<ActionResult<APIResponse>> CrearNumeroVilla([FromBody] NumeroVillaCreateDTO createDTO)
        {
            try
            {
                if (!ModelState.IsValid) // si el modelo es válido, si no se cumplen las dataanotaction
                {
                    return BadRequest(ModelState);
                }

                // validación personalizada, comprobar que no ingresen nombres repetidas
                // if (VillaStore.villaLista.FirstOrDefault(v => v.Nombre.ToLower() == villaDTO.Nombre.ToLower()) != null)
                //if(_db.Villas.FirstOrDefault(v => v.Nombre.ToLower() == villaDTO.Nombre.ToLower()) != null)
                //if (await _db.Villas.FirstOrDefaultAsync(v => v.Nombre.ToLower() == createDTO.Nombre.ToLower()) != null)
                if (await _numeroRepo.Obtener(v => v.VillaNo == createDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("NombreExiste", "El  número Villa ya existe"); //("nombre validación", "Mensaje a mostrar")
                    return BadRequest(ModelState);
                }

                if (createDTO == null)
                {
                    return BadRequest(createDTO);
                }

                //if (villaDTO.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}

                //villaDTO.Id = VillaStore.villaLista.OrderByDescending(v => v.Id).First().Id + 1;
                //VillaStore.villaLista.Add(villaDTO);


                // *** antes del automapper
                //Villa modelo = new()
                //{
                //    //Id = villaDTO.Id,
                //    Nombre = villaDTO.Nombre,
                //    Detalle = villaDTO.Detalle,
                //    ImagenUrl = villaDTO.ImagenUrl,
                //    Ocupantes = villaDTO.Ocupantes,
                //    Tarifa = villaDTO.Tarifa,
                //    MetrosCuadrados = villaDTO.MetrosCuadrados,
                //    Amenidad = villaDTO.Amenidad,
                //};

                if(await _villaRepo.Obtener(v => v.Id == createDTO.VillaId) == null)
                {
                    ModelState.AddModelError("ClaveForanea", "El Id de la villa no existe"); //("nombre validación", "Mensaje a mostrar")
                    return BadRequest(ModelState);
                }

                NumeroVilla modelo = _mapper.Map<NumeroVilla>(createDTO); // este código reemplaza al modelo anterior

                // agregar registro a la base de datos
                //_db.Villas.Add(modelo);
                //await _db.Villas.AddAsync(modelo); // asincrono
                //await _db.SaveChangesAsync(); // el SaveChanges ya viene incorporado en el Crear
                modelo.FechaCreacion = DateTime.Now; // para guardar la fecha de sistema real en la base de datos linea nueva
                modelo.FechaActualizacion = DateTime.Now; // para guardar la fecha de sistema real en la base de datos linea nueva

                await _numeroRepo.Crear(modelo);
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                //return Ok(villaDTO);
                //return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
                //return CreatedAtRoute("GetVilla", new { id = modelo.Id }, modelo);
                return CreatedAtRoute("GetNumeroVilla", new { id = modelo.VillaNo }, _response);

            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString()};
            }

            return _response;
        }

        [HttpDelete("{id:int}")]
        // se utiliza IActionResult la iterfaz no se necesita del modelo, porque debe retornar un NoContent
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> DeleteVilla(int id) // *** No necesita mapeo
        // en el delete se utiliza IActionResult e interfaces, pero 
        // las interfaces no puede llevar un tipo( Task<IActionResult<APIResponse>> )
        // entonces no se debe colocar el <APIResponse> y colocar en BadRequest(_response)
        public async Task<IActionResult> DeleteNumeroVilla(int id) // *** No necesita mapeo
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false; // linea nueva
                    _response.statusCode = HttpStatusCode.BadRequest; // linea nueva
                    //return BadRequest();
                     return BadRequest(_response); // // linea nueva
                }
                //var villa = VillaStore.villaLista.FirstOrDefault(v=> v.Id == id);
                //var villa = _db.Villas.FirstOrDefault(v => v.Id == id);
                //var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);

                if (numeroVilla == null)
                {
                    _response.IsExitoso = false; // linea nueva
                    _response.statusCode = HttpStatusCode.NotFound; // linea nueva
                    // return NotFound(); 
                    return NotFound(_response); // linea nueva
                }

                //VillaStore.villaLista.Remove(villa);
                /*_db.Villas.Remove(villa);*/ // Remove no tiene método asíncrono
                await _numeroRepo.Remover(numeroVilla);

                _response.statusCode = HttpStatusCode.NoContent;
                //await _db.SaveChangesAsync(); // ya viene incorporado

                //return NoContent(); // NoContent() no permite valores
                return Ok(_response); // linea nueva
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return BadRequest(_response);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO) 
        //public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaDTO)
        public async Task<IActionResult> UpdateNumeroVilla(int id, [FromBody] NumeroVillaUpdateDTO updateDTO)
        {
            if(updateDTO == null || id != updateDTO.VillaNo)
            {
                _response.IsExitoso = false; // linea nueva
                _response.statusCode = HttpStatusCode.BadRequest; // linea nueva
                //return BadRequest();
                return BadRequest(_response);
            }
            //var villa = VillaStore.villaLista.FirstOrDefault(v => v.Id == id);
            //villa.Nombre = villaDTO.Nombre;
            //villa.Ocupantes = villaDTO.Ocupantes;
            //villa.MetrosCuadrados = villaDTO.MetrosCuadrados;

            // *** modelo sin mapper
            //Villa modelo = new()
            //{
            //    Id = villaDTO.Id,
            //    Nombre = villaDTO.Nombre,
            //    Detalle = villaDTO.Detalle,
            //    ImagenUrl = villaDTO.ImagenUrl,
            //    Ocupantes = villaDTO.Ocupantes,
            //    Tarifa = villaDTO.Tarifa,
            //    MetrosCuadrados = villaDTO.MetrosCuadrados,
            //    Amenidad = villaDTO.Amenidad,
            //};

            if(await _villaRepo.Obtener(v => v.Id == updateDTO.VillaId) == null) // si el null, el id padre que nos envian no existe
            {
                ModelState.AddModelError("ClaveForanea", "El Id de la Villa no existe");
                return BadRequest(ModelState);   
            }

            NumeroVilla modelo = _mapper.Map<NumeroVilla>(updateDTO); // este código reemplaza al modelo anterior 

            //_db.Update(modelo);
            /*_db.Villas.Update(modelo);*/ // Update no tiene método asíncrono
            //await _db.SaveChangesAsync() ; // ya viene incorporado
            await _numeroRepo.Actualizar(modelo);
            _response.statusCode = HttpStatusCode.NoContent;

            //return NoContent();
            return Ok(_response);
        }

        
    }
}
