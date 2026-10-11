using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using EXAM.Models;

namespace EXAM.Services
{
    public class SqsService
    {
        private readonly IAmazonSQS _sqs;
        private readonly string _queueUrl;

        public SqsService(IAmazonSQS sqs, string queueUrl)
        {
            _sqs = sqs;
            _queueUrl = queueUrl;
        }

        public async Task SendTicketCreatedAsync(Ticket ticket)
        {
            var payload = new { ticket.Id, ticket.Subject, Priority = ticket.Priority.ToString() };
            var messageBody = JsonSerializer.Serialize(payload);

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = messageBody
            };

            await _sqs.SendMessageAsync(request);
        }
    }
}
