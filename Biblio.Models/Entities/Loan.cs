// Doel: Uitlening-entiteit met relaties naar Boek en Lid, data voor periodes en status.
// Beschrijving: Bewaart start-, eind- en inleverdatum en koppelt boek en lid; heeft IsClosed afgeleid van ReturnedAt.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblio.Models.Entities;


public class Loan : BaseEntity
{
 public int BookId { get; set; }
 public Book? Book { get; set; }


 public int MemberId { get; set; }
 public Member? Member { get; set; }


 public DateTime StartDate { get; set; }
 public DateTime DueDate { get; set; }
 public DateTime? ReturnedAt { get; set; }


 public bool IsClosed => ReturnedAt.HasValue;
}