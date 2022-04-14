using Exercise17.Shared;

namespace Exercise17.App.Shared
{
    public class AppState
    {
        public Machine Machine { get; private set; }

        public event Action OnChange;

        public void SetMachine(Machine machine)
        {
            Machine = machine;
            StateChanged();
      }
        private void StateChanged() => OnChange.Invoke();
    }
}
