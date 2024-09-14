using Chowdeck.DTOs;
using Chowdeck.Events;
using Chowdeck.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Azure;
using PubSub;
using Azure.Core;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Chowdeck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ChowdeckContext _context;
        private readonly IMediator _mediator;
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly Hub _hub;

        public OrdersController(ChowdeckContext context, IMediator mediator, 
            IHttpClientFactory httpClient, IConfiguration configuration) 
        { 
            _context = context;
            _mediator = mediator;
            _httpClient = httpClient;
            _configuration = configuration;
            _hub = Hub.Default;
        }
        // GET: api/<OrdersController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> CreateOrder(CreateOrderDto orderDto)
        {
            // validate order
            // check if restaurant exists
            // check if menu item exists in restaurant


            // create order and calculate price and service charge
            Restaurant? restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == orderDto.RestaurantId);
            if (restaurant == null) return BadRequest(new { message = "Invalid restaurant" });

            if(orderDto.OrderItems.Count < 1) return BadRequest(new { message = "Order at least 1 item" });

            List<string> menuIDs = orderDto.OrderItems.Select(item => item.MenuId).ToList();

            List<RestaurantMenu> menus = _context.RestaurantMenus.Where(
                menu => menuIDs.Contains(menu.Id) && menu.RestaurantId == orderDto.RestaurantId
            ).ToList();

            if (menuIDs.Count != menus.Count) return BadRequest(new { message = "Invalid Menu Items provided" });

            double totalAmount = orderDto.OrderItems.Aggregate(0, (double acc, OrderItemDto item) =>
            {
                RestaurantMenu? menu = menus.FirstOrDefault(m => m.Id == item.MenuId);

                if (menu == null) throw new BadHttpRequestException("Invalid Menu items provided");

                return acc + (menu.Price * item.Quantity);
            });

            Order order = new Order
            {
                OrderItems = orderDto.OrderItems.Select(item => 
                new OrderItem { 
                    MenuId = item.MenuId, 
                    Quantity = item.Quantity,
                    Amount = menus.FirstOrDefault(m => m.Id == item.MenuId)!.Price * item.Quantity * 1.0
                }).ToList(),

                TotalAmount = totalAmount * 1.0,
                ServiceCharge = totalAmount * 0.01,
                RestaurantId = orderDto.RestaurantId,
                PaymentStatus = "pending",
                Status = "in_progress",
                UserId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            string email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;
            // TODO: make payment transaction here
            var httpClient = _httpClient.CreateClient();

            var requestData = JsonSerializer.Serialize(new
            {
                email,
                amount = (int)Math.Ceiling((order.TotalAmount + order.ServiceCharge) * 100),
                reference = order.Id
            });

            Console.WriteLine(requestData);

            var requestContent = new StringContent(requestData, Encoding.UTF8, "application/json");

            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://api.paystack.co/transaction/initialize")
            {
                Headers = { { HeaderNames.Authorization, $"Bearer {Environment.GetEnvironmentVariable("PAYSTACK_SECRET_KEY")}" } },
                Content = requestContent
            };

            var response = await httpClient.SendAsync(httpRequestMessage);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var paystackTransaction = JsonSerializer.Deserialize<JsonNode>(content);

            Console.WriteLine(paystackTransaction);

            return Ok(new { order = new { 
                order.Id, order.Status, order.TotalAmount, 
                order.ServiceCharge,
                paymentLink = paystackTransaction!["data"]!["authorization_url"]
            } });
        }

        [HttpGet("")]
        [Authorize]
        public IActionResult GetOrders()
        {
            string filter = HttpContext.Request.Query["status"].ToString() ?? "in_progress";
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            List<Order> orders = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.Timeline)
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId && o.Status == filter)
                .ToList();

            return Ok(new { orders });
        }

        [HttpGet("{orderId}")]
        [Authorize]
        public IActionResult GetOrderDetails(string orderId)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            Order? order = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.Timeline)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Menu)
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return NotFound(new { message = "No order was found" });

            //List<OrderItem> orderItems = _context.OrderItems.Where(i => i.OrderId == orderId).ToList();
            //_context.RestaurantMenus.Where(m => orderItems.Select(i => i.MenuId).Contains(m.Id));
            //Console.WriteLine(value);
            return Ok(order);
        }

        [HttpPost("{orderId}/payments")]
        [Authorize]
        public async Task<IActionResult> InitializeOrderPayment(string orderId)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            string email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;
            Order? order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.UserId == userId);
            if (order == null) return NotFound(new { message = "No order was found" });

            var httpClient = _httpClient.CreateClient();

            var requestData = JsonSerializer.Serialize(new { 
                email,
                amount = (int) Math.Ceiling((order.TotalAmount + order.ServiceCharge) * 100),
                reference = order.Id
            });

            Console.WriteLine(requestData);

            var requestContent = new StringContent(requestData, Encoding.UTF8, "application/json");

            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://api.paystack.co/transaction/initialize")
            {
                Headers = { { HeaderNames.Authorization, $"Bearer {Environment.GetEnvironmentVariable("PAYSTACK_SECRET_KEY")}" } },
                Content = requestContent
            };

            var response = await httpClient.SendAsync(httpRequestMessage);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var paystackTransaction = JsonSerializer.Deserialize<JsonNode>(content);

            Console.WriteLine(paystackTransaction);
            return Ok(new { paymentLink = paystackTransaction!["data"]!["authorization_url"] });
        }

        [HttpPost("processPayment")]
        public async Task<IActionResult> ProcessPaystackWebhook([FromBody] dynamic requestData)
        {
            var data = JsonSerializer.Deserialize<JsonNode>(requestData)["data"];
            string reference = (string) data["reference"];

            if ((string) data["status"] != "success") return BadRequest();

            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.paystack.co/transaction/verify/{reference}")
                {
                    Headers = { { HeaderNames.Authorization, $"Bearer {Environment.GetEnvironmentVariable("PAYSTACK_SECRET_KEY")}" } },
                };

            var httpClient = _httpClient.CreateClient();
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            dynamic? verificationData;

            var content = await response.Content.ReadAsStringAsync();
            verificationData = JsonSerializer.Deserialize<JsonNode>(content);

            if (verificationData == null) return BadRequest();

            if ((string) verificationData["data"]!["status"] != "success") return BadRequest();

            //_mediator.Send(new OrderPaymentSuccessCommand { orderId = reference });
            Order? order = _context.Orders.FirstOrDefault(o => o.Id == reference);
            if (order == null) return NotFound(new { message = "No order was found" });

            order.PaymentStatus = "completed";

            _context.SaveChanges();

            Task.Run(async () => PaymentOrderSuccessCommandHandler.HandlePendingOrder(reference));

            return Ok();
        }

        [HttpGet("timelines")]
        public async Task StreamOrderTimelines(string orderId, CancellationToken ct)
        {
            Console.WriteLine("connecting to sse. Start of code at least");

            var response = Response;
            response.StatusCode = 200;
            response.Headers.Add("Content-Type", "text/event-stream");

            Console.WriteLine("connecting to sse");

            try
            {
                OrderTimeline? messageData = null;

                _hub.Subscribe<OrderTimeline>(async data => messageData = data);

                while (!ct.IsCancellationRequested)
                {
                    if (messageData != null)
                    {
                        Console.WriteLine("Received data");
                        Console.WriteLine(messageData);

                        string message = JsonSerializer.Serialize(messageData);

                        await response
                            .WriteAsync($"data: {message}");
                        await response.WriteAsync($"\n\n");
                        await response.Body.FlushAsync();
                        response.Body.Close();

                        messageData = null;
                    }
                }

                _hub.Unsubscribe();

            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during streaming
                Console.WriteLine(ex.Message); // Log or handle the exception appropriately
            }
        }
    }
}
