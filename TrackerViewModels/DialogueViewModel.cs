using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace TrackerVM
{
    // Class for the DeleteTrackedItemDialog dialogue box.
    public class DialogViewModel : BindableBase, IDialogAware
    {
        private string _iconSource;
        public string IconSource
        {
            get { return _iconSource; }
            set { SetProperty(ref _iconSource, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _actionLabel;
        public string ActionLabel
        {
            get { return _actionLabel; }
            set { SetProperty(ref _actionLabel, value); }
        }

        private string _cancelLabel;
        public string CancelLabel
        {
            get { return _cancelLabel; }
            set { SetProperty(ref _cancelLabel, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public IDialogParameters ActionParams { get; set; }

        public event Action<IDialogResult> RequestClose;
        public DelegateCommand<object> CloseDialogCommand { get; private set; }

        public DialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand<object>(OnDeleteHistoryCommand);
        }


        public void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            TrackingDialogParameters dialogParameters = parameters.GetValue<TrackingDialogParameters>("Delete Tracking");
            Title = dialogParameters.Title;
            ActionLabel = dialogParameters.ActionLabel;
            CancelLabel = dialogParameters.CancelLabel;
            Message = dialogParameters.Content;
            ActionParams = parameters;
        }

        /// <summary>
        /// This method handles the user input from the DeleteTrackedItemDialog dialogue
        /// and closes the dialogue.
        /// </summary>
        /// <param name="answer"></param>
        private void OnDeleteHistoryCommand(object answer)
        {
            ButtonResult buttonResult = ButtonResult.None;

            if ((string)answer == "Cancel")
                buttonResult = ButtonResult.Cancel;
            else if ((string)answer == "OK")
                buttonResult = ButtonResult.OK;

            RaiseRequestClose(new DialogResult(buttonResult, ActionParams));
        }
    }
}

