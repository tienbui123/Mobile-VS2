using System;
using System.Linq;
using System.Collections.Generic;
using UIKit;
using MonoTouch.Dialog;
using Foundation;
using EventKit;
using School.Core;
using System.Globalization;
using System.Threading.Tasks;

namespace School.iOS
{
	public class VCHomeReminder
	{
		protected CreateEventEditViewDelegate eventControllerDelegate;
		UIViewController controller;
		public chiTietLH ct;
		public LichHoc lh;
		public LichThi lt;
		List<string> listTimeLH;
		DateTime srtTime;
		DateTime endTime;
		public string time;
		static LHRemindItem ItemLH;
		static LTRemindItem ItemLT;
		string title;
		string tenmh;
		static bool isUpdate = false;
		public VCHomeReminder(UIViewController controller)
		{
			this.controller=controller;
			listTimeLH= new List<string> {"07g00","07g50","09g00","09g50","10g40","12g00","12g50","14g00","14g50","15g40","17g00","17g50"
				,"18g40","19g30"};
		}
		public void RemindLH()
		{

			RequestAccess (EKEntityType.Event, () => {

				title="Lịch Học";

				try{
					tenmh = BMonHoc.GetMH (SQLite_iOS.GetConnection(),lh.MaMH).TenMH;
					time=ct.Tuan;
					srtTime=GetTime(covertToExact(time),listTimeLH[int.Parse(ct.TietBatDau)-1]);
					string timeEnd=listTimeLH[int.Parse(ct.TietBatDau)+int.Parse(ct.SoTiet)-1];
					switch(timeEnd)
					{
					case "09g00":
						timeEnd="08g40";
						break;
					case "14g00":
						timeEnd="13g40";
						break;
					case "12g00":
						timeEnd="11g30";
						break;
					case "17g00":
						timeEnd="16g30";
						break;
					}
					endTime=GetTime(covertToExact(time),timeEnd);
				}
				catch
				{
				}
				LaunchCreateNewEvent ();
			});

		}

		public void RemindLT()
		{

			RequestAccess (EKEntityType.Event, () => {
				title="Lịch Thi";
				try{
					tenmh = BMonHoc.GetMH (SQLite_iOS.GetConnection(),lt.MaMH).TenMH;
					srtTime=GetTime(lt.NgayThi,lt.GioBD);
					endTime=srtTime.AddMinutes(lt.SoPhut);
				}
				catch{
				}
				LaunchCreateNewEvent ( );
			});

		}

		public Task RemindALLLH(List<LichHoc> listlh,string content)
		{
			return Task.Run(()=>
				{
					RequestAccess (EKEntityType.Event, () => {
						foreach (LichHoc lh in listlh) {
							title="Lịch Học";
							List<chiTietLH> cts = BLichHoc.GetCTLH (SQLite_iOS.GetConnection (), lh.Id);
							foreach (chiTietLH ct in cts)
							{
								List<string> listNgayHoc = ApiHelper.strListTuanToArrayString (ct.Tuan);
								foreach (string s in listNgayHoc) {
									time = s;
									this.ct = ct;
									this.lh = lh;

									AutoCreateEventLH(content);
								}
							}

						}});
					
				}
			);
		}
		public Task RemindAllLT( List<LichThi> listlt)
		{

			return Task.Run(()=>
				{
					
						RequestAccess (EKEntityType.Event, () => {
							foreach (LichThi lt in listlt) {
								this.lt = lt;
							title="Lịch Thi";
							AutoCreateEventLT ();
						}
					});
				}

				);
		}

