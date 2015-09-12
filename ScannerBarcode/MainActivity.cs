using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Symbol.XamarinEMDK;
using Android.Text;
using System.Xml;
using System.IO;

namespace ScannerBarcode
{
    [Activity(Label = "ScannerBarcode", MainLauncher = true, Icon = "@drawable/icon", Name = "com.pietromaggi.sample.barcodeIntent.MainActivity")]
    public class MainActivity : Activity, EMDKManager.IEMDKListener
    {
        private EMDKManager mEmdkManager = null;
        private ProfileManager mProfileManager = null;
        private const String mProfileName = "Dubai2015";

        // This intent string contains the source of the data as a string  
        private static string SOURCE_TAG = "com.motorolasolutions.emdk.datawedge.source";
        // This intent string contains the barcode symbology as a string  
        private static string LABEL_TYPE_TAG = "com.motorolasolutions.emdk.datawedge.label_type";
        // This intent string contains the captured data as a string  
        // (in the case of MSR this data string contains a concatenation of the track data)  
        private static string DATA_STRING_TAG = "com.motorolasolutions.emdk.datawedge.data_string";
        // Intent Action for our operation
        private static string ourIntentAction = "barcodescanner.RECVR";

        // Let's define the API intent strings for the soft scan trigger
        private static String ACTION_SOFTSCANTRIGGER = "com.motorolasolutions.emdk.datawedge.api.ACTION_SOFTSCANTRIGGER";
        private static String EXTRA_PARAM = "com.motorolasolutions.emdk.datawedge.api.EXTRA_PARAMETER";
        private static String DWAPI_TOGGLE_SCANNING = "TOGGLE_SCANNING";

        // Receives the Intent that has barcode info and displays to the user
        [BroadcastReceiver()]
        class MyReceiver : BroadcastReceiver
        {
            public event EventHandler<String> scanDataReceived;

            public override void OnReceive (Context context, Intent i) {
                // check the intent action is for us  
                if (i.Action.Equals(ourIntentAction))
                {
                    // define a string that will hold our output  
                    String Out = "";
                    // get the source of the data  
                    String source = i.GetStringExtra(SOURCE_TAG);
                    // save it to use later  
                    if (source == null) source = "scanner";
                    // get the data from the intent  
                    String data = i.GetStringExtra(DATA_STRING_TAG);
                    // let's define a variable for the data length  
                    int data_len = 0;
                    // and set it to the length of the data  
                    if (data != null) data_len = data.Length;
                    // check if the data has come from the barcode scanner  
                    if (source.Equals("scanner"))
                    {
                        // check if there is anything in the data  
                        if (data != null && data.Length > 0)
                        {
                            // we have some data, so let's get it's symbology  
                            String sLabelType = i.GetStringExtra(LABEL_TYPE_TAG);
                            // check if the string is empty  
                            if (sLabelType != null && sLabelType.Length > 0)
                            {
                                // format of the label type string is LABEL-TYPE-SYMBOLOGY  
                                // so let's skip the LABEL-TYPE- portion to get just the symbology  
                                sLabelType = sLabelType.Substring(11);
                            }
                            else
                            {
                                // the string was empty so let's set it to "Unknown"  
                                sLabelType = "Unknown";
                            }

                            // let's construct the beginning of our output string  
                            Out = "Source: Scanner, " + "Symbology: " + sLabelType + ", Length: " + data_len.ToString() + ", Data: " + data.ToString() + "\r\n";
                        }
                    }
                    // check if the data has come from the MSR  
                    if (source.Equals("msr"))
                    {
                        // construct the beginning of our output string  
                        Out = "Source: MSR, Length: " + data_len.ToString() + ", Data: " + data.ToString() + "\r\n";
                    }

                    if (scanDataReceived != null) {
                        scanDataReceived(this, Out);
                    }
                }
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            EMDKResults result = EMDKManager.GetEMDKManager(Application.Context, this);

            if (result.StatusCode != EMDKResults.STATUS_CODE.Success)
            {
                Toast.MakeText(this, "Error opening the EMDK Manager", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(this, "EMDK Manager is available", ToastLength.Long).Show();
            }

            // Get our button from the layout resource,
            // and attach an event to toggle scanning option
            Button scanButton = FindViewById<Button>(Resource.Id.btn_scan);

            scanButton.Click += delegate
            {
                var intent = new Intent();
                intent.SetAction(ACTION_SOFTSCANTRIGGER);
                intent.PutExtra(EXTRA_PARAM, DWAPI_TOGGLE_SCANNING);
                SendBroadcast(intent);
            };

            MyReceiver _broadcastReceiver = new MyReceiver();

            EditText editText = FindViewById<EditText>(Resource.Id.editbox);

            _broadcastReceiver.scanDataReceived += (s, scanData) =>
            {
                editText.Text = scanData;
            };

            // Register the broadcast receiver
            IntentFilter filter = new IntentFilter("barcodescanner.RECVR");
            filter.AddCategory("android.intent.category.DEFAULT");
            Application.Context.RegisterReceiver(_broadcastReceiver, filter);
        }

        protected override void OnDestroy()
        {
            if (mProfileManager != null)
            {
                mProfileManager = null;
            }

            if (mEmdkManager != null)
            {
                mEmdkManager.Release();
                mEmdkManager = null;
            }

            base.OnDestroy();
        }

        public void OnClosed()
        {
            if (mEmdkManager != null)
            {
                mEmdkManager.Release();
                mEmdkManager = null;
            }
        }

        public void OnOpened(EMDKManager emdkManager)
        {
            mEmdkManager = emdkManager;
            String strStatus = "";
            String[] modifyData = new String[1];

            mProfileManager = (ProfileManager)mEmdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Profile);

            EMDKResults results = mProfileManager.ProcessProfile(mProfileName, ProfileManager.PROFILE_FLAG.Set, modifyData);

            if (results.StatusCode == EMDKResults.STATUS_CODE.Success)
            {
                strStatus = "Profile processed succesfully";
            }
            else if (results.StatusCode == EMDKResults.STATUS_CODE.CheckXml)
            {
                //Inspect the XML response to see if there are any errors, if not report success
                using (XmlReader reader = XmlReader.Create(new StringReader(results.StatusString)))
                {
                    String checkXmlStatus = "Status:\n\n";
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.Name)
                                {
                                    case "parm-error":
                                        checkXmlStatus += "Parm Error:\n";
                                        checkXmlStatus += reader.GetAttribute("name") + " - ";
                                        checkXmlStatus += reader.GetAttribute("desc") + "\n\n";
                                        break;
                                    case "characteristic-error":
                                        checkXmlStatus += "characteristic Error:\n";
                                        checkXmlStatus += reader.GetAttribute("type") + " - ";
                                        checkXmlStatus += reader.GetAttribute("desc") + "\n\n";
                                        break;
                                }
                                break;
                        }
                    }
                    if (checkXmlStatus == "Status:\n\n")
                    {
                        strStatus = "Status: Profile applied successfully ...";
                    }
                    else
                    {
                        strStatus = checkXmlStatus;
                    }

                }
            }
            else
            {
                strStatus = "Something wrong on processing the profile";
            }

            Toast.MakeText(this, strStatus, ToastLength.Long).Show();
        }

    }
}

