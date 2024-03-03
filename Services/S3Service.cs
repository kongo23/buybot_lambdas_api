using Amazon.CloudFront;

namespace buybot_lambdas_api.Services
{
    public class S3Service : IS3Service
    {
        public string GetDownloadFileUrl(string key)
        {
            DateTime expiration = DateTime.UtcNow.AddMinutes(2);
            var signedUrl = GenerateCloudFrontSignedUrl(key, expiration);

            return signedUrl;
        }

        private string GenerateCloudFrontSignedUrl(string key, DateTime expiration)
        {
            var distributionDomainFileUrl = $"https://d77m8j0ml5f1o.cloudfront.net/app/{key}"; // Replace with your CloudFront distribution domain
            var keyPairId = "E321NQQX5MM1ER"; // Replace with your CloudFront key ID
            var privateKeyPath = "private_key.pem"; // Replace with the path to your CloudFront private key

            using (var reader = new StreamReader(privateKeyPath))
            {
                var signedUrl = AmazonCloudFrontUrlSigner.SignUrlCanned(
                    distributionDomainFileUrl,
                    keyPairId,
                    reader,
                    expiration
                );

                Console.WriteLine("Signed URL: " + signedUrl);

                return signedUrl.ToString();
            }
        }
    }
}
