using MagicVilla_API;
using MagicVilla_API.Datos;
using MagicVilla_API.Repositorio;
using MagicVilla_API.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson(); // soporte para que se agregue al servicio
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// conecta la cadena de conexión con el DbContext,
// se debe implementar en la clase DbContext el constructor: 
// public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)  { }
builder.Services.AddDbContext<ApplicationDbContext>(option => 
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// servicio para el mapeo
builder.Services.AddAutoMapper(typeof(MappingConfig));

//builder.Services.AddSingleton
builder.Services.AddScoped<IVillaRepositorio, VillaRepositorio>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
