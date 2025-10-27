// Doel: Basisklasse voor entiteiten met soft-delete ondersteuning (IsDeleted, DeletedAt).
// Beschrijving: Biedt Id, IsDeleted en DeletedAt voor logische verwijderingen op alle afgeleide entiteiten.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblio.Models.Entities;


public abstract class BaseEntity
{
 public int Id { get; set; }
 public bool IsDeleted { get; set; }
 public DateTime? DeletedAt { get; set; }
}