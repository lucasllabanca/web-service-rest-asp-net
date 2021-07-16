using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrabalhoPraticoDM106.Models
{
	public class OrderItem
	{

		public int Id { get; set; }

		public int Qtd { get; set; }

		public int ProductId { get; set; }

		public virtual Product Product { get; set; }

		public int OrderId { get; set; }

	}
}