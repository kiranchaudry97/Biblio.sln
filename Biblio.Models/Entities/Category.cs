// Bestand: Biblio.Models/Entities/Category.cs
// Doel: Categorie-entiteit met naam en relatie naar boeken.
// Beschrijving: Bevat naam met lengtevalidatie en navigatiecollectie naar gekoppelde boeken.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Biblio.Models.Entities;

public class Category : BaseEntity
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}