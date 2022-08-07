using ExternalTrackingequests;
using HistoricalTracking;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TrackerModel;

namespace TrackerVM
{
    public class TrackerViewModel : BindableBase
    {
        private int TRACKING_NUMBER_LENGTH = 22;
        private readonly string DELETE_TRACKING_NUMBER = "Tracking number xxx";

        private string _internalErrorDescription;
        private IDialogService _dialogService;
        private HistoricalTrackingAccessMongoDB _db;

        ///****************************************************************************************************
        ///
        /// <summary>
        ///     Testing View Model constructor. Initializes the HistoricalTrackingAccessMongoDB to the test DB.
        ///     Finally, some items in the View are initialized.
        /// </summary>
        /// <param name="dbName">
        ///     The database name attached to test DB.
        /// </param>
        ///
        ///****************************************************************************************************
        public TrackerViewModel(string dbName)
        {
            _db = new HistoricalTrackingAccessMongoDB(dbName);
        }

        ///****************************************************************************************************
        ///
        /// <summary>
        ///     Main View Model constructor. It attaches to the dialog service for the popups and initializes
        ///     the instance of the HistoricalTrackingAccessMongoDB class.
        /// </summary>
        /// <param name="dbName">
        ///     The database name attached to test DB.
        /// </param>
        ///
        ///****************************************************************************************************
        public TrackerViewModel(string dbName)
        {
            HistoricalTrackingAccessMongoDB.InitializeDB(dbName);
        }

        ///****************************************************************************************************
        ///
        /// <summary>
        ///     Main View Model constructor. It attaches to the dialog service for the popups and initializes
        ///     the instance of the HistoricalTrackingAccessMongoDB class.
        /// </summary>
        /// <param name="dialogService">
        ///     The DialogService is connected in the Unity Container.
        /// </param>
        ///
        ///****************************************************************************************************
        internal TrackerViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;  // Get a pointer to the Dialog service to show the Delete Tracking dialog.

            _db = new HistoricalTrackingAccessMongoDB("PackageTracker");
            TrackSingleCommand = new DelegateCommand(async () => await TrackSingle(), TrackSingleCanExecute);
            DeleteHistoryCommand = new DelegateCommand<object>(OnDeleteHistoryCommand);
            PreviousTrackingRefresh = new DelegateCommand(async () => await RefreshPreviousTracking());

            // Clear the Description, Tracking Id and set the Refresh button to indicate active.
            RefreshEnabled = true;
            SingleTrackingDescription = "";
            SingleTrackingId = "";

            // Collapse the tracking status for the single tracking since we don't have one
            // Get the past histories asynchronously and display them.
            SingleTrackingSummaryVisibility = Visibility.Collapsed;
            Task.Run(TrackPastHistories);

            // Action to close all other expanded expanders in Tracking History list when one is expanded.
            // The Action is invoked in TrackingInfo when the IsExpanded binding is actived and Enabled.
            TrackingInfoChangedNotifier.ExpanderUpdated = EnabledUpdated;
        }

        ///****************************************************************************************************
        ///
        /// <summary>
        ///     Properties for Single Tracking History
        /// </summary>
        ///
        ///****************************************************************************************************
        // Delegate command to track the entered tracking ID.
        public DelegateCommand TrackSingleCommand { get; private set; }

        private string _singleTrackingDescription;
        public string SingleTrackingDescription
        {
            get { return _singleTrackingDescription; }
            set { _ = SetProperty(ref _singleTrackingDescription, value); }
        }

        private string _singleTrackingId;
        public string SingleTrackingId
        {
            get { return _singleTrackingId; }
            set
            {
                _ = SetProperty(ref _singleTrackingId, value);

                // Trigger CanExecute for Track button.
                TrackSingleCommand.RaiseCanExecuteChanged();
            }
        }

        private string _singleTrackingHistory;
        public string SingleTrackingHistory
        {
            get { return _singleTrackingHistory; }
            set { _ = SetProperty(ref _singleTrackingHistory, value); }
        }

        private Visibility _singleTrackingSummaryVisibility;
        public Visibility SingleTrackingSummaryVisibility
        {
            get { return _singleTrackingSummaryVisibility; }
            private set { _ = SetProperty(ref _singleTrackingSummaryVisibility, value); }
        }

        private TrackingRequestStatus _trackSinglePackageStatusColor = TrackingRequestStatus.NoRecord;
        public TrackingRequestStatus TrackSinglePackageStatusColor
        {
            get { return _trackSinglePackageStatusColor; }
            set { _ = SetProperty(ref _trackSinglePackageStatusColor, value); }
        }

