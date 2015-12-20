
using System;

using Foundation;
using UIKit;
using School.Core;
using System.Collections.Generic;
using System.Globalization;
using CoreGraphics;

namespace School.iOS
{
	public partial class VCLichHocTuan : UIViewController
	{
		List<chiTietLH> listCT;
		string begining;
		string end;
		bool isfirst;
		public bool isReload=false;
		public static DateTime LoadedDate;
		public static VCLichHocTuan instance;
		public VCLichHocTuan () : base ("VCLichHocTuan", null)
		{
			instance = this;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			isfirst = true;
			btTuanKe.BackgroundColor=LayoutHelper.ourDarkCyan;
			btTuanKe.SetTitleColor (UIColor.Black, UIControlState.Normal);
			btTuanTrc.BackgroundColor=LayoutHelper.ourDarkCyan;
			btTuanTrc.SetTitleColor (UIColor.Black, UIControlState.Normal);

			errorLB = LayoutHelper.ErrLabel (errorLB);
			CGRect frame = listContent.Frame;
			frame.Width = App.Current.width;
			frame.Height = App.Current.height / 2+50;
			listContent.Frame = frame;

			progress.Hidden = true;
			progress = LayoutHelper.progressDT (progress);


			frame = timeLHTuan.Frame;

			frame.Y = 70;
			frame.Width = App.Current.width;
			timeLHTuan.Frame = frame;
			frame = txtngayLHTuan.Frame;

			frame.Y = 100;
			frame.Width = App.Current.width;
			txtngayLHTuan.Frame = frame;

			frame = btTuanKe.Frame;
			frame.X = App.Current.width-60-frame.Width;
			frame.Y = App.Current.height - 40;
			btTuanKe.Frame = frame;
			frame = btTuanTrc.Frame;
			frame.Y = App.Current.height - 40;
			frame.X = 60;
			btTuanTrc.Frame = frame;
			timeLHTuan.Text = "";
			txtngayLHTuan.Text = "";
			mytitle.Frame = LayoutHelper.setlayoutForTimeTT (mytitle.Frame);
			mytitle.Font = UIFont.FromName ("AmericanTypewriter", 21f);
			LoadData_Tuan (DateTime.Today);
			btTuanKe.TouchUpInside+= BtTuanKe_TouchUpInside;
			btTuanTrc.TouchUpInside+= BtTuanTrc_TouchUpInside;
			btMenu=LayoutHelper.NaviButton (btMenu, mytitle.Frame.Y);
			btMenu.TouchUpInside+= (object sender, EventArgs e) => {
				RootViewController.Instance.navigation.ToggleMenu();
			};
			mytitle.BackgroundColor = UIColor.FromRGBA((float)0.9, (float)0.9, (float)0.9, (float)1);
			// Perform any additional setup after loading the view, typically from a nib.
		}

		void BtTuanTrc_TouchUpInside (object sender, EventArgs e)
		{
			try
			{
				((LichHocTSource)listContent.Source).Items.Clear();
			}
			catch{
			}
			listContent.ReloadData();
			isReload=true;
			LoadData_Tuan (convertFromStringToDate (begining).AddDays (-7));
		}

		void BtTuanKe_TouchUpInside (object sender, EventArgs e)
		{
			try
			{
			((LichHocTSource)listContent.Source).Items.Clear();
			}
			catch{
			}
			listContent.ReloadData();
			isReload=true;
			LoadData_Tuan (convertFromStringToDate (begining).AddDays (7));

		}




