using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Serilog;
using System.Text.Json;
using System.Text;
using Gateway.Messages;
using Gateway.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddBookController : ControllerBase
    {
        private readonly IMessageProducer _messageProducer;
        public AddBookController(IMessageProducer messageProducer)
        {
            _messageProducer = messageProducer;
        }

        // POST api/<AddBookController>
        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] AddBook addBook)
        {
            try
            {
                if (addBook == null)
                {
                    Log.Warning("AddBook object was null.");
                    return BadRequest("Invalid input");
                }

                // Log input til debugging
                Log.Information($"Received Book: " +
                    $"ISBN: {addBook.ISBN}, " +
                    //$"ISBN_13: {addBook.ISBN_secondary}, " +
                    $"Title: {addBook.Title}, " +
                    $"Author: {addBook.Author}"//, " +
                    //$"Seller: {addBook.Seller_Name}, " +
                    //$"Price: {addBook.Price}"
                );
                // Sender addBook
                await _messageProducer.SendMessage(addBook);

                return Ok("Book added succesfully");
            }
            catch(Exception ex)
            {
                Log.Error(ex, "An error occured while processing the AddBook request.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
