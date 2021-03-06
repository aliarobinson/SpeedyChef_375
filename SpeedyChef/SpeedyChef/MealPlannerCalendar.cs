﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

/// <summary>
/// Meal planner calendar page.
/// </summary>
namespace SpeedyChef
{
	[Activity (Theme = "@style/MyTheme", Label = "MealPlannerCalendar")]			
	public class MealPlannerCalendar : CustomActivity
	{
		private Boolean resumeHasRun = false;


		/// <summary>
		/// Button currently highlighted after being clicked on.
		/// </summary>
		DateButton selected = null;

		/// <summary>
		/// Layout where meal information is displayed.
		/// </summary>
		LinearLayout mealDisplay = null;

		/// <summary>
		/// The current day of app.
		/// </summary>
		DateTime current = DateTime.Now;

		/// <summary>
		/// DateTime for the date to view on screen.
		/// </summary>
		DateTime viewDate = DateTime.Now;

		/// <summary>
		/// Debug text bar to use to help present data.
		/// </summary>
		TextView debug = null;

		/// <summary>
		/// Month banner that needs to be adjusted with the date.
		/// </summary>
		TextView monthInfo = null;

		/// <summary>
		/// Current date TextView object that will be highlighted.
		/// </summary>
		TextView currentDate = null;

		/// <summary>
		/// List of all buttons in display area to be selected.
		/// </summary>
		DateButton[] daysList = null;

		/// <summary>
		/// The add bar, location of add button.
		/// </summary>
		RelativeLayout addBar = null;



		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			SetContentView (Resource.Layout.MealPlannerCalendar);

			// Provides global 
			mealDisplay = FindViewById<LinearLayout> (Resource.Id.mealDisplay);
			debug = FindViewById<TextView> (Resource.Id.debug);
			monthInfo = FindViewById<TextView> (Resource.Id.weekOf);
			daysList = new DateButton[7];
			addBar = FindViewById<RelativeLayout> (Resource.Id.addBar);
			// Makes sure day is selected before you can add a meal
			if (selected == null) {
				addBar.Visibility = Android.Views.ViewStates.Invisible;
				mealDisplay.Visibility = Android.Views.ViewStates.Invisible;
			}

			Button addButton = FindViewById<Button> (Resource.Id.addMeal);
			addButton.Click += (sender, e) => {
				Intent intent = new Intent (this, typeof(MealDesign));
				intent.PutExtra ("Date", selected.GetDateField ().ToBinary ());
				StartActivityForResult (intent, 0);
			};

			//MENU VIEW
			Button menu_button = FindViewById<Button> (Resource.Id.menu_button);
			MenuButtonSetupSuperClass (menu_button);

			// Define variables
			Calendar c = Calendar.GetInstance (Java.Util.TimeZone.Default); 
			Button[] shifters = new Button[2];

			// Setting up month bar
			monthInfo.Text = current.ToString ("MMMMMMMMMM") + " of " + current.Year;

			// Retrieve buttons from layouts
			daysList [0] = new DateButton (FindViewById<Button> (Resource.Id.day1));
			daysList [1] = new DateButton (FindViewById<Button> (Resource.Id.day2));
			daysList [2] = new DateButton (FindViewById<Button> (Resource.Id.day3));
			daysList [3] = new DateButton (FindViewById<Button> (Resource.Id.day4));
			daysList [4] = new DateButton (FindViewById<Button> (Resource.Id.day5));
			daysList [5] = new DateButton (FindViewById<Button> (Resource.Id.day6));
			daysList [6] = new DateButton (FindViewById<Button> (Resource.Id.day7));
			// Assigning dates to days
			handleCalendar (current);
			// Adding action listeners
			for (int i = 0; i < daysList.Length; i++) {
				daysList [i].wrappedButton.Click +=
					new EventHandler ((s, e) => dayClick (s, e));
			}

			// Go backwards a week button
			shifters [0] = FindViewById<Button> (Resource.Id.leftShift);
			shifters [0].Click += delegate {
				GoBackWeek ();
			};

			// Advance week button
			shifters [1] = FindViewById<Button> (Resource.Id.rightShift);
			shifters [1].Click += delegate {
				GoForwardWeek ();
			};
			debug.Text = "";
			// LinearLayout ll = FindViewById<LinearLayout> (Resource.Id.MealDisplay);
			// Console.WriteLine (ll.ChildCount + " Look for me");
		}

		protected override void OnActivityResult (int requestCode, 
			Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			if (resultCode == Result.Ok) {
				// Console.WriteLine (data.GetStringExtra ("Result"));

				RefreshMeals ();
				// Console.WriteLine ("HERE");
				// Console.WriteLine ("Request code = "+ requestCode);
			}
		}

