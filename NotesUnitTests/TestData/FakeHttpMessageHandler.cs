using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NotesUnitTests
{
    /// <summary>
    /// Имитирует HTTP-ответ без настоящего обращения к GitHub.
    /// </summary>
    public sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string body;
        private readonly HttpStatusCode code;

        /// <summary>
        /// Создает fake HTTP handler.
        /// </summary>
        /// <param name="body">Тело ответа.</param>
        /// <param name="code">HTTP-статус.</param>
        public FakeHttpMessageHandler(string body, HttpStatusCode code)
        {
            this.body = body;
            this.code = code;
        }

        /// <summary>
        /// Возвращает заранее заданный HTTP-ответ.
        /// </summary>
        /// <param name="request">HTTP-запрос.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>HTTP-ответ.</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(code);
            response.Content = new StringContent(body);

            return Task.FromResult(response);
        }
    }
}