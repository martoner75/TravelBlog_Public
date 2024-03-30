using MetroLog.Maui;

namespace TravelBlog
{
    public partial class App : Application
    {
        public App(MainPage page)
        {
            InitializeComponent();

            MainPage = page;

            LogController.InitializeNavigation(
                page => MainPage!.Navigation.PushModalAsync(page),
                () => MainPage!.Navigation.PopModalAsync());
        }
    }
}