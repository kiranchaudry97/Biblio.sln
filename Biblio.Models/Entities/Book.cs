// Doel: Boek-entiteit met validatie, relatie naar Category en collectie van Loans.
// Beschrijving: Bevat titel, auteur, genormaliseerde ISBN-validatie en FK naar Category; ondersteunt soft-delete via BaseEntity.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblio.Models.Entities;

public class Book : BaseEntity
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Author { get; set; } = string.Empty;

    [StringLength(17)]
    [RegularExpression(@"^(?:97[89])?\d{9}(\d|X)$", ErrorMessage = "Ongeldige ISBN.")]
    public string Isbn { get; set; } = string.Empty;


    // FK → Category
    [Required]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }


    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}