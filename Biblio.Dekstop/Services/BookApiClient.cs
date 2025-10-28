using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Biblio.Models.Entities;

namespace Biblio.Dekstop.Services;

public class BookApiClient : IBookApiClient
{
 private readonly HttpClient _http;

 public BookApiClient(HttpClient http)
 {
 _http = http;
 }

 public async Task<IEnumerable<Book>> GetAllAsync()
 {
 var res = await _http.GetFromJsonAsync<IEnumerable<Book>>("api/books");
 return res ?? new List<Book>();
 }

 public async Task<Book?> GetAsync(int id)
 {
 return await _http.GetFromJsonAsync<Book>($"api/books/{id}");
 }

 public async Task<Book> CreateAsync(Book book)
 {
 var resp = await _http.PostAsJsonAsync("api/books", new
 {
 title = book.Title,
 author = book.Author,
 isbn = book.Isbn,
 categoryId = book.CategoryId
 });
 resp.EnsureSuccessStatusCode();
 var created = await resp.Content.ReadFromJsonAsync<Book>();
 return created!;
 }

 public async Task UpdateAsync(int id, Book book)
 {
 var resp = await _http.PutAsJsonAsync($"api/books/{id}", new
 {
 title = book.Title,
 author = book.Author,
 isbn = book.Isbn,
 categoryId = book.CategoryId
 });
 resp.EnsureSuccessStatusCode();
 }

 public async Task DeleteAsync(int id)
 {
 var resp = await _http.DeleteAsync($"api/books/{id}");
 resp.EnsureSuccessStatusCode();
 }
}
