using GalaSoft.MvvmLight;

namespace IotDemo.ViewModels
{
    public class Pin : ViewModelBase
    {
        private string title;
        private int id;
        private bool state;

        public string Title
        {
            get
            {
                return title;
            }

        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public Pin(string title, int id)
        {
            this.title = title;
            this.id = id;
        }

        public bool State
        {
            get
            {
                return state;
            }
            set
            {
                if (state != value)
                {
                    state = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}