		protected void RequestAccess (EKEntityType type, Action completion)
		{
			App.Current.EventStore.RequestAccess (type,
				(bool granted, NSError e) => {
					controller.InvokeOnMainThread (() => {
						if (granted)
							completion.Invoke ();
						else
							new UIAlertView ("Access Denied", "User Denied Access to Calendars/Reminders", null, "ok", null).Show ();
					});
				});
		}
		protected void AutoCreateEventLT()
		{
			try{
				tenmh = BMonHoc.GetMH (SQLite_iOS.GetConnection(),lt.MaMH).TenMH;
				srtTime=GetTime(lt.NgayThi,lt.GioBD);
				endTime=srtTime.AddMinutes(lt.SoPhut);
			}
			catch{
			}
			EKEvent newEvent = EKEvent.FromStore (App.Current.EventStore);
			// set the alarm for 10 minutes from now
			newEvent.AddAlarm (EKAlarm.FromDate ((NSDate)srtTime.AddMinutes (-60)));
			// make the event start 20 minutes from now and last 30 minutes
			newEvent.StartDate = (NSDate)srtTime;
			newEvent.EndDate = (NSDate)endTime;
			newEvent.Title = title;

			newEvent.Notes = "Môn:" + tenmh + " Phòng Thi: " + lt.PhongThi + " Ghi chú: ";
			newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;
			NSError error;
			App.Current.EventStore.SaveEvent (newEvent, EKSpan.ThisEvent,out error);
			LTRemindItem item = new LTRemindItem ();
			item.EventID = newEvent.EventIdentifier;
			item.HocKy = lt.HocKy;
			item.NamHoc = lt.NamHoc;
			item.MaMH = lt.MaMH;
			BRemind.SaveLTRemind (SQLite_iOS.GetConnection (),item);
		}
		protected void AutoCreateEventLH(string content)
		{
			try{
				tenmh = BMonHoc.GetMH (SQLite_iOS.GetConnection(),lh.MaMH).TenMH;

				srtTime=GetTime(covertToExact(time),listTimeLH[int.Parse(ct.TietBatDau)-1]);
				string timeEnd=listTimeLH[int.Parse(ct.TietBatDau)+int.Parse(ct.SoTiet)];
				switch(timeEnd)
				{
				case "09g00":
					timeEnd="08g40";
					break;
				case "14g00":
					timeEnd="13g40";
					break;

				}
				endTime=GetTime(covertToExact(time),timeEnd);
			}
			catch
			{
			}
			EKEvent newEvent = EKEvent.FromStore (App.Current.EventStore);
			// set the alarm for 10 minutes from now
			newEvent.AddAlarm (EKAlarm.FromDate ((NSDate)srtTime.AddMinutes (-60)));
			// make the event start 20 minutes from now and last 30 minutes
			newEvent.StartDate = (NSDate)srtTime;
			newEvent.EndDate = (NSDate)endTime;
			newEvent.Title = title;
			newEvent.Notes = "Môn:" + tenmh + " Phòng Học:" + ct.Phong +  " Ghi chú:"+content;
			newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;
			NSError error;
			App.Current.EventStore.SaveEvent (newEvent, EKSpan.ThisEvent,out error);
			LHRemindItem item = new LHRemindItem ();
			item.EventID = newEvent.EventIdentifier;
			item.Date = time;
			item.IDLH = lh.Id;
			BRemind.SaveLHRemind(SQLite_iOS.GetConnection (),item);
		}
		protected void LaunchCreateNewEvent ()
		{  


			// create a new EKEventEditViewController. This controller is built in an allows
			// the user to create a new, or edit an existing event.


			EKEvent newEvent = EKEvent.FromStore (App.Current.EventStore);
			// set the alarm for 10 minutes from now
			newEvent.AddAlarm (EKAlarm.FromDate ((NSDate)srtTime.AddMinutes (-60)));
			// make the event start 20 minutes from now and last 30 minutes
			newEvent.StartDate = (NSDate)srtTime;
			newEvent.EndDate = (NSDate)endTime;
			newEvent.Title = title;
			if (title.Equals("Lịch Thi")) {
				newEvent.Notes = "Môn:" + tenmh + " Phòng Thi: " + lt.PhongThi + " Ghi chú";
				ItemLT = new LTRemindItem ();
				ItemLT.EventID = newEvent.EventIdentifier;
				ItemLT.MaMH = lt.MaMH;
				ItemLT.NamHoc = lt.NamHoc;
				ItemLT.HocKy = lt.HocKy;

			} else {
				newEvent.Notes = "Môn:" + tenmh + " Phòng Học:" + ct.Phong +  " Ghi chú:";
				ItemLH = new LHRemindItem ();
				ItemLH.EventID = newEvent.EventIdentifier;
				ItemLH.Date = time;
				ItemLH.IDLH = lh.Id;
			}
			// create a new EKEventEditViewController. This controller is built in an allows
			// the user to create a new, or edit an existing event.					
			EventKitUI.EKEventEditViewController eventController =
				new EventKitUI.EKEventEditViewController ();

			// set the controller's event store - it needs to know where/how to save the event
			eventController.EventStore = App.Current.EventStore;
			eventController.Event = newEvent;

			// wire up a delegate to handle events from the controller
			eventControllerDelegate = new CreateEventEditViewDelegate (eventController);
			eventController.EditViewDelegate = eventControllerDelegate;



			// show the event controller
			controller.PresentViewController (eventController, true, null);
		}
		public void LoadEvent(string eventID,LHRemindItem rmItem)
		{
			try
			{
				
				if (rmItem!=null)
				{
					ItemLH= new LHRemindItem();
					ItemLH=rmItem;
				}
				else
				{
					ItemLT=new LTRemindItem();
				}

				EKEvent mySavedEvent = App.Current.EventStore.EventFromIdentifier (eventID);
				EventKitUI.EKEventEditViewController eventController =
					new EventKitUI.EKEventEditViewController ();

				eventController.EventStore = App.Current.EventStore;
				eventController.Event = mySavedEvent;

				// wire up a delegate to handle events from the controller
				eventControllerDelegate = new CreateEventEditViewDelegate (eventController);
				eventController.EditViewDelegate = eventControllerDelegate;
				isUpdate=true;
				controller.PresentViewController (eventController, true, null);
			
			}
			catch {
				BRemind.RemoveRemind (SQLite_iOS.GetConnection (), eventID);
				UIAlertView _error = new UIAlertView ("Lỗi", "Không tìm thấy Nhắc lịch đã tạo", null, "Ok", null);
				if (VCLichHoc.instance != null)
					VCLichHoc.Instance.LoadData ();
				if (VCLichHocTuan.instance != null&&VCLichHocTuan.LoadedDate.Year>1)
					
					VCLichHocTuan.Instance.LoadData_Tuan (VCLichHocTuan.LoadedDate);
				if (VCLichThi.instance != null)
					VCLichThi.Instance.LoadData ();
				_error.Show ();
			}
		}

