using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace CartsApi.Consumer
{
    using Models;
   

    public class OrderStatusConsumer : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _env;
        private IConnection _connection;
        private IModel _channel;
        private readonly CartDBContext _context;


        public OrderStatusConsumer(CartDBContext context,ILoggerFactory loggerFactory, IConfiguration env)
        {
            this._logger = loggerFactory.CreateLogger<OrderStatusConsumer>();
            this._env = env;
            _context = context;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _env.GetSection("RABBITMQHOST").Value,
                Port = Convert.ToInt32(_env.GetSection("RABBITMQPORT").Value),
                UserName = _env.GetSection("RABBITUSER").Value,
                Password = _env.GetSection("RABBITPASSWORD").Value
            };

            // create connection  
            _connection = factory.CreateConnection();
            // create channel  
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "order-processed", durable: false, exclusive: false, autoDelete: false, arguments: null);
           
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
           
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
            // received message  
            var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

            // handle the received message  
            HandleMessageAsync(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("orders", false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(string content)
        {        

            // we just print this message   
            _logger.LogInformation($"order consumer received {content}");

            try
            {
                var orderProcessed = JsonConvert.DeserializeObject<OrderStatus>(content);

                if (orderProcessed != null)
                {
                    _logger.LogInformation($"after received order status: {orderProcessed.CartId}/{orderProcessed.OrderId}/{orderProcessed.Status}");

                    var result = await UpdateCartStatus(orderProcessed);

                    if (result>0) _logger.LogInformation("The cart has been updated succesfully");
                  
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in DeserializeObject:"+e.Message );
                if (e.InnerException !=null) Console.WriteLine(e.InnerException.Message );
            }

        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        private async Task<int> UpdateCartStatus(OrderStatus orderProcessed)
        {
            int result = 0;

            try
            {
                
                var cart = _context.Carts.Where(x=>x.CartId ==orderProcessed.CartId ).FirstOrDefault ();
                if (cart!=null) 
                {
                    cart.OrderId = orderProcessed.OrderId;
                    cart.Status = orderProcessed.Status;
                }
                _context.Update(cart);
                result =await _context.SaveChangesAsync();
                
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                if (e.InnerException!=null)
                    _logger.LogError(e.InnerException.Message);
            }

           

            return result;
        }

        

    }
}