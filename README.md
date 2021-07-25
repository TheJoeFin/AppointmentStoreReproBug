# AppointmentStoreReproBug

This app was made to demonstrate an issue I've been experiencing with the AppointmentManager crashing Ink Calendar. This bug feedback item is recorded at https://aka.ms/AAd9vxx

This app highlights several different stange behaviors within the `AppointmentManager` API.

1. `StoreChanged` event fires several times when making a new appointment
2. `ShowEditNewAppointmentAsync` does not respect the `AllDay` property of the `Appointment` parameter
3. `AppointmentCalendar` `RegisterSyncManagerAsync` fails, and docs are not clear on how it is to be used.
4. `AppointmentStore` function `GetChangeTracker("");` takes a string as a parameter, but it is unclear what that parameter should be or what effect it has.
5. **Major bug within:** `AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);` Reproduced by:
- Running app x64, Released
- Start and stop the app several times, requesting `AppointmentStore` each time
- Eventually the step requesting the `AppointmentStore` fails but crashes the app, even when within a try/catch
- Switching back to debug the System ContentDialog requesting calendar access appears again and the app works fine.
