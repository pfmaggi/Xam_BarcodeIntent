package md586d360d2b8d2a68dc0a036c9ee4a016c;


public class MainActivity_MyReceiver
	extends android.content.BroadcastReceiver
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onReceive:(Landroid/content/Context;Landroid/content/Intent;)V:GetOnReceive_Landroid_content_Context_Landroid_content_Intent_Handler\n" +
			"";
		mono.android.Runtime.register ("ScannerBarcode.MainActivity/MyReceiver, ScannerBarcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", MainActivity_MyReceiver.class, __md_methods);
	}


	public MainActivity_MyReceiver () throws java.lang.Throwable
	{
		super ();
		if (getClass () == MainActivity_MyReceiver.class)
			mono.android.TypeManager.Activate ("ScannerBarcode.MainActivity/MyReceiver, ScannerBarcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onReceive (android.content.Context p0, android.content.Intent p1)
	{
		n_onReceive (p0, p1);
	}

	private native void n_onReceive (android.content.Context p0, android.content.Intent p1);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
