using Bogus;
using StarWars.Model;

namespace StarWars.Common
{
    public static class MovieFaker
    {
        static readonly Random _random = new();

        static string GenerateUniqueCode(int codeLength, string alphabet)
        {
            var codeChars = new char[codeLength];
            int maxValue = alphabet.Length;

            for (var i = 0; i < codeLength; i++)
            {
                var randomIndex = _random.Next(maxValue);
                codeChars[i] = alphabet[randomIndex];
            }

            return new string(codeChars).ToUpper();
        }

        static string GenerateMovieTitle(int minWords = 5)
        {
            var faker = new Faker();
            // Generate words and join them with spaces, capitalize first letter of each word
            var words = Enumerable.Range(0, minWords)
                                  .Select(_ => faker.Lorem.Word())
                                  .Select(w => char.ToUpper(w[0]) + w.Substring(1))
                                  .ToArray();
            return string.Join(" ", words);
        }

        public static List<Movie> Generate(int count, string alphabet)
        {
            var faker = new Faker();
            var lstMovie = new List<Movie>();

            for (int i = 0; i < count; i++)
            {
                var id = GenerateUniqueCode(9, alphabet);
                var title = GenerateMovieTitle();
                var year = faker.Date.Past(20).Year.ToString();
                var posterUrl = faker.Image.PicsumUrl();
                var price = faker.Finance.Amount(5, 1000, 2);    

                lstMovie.Add(new Movie { 
                    ID = id, 
                    Title = title, 
                    Year = year, 
                    Poster = posterUrl, 
                    Price = price
                });
            }
            return lstMovie;
        }
    }
}
