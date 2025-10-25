using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblio.Models.Entities;


public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;


    // FK → Category
    public int CategoryId { get; set; }
    public Category? Category { get; set; }


    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}