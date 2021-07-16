using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrabalhoPraticoDM106.Models
{
	public class Product
	{

		public int Id { get; set; }

		[Required(ErrorMessage = "O campo Nome do Produto é obrigatório")]
		public string Nome { get; set; }

		public string Descricao { get; set; }

		public string Cor { get; set; }

		[Required(ErrorMessage = "O campo Modelo do Produto é obrigatório")]
		public string Modelo { get; set; }

		[Required(ErrorMessage = "O campo Código do Produto é obrigatório")]
		public string Codigo { get; set; }

		public decimal Preco { get; set; }

		public decimal Peso { get; set; }

		public decimal? Altura { get; set; }

		public decimal? Largura { get; set; }

		public decimal? Comprimento { get; set; }

		public decimal? Diametro { get; set; }

		public string Url { get; set; }

	}
}