using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AppointmentStoreReproBug
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public AppointmentStore ApptStore { get; set; }

        public MainPage()
        {
            InitializeComponent();
        }

        private async Task GetAppointmentAccess()
        {
            if (ApptStore != null)
            {
                WriteToResults("Appointment Store Exists");
            }
            else
            {
                try
                {
                    ApptStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);
                }
                catch (Exception ex)
                {
                    WriteToResults("Failed to get Appointment Store");
                    WriteToResults(ex.Message);
                    return;
                }
            }

            if (ApptStore != null)
            {
                if (ApptStore.ChangeTracker.IsTracking == false)
                    ApptStore.ChangeTracker.Enable();
                ApptStore.StoreChanged -= ApptStore_StoreChanged;
                ApptStore.StoreChanged += ApptStore_StoreChanged;
                WriteToResults("Established Appointment Store and Change Tracker");

                await SyncCalendars();
            }
            else
            {
                // show error, unable to access appointments
                ResultsTextBox.Text += "\n> Failed to get Appointment Access";
            }
        }

        private async Task SyncCalendars()
        {
            IReadOnlyList<AppointmentCalendar> calendars = await ApptStore.FindAppointmentCalendarsAsync();

            foreach (AppointmentCalendar apptCal in calendars)
            {
                bool syncStarted = false;

                try
                {
                    await apptCal.RegisterSyncManagerAsync();
                    syncStarted = await apptCal.SyncManager.SyncAsync();
                }
                catch (Exception ex)
                {
                    WriteToResults($"Exception registering or syncing {apptCal.DisplayName}");
                    WriteToResults(ex.Message);

                }

                if (syncStarted == true)
                    WriteToResults($"Successfully started sync of {apptCal.DisplayName}");
                else
                    WriteToResults($"Failed to start sync of {apptCal.DisplayName}");

            }
        }

        public void WriteToResults(string stringToWrite)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\n');
            if (stringToWrite.Length > 1 && stringToWrite[0] != '>')
                sb.Append("> ");
            sb.Append(stringToWrite);

            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                ResultsTextBox.Text += sb.ToString();
            });
        }

        private async void ApptStore_StoreChanged(AppointmentStore sender, AppointmentStoreChangedEventArgs args)
        {
            AppointmentStoreChangeTracker changeTracker = ApptStore.GetChangeTracker("");
            AppointmentStoreChangeReader changeReader = changeTracker.GetChangeReader();
            IReadOnlyList<AppointmentStoreChange> changes = await changeReader.ReadBatchAsync();

            foreach (AppointmentStoreChange chg in changes)
            {
                WriteToResults($"{ chg.Appointment.Subject} \t | \t{ chg.ChangeType}");
            }

            changeReader.AcceptChanges();
        }

        private async void GetAccessButton_Click(object sender, RoutedEventArgs e)
        {
            await GetAppointmentAccess();
        }

        private async void NewApptAddButton_Click(object sender, RoutedEventArgs e)
        {
            DateTimeOffset newDate = NewApptCalPicker.Date.Value;
            Appointment newAppt = new Appointment()
            {
                StartTime = new DateTimeOffset(newDate.Year, newDate.Month, newDate.Day, 12, 0, 0, DateTimeOffset.Now.Offset),
                AllDay = true,
                Subject = NewApptSubjectTextBox.Text
            };

            if (ApptStore == null)
                await GetAppointmentAccess();

            string returnedID = await ApptStore.ShowEditNewAppointmentAsync(newAppt);

            if (string.IsNullOrEmpty(returnedID) == false)
            {
                NewApptSubjectTextBox.ClearValue(TextBox.TextProperty);
                NewApptCalPicker.ClearValue(CalendarDatePicker.DateProperty);
            }
        }
    }
}