        private string _singleTrackingSummary;
        public string SingleTrackingSummary
        {
            get { return _singleTrackingSummary; }
            private set { _ = SetProperty(ref _singleTrackingSummary, value); }
        }

        ///****************************************************************************************************
        ///
        /// <summary>
        ///     Properties for Multiple Tracking Histories
        /// </summary>
        ///
        ///****************************************************************************************************

        private ObservableCollection<TrackingInfo> _multipleTrackingHistory = null;
        public ObservableCollection<TrackingInfo> MultipleTrackingHistory
        {
            get { return _multipleTrackingHistory; }
            set { _ = SetProperty(ref _multipleTrackingHistory, value); }
        }

        // History Refresh enabled is manually controlled.
        private bool _refreshEnabled;
        public bool RefreshEnabled
        {
            get { return _refreshEnabled; }
            private set { _ = SetProperty(ref _refreshEnabled, value); }
        }

        // Delegate command to track previous tracking Ids.
        public DelegateCommand PreviousTrackingRefresh { get; private set; }

        // Delegate command to delete a previous tracking Id.
        public DelegateCommand<object> DeleteHistoryCommand { get; private set; }

        public TrackerViewModel()
        {
            // Empty. For use by unit tests.
        }

        ///****************************************************************************************************
        ///
        /// <summary>
        ///     Single tracking Id processing
        /// </summary>
        ///
        ///****************************************************************************************************

        ///
        /// <summary>
        ///     Asynchronous task to track the entered Description and Tracking ID.
        /// </summary>
        ///
        private async Task TrackSingle()
        {
            TrackingInfo singleTrackingHistory = null;
            string response = "";
            TrackingRequestStatus status = TrackingRequestStatus.InternalError;

            await Task.Run(() =>
            {
                // For a better user experience, clear the tracking status and wait at least one second to populate the status again.
                // Depending on bandwidth, updates can be subsecond and there will be the expectation that something as complicated
                // as getting the tracking should take a while. This also lets the user know that something actually happended
                // in the case where the status is unchanged from before.
                DateTime start = DateTime.Now;
                SingleTrackingSummary = "";

                // Turn off saving of history when descrption is updated to avoid premature saving of history.
                TrackingInfoChangedNotifier.DescriptionUpdated = null;

                // Make the web call to USPS to get the tracking historiy and parse it.
                // UPS Access Code: ADB7035AF6F2FA85
                _singleTrackingId = string.Concat(_singleTrackingId.Where(c => !char.IsWhiteSpace(c))); // Get rid of any whitespace.
                response = USPSTrackerWebAPICall.GetTrackingFieldInfo(_singleTrackingId);
                singleTrackingHistory = USPSTrackingResponseParser.USPSParseTrackingXml(response, "", _singleTrackingDescription);

                // Wait for the rest of the one second delay.
                TimeSpan duration = DateTime.Now - start;
                int waitTime = 1000 - (int)duration.TotalMilliseconds;
                if (waitTime > 0)
                    Thread.Sleep((int)waitTime);

                // Null is returned if there is an internal error with the Internet.
                if (singleTrackingHistory == null)
                {
                    SingleTrackingSummary = "There was an external error. Check the Internet connection.";
                    status = TrackingRequestStatus.InternalError;
                }
                else
                {
                    // For single tracking, peel off the first tracking history result.
                    // Set the Single Tracking Summary and status.
                    SingleTrackingSummary = singleTrackingHistory.StatusSummary;
                    SingleTrackingHistory = singleTrackingHistory.TrackingHistory;
                    status = singleTrackingHistory.TrackingStatus;
                }

                // Update the Single Tracking Summary and make it visible.
                SingleTrackingSummaryVisibility = Visibility.Visible;
                TrackSinglePackageStatusColor = status;
            });

            // If there were no errors, add this tracking to the list of tracked packages unless it is already there.
            // Save the histories to storage.
            if (status != TrackingRequestStatus.InternalError)
            {
                if (status == TrackingRequestStatus.Delivered || status == TrackingRequestStatus.InTransit || status == TrackingRequestStatus.NoRecord)
                {
                    // Do not add to history if it is already there.
                    if (_multipleTrackingHistory.Where(history => history.TrackingId == _singleTrackingId).Count() == 0)
                    {
                        MultipleTrackingHistory.Insert(0, singleTrackingHistory);
                        _db.SaveHistory(singleTrackingHistory);
                    }

                    // Clear the Description and Tracking ID.
                    SingleTrackingDescription = "";
                    SingleTrackingId = "";
                }
            }

            // Restore/set the Delegate to allow TrackingInfo to inform the VM of a Description change
            // by the view.
            TrackingInfoChangedNotifier.DescriptionUpdated = DescriptionUpdated;
        }

