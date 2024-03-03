using System.Text.Json;

namespace buybot_lambdas_api.Services
{
    public class UrlGenerator
    {
        private IS3Service _s3Service;

        public UrlGenerator(
            IS3Service s3Service)
        {
            _s3Service = s3Service;
        }

        public async Task<string?> GetDownloadUrl(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            //move to secret config
            string secretKey = Environment.GetEnvironmentVariable("CAPTCHA_SECRET") ?? string.Empty;

            if(!string.IsNullOrEmpty(secretKey))
            {
                bool isRecaptchaValid = await VerifyRecaptchaAsync(secretKey, id);

                if (isRecaptchaValid)
                {
                    var url = _s3Service.GetDownloadFileUrl("moonbot-free.zip");

                    return url;
                }
            }

            return null;
        }

        private async Task<bool> VerifyRecaptchaAsync(string secretKey, string userResponse)
        {
            using (var client = new HttpClient())
            {
                var requestData = new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", userResponse }
                };

                var content = new FormUrlEncodedContent(requestData);

                HttpResponseMessage response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    return result!.Success;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    public class RecaptchaVerificationResponse
    {
        public bool Success { get; set; }
    }
}
