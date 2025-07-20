using StarWars.Model.ViewModels;
using System.Threading.Tasks;

namespace StarWars.Interface
{
    public interface IWebJetService
    {
        Task<MovieView[]> GetAll(string provider);
        Task<MovieRatingView> Get(string provider, string id);
    }
}
