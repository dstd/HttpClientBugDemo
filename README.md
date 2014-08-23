Minimal example of the issue with cancellation of POST-request performed with HttpClient library:

- In MainPage.xaml.cs added long-polling: using HttpClient.PostAsync with specified CancellationToken to make it cancellable;
- In App.xaml.cs added calls to MainPage.CancelRequests() to Application_Deactivated and Application_Closing;

Web-service is a comet-style service: waiting for some events and responds with events, or with empty result if timed out.
The timeout is about 30 seconds.

Odd thing is that when I close the application, the polling-request won't be cancelled. So application become zombie-app until a web-service will timed out and the request will be completed.
"Zombie" is a state when it is already not alive, but yet couldn't be started once again - the system show "Loading..." message.

So user experience for such application looks like this:
1. User started the application
2. User hit Back quickly right after the start, and returned to the apps list
3. User starting the app once again
4. User getting "Loading..." for up to 30 seconds!
