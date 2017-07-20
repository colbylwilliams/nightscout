using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Mobile;

using Plugin.VersionTracking;

namespace Nightscout
{
	public static class Bootstrap
	{
		public static void Run ()
		{
			CrossVersionTracking.Current.Track ();

			Log.Debug ($"\n{CrossVersionTracking.Current}");

			Settings.RegisterDefaultSettings ();

			Microsoft.Azure.Mobile.Crashes.Crashes.GetErrorAttachments = (report) => new List<Microsoft.Azure.Mobile.Crashes.ErrorAttachmentLog>
			{ Microsoft.Azure.Mobile.Crashes.ErrorAttachmentLog.AttachmentWithText (CrossVersionTracking.Current.ToString (), "versionhistory.txt") };


#if __IOS__
			Microsoft.Azure.Mobile.Distribute.Distribute.DontCheckForUpdatesInDebug ();
#elif __ANDROID__
            Settings.VersionNumber = CrossVersionTracking.Current.CurrentVersion;

            Settings.BuildNumber = CrossVersionTracking.Current.CurrentBuild;
#endif

			if (!string.IsNullOrEmpty (Keys.MobileCenter.AppSecret))
			{
				Log.Debug ("Starting Mobile Center...");

				MobileCenter.Start (Keys.MobileCenter.AppSecret,
								   typeof (Microsoft.Azure.Mobile.Analytics.Analytics),
								   typeof (Microsoft.Azure.Mobile.Crashes.Crashes),
								   typeof (Microsoft.Azure.Mobile.Distribute.Distribute));
			}
			else
			{
				Log.Debug ("To use Mobile Center, add your App Secret to Keys.MobileCenter.AppSecret");
			}

			Task.Run (async () =>
			{
				var installId = await MobileCenter.GetInstallIdAsync ();

				Settings.UserReferenceKey = installId?.ToString ("N");
			});
		}
	}
}