		protected void RetrieveEvent (string eventID)
		{


			// to retrieve the event you can call
			EKEvent mySavedEvent = App.Current.EventStore.EventFromIdentifier (eventID);
			Console.WriteLine ("Retrieved Saved Event: " + mySavedEvent.Title);

		}
		protected static void UpdateEvents(EKEvent myevent)
		{
			NSError e;
			NSDate startT = myevent.StartDate;
			List<LHRemindItem> events = BRemind.GetLHRemind (SQLite_iOS.GetConnection(),ItemLH.IDLH);
			foreach (var item in events) {
				try
				{
				EKEvent mySavedEvent = App.Current.EventStore.EventFromIdentifier (item.EventID);
					if (mySavedEvent.StartDate.Compare(startT)>0)
					{
						mySavedEvent.Notes= myevent.Notes;
						App.Current.EventStore.SaveEvent(mySavedEvent,EKSpan.ThisEvent,out e);
					}

				}
				catch {
				}
			}


		}
		public void RemoveEvent(string eventID)
		{
			NSError e;
			EKEvent mySavedEvent = App.Current.EventStore.EventFromIdentifier (eventID);
			App.Current.EventStore.RemoveEvent (mySavedEvent, EKSpan.ThisEvent, true, out e);

			Console.WriteLine ("Event Deleted.");
		}
		public  void RemoveEvents(List<LHRemindItem> rmItem)
		{
			foreach (var remindIT in rmItem) {
				try
				{
				RemoveEvent (remindIT.EventID);
				}
				catch {
				}
				BRemind.RemoveRemind (SQLite_iOS.GetConnection (), remindIT.EventID);
			}
			if (VCLichHoc.instance != null)
				VCLichHoc.Instance.LoadData ();


		}
		public Task RemoveAllEvent()
		{
			return Task.Run (() => {
				List<LHRemindItem> list1= BRemind.GetAllLHRemind(SQLite_iOS.GetConnection());
				foreach (var item in list1)
				{
					RemoveEvent(item.EventID);
				}
				List<LTRemindItem> list2= BRemind.GetAllLTRemind(SQLite_iOS.GetConnection());
				foreach (var item in list2)
				{
					RemoveEvent(item.EventID);
				}
			});
		}

