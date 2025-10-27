// Doel: Lid-entiteit met validatie en relatie naar uitleningen.
// Beschrijving: Bevat voornaam, naam, e-mail (uniek in DB), telefoon en adres; ondersteunt soft-delete via BaseEntity.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Biblio.Models.Entities;

public class Member : BaseEntity
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}