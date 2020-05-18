using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Apbd_example_tutorial_10.DTOs.Requests
{
	public class PromoteStudentRequest
	{
		[Required]
		public String Studies { get; set; }

		[Required]
		public int Semester { get; set; }
	}
}
