using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace ClientApi.Api.Models {
	public class CreateAccountPost {
		[JsonProperty("email")]
		public string Email { get; set; }
		[JsonProperty("confirm_email")]
		public string ConfirmEmail { get; set; }
		[JsonProperty("password")]
		public string Password { get; set; }
		[JsonProperty("confirm_password")]
		public string ConfirmPassword { get; set; }
		[JsonProperty("birthday")]
		public string Birthday { get; set; }
		[JsonProperty("country")]
		public string Country { get; set; }
		[JsonProperty("email_optin")]
		public bool EmailOptIn { get; set; }
		[JsonProperty("referral_key")]
		public string ReferralKey { get; set; }
	}
}
