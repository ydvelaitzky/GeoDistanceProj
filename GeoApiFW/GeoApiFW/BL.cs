using GeoApiFW.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace GeoApiFW
{
	public static class BL
	{
		public static async Task<decimal> GetDistance(string source, string dest)
		{
			using (WorkDBEntities context = new WorkDBEntities())
			{
				//prevent multi records between source and destination manipulation:
				string[] sortedParams = new string[] { source, dest }.OrderBy(x => x).ToArray();
				source = sortedParams[0]; dest = sortedParams[1];

				var any = context.GeoDistance.Any(x => x.Source == source && x.Destination == dest);
				if (!any)
				{
					decimal dist = await GetDistAsync(source, dest);
					if (dist <= 0) return dist;

					//add it into the DB:
					var newRecord = new GeoDistance() { Destination = dest, Source = source, KMS = dist };
					context.GeoDistance.Add(newRecord);
					context.SaveChanges();
				}
				var result = context.GeoDistance.First(x => x.Source == source && x.Destination == dest);

				//update its hits:
				result.Hits = result.Hits + 1;
				context.SaveChanges();

				return result.KMS;
			}
		}


		static async Task<decimal> GetDistAsync(string src, string dest)
		{
			var TempKeyToken = "IjSGxFtGyqXuykobtpn0yKjqlelav";
			string distMatrixAPI = $"https://api.distancematrix.ai/maps/api/distancematrix/json?origins={src}&destinations={dest}&key={TempKeyToken}";
			
			var httpClient = new HttpClient();

			//to avoid the 403 - calling it like a browser :
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

			var response = await httpClient.GetAsync(new Uri(distMatrixAPI));

			response.EnsureSuccessStatusCode();
			using (var responseStream = await response.Content.ReadAsStreamAsync())
			{
				StreamReader reader = new StreamReader(responseStream);
				string text = reader.ReadToEnd();
				if (text.Contains("INVALID_REQUEST")) { return -1; }

				JObject json = JObject.Parse(text);
				string kmStr = json["rows"].First()["elements"].First()["distance"].First().First().ToString();
				kmStr = Regex.Replace(kmStr, "[^.0-9]", "");//remove all non-numeric (such as m or km)
				return decimal.Parse(kmStr);
			}

		}
		//test: //https://api.distancematrix.ai/maps/api/distancematrix/json?origins=Haifa&destinations=jerusalem&key=IjSGxFtGyqXuykobtpn0yKjqlelav
	}
}