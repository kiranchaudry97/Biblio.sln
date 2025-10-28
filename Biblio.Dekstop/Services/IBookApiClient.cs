using System.Collections.Generic;
using System.Threading.Tasks;
using Biblio.Models.Entities;

namespace Biblio.Dekstop.Services;

public interface IBookApiClient
{
 Task<IEnumerable<Book>> GetAllAsync();
 Task<Book?> GetAsync(int id);
 Task<Book> CreateAsync(Book book);
 Task UpdateAsync(int id, Book book);
 Task DeleteAsync(int id);
}