        ///
        /// <summary>
        ///     Enables the Track Single when the entry becomes valid.
        /// </summary>
        /// <returns>
        ///     True if the tracking ID is valid.
        /// </returns>
        ///
        private bool TrackSingleCanExecute()
        {
            // Allow spaces in the middle of the string to ease entry; i.e. as xxxx xxxx xxxx xxxx xxxx xx for USPS.
            string nonSpace = string.Concat(_singleTrackingId.Where(c => !char.IsWhiteSpace(c))).ToUpper();
            bool isValidTrackingNUmber = (nonSpace.StartsWith("1Z") && IsvalidUPSCheckDigit(nonSpace))
                || (nonSpace.Length == TRACKING_NUMBER_LENGTH && nonSpace.All(char.IsNumber));
            return isValidTrackingNUmber;
        }

        /// <summary>
        /// Delegate for Description updated.
        /// </summary>
        /// <param name="history"></param>
        public void DescriptionUpdated(TrackingInfo history)
        {
            _db.SaveHistory(history);
        }

        /// <summary>
        /// Delegate for Enabled updated.
        /// </summary>
        /// <param name="history"></param>
        public void EnabledUpdated(TrackingInfo history)
        {
            // Turn off saving of history when Descrption is updated to avoid saving of history.
            TrackingInfoChangedNotifier.DescriptionUpdated = null;

            for (int i = 0; i < MultipleTrackingHistory.Count; i++)
            {
                if (MultipleTrackingHistory[i].IsExpanded && MultipleTrackingHistory[i].TrackingId != history.TrackingId)
                    MultipleTrackingHistory[i] = new TrackingInfo(MultipleTrackingHistory[i]);
            }

            // Restore the Delegate to allow TrackingInfo to inform the VM of a Description change
            // by the view.
            TrackingInfoChangedNotifier.DescriptionUpdated = DescriptionUpdated;
        }

        ///****************************************************************************************************
        ///
        /// <summary>
        ///     Tracking History processing, initialize/update.
        /// </summary>
        ///
        ///****************************************************************************************************

        ///
        /// <summary>
        ///     Asynchronous task to refresh tracking for previous items.
        ///     Method is invoked on startup and when the Refresh button is pressed. The
        ///     tracking will be refreshed for all Ids that are not Delivered.
        /// </summary>
        ///
        private async Task RefreshPreviousTracking()
        {
            // Blink the refesh buttpn gray to let the user know we did something.
            RefreshEnabled = false;
            await Task.Run(() =>
            {
                DateTime start = DateTime.Now;
                Task.Run(TrackPastHistories);

                // Wait for the rest of the one second delay.
                TimeSpan duration = DateTime.Now - start;
                int waitTime = 1000 - (int)duration.TotalMilliseconds;
                if (waitTime > 0)
                    Thread.Sleep((int)waitTime);
                RefreshEnabled = true;

                // Clear the Single Tracking Summary and make it collapsed to remove old information.
                SingleTrackingSummary = "";
                SingleTrackingSummaryVisibility = Visibility.Collapsed;
            });
        }

        ///
        /// <summary>
        ///     Aynchronous task to pull in all of the past histories saved in storage
        ///     and update those that are not Delivered.
        /// </summary>
        ///
        private async Task TrackPastHistories()
        {
            await Task.Run(() =>
            {
                // Delegate will be null until history loaded to turn off
                // saving of the histories. Tracking history is saved
                // whenever the Description is updated, which would happen
                // each time a history is loaded if the Delegate were not null.
                TrackingInfoChangedNotifier.DescriptionUpdated = null;

                // Retrieve past tracking histories while updating nondelivered tracking and parse them.
                // WebApi calls will only be made to update nondelivered items.
                // Convert the List to an ObservableCollection for display.
                List<TrackingInfo> trackingList = _db.GetSavedHistories();
                trackingList.Sort((x, y) => -x.FirstEventDateTime.CompareTo(y.FirstEventDateTime)); // Latest on top.

                // Update nondelivered tracking and parse them.
                // WebApi calls will only be made to update nondelivered items.
                bool hadInternalError = UpdateUndeliveredTracking(trackingList);

                // If there was a problem with the Internet, put an error message in the single tracking summary and make it visible.
                // A problem will show up if any of the in-transit IDs attempt to update and fail.
                if (hadInternalError)
                {
                    // Update the Single Tracking Summary and make it visible.
                    SingleTrackingSummaryVisibility = Visibility.Visible;
                    SingleTrackingSummary = _internalErrorDescription;
                    SingleTrackingHistory = _internalErrorDescription;
                    TrackSinglePackageStatusColor = TrackingRequestStatus.InternalError;
                }

                // Convert the List to an ObservableCollection for display and update display.
                MultipleTrackingHistory = new ObservableCollection<TrackingInfo>(trackingList);

                // Restore/set the Delegate to allow TrackingInfo to inform the VM of a Description change
                // by the view.
                TrackingInfoChangedNotifier.DescriptionUpdated = DescriptionUpdated;
            });
        }

