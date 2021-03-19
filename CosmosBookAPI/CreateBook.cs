using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Bson;

namespace CosmosBookAPI
{
    public class CreateBook
    {
        public readonly ILogger<CreateBook> _logger;
        public readonly IBookService _bookService;

        public CreateBook(ILogger<CreateBook> logger, IBookService bookService)
        {
            _logger = logger;
            _bookService = bookService;
        }

        [FunctionName(nameof(CreateBook))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Book")] HttpRequest req)
        {
            IActionResult result;

            try
            {
                var incomingRequest = await new StreamReader(req.Body).ReadToEndAsync();

                var bookRequest = JsonConvert.DeserializeObject<Book>(incomingRequest);

                var book = new Book
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    BookName = bookRequest.BookName,
                    Price = bookRequest.Price,
                    Category = bookRequest.Category,
                    Author = bookRequest.Author
                };

                await _bookService.CreateBook(book);

                result = new StatusCodeResult(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal Server Error. Exception: {ex.Message}");
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}