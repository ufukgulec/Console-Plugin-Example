using PluginBase;

namespace GithubPlugin
{
    public class GithubCommand : ICommand
    {
        public string Name { get => "Github"; }
        public string Message { get => @$"{Name} açıklaması..."; }

        public int Execute()
        {
            // HttpClient nesnesi oluştur
            using HttpClient client = new HttpClient();

            // GitHub API'si kullanıcı aracısının belirlenmesi (GitHub, kullanıcının kimliğini tanımlamak için User-Agent başlığını zorunlu kılar)
            client.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            // İstek yapmak istediğiniz URL
            string url = "https://api.github.com/users/ufukgulec";

            // İsteği gönder
            HttpResponseMessage response = client.GetAsync(url).Result;

            // Yanıtın durumunu kontrol et
            if (response.IsSuccessStatusCode)
            {
                // Yanıtı al
                string responseData = response.Content.ReadAsStringAsync().Result;


                // Yanıtı yazdır
                Console.WriteLine("Yanıt: ");
                Console.WriteLine(responseData);
            }
            else
            {
                Console.WriteLine($"İstek başarısız oldu: {response.StatusCode}");
            }
            return 0;
        }
    }
}
