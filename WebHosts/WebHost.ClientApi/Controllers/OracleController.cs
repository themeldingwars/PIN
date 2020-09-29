using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using WebHost.ClientApi.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.ClientApi.Controllers {
	[ApiController]
	public class OracleController : ControllerBase {
		[Route("/api/v1/oracle/ticket")]
		[HttpPost]
		public OracleTicket GetOracleTicket() {
			return new OracleTicket {
				MatrixUrl = "localhost:25000",
				//Ticket = "UjX52MMObRnEtgZe1pjrRFS6iRz3t7aR69fgLdzwxJQumRt7mhpNqPkejXFT\nBf3H2a5bZI/zQhO4CvKj+Z5Jctk4yMU4mgPzHiN+FJb+CiKvcQGhjNqAskD3\nalZQkZ/N+v1dSC25DLGR0Ky/3V1fsw0Y2bh+xsAgoKg1BkIJHiltTW3spuVT\nUd8fo9oLG0UzhCWP/NNIfcGX+Ur/e7UYxoUCiwHhRH3673Q1TtCoociHwvpj\np4QExjp3Cd2LTolR00l8zYAvodMBPJyOuMf/BB8KDkoP8hnpNh8ZIpmxeWXr\ndZ2R5r8hSAIht3uNMZd/Wa3ewQgqwj/womRSCqhSOpdPFebbgI2TVnth7IA0\nZq4EvvI436cBOc1P1wVfvFW6EUebqCzfIxn63UYQWXc1+KnCjLh9r4l60xm3\n6Yes+7zJwS2r02UslF+QgpUuXJw4I4h7OK+YRrHnOFtiKOUnC3hJMUbY6yZA\nR6/ZdfvBLt9XlA==\n",
				Ticket = "UjX52MMObRnEtgZe1pjrRFS6iRz3t7aR69fgLdzwxJQumRt7mhpNqPkejXFTBf3H2a5bZI/zQhO4CvKj+Z5Jctk4yMU4mgPzHiN+FJb+CiKvcQGhjNqAskD3alZQkZ/N+v1dSC25DLGR0Ky/3V1fsw0Y2bh+xsAgoKg1BkIJHiltTW3spuVTUd8fo9oLG0UzhCWP/NNIfcGX+Ur/e7UYxoUCiwHhRH3673Q1TtCoociHwvpjp4QExjp3Cd2LTolR00l8zYAvodMBPJyOuMf/BB8KDkoP8hnpNh8ZIpmxeWXrdZ2R5r8hSAIht3uNMZd/Wa3ewQgqwj/womRSCqhSOpdPFebbgI2TVnth7IA0Zq4EvvI436cBOc1P1wVfvFW6EUebqCzfIxn63UYQWXc1+KnCjLh9r4l60xm36Yes+7zJwS2r02UslF+QgpUuXJw4I4h7OK+YRrHnOFtiKOUnC3hJMUbY6yZAR6/ZdfvBLt9XlA==",
				Datacenter = "localhost",
				OperatorOverride = new OperatorOverride {
					IngameHost = "https://localhost:44303",
					ClientapiHost = "https://localhost:44302"
				},
				SessionId = new Guid("11111111-2222-3333-4444-555555555555"),
				Hostname = "localhost",
				Country = "US"
			};
		}
	}
}