        ///
        /// <summary>
        ///     Setup the custom dialog popup with the values for history tracking Id delete.
        /// </summary>
        /// <param name="trackingId">
        ///     Our generic Diialog Delgate takes an object as a parameter for flexibility.
        ///     In this instance, the parameter is the tracking ID, a string.
        /// </param>
        ///
        private void OnDeleteHistoryCommand(object trackingId)
        {
            IDialogParameters parameters = new DialogParameters();
            TrackingDialogParameters deleteParams = new TrackingDialogParameters
            {
                Title = "Delete Tracking Item",
                Content = DELETE_TRACKING_NUMBER.Replace("xxx", (string)trackingId),
                ActionLabel = "Delete",
                CancelLabel = "Cancel",
                ActionParams = trackingId
            };

            parameters.Add("Delete Tracking", deleteParams);
            _dialogService.ShowDialog("DeleteTrackingDialog", parameters, OnDialogClosed);
        }

        /// <summary>
        ///     Gets the result from the Delete Tracking dialog and acts on it.
        /// </summary>
        /// <param name="result">
        ///     The only result we care about is ButtonResult.OK to indicate deletion is OK.
        ///     The other result returned from the Delete Dialog is ButtonResult.Cancel.
        /// </param>
        private void OnDialogClosed(IDialogResult result)
        {
            if (result.Result == ButtonResult.OK)
            {
                TrackingDialogParameters dialogParameters = result.Parameters.GetValue<TrackingDialogParameters>("Delete Tracking");
                string trackingId = (string)dialogParameters.ActionParams;

                _multipleTrackingHistory.Remove(_multipleTrackingHistory.Where(history => history.TrackingId == trackingId).FirstOrDefault());
                _db.DeleteHistory(trackingId);
            }
        }

        /// <summary>
        ///      Updates the tracking history of all undelivered packages.
        /// </summary>
        /// <param name="trackingHistories"></param>
        ///      The List of tracking histories.
        /// <returns name="hadInternalError">
        ///      Indicates an internal error from the updates.
        /// </returns>
        public bool UpdateUndeliveredTracking(List<TrackingInfo> trackingHistories)
        {
            bool hadInternalError = false;

            // Delegate will be null until history loaded to turn off
            // saving of the histories. Tracking history is saved
            // whenever the Description is updated, which would happen
            // each time a history is updated if the Delegate were not null.
            TrackingInfoChangedNotifier.DescriptionUpdated = null;

            // Loop through all of the histories and update the tracking for those not yet
            // delivered before adding them into the list.
            for (int i = 0; i < trackingHistories.Count; i++)
            {
                TrackingInfo history = trackingHistories[i];

                // Do not update outdated undelivered tracking requests. USPS IDs for valid for only 120 days.
                if (!history.TrackingComplete && history.FirstEventDateTime >= DateTime.Now.AddDays(-120))
                {
                    string response = USPSTrackerWebAPICall.GetTrackingFieldInfo(history.TrackingId);
                    if (response.StartsWith("Error"))
                    {
                        hadInternalError = true;
                        _internalErrorDescription = response;
                    }
                    else
                    {
                        TrackingInfo update = USPSTrackingResponseParser.USPSParseTrackingXml(response, "", history.Description);
                        if (update.TrackingStatus == TrackingRequestStatus.InternalError)
                        {
                            hadInternalError = true;
                            _internalErrorDescription = update.StatusSummary;
                        }
                        else
                        {
                            update.Description = history.Description; // Restore the Description.
                            update.Id = history.Id; // Restore the Id.
                            trackingHistories[i] = update;  // Update the history.
                            _db.SaveHistory(trackingHistories[i]);
                        }
                    }
                }
                else
                {
                    // If it was not delivered and the ID has expired, set the status to Lost and tracking completed.
                    if (!history.TrackingComplete && history.FirstEventDateTime < DateTime.Now.AddDays(-120))
                    {
                        List<TrackingInfo> trackingList = _db.GetSavedHistories();

                        history.TrackingComplete = true;
                        history.TrackingStatus = TrackingRequestStatus.Lost;
                        trackingHistories[i] = history;  // Update the history.
                        _db.SaveHistory(trackingHistories[i]);
                    }
                }
            }

            // Restore/set the Delegate to allow TrackingInfo to inform the VM of a Description change
            // by the view.
            TrackingInfoChangedNotifier.DescriptionUpdated = DescriptionUpdated;

            return hadInternalError;
        }