		protected class CreateEventEditViewDelegate : EventKitUI.EKEventEditViewDelegate
		{
			// we need to keep a reference to the controller so we can dismiss it
			protected EventKitUI.EKEventEditViewController eventController;

			public CreateEventEditViewDelegate (EventKitUI.EKEventEditViewController eventController)
			{
				// save our controller reference
				this.eventController = eventController;
			}

			// completed is called when a user eith
			public async override void Completed (EventKitUI.EKEventEditViewController controller, EventKitUI.EKEventEditViewAction action)
			{
				eventController.DismissViewController (true, null);

				// action tells you what the user did in the dialog, so you can optionally
				// do things based on what their action was. additionally, you can get the
				// Event from the controller.Event property, so for instance, you could
				// modify the event and then resave if you'd like.
				switch (action) {

				case EventKitUI.EKEventEditViewAction.Canceled:
					ItemLH = null;
					ItemLT = null;
					isUpdate = false;
					break;
				case EventKitUI.EKEventEditViewAction.Deleted:
					BRemind.RemoveRemind (SQLite_iOS.GetConnection (), controller.Event.EventIdentifier);
					if (VCLichHoc.instance != null)
						VCLichHoc.Instance.LoadData ();
					if (VCLichHocTuan.instance != null&&VCLichHocTuan.LoadedDate.Year>1)
						VCLichHocTuan.Instance.LoadData_Tuan (VCLichHocTuan.LoadedDate);
					if (VCLichThi.instance != null)
						VCLichThi.Instance.LoadData ();
					break;
				case EventKitUI.EKEventEditViewAction.Saved:
					// if you wanted to modify the event you could do so here, and then
					// save:
					//App.Current.EventStore.SaveEvent ( controller.Event, )
					if (ItemLH != null) {
						ItemLH.EventID = controller.Event.EventIdentifier;
						BRemind.SaveLHRemind (SQLite_iOS.GetConnection (), ItemLH);
						if (isUpdate) {
							EKEvent mySavedEvent = App.Current.EventStore.EventFromIdentifier (ItemLH.EventID);
							bool accepted=await LayoutHelper.ShowAlert ("Cập nhật", "Bạn có muốn áp dụng thay đổi này cho tất cả các nhắc lịch môn này về sau");
							if (accepted) {
								UpdateEvents (mySavedEvent);
							}
							isUpdate = false;
						}
						if (VCLichHoc.instance != null)
							VCLichHoc.Instance.LoadData ();
						if (VCLichHocTuan.instance != null&&VCLichHocTuan.LoadedDate.Year>1)
							VCLichHocTuan.Instance.LoadData_Tuan (VCLichHocTuan.LoadedDate);
						ItemLH = null;
					} else {
						ItemLT.EventID = controller.Event.EventIdentifier;
						BRemind.SaveLTRemind (SQLite_iOS.GetConnection (), ItemLT);
						if (VCLichThi.instance != null)
							VCLichThi.Instance.LoadData ();
						ItemLT = null;
						isUpdate = false;
					
					}
					break;
				}
			}
		}
		DateTime GetTime(string date,string GioBD)
		{
			DateTime time = DateTime.ParseExact (date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

			int gio = int.Parse(GioBD.Substring (0, 2));
			int phut=int.Parse(GioBD.Substring (3, 2));




			DateTime now=DateTime.Now.AddYears(time.Year-DateTime.Now.Year).AddMonths(time.Month-DateTime.Now.Month).AddDays(time.Day-DateTime.Now.Day)
				.AddHours(gio-DateTime.Now.Hour).AddMinutes(phut-DateTime.Now.Minute);
			return now;
		}
		string covertToExact(string date)
		{
			string dd,mm,yy;
			mm = date.Substring (0, 2);
			dd = date.Substring (3, 2);
			yy=date.Substring (6, 4);
			return dd + "/" + mm + "/" + yy;
		}

	}
}

