using System.Linq.Expressions;

namespace MagicVilla_API.Repositorio.IRepositorio
{
    // Interfaz de repositorio genérico
    public interface IRepositorio<T> where T : class
    {
        Task Crear(T entidad);

        // el ? es para que en caso de que no envíe filtro, sólo devolverá la lista
        Task<List<T>> ObtenerTodos(Expression<Func<T, bool>>? filtro = null );
        Task<T> Obtener(Expression<Func<T, bool>>? filtro = null, bool tracked = true);
        Task Remover(T entidad);
        Task Grabar(); //método que se encargará del SaveChaged()
    }
}
