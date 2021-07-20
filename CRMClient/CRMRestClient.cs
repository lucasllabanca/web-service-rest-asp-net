using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Web;
using System.Text.Json;
using TrabalhoPraticoDM106.Models;
using System.Threading.Tasks;
using System.Net;

namespace TrabalhoPraticoDM106.CRMClient
{
	public class CRMRestClient
	{

		private HttpClient client;

		public CRMRestClient()
		{

			client = new HttpClient();

			//TODO - configure o endereço do CRM
			client.BaseAddress = new Uri("http://siecolacrm.azurewebsites.net/api/");

			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			//Mount the credentials in base64 encoding
			byte[] str1Byte = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", "crmwebapi", "crmwebapi"));
			string plaintext = Convert.ToBase64String(str1Byte);

			//Set the authorization header
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", plaintext);

		}

		public HttpResponseMessage PostCustomer(RegisterBindingModel model)
		{

			string jsondata = JsonSerializer.Serialize(model);
			StringContent content = new StringContent(jsondata, System.Text.Encoding.UTF8, "application/json");
			return client.PostAsync("customers", content).Result;

		}

		public HttpResponseMessage GetCustomerByEmail(string email)
		{

			var response = client.GetAsync("customers/byemail?email=" + email).Result;

			if (response.IsSuccessStatusCode)
			{
				return response;
			}
			else if (response.StatusCode == HttpStatusCode.InternalServerError)
			{
				response.Content = new StringContent("Não foi possível acessar o servidor de CRM devido à um erro interno! Não é possível calcular o frete.");
				return response;
			}
			else if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				response.Content = new StringContent("Recurso não está autorizado à acessar o servidor de CRM! Não é possível calcular o frete.");
				return response;
			}
			else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
			{
				response.Content = new StringContent("O Servidor de CRM está indisponível! Não é possível calcular o frete.");
				return response;
			}
			else if (response.StatusCode == HttpStatusCode.BadRequest)
			{
				response.Content = new StringContent("O Servidor de CRM não conseguiu processar a requisição! Não é possível calcular o frete.");
				return response;
			}
			else if (response.StatusCode == HttpStatusCode.Forbidden)
			{
				response.Content = new StringContent("O Servidor de CRM bloqueou a consulta ao CEP do usuário! Não é possível calcular o frete.");
				return response;
			}
			else if (response.StatusCode == HttpStatusCode.NotFound)
			{
				response.Content = new StringContent("Não foi possível encontrar o usuário no CRM! Não é possível calcular o frete.");
				return response;
			}

			response.Content = new StringContent(response.ReasonPhrase);
			return response;

		}

	}
}