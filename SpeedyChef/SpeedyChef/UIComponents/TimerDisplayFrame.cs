﻿using System;
using Android.Views;
using Android.Widget;

namespace SpeedyChef
{
	public class TimerDisplayFrame : ITimerObserver
	{
		private ViewGroup displayFrame;
		private TextView countdownDisplay;
		private ProgressBar progressBar;

		private RecipeStepTimerHandler handler;

		public TimerDisplayFrame (ViewGroup frame)
		{
			this.displayFrame = frame;
			this.countdownDisplay = frame.FindViewById<TextView> (Resource.Id.walkthrough_time);
			this.progressBar = frame.FindViewById<ProgressBar> (Resource.Id.walkthrough_bar);
		}

		public void setTimer(RecipeStepTimerHandler h) {
			this.handler = h;
			this.progressBar.Max = h.GetFullTime ();
			TextView tv = this.displayFrame.FindViewById<TextView> (Resource.Id.walkthrough_text);
			tv.Text = h.GetTimerName();
			h.addObserver (this);
			this.displayFrame.Visibility = ViewStates.Visible;
		}

		public void clearTimer() {
			this.displayFrame.Visibility = ViewStates.Gone;
			this.handler.removeObserver (this);
		}

		public void timerUpdate(int seconds) {
			int mins = seconds / 60;
			seconds = seconds % 60;
			string display;
			if (seconds < 10) {
				display = mins + ":0" + seconds;
			} else {
				display = mins + ":" + seconds;
			}

			this.countdownDisplay.Text = display;

			if (this.progressBar != null) {
				this.progressBar.IncrementProgressBy (1);
			}

		}
	}
}