		/// <summary>
		/// Event handler method to help get date to pass to next object
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected void dayClick (object sender, EventArgs e)
		{
			if (selected != null) {
				selected.wrappedButton.SetBackgroundColor (Resources.GetColor 
					(Resource.Color.light_gray));
				selected.wrappedButton.SetTextColor (Resources.GetColor 
					(Resource.Color.black_text));
			}
			if (currentDate != null) {
				currentDate.SetBackgroundColor (Resources.GetColor 
					(Resource.Color.current_date));
			}

			selected = GetDateButton ((Button)sender);
			mealDisplay.Visibility = Android.Views.ViewStates.Visible;
			// Console.WriteLine(selected.GetDateField().ToBinary());
			selected.wrappedButton.SetBackgroundColor 
			(Resources.GetColor (Resource.Color.selected_date));
			selected.wrappedButton.SetTextColor 
			(Resources.GetColor (Resource.Color.white_text));
			// Can click the button after an action listener finds this.
			addBar.Visibility = Android.Views.ViewStates.Visible;
			RefreshMeals ();
		}


		protected override void OnResume ()
		{
			base.OnResume ();
			if (!resumeHasRun) {
				resumeHasRun = true;
				// Console.WriteLine ("Comes here");
				return;
			}
		}

		/// <summary>
		/// Refreshs the meal displaying area after calling Json.
		/// </summary>
		private async void RefreshMeals ()
		{
			// Console.WriteLine ("Refreshing");
			// Below handles connection to the database and the parsing of Json
			LinearLayout mealDisplay = FindViewById<LinearLayout> (Resource.Id.MealDisplay);
			mealDisplay.RemoveAllViews ();
			string user = "tester";
			string useDate = selected.GetDateField ().ToString ("yyyy-MM-dd");
			// System.Diagnostics.Debug.WriteLine (useDate);
			string url = "http://speedychef.azurewebsites.net/" +
				"CalendarScreen/GetMealDay?user=" + user + "&date=" + useDate;
			JsonValue json = await FetchMealData (url);

			// System.Diagnostics.Debug.WriteLine (json.ToString ());
			parseMeals (json);
		}

		/// <summary>
		/// Parses the meals from Json to display on the calendar page. Creates buttons 
		/// and views programmatically.
		/// </summary>
		/// <param name="json">Json to parse for meals.</param>
		private void parseMeals (JsonValue json)
		{
			LinearLayout mealDisplay = FindViewById<LinearLayout> (Resource.Id.MealDisplay);
			// PRINTS
			mealDisplay.RemoveAllViews ();
			// mealDisplay.SetBackgroundColor (Android.Graphics.Color.White);
			// System.Diagnostics.Debug.WriteLine (json.Count);
			for (int i = 0; i < json.Count; i++) {
				makeObjects (json [i], i, mealDisplay);
			}
		}