		public async void LoadData_Tuan (DateTime dateOfWeek)
		{
			try
			{
				listContent.Hidden= false;
				errorLB.Hidden=true;
				btTuanKe.Hidden=true;
				btTuanTrc.Hidden=true;
				progress.Hidden = false;
				timeLHTuan.Hidden=true;
				txtngayLHTuan.Hidden=true;
				progress.StartAnimating ();
				bool sync = SettingsHelper.LoadSetting ("AutoUpdate"); 
				List<LichHoc> listLH = new List<LichHoc> ();
				if (sync&&isfirst)
				{
					bool accepted =false;
					while (Reachability.InternetConnectionStatus ()==NetworkStatus.NotReachable&&!accepted)
					{
						accepted = await LayoutHelper.ShowAlert("Lỗi", "Bạn cần mở kết nối để cập nhật dữ liệu mới nhất");


					}
					if (Reachability.InternetConnectionStatus ()!=NetworkStatus.NotReachable)
					{
						UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
						var newlistlh= BLichHoc.MakeDataFromXml (SQLite_iOS.GetConnection ());
						List<LichHoc> newListLH= await newlistlh;var checkRemind=SettingsHelper.LoadSetting("Remind");
						if (newListLH==null)
						{
							UIAlertView _error = new UIAlertView ("Lỗi", "Xảy ra lỗi trong quá trình cập nhật dữ liệu từ server", null, "Ok", null);

							_error.Show ();
						}
						else {
							if (checkRemind){
								VCHomeReminder remind= new VCHomeReminder(this);
								await remind.RemindALLLH(newListLH,"");
							}
						}
						UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
					}

				}
				progress.StopAnimating ();
				listLH = BLichHoc.GetNewestLH (SQLite_iOS.GetConnection ());


				listCT = new List<chiTietLH> ();
				foreach (var item in listLH) {
					List<chiTietLH> list = BLichHoc.GetCTLH (SQLite_iOS.GetConnection (), item.Id);
					foreach (var ct in list) {
						String result = checkTuan (ct.Tuan, dateOfWeek);
						if (result != "") {
							ct.Tuan = result;
							listCT.Add (ct);
							break;
						}

					}

				}
				if (listLH.Count>0) 
				{
				btTuanKe.Hidden=false;
				btTuanTrc.Hidden=false;
				timeLHTuan.Hidden=false;
				txtngayLHTuan.Hidden=false;
				isfirst = false;
				LoadedDate=dateOfWeek;
				}
				if (listCT.Count==0) 
				{
					listContent.Hidden= true;
					errorLB.Hidden=false;
				}

				GetWeek (dateOfWeek, out begining, out end);
				txtngayLHTuan.Text = "Từ " + begining + " Đến " + end;
				timeLHTuan.Text = "Học Kỳ " + listLH [0].HocKy + " Năm học " + listLH [0].NamHoc;
				if (listCT.Count > 0) {
					


					listContent.Source = new LichHocTSource (listCT, this);
					listContent.ReloadData ();
					btTuanKe.Hidden=false;
					btTuanTrc.Hidden=false;
				}
			}
			catch {
			}
		}
		public static DateTime convertFromStringToDate (string date)
		{
			return DateTime.ParseExact (date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

		}

		public static string convertFromDateToString (DateTime date)
		{
			return date.ToString ("dd/MM/yyyy");
		}


		public static string checkTuan (string s, DateTime dayOfWeek)
		{
			int number = s.ToCharArray ().Length / 10;
			for (int i = 1; i <= number; i++) {
				string a = s.Substring ((i - 1) * 10, 10);
				if (DateInsideOneWeek (Convert.ToDateTime (a), dayOfWeek)) {
					return a;
				}
			}
			return "";
		}

		public static bool DateInsideOneWeek (DateTime checkDate, DateTime referenceDate)
		{
			// get first day of week from your actual culture info, 
			DayOfWeek firstWeekDay = DayOfWeek.Monday;
			// or you can set exactly what you want: firstWeekDay = DayOfWeek.Monday;
			// calculate first day of week from your reference date
			DateTime startDateOfWeek = referenceDate;
			while (startDateOfWeek.DayOfWeek != firstWeekDay) {
				startDateOfWeek = startDateOfWeek.AddDays (-1d);
			}
			// fist day of week is find, then find last day of reference week
			DateTime endDateOfWeek = startDateOfWeek.AddDays (6d);
			// and check if checkDate is inside this period
			return checkDate >= startDateOfWeek && checkDate <= endDateOfWeek;
		}
		private static void GetWeek (DateTime now, out string begining, out string end)
		{
			CultureInfo cultureInfo = CultureInfo.CurrentCulture;
			var firstDayOfWeek = DayOfWeek.Monday;
			int offset = firstDayOfWeek - now.DayOfWeek;
			if (offset != 1) {
				DateTime weekStart = now.AddDays (offset);
				DateTime endOfWeek = weekStart.AddDays (6);
				begining = convertFromDateToString (weekStart);
				end = convertFromDateToString (endOfWeek);
			} else {
				begining = convertFromDateToString (now.AddDays (-6));
				end = convertFromDateToString (now);
			}
		}
		public static VCLichHocTuan Instance
		{
			get
			{
				if (instance == null)
					instance = new VCLichHocTuan ();
				return instance;
			}
		}
	}
}

