using JuniorProject.Backend;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace JuniorProject.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Page
    {
        bool loadingDone = false;


        public Loading()
        {
            InitializeComponent();
            Debug.Print("Entering Loading Screen.");
            CompositionTarget.Rendering += Loop;
        }

        public void Loop(object? sender, EventArgs e)
        {
            bool loadingDone = ClientCommunicator.GetData<bool>("LoadingDone");
            this.LoadingMessage.Content = ClientCommunicator.GetData<string>("LoadingMessage");
            if (loadingDone)
            {
                NavigationService.Navigate(new Simulation());
                CompositionTarget.Rendering -= Loop;
                ClientCommunicator.UnregisterData("LoadingMessage");
                ClientCommunicator.UnregisterData("LoadingDone");
                return;
            }


        }

    }
}