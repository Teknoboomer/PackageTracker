using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Windows;
using TrackerVM;

namespace PackageTrackerWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            MainWindow w = Container.Resolve<MainWindow>();
            Application.Current.MainWindow = this.MainWindow; return w;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<DeleteTrackedItemDialog, DialogViewModel>("DeleteTrackingDialog");

            ViewModelLocationProvider.Register(typeof(MainWindow).ToString(), () => Container.Resolve<TrackerViewModel>());
            ViewModelLocationProvider.Register(typeof(DeleteTrackedItemDialog).ToString(), () => Container.Resolve<DialogViewModel>());

            containerRegistry.Register<IDialogService, DialogService>();
        }
    }
}
