using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http.Description;
using TrabalhoPraticoDM106.Data;
using TrabalhoPraticoDM106.Models;
using Microsoft.AspNet.Identity.Owin;
using TrabalhoPraticoDM106.CRMClient;
using TrabalhoPraticoDM106.br.com.correios.ws;

namespace TrabalhoPraticoDM106.Controllers
{

    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private TrabalhoPraticoDM106Context db = new TrabalhoPraticoDM106Context();

        private CRMRestClient client = new CRMRestClient();

        // GET: api/Orders
        [Authorize(Roles = "ADMIN")]
        public List<Order> GetOrders()
        {
            return db.Orders.Include(order => order.OrderItems).ToList();
        }

        // GET: api/Orders/byemail?email=email@trabalho.com.br
        [ResponseType(typeof(List<Order>))]
        [HttpGet]
        [Route("byemail")]
        public IHttpActionResult GetOrdersByEmail(string email)
        {

            var orders = db.Orders.Where(order => order.UserEmail == email).Include(order => order.OrderItems).ToList();

            if (orders == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("ADMIN") && !User.Identity.Name.Equals(email))
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            return Ok(orders);
        }

        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("ADMIN") && !User.Identity.Name.Equals(order.UserEmail))
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }
            
            return Ok(order);
        }
     
        // PUT: api/Orders/close/5
        [ResponseType(typeof(void))]
        [HttpPut]
        [Route("close/{id}")]
        public HttpResponseMessage CloseOrder(int id)
        {
            var response = new HttpResponseMessage();

            Order order = db.Orders.Find(id);

            if (order == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Content = new StringContent("O Pedido não foi encontrado!");
                return response;
            }

            if (!User.IsInRole("ADMIN") && !User.Identity.Name.Equals(order.UserEmail))
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Content = new StringContent("Não autorizado! Você não pode fechar um pedido que não é seu.");
                return response;
            }

            if (order.PrecoFrete == 0) 
            {

                response.StatusCode = HttpStatusCode.PreconditionFailed;
                response.Content = new StringContent("Pedido não pode ser fechado! Frete ainda não calculado.");
                return response;

            }

            order.Status = "fechado";

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Content = new StringContent("O Pedido não foi encontrado!");
                    return response;
                }
                else
                {
                    throw;
                }
            }

            response.StatusCode = HttpStatusCode.NoContent;
            response.Content = new StringContent("O Pedido foi fechado com sucesso!");
            return response;

        }

        // PUT: api/Orders/shipping/5
        [ResponseType(typeof(void))]
        [HttpPut]
        [Route("shipping/{id}")]
        public HttpResponseMessage CalcShippingForOrder(int id)
        {

            var cepAmazonCajamarSP = "07776901";
            var cepDestino = "";

            var response = new HttpResponseMessage();

            Order order = db.Orders.Find(id);

            if (order == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Content = new StringContent("O Pedido não foi encontrado!");
                return response;
            }

            if (!User.IsInRole("ADMIN") && !User.Identity.Name.Equals(order.UserEmail))
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Content = new StringContent("Não autorizado! Você não pode calcular o frete de um pedido que não é seu.");
                return response;
            }

            if (!order.Status.Equals("novo"))
			{
                response.StatusCode = HttpStatusCode.PreconditionFailed;
                response.Content = new StringContent("Pedido não possui status de 'novo'! Não foi possível calcular o frete do pedido.");
                return response;
            }

            if (order.OrderItems.Count == 0)
			{
                response.StatusCode = HttpStatusCode.Conflict;
                response.Content = new StringContent("Pedido não possui itens! Não foi possível calcular o frete do pedido.");
                return response;
            }

            response = client.GetCustomerByEmail(order.UserEmail);

            if (!response.IsSuccessStatusCode)
            {
                return response;
            }
			else
			{

                Customer customer = response.Content.ReadAsAsync<Customer>().Result;

                if (customer == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Content = new StringContent($"O Usuário {order.UserEmail} não foi encontrado no CRM! Não é possível calcular o frete.");
                    return response;
                }

                if (string.IsNullOrEmpty(customer.zip)) 
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Content = new StringContent($"O CEP do usuário {order.UserEmail} no CRM está vazio! Não é possível calcular o frete.");
                    return response;
                }

                cepDestino = customer.zip;

            }
      
            decimal alturaMax = (decimal)order.OrderItems.Max(orderI => orderI.Product.Altura);
            decimal largurMax = (decimal)order.OrderItems.Max(orderI => orderI.Product.Largura);
            decimal compTotal = (decimal)order.OrderItems.Sum(orderI => orderI.Qtd * orderI.Product.Comprimento);
            decimal diametMax = (decimal)order.OrderItems.Max(orderI => orderI.Product.Diametro);
            decimal pesoTotal = order.OrderItems.Sum(orderI => orderI.Qtd * orderI.Product.Peso);
            decimal precoTotal = order.OrderItems.Sum(orderI => orderI.Qtd * orderI.Product.Preco);
            
            CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
            cResultado resultado = correios.CalcPrecoPrazo("", "", "04014", cepAmazonCajamarSP, cepDestino, pesoTotal.ToString(), 1, compTotal, alturaMax, largurMax, diametMax, "N", precoTotal, "N");

            if (!resultado.Servicos[0].Erro.Equals("0"))
			{
               //TO DO
			}

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent($"Valor do frete: {resultado.Servicos[0].Valor} - Prazo de entrega: {resultado.Servicos[0].PrazoEntrega} dia(s)");
            return response;

            order.PesoTotal = pesoTotal;
            order.PrecoTotal = precoTotal;
            order.Status = "fechado";

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Content = new StringContent("O Pedido não foi encontrado!");
                    return response;
                }
                else
                {
                    throw;
                }
            }

            response.StatusCode = HttpStatusCode.NoContent;
            response.Content = new StringContent("O Pedido foi fechado com sucesso!");
            return response;

        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (User.IsInRole("ADMIN"))
            {
                if (string.IsNullOrEmpty(order.UserEmail))
                {
                    return BadRequest();
                }

                if (!UserManager.Users.Where(u => u.Email == order.UserEmail).Any())
				{
                    return NotFound();
				}

            }
            else //USER
            {

                if (!string.IsNullOrEmpty(order.UserEmail) && !order.UserEmail.Equals(User.Identity.Name))
                {
                    return BadRequest();
                }

                order.UserEmail = User.Identity.Name;

            }
    
            order.Status = "novo";
            order.PesoTotal = 0;
            order.PrecoFrete = 0;
            order.PrecoTotal = 0;
            order.Data = DateTime.Now;

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("ADMIN") && !User.Identity.Name.Equals(order.UserEmail))
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            db.Orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}