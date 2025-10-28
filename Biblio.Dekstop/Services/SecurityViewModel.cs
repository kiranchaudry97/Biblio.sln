using System.ComponentModel;

namespace Biblio.Dekstop.Services;

public class SecurityViewModel : INotifyPropertyChanged
{
 private bool _isAdmin;
 private bool _isStaff;

 public bool IsAdmin
 {
 get => _isAdmin;
 set
 {
 if (_isAdmin == value) return;
 _isAdmin = value;
 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAdmin)));
 }
 }

 public bool IsStaff
 {
 get => _isStaff;
 set
 {
 if (_isStaff == value) return;
 _isStaff = value;
 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStaff)));
 }
 }

 public void Reset()
 {
 IsAdmin = false;
 IsStaff = false;
 }

 public event PropertyChangedEventHandler? PropertyChanged;
}
