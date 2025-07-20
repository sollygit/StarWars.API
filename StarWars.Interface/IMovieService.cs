using StarWars.Model;
using StarWars.Model.ViewModels;
using System.Threading.Tasks;

namespace StarWars.Interface
{
    public interface IMovieService
    {
        Task<MovieView[]> All();
        Task<MovieView> Get(string id);
        Task<MovieView> Create(Movie movie);
    }
}
