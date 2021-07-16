using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrabalhoPraticoDM106.Models
{
	public class Order
	{

		public Order()
		{
			OrderItems = new HashSet<OrderItem>();
		}

		public int Id { get; set; }

		public string UserEmail { get; set; }

		public DateTime? Data { get; set; }

		public DateTime? DataEntrega { get; set; }

		public string Status { get; set; }

		public decimal PrecoTotal { get; set; }

		public decimal PesoTotal { get; set; }

		public decimal PrecoFrete { get; set; }

		public virtual ICollection<OrderItem> OrderItems
		{
			get;
			set;
		}

	}
}