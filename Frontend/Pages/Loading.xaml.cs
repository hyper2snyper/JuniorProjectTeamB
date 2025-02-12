using JuniorProject.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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