		/// <summary>
		/// Makes meal segments for the calendar page
		/// </summary>
		/// <param name="json">Json to parse information to use for displaying.</param>
		/// <param name="count">Count used for unique ids.</param>
		/// <param name="mealDisplay">Meal display (LinearLayout) that all of the 
		/// 		objects are going to be added to.</param>
		private async void makeObjects (JsonValue json, 
			int count, LinearLayout mealDisplay)
		{
			LinearLayout mealObject = new LinearLayout (this);
			LinearLayout.LayoutParams mealObjectLL = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.MatchParent, 
					LinearLayout.LayoutParams.WrapContent);
			handleMealObjectCreation (mealObject, 25, 25, mealObjectLL, count, json);
			// Additional Json information to be used
			string user = "tester";
			int mealId = json ["Mealid"];
			string url = "http://speedychef.azurewebsites.net/" +
				"CalendarScreen/GetRecipesForMeal?user="
				+ user + "&mealId=" + mealId;
			JsonValue recipeResult = await FetchMealData (url);
			mealObject.AddView (ButtonView (json, recipeResult, count));
			mealObject.SetPadding (0, 0, 0, 40);
			mealDisplay.AddView (mealObject);
		}

		public void handleMealObjectCreation(LinearLayout mealObject, int width, int height, LinearLayout.LayoutParams mealObjectLL, int count, JsonValue json) {
			mealObject.Orientation = Orientation.Vertical;
			mealObject.SetMinimumWidth (width);
			mealObject.SetMinimumHeight (height);
			mealObject.LayoutParameters = mealObjectLL;
			mealObject.Id = count * 20 + 5;
			mealObject.AddView (CreateButtonContainer (json, count));
		}

		/// <summary>
		/// Creates a button view to be added to a meal to start the walkthrough
		/// </summary>
		/// <returns>The view (LinearLayout) containing buttons and 
		/// 			other fields for a meal button.</returns>
		/// <param name="json">Json for a meal.</param>
		/// <param name="recipeResult">Recipe result in Json for a given meal.</param>
		/// <param name="count">Count for unique ids.</param>
		private LinearLayout ButtonView (JsonValue json, JsonValue recipeResult, int count)
		{
			LinearLayout walkthroughButton = new LinearLayout (this);
			walkthroughButton.Orientation = Orientation.Vertical;
			LinearLayout.LayoutParams wtll = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.MatchParent, 
					LinearLayout.LayoutParams.WrapContent);
			walkthroughButton.LayoutParameters = wtll;
			walkthroughButton.AddView (CreateMealInfo (json, recipeResult, count));
			MealButton button = new MealButton (this);
			button.mealName = json ["Mealname"];
			button.mealId = json ["Mealid"];
			button.mealSize = json ["Mealsize"];
			button.Text = "Start Walkthrough";
			button.SetHeight (150);
			button.Click += (object sender, EventArgs e) => {
				Intent i = new Intent (this, typeof(StepsActivity));
				// System.Diagnostics.Debug.WriteLine (button.mealId);
				i.PutExtra ("mealId", button.mealId);
				// requestCode of walkthrough is 1
				StartActivityForResult (i, 1);
			};
			button.Gravity = GravityFlags.Center;
			LinearLayout.LayoutParams bll = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.MatchParent, 
					LinearLayout.LayoutParams.WrapContent);
			bll.SetMargins (10, 10, 10, 10);
			button.LayoutParameters = bll;
			button.SetPadding (0, 0, 0, 10);
			walkthroughButton.AddView (button);
			return walkthroughButton;
		}

		public void handleMealButtonCreation(MealButton button, String name, String id, 
			String mealSize, String text, int height, JsonValue json) {
			button.mealName = json [name];
			button.mealId = json [id];
			button.mealSize = json [mealSize];
			button.Text = text;
			button.SetHeight (height);
		}

		/// <summary>
		/// Creates the meal info area in the programmitcally generated by Json.
		/// </summary>
		private LinearLayout CreateMealInfo (JsonValue json, 
			JsonValue recipeResult, int count)
		{
			LinearLayout mealInfo = new LinearLayout (this);
			LinearLayout.LayoutParams mealInfoLL = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.MatchParent, 
					LinearLayout.LayoutParams.WrapContent);
			handleMealInfoLayout (mealInfo, 25, 25, mealInfoLL, count, json);

			// Set image icon
			ImageView dinerIcon = new ImageView (this);
			dinerIcon.SetImageResource (Resource.Drawable.gray_person);
			dinerIcon.LayoutParameters = new 
				LinearLayout.LayoutParams (50, LinearLayout.LayoutParams.MatchParent);
			// Finish setting the image icon
			TextView mealSize = new TextView (this);
			TextView recipeInfo = new TextView (this);
			LinearLayout.LayoutParams rill = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, 
					LinearLayout.LayoutParams.WrapContent);
			recipeInfo.LayoutParameters = rill;
			LinearLayout.LayoutParams tvll = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, 
					LinearLayout.LayoutParams.WrapContent);
			handleRecipeInfo (recipeInfo, rill, recipeResult);

			handleMealSize (mealSize, tvll, json, "Mealsize");

			// Add image icon
			mealInfo.AddView (mealSize);
			mealInfo.AddView (dinerIcon);
			mealInfo.AddView (recipeInfo);
			return mealInfo;
		}

		public void handleMealSize (TextView mealSize, LinearLayout.LayoutParams tvll, JsonValue json, String mealSizeString) {
			mealSize.SetTextAppearance (this, Android.Resource.Style.TextAppearanceSmall);
			mealSize.LayoutParameters = tvll;
			mealSize.Text = json [mealSizeString].ToString ();
			mealSize.Gravity = GravityFlags.Right;
		}

		public void handleRecipeInfo (TextView recipeInfo, LinearLayout.LayoutParams rill, JsonValue recipeResult) {
			recipeInfo.Text = handleRecipeJson (recipeResult);
			recipeInfo.SetTextAppearance (this, Android.Resource.Style.TextAppearanceSmall);
			recipeInfo.SetLines (1);
			recipeInfo.SetPadding (10, 0, 0, 0);
		}

		public void handleMealInfoLayout (LinearLayout mealInfo, int width, int height, 
			LinearLayout.LayoutParams mealInfoLL, int count, JsonValue json) {
			mealInfo.Orientation = Orientation.Horizontal;
			mealInfo.SetMinimumWidth (25);
			mealInfo.SetMinimumHeight (25);
			mealInfoLL.SetMargins (5, 5, 5, 5);
			mealInfo.LayoutParameters = mealInfoLL;
			mealInfo.Id = count * 20 + 7;
		}

		/// <summary>
		/// Creates the button container. Used to clean up code and with button for desiging meal/
		/// </summary>
		private LinearLayout CreateButtonContainer (JsonValue json, int count)
		{
			LinearLayout buttonCont = new LinearLayout (this);
			LinearLayout.LayoutParams bcll = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.MatchParent, 
					LinearLayout.LayoutParams.WrapContent);
			handleMealInfoLayout (buttonCont, 25, 100, bcll, count, json);
			buttonCont.Visibility = Android.Views.ViewStates.Visible;
			MealButton button = new MealButton (this, null, 
				Resource.Style.generalButtonStyle); 

			LinearLayout.LayoutParams lp = 
				new LinearLayout.LayoutParams (LinearLayout.LayoutParams.MatchParent, 
					LinearLayout.LayoutParams.MatchParent);

			handleMealButtonInButtonContainer (button, "Mealname", "Mealid", 
				"Mealsize", 150, json, lp, "Mealname");
			buttonCont.AddView (button);
			return buttonCont;
		}

		public void handleMealButtonInButtonContainer (MealButton button, String name, String id, 
			String mealSize, int height, JsonValue json, LinearLayout.LayoutParams lp, String text) {
			button.mealName = json [name];
			button.mealSize = (json [mealSize]);
			button.SetHeight (height);
			button.mealId = (json [id]);
			button.Click += (object sender, EventArgs e) => {
				Intent intent = new Intent (this, typeof(MealDesign));
				LinearLayout mealDisplay = FindViewById<LinearLayout> (Resource.Id.MealDisplay);
				// PRINTS
				mealDisplay.RemoveAllViews ();
				intent.PutExtra ("Name", button.mealName);
				intent.PutExtra ("Mealsize", button.mealSize);
				intent.PutExtra ("mealId", button.mealId);
				StartActivityForResult (intent, 3);
				// requestCode for Design page 3

			};
			button.LayoutParameters = lp;
			button.Text = json ["Mealname"];
			button.Visibility = Android.Views.ViewStates.Visible;
			button.SetBackgroundColor (Resources.GetColor (Resource.Color.orange_header));
			button.Gravity = GravityFlags.Center;
		}

		/// <summary>
		/// Takes the recipes and makes into nice string to display
		/// </summary>
		public string handleRecipeJson (JsonValue json)
		{
			string finalString = "";
			for (int i = 0; i < json.Count; i++) {
				finalString += json [i] ["Recname"] + ", ";
			}
			if (finalString.Length > 2) {
				finalString = finalString.Substring (0, finalString.Length - 2);
			}
			// System.Diagnostics.Debug.WriteLine (finalString);
			return finalString;
		}


		/// <summary>
		/// Fetchs the meal data.
		/// </summary>
		private async Task<JsonValue> FetchMealData (string url)
		{
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "GET";

			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync ().ConfigureAwait (true)) {
				// Get a stream representation of the HTTP web response:
				using (Stream stream = response.GetResponseStream ()) {
					// Use this stream to build a JSON document object:
					JsonValue jsonDoc = await Task.Run (() => JsonObject.Load (stream)).ConfigureAwait (true);
					return jsonDoc;
				}
			}

		}

		/// <summary>
		/// Gets the date button.
		/// </summary>
		private DateButton GetDateButton (Button b)
		{
			for (int i = 0; i < daysList.Length; i++) {
				if (daysList [i].wrappedButton.GetHashCode () == b.GetHashCode ()) {
					return daysList [i];
				}
			}
			return daysList [0];
		}

		/// <summary>
		/// Method that handles updating all the boxes in the calendar so that
		/// the dates line up in the week
		/// </summary>
		public void handleCalendar (DateTime date)
		{
			currentDate = null;
			string day = date.AddDays (-date.DayOfWeek.GetHashCode ())
				.ToString ("M/d");
			DateTime temp = date.AddDays (-date.DayOfWeek.GetHashCode ());
			string weekDay = temp.ToString ("ddd");
			monthInfo.Text = temp.ToString ("MMMMMMMMMM") + " of " + temp.Year;
			for (int i = 0; i < 7; i++) {
				// Determines the day from how far away from the beginning (Sunday)
				// and displays appropriately
				day = date.AddDays (-date.DayOfWeek.GetHashCode () + i).ToString ("MM/d");
				weekDay = date.AddDays (-date.DayOfWeek.GetHashCode () + i)
					.ToString ("ddddddd");

				daysList [i].wrappedButton.Text = weekDay.Substring (0, 3) + "\n" + day;
				// Change the above line from a 3 to a 1
				daysList [i].SetDateField (date.AddDays 
					(-date.DayOfWeek.GetHashCode () + i));
				// Sets all the buttons to the default colors
				daysList [i].wrappedButton.SetBackgroundColor (Resources.GetColor
					(Resource.Color.light_gray));
				daysList [i].wrappedButton.SetTextColor (Resources.GetColor 
					(Resource.Color.black_text));
				// Handles setting the highlighting of the current day on the phone
				if (i == current.DayOfWeek.GetHashCode ()
					&& date.Date.Equals (current.Date)) {
					currentDate = daysList [i].wrappedButton;
					daysList [i].wrappedButton.SetBackgroundColor 
					(Resources.GetColor (Resource.Color.selected_date));
					daysList [i].wrappedButton.SetBackgroundColor 
					(Resources.GetColor (Resource.Color.current_date));
					// daysList [i].wrappedButton.SetBackgroundResource ();
				}
			}
			// Removes any selected day
			selected = null;
			mealDisplay.Visibility = Android.Views.ViewStates.Invisible;
			// Makes the add button invisible
			addBar.Visibility = Android.Views.ViewStates.Invisible;
		}

		/// <summary>
		/// Goes back a week.
		/// </summary>
		public void GoBackWeek ()
		{
			LinearLayout mealDisplay = FindViewById<LinearLayout> 
				(Resource.Id.MealDisplay);
			mealDisplay.RemoveAllViews ();
			viewDate = viewDate.AddDays (-7);
			handleCalendar (viewDate);
		}

		/// <summary>
		/// Goes forward a week.
		/// </summary>
		public void GoForwardWeek ()
		{
			viewDate = viewDate.AddDays (7);
			LinearLayout mealDisplay = FindViewById<LinearLayout> 
				(Resource.Id.MealDisplay);
			mealDisplay.RemoveAllViews ();
			handleCalendar (viewDate);
		}
	}

	/// <summary>
	/// Wrapper class for button to help handle passing the dates.
	/// </summary>
	public class DateButton
	{
		/**
		 * Datefield for button container
		 **/
		/// <summary>
		/// The date field for the button container.
		/// </summary>
		private DateTime dateField;

		/// <summary>
		/// The wrapped button for the button container.
		/// </summary>
		public Button wrappedButton;

		/// <summary>
		/// Initializes a new instance of the <see cref="SpeedyChef.DateButton"/> class. 
		/// This is a container class.
		/// </summary>
		/// <param name="button">Button of the container class.</param>
		public DateButton (Button button)
		{
			wrappedButton = button;
			this.dateField = DateTime.Now.AddDays (-100);
		}

		/// <summary>
		/// Sets the date field.
		/// </summary>
		/// <param name="date">Date.</param>
		public void SetDateField (DateTime date)
		{
			// Console.WriteLine ("Wrote date");
			this.dateField = date;
		}

		/// <summary>
		/// Gets the date field.
		/// </summary>
		/// <returns>The date field.</returns>
		public DateTime GetDateField ()
		{
			return this.dateField;
		}
	}

	/// <summary>
	/// Button class that contains extra fields to be used for getting 
	/// additional information
	/// </summary>
	public class MealButton : Button
	{

		/// <summary>
		/// Gets or sets the meal identifier.
		/// </summary>
		public int mealId { get; set; }

		/// <summary>
		/// Gets or sets the name of the meal.
		/// </summary>
		public string mealName { get; set; }

		/// <summary>
		/// Gets or sets the size of the meal.
		/// </summary>
		public int mealSize { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SpeedyChef.MealButton"/> class.
		/// </summary>
		public MealButton (Context context) : base (context)
		{
			this.mealId = -1;
			this.mealName = "";
			this.mealSize = -1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpeedyChef.MealButton"/> class.
		/// </summary>
		public MealButton (Context context, 
			Android.Util.IAttributeSet set, int style) :
		base (context, set, style)
		{
			this.mealId = -1;
			this.mealName = "";
		}
	}
}