using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Serilog;
using System.Text.Json;
using System.Text;
using Bogus;
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

        // POST api/<AddBookController>/GenerateBooks
        [HttpPost("GenerateBooks")]
        public async Task<IActionResult> GenerateBooks([FromQuery] int count, [FromQuery] int seed)
        {
            try
            {
                if (count <= 0)
                {
                    return BadRequest("Count must be greater than 0.");
                }

                // Configure Bogus with seed - use same seed to generate same data again
                Randomizer.Seed = new Random(seed);

                // Create a Faker for AddBook
                var bookFaker = new Faker<AddBook>()
                    .RuleFor(b => b.ISBN, f => f.Commerce.Ean13()) // Generate random ISBNs
                    .RuleFor(b => b.Title, f => f.Lorem.Sentence(3)) // Generate random book titles
                    .RuleFor(b => b.Author, f => f.Name.FullName()); // Generate random author names

                // Generate the specified number of AddBook objects
                var books = bookFaker.Generate(count);

                // Send each book message
                foreach (var book in books)
                {
                    await _messageProducer.SendMessage(book);
                }

                return Ok($"{count} books generated and sent successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while generating books.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
