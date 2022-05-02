using Prism.Common;
using Prism.Ioc;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Prism.Services.Dialogs
{
    public class DialogService : IDialogService
    {
        private readonly IContainerExtension _containerExtension;

        public DialogService(IContainerExtension containerExtension)
        {
            _containerExtension = containerExtension;
        }

        public void Show(string name, IDialogParameters parameters, Action<IDialogResult> callback)
        {
            IDialogWindow dialogWindow = CreateDialogWindow(name, parameters, callback);
            dialogWindow.Show();
        }

        public void ShowDialog(string name, IDialogParameters parameters, Action<IDialogResult> callback)
        {
            IDialogWindow dialogWindow = CreateDialogWindow(name, parameters, callback);
            dialogWindow.ShowDialog();
        }

        IDialogWindow CreateDialogWindow(string name, IDialogParameters parameters, Action<IDialogResult> callback)
        {
            IDialogWindow dialogWindow = _containerExtension.Resolve<IDialogWindow>();
            FrameworkElement dialogContent = (FrameworkElement)_containerExtension.Resolve<object>(name);
            IDialogAware viewModel = (IDialogAware)dialogContent.DataContext;

            ConfigureDialogWindowEvents(dialogWindow, viewModel, callback);
            ConfigureDialogWindowContent(dialogWindow, dialogContent, viewModel, parameters);

            return dialogWindow;
        }

        void ConfigureDialogWindowContent(IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel, IDialogParameters parameters)
        {

            ConfigureDialogWindowProperties(window, dialogContent, viewModel);

            MvvmHelpers.ViewAndViewModelAction<IDialogAware>(viewModel, d => d.OnDialogOpened(parameters));
        }

        void ConfigureDialogWindowEvents(IDialogWindow dialogWindow, IDialogAware viewModel, Action<IDialogResult> callback)
        {
            Action<IDialogResult> requestCloseHandler = (o) =>
            {
                dialogWindow.Result = o;
                dialogWindow.Close();
            };

            RoutedEventHandler loadedHandler = (o, e) =>
            {
                viewModel.RequestClose += requestCloseHandler;
            };
            dialogWindow.Loaded += loadedHandler;

            CancelEventHandler closingHandler = (o, e) =>
            {
                if (!viewModel.CanCloseDialog())
                    e.Cancel = true;
            };
            dialogWindow.Closing += closingHandler;

            EventHandler closedHandler = (o, e) =>
            {
                dialogWindow.Closing -= closingHandler;
                viewModel.RequestClose -= requestCloseHandler;

                viewModel.OnDialogClosed();

                if (dialogWindow.Result == null)
                    dialogWindow.Result = new DialogResult();

                callback?.Invoke(dialogWindow.Result);

                dialogWindow.DataContext = null;
                dialogWindow.Content = null;
            };
            dialogWindow.Closed += closedHandler;
        }

        void ConfigureDialogWindowProperties(IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel)
        {
            Style? windowStyle = Dialog.GetWindowStyle(dialogContent);
            if (windowStyle != null)
                window.Style = windowStyle;

            window.Content = dialogContent;

            if (window.Owner == null)
                window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        }

        public void Show(string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName)
        {
            throw new NotImplementedException();
        }

        public void ShowDialog(string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName)
        {
            throw new NotImplementedException();
        }
    }
}
