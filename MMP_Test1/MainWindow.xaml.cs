#region Using
using System;
using System.IO;
using System.Linq;
using Awesomium.Core;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
#endregion

namespace MMP_Test1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();
        }


        #region Methods
        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            // Destroy the WebControl and its underlying view.
            webControl.Dispose();
        }
        #endregion


        private void webControl_ConsoleMessage(object sender, ConsoleMessageEventArgs e) {
            Debug.Print(String.Format(">{0}", e.Message));
        }

        private void webControl_NativeViewInitialized(object sender, WebViewEventArgs e) {
            // We demonstrate the creation of a child global object.
            // Acquire the parent first.
            JSObject external = webControl.CreateGlobalJavascriptObject("external");

            if (external == null)
                return;

            using (external) {
                // Create a child using fully qualified name. This only succeeds if
                // the parent is created first.
                JSObject app = webControl.CreateGlobalJavascriptObject("external.app");

                if (app == null)
                    return;

                using (app)
                    // Create and bind to an asynchronous custom method. This is called
                    // by JavaScript to have the native app perform some heavy work.
                    // (See: /web/index.html)
                    app.Bind("startGame", false, StartGame);
            }
        }

        private void StartGame(object sender, JavascriptMethodEventArgs e) {
            // Must be a function object.
            if (!e.Arguments[0].IsObject)
                return;

            // You can cache this callback and call it only when your application 
            // has performed all work necessary and has a result ready to send.
            // Note that this callback object is valid for as long as the current 
            // page is loaded. A navigation will invalidate it!
            JSObject callbackArg = e.Arguments[0];

            // Make sure it's a function object.
            if (!callbackArg.HasMethod("call"))
                return;

            // See it!
            Debug.Print(callbackArg.ToString());

            // You need to copy the object if you intend to cache it. The original
            // object argument passed to the handler is destroyed by default by 
            // native Awesomium, when the handler returns. A copy will keep it alive.
            JSObject callback = callbackArg.Clone();

            // Perform your heavy work.
            Task.Factory.StartNew(
                (Func<object, string>)PerformWork, callback).ContinueWith(
                /* Send a response when complete. */
                SendResponse,
                /* Make sure the response is sent on the 
                 * initial thread. */
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private string PerformWork(object callback) {
            // Perform heavy work.
            string appPath = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString();
            string gamePath = Path.Combine(appPath, "game", "R2R_prototype_0-0-2.exe");

            if (File.Exists(gamePath)) {
                Process p = new Process();
                p.StartInfo.FileName = gamePath;
                p.Start();

                Thread.Sleep(500);
                return "Success";
            }
            else {
                Debug.Print("Could not open game as file was not found at: " + gamePath);
                
                Thread.Sleep(500);
                return "Failure";
            }                        
        }

        private void SendResponse(Task<string> t) {
            // Get the callback. JSObject supports the DLR so for 
            // this example we assign to a dynamic which makes invoking the
            // callback, pretty straightforward (just as you would in JS).
            dynamic callback = t.AsyncState;

            if (callback == null)
                return;

            using (callback) {
                if ((t.Exception != null) || String.IsNullOrEmpty(t.Result))
                    // We failed.
                    return;

                // Invoke the callback.
                callback(t.Result);

                // JSObject supports the DLR. However, if you choose to explicitly cast back to 
                // JSObject, this is the technique for invoking the callback, using regular pattern:
                //callback.InvokeAsync( "call", callback, t.Result );
            }
        }

        private void webControl_WindowClose(object sender, WindowCloseEventArgs e)
        {
            if (!e.IsCalledFromFrame)
                this.Close();
        }
    }

}