        public void DisableDescriptionUpdateDelegate()
        {
            TrackingInfoChangedNotifier.DescriptionUpdated = null;
        }

        /// <summary> ADB7035AF6F2FA85
        ///     UPS Check Digit Calculation Method.
        ///     Tracking number is at least 10 characters long.
        ///     
        ///     There is no need for character set validation since a
        ///     bad character would fail the validation anyway.
        /// </summary>
        /// <param name="trackingNumber">
        ///     The UPS tracking number (string) 1Z 53Y6 1190 6907 3535 1z9e79559027281578
        /// </param>
        public static bool IsvalidUPSCheckDigit(string trackingNumber)
        {
            if (trackingNumber.Length < 18) // Minimum length of UPS tracking number.
                return false;

            int sum = 0;
            int checkDigit = 0;
            char[] trackingNumberArray = trackingNumber.ToUpper().ToCharArray();
            int lastDigit = trackingNumberArray[trackingNumber.Length - 1] - '0';

            // Loop through the array calculating the checksum starting after the "1Z".
            for (int i = 2; i < trackingNumber.Length - 1; i++)
            {
                int valueToSum = trackingNumberArray[i];
                if (valueToSum >= 'A') /* If letter, convert to digit using UPS formula. */
                {
                    if (valueToSum >= 'S')       // Between 'S' and 'Z'
                        valueToSum -= 35;
                    else if (valueToSum >= 'I')  // Between 'I' and 'R'
                        valueToSum -= 25;
                    else                         // Between 'A' and 'H'
                        valueToSum -= 15;
                }

                valueToSum -= '0'; // Convert to the numeric of digit.
                sum += (i % 2 == 0) ? valueToSum : valueToSum * 2; // Double it if is an odd index.
            }

            // Extract single digit from sum.
            // Round to the next highest ten (122 becomes 130) then subtract the sum, which gives 8.
            checkDigit = (int)(Math.Ceiling(sum / 10.0d) * 10) - sum;

            // If the last digit matches the check digit the number is valid.
            return lastDigit == checkDigit;
        }
        //01    UPS United States Next Day Air("Red")
        //02    UPS United States Second Day Air("Blue")
        //03    UPS United States Ground    
        //12    UPS United States Third Day Select    
        //13    UPS United States Next Day Air Saver("Red Saver")
        //15    UPS United States Next Day Air Early A.M.
        //22    UPS United States Ground - Returns Plus - Three Pickup Attempts
        //32    UPS United States Next Day Air Early A.M. - COD
        //33    UPS United States Next Day Air Early A.M. - Saturday Delivery, COD
        //41    UPS United States Next Day Air Early A.M. - Saturday Delivery    
        //42    UPS United States Ground - Signature Required    
        //44    UPS United States Next Day Air - Saturday Delivery    
        //66    UPS United States Worldwide Express
        //72    UPS United States Ground - Collect on Delivery
        //78    UPS United States Ground - Returns Plus - One Pickup Attempt
        //90    UPS United States Ground - Returns - UPS Prints and Mails Label
        //A0    UPS United States Next Day Air Early A.M. - Adult Signature Required
        //A1    UPS United States Next Day Air Early A.M. - Saturday Delivery, Adult Signature Required
        //A2    UPS United States Next Day Air - Adult Signature Required
        //A8    UPS United States Ground - Adult Signature Required
        //A9    UPS United States Next Day Air Early A.M. - Adult Signature Required, COD
        //AA    UPS United States Next Day Air Early A.M. - Saturday Delivery, Adult Signature Required, COD
    }
}
