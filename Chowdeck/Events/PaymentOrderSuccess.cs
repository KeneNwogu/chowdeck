using Azure.Core;
using Chowdeck.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using PubSub;
using System.Security.Policy;
using System.Threading;

namespace Chowdeck.Events
{
    public interface IOrderPaymentSuccessCommand : IRequest
    {
        string orderId { get; set; }
    }

    public class OrderPaymentSuccessCommand : IRequest
    {
        public string orderId { get; set; }
    }

    public interface IOrderPaymentSuccessEvent : INotification
    {
        string orderId { get; set; }
    }

    public class PaymentOrderSuccessCommandHandler : IRequestHandler<OrderPaymentSuccessCommand>
    {
        //private ChowdeckContext _context;
        private static readonly Hub _hub;

        //public PaymentOrderSuccessCommandHandler()
        //{
        //    //_context = context;
        //    _hub = Hub.Default;
        //}

        static PaymentOrderSuccessCommandHandler()
        {
            //_context = context;
            _hub = Hub.Default;
        }

        public static async Task HandlePendingOrder(string orderId)
        {
            try
            {
                using (var _context = new ChowdeckContext())
                {
                    // Use the context here
                    Console.WriteLine("Started Processing.");

                    Order? order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
                    User? rider = _context.Users.FirstOrDefault(u => u.Role == "RIDER");

                    if (order == null) { return; }


                    await Task.Delay(3000);

                    List<string> timelineMessages = new List<string>
                    {
                        "Order received by vendor",
                        "Vendor is preparing your order.",
                        "Rider has accepted your order.",
                        "Rider at vendor.",
                        "Rider has picked up your order.",
                        "Your order has arrived.",
                    };

                    for(int i = 1; i < 7; i++)
                    {
                        if(_context.OrderTimelines.FirstOrDefault(t => t.OrderId == orderId && t.Stage == (OrderTimelineStageEnum) i) != null)
                        {
                            continue;
                        }

                        OrderTimeline orderTimeline = new OrderTimeline
                        {
                            Stage = (OrderTimelineStageEnum) i,
                            OrderId = orderId,
                            Name = timelineMessages[i - 1],
                            RiderId = rider != null ? rider.Id : null,
                            Completed = true
                        };

                        Console.WriteLine(timelineMessages[i - 1]);
                        
                        _context.OrderTimelines.Add(orderTimeline);
                        _context.SaveChanges();
                        _hub.Publish(orderTimeline);

                        await Task.Delay(2000 * i);
                    }

                    order.Status = "completed";
                    _context.SaveChanges();
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.St
                throw;
            }
        }

        public async Task Handle(OrderPaymentSuccessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                using (var _context = new ChowdeckContext())
                {
                    // Use the context here
                    Console.WriteLine("Started Processing.");
                    Order? order = _context.Orders.FirstOrDefault(o => o.Id == request.orderId);

                    if (order == null) { return; }

                    await Task.Delay(3000);

                    OrderTimeline orderReceived = new OrderTimeline
                    {
                        Stage = OrderTimelineStageEnum.OrderReceived,
                        OrderId = request.orderId,
                        Name = "Order received by vendor.",
                        Completed = true
                    };

                    Console.WriteLine("Order Received.");
                    _hub.Publish(orderReceived);
                    _context.OrderTimelines.Add(orderReceived);
                    _context.SaveChanges();
                    // TODO: Send notification

                    // 2-minute delay
                    await Task.Delay(1000 * 6 * 2);
                    OrderTimeline preparingOrder = new OrderTimeline
                    {
                        Stage = OrderTimelineStageEnum.PreparingOrder,
                        OrderId = request.orderId,
                        Name = "Vendor is preparing your order.",
                        Completed = true
                    };

                    Console.WriteLine("Order Preparing.");
                    _hub.Publish(preparingOrder);
                    _context.OrderTimelines.Add(preparingOrder);
                    _context.SaveChanges();

                    // 3-minute delay
                    await Task.Delay(1000 * 60 * 3);
                    User? Rider = _context.Users.FirstOrDefault(u => u.Role == "RIDER");

                    order.RiderId = Rider!.Id;

                    OrderTimeline riderAcceptedOrder = new OrderTimeline
                    {
                        Stage = OrderTimelineStageEnum.RiderAcceptedOrder,
                        OrderId = request.orderId,
                        RiderId = Rider!.Id,
                        Name = "Rider has accepted your order.",
                        Completed = true
                    };

                    Console.WriteLine("Rider Accepted Order.");
                    _hub.Publish(riderAcceptedOrder);
                    _context.OrderTimelines.Add(riderAcceptedOrder);
                    _context.SaveChanges();

                    // 1-minute delay
                    await Task.Delay(1000 * 60 * 1);
                    OrderTimeline riderAtVendor = new OrderTimeline
                    {
                        Stage = OrderTimelineStageEnum.RiderAtVendor,
                        OrderId = request.orderId,
                        RiderId = Rider!.Id,
                        Name = "Rider at vendor.",
                        Completed = true
                    };

                    Console.WriteLine("Rider At Vendor.");
                    _hub.Publish(riderAtVendor);
                    _context.OrderTimelines.Add(riderAtVendor);
                    _context.SaveChanges();

                    // 1-minute delay
                    await Task.Delay(1000 * 60 * 1);
                    OrderTimeline riderPickedUpOrder = new OrderTimeline
                    {
                        Stage = OrderTimelineStageEnum.RiderPickedUpOrder,
                        OrderId = request.orderId,
                        RiderId = Rider!.Id,
                        Name = "Order has picked up your order.",
                        Completed = true
                    };

                    Console.WriteLine("Rider Picked Order.");
                    _hub.Publish(riderPickedUpOrder);
                    _context.OrderTimelines.Add(riderPickedUpOrder);
                    _context.SaveChanges();

                    // 1-minute delay
                    await Task.Delay(1000 * 60 * 1);
                    OrderTimeline orderArrived = new OrderTimeline
                    {
                        Stage = OrderTimelineStageEnum.OrderArrived,
                        OrderId = request.orderId,
                        RiderId = Rider!.Id,
                        Name = "Your order has arrived.",
                        Completed = true
                    };

                    Console.WriteLine("Order Arrived.");
                    _hub.Publish(orderArrived);
                    _context.OrderTimelines.Add(orderArrived);
                    _context.SaveChanges();
                }
            
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.St
                throw;
            }  
        }
    }
}
