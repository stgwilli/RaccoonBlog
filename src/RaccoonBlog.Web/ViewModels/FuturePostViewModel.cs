using System;

namespace RaccoonBlog.Web.ViewModels
{
	public class FuturePostViewModel
	{
		public string Title { get; set; }

		public DateTimeOffset PublishAt { get; set; }

		public string Time
		{
			get
			{
				var totalMinutes = (PublishAt - DateTimeOffset.Now).TotalMinutes;
				return DistanceOfTimeInWords(totalMinutes) + " from now";
			}
		}


		static string DistanceOfTimeInWords(double minutes)
		{
			if (minutes < 1)
			{
				return "less than a minute";
			}
			else if (minutes < 50)
			{
				return minutes + " minutes";
			}
			else if (minutes < 90)
			{
				return "about one hour";
			}
			else if (minutes < 1080)
			{
				return Math.Round(new decimal((minutes / 60))) + " hours";
			}
			else if (minutes < 1440)
			{
				return "one day";
			}
			else if (minutes < 2880)
			{
				return "about one day";
			}
			else
			{
				return Math.Round(new decimal((minutes / 1440))) + " days";
			}
		}
	}
}
