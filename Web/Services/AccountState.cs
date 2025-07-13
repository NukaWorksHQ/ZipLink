using Shared.Entities;

namespace Web.Services
{
    public class AccountState
    {
        private User? User { get; set; }

        public User? Property { 
            get => User;
            set
            {
                User = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
