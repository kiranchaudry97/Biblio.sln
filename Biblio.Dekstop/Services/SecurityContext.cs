// Doel: Eenvoudige statische context om autorisatierollen in de UI te bewaren (Admin/Staff) voor conditionele weergave..
// Beschrijving: Houdt flags bij voor rolgebaseerde zichtbaarheid in de WPF UI en kan worden gereset bij afmelden.
namespace Biblio.Dekstop.Services
{
 public static class SecurityContext
 {
 public static bool IsAdmin { get; set; }
 public static bool IsStaff { get; set; }
 public static void Reset()
 {
 IsAdmin = false;
 IsStaff = false;
 }
 }
}
