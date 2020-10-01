using GeoApiFW.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace GeoApiFW.Controllers
{
	[EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
	public class DistanceController : ApiController
	{

		[HttpGet]
		[Route("api/Distance/GetDistance/{source}/{dest}")]
		public async Task<decimal> GetDistanceAsync(string source, string dest)
		{
			var KMS = await BL.GetDistance(source, dest);
			return KMS;

		}
		[HttpGet]
		[Route("api/Distance/PopularSearches/{top}")]
		public GeoDistance[] PopularSearches(int top = 1)
		{
			using (WorkDBEntities context = new WorkDBEntities())
			{
				var populars = context.GeoDistance.OrderByDescending(x => x.Hits).Take(top);
				var res = populars.ToArray();
				return res;
			}
		}



	}
}
