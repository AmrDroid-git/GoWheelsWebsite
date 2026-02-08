using Microsoft.EntityFrameworkCore;
using GoWheels.Models;
using Microsoft.AspNetCore.Identity;

namespace GoWheels.Data
{
    public class AltSeeder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AltSeeder> _logger;
        private readonly Random _random = new();

        // generation configuration
        private readonly int UsersPopulationSize = 5;
        private readonly int PostsPopulationSize = 160;
        
        // Configuration lists
        private readonly List<string> _marques = new() { "Toyota", "BMW", "Mercedes", "Audi", "Renault", "Peugeot", "Ford", "Hyundai" };
        private readonly List<string> _gearboxes = new() { "manuelle", "automatique" };
        private readonly List<string> _transmissions = new() { "Traction", "Propulsion", "Intégrale" };
        private readonly List<string> _energies = new() { "Essence", "Diesel", "Hybride", "Électrique" };
        private readonly List<string> _carrosseries = new() { "Berline", "SUV", "Citadine", "Break", "Coupé", "4x4" };
        private readonly List<string> _gouvernorats = new() { "Tunis", "Ariana", "Ben Arous", "Manouba", "Nabeul", "Sousse", "Sfax" };
        private readonly string[] _firstNames = { "Ali", "Mohamed", "Fatma", "Mariem", "Ahmed", "Salah", "Leila", "Nadia" };
        private readonly string[] _lastNames = { "Ben Ali", "Trabelsi", "Miled", "Chouchen", "Bouzid", "Gharbi", "Saidi" };
        
        // RAW DATA STORAGE
        private List<Dictionary<string, object>> _adminData = new();
        private List<Dictionary<string, object>> _expertData = new();
        private List<Dictionary<string, object>> _userData = new();
        private List<Dictionary<string, object>> _postData = new();
        private List<Dictionary<string, object>> _commentData = new();
        private List<Dictionary<string, object>> _postRatingData = new();
        private List<Dictionary<string, object>> _userRatingData = new();
        
        // ID MAPPINGS (index -> generated GUID)
        private Dictionary<int, string> _adminIdMap = new();
        private Dictionary<int, string> _expertIdMap = new();
        private Dictionary<int, string> _userIdMap = new();
        private Dictionary<int, string> _postIdMap = new();

        public AltSeeder(IServiceProvider serviceProvider, ILogger<AltSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // Helper methods
        private string RandomWord()
        {
            string[] syllables = { "ba", "be", "bi", "bo", "bu", "ca", "ce", "ci", "co", "cu", 
                                "da", "de", "di", "do", "du", "fa", "fe", "fi", "fo", "fu" };
            int syllableCount = _random.Next(2, 5);
            var word = "";
            for (int i = 0; i < syllableCount; i++)
            {
                word += syllables[_random.Next(syllables.Length)];
            }
            return char.ToUpper(word[0]) + word.Substring(1);
        }
        
        private DateOnly RandomDateOnly(int minYear)
        {
            var now = DateTime.Now;
            var minDate = new DateTime(minYear, 1, 1);
            var maxDate = now;
            
            int range = (maxDate - minDate).Days;
            var randomDate = minDate.AddDays(_random.Next(range));
            return DateOnly.FromDateTime(randomDate);
        }
        
        private int RandomRange(int min, int max, int quantum = 1)
        {
            int steps = (max - min) / quantum;
            return min + _random.Next(0, steps + 1) * quantum;
        }
        
        private string RandomPhone()
        {
            string[] prefixes = { "20", "21", "22", "23", "24", "25", "26", "27", "28", "29" };
            string prefix = prefixes[_random.Next(prefixes.Length)];
            string suffix = "";
            for (int i = 0; i < 6; i++)
            {
                suffix += _random.Next(0, 10).ToString();
            }
            return prefix + suffix;
        }
        
        private string RandomAddress()
        {
            string[] streets = { "Rue", "Avenue", "Boulevard", "Impasse" };
            string[] names = { "Liberté", "Juin", "Indépendance", "Habbib Bourguiba", "Med V", "France" };
            return $"{streets[_random.Next(streets.Length)]} {names[_random.Next(names.Length)]} {_random.Next(1, 201)}";
        }
        
        // MAIN SEED METHOD
        public async Task SeedRandomDataAsync(GoWheelsDbContext context, UserManager<ApplicationUser> userManager, bool clearFirst = true)
        {
            Console.WriteLine("=== STARTING RANDOM SEED ===");
            
            // 1. Generate all raw data with indices
            Console.WriteLine("1. Generating raw data...");
            GenerateAllRawData();
            
            // 2. Clear database if requested
            if (clearFirst)
            {
                Console.WriteLine("2. Clearing existing data...");
                await ClearDatabase(context);
            }
            
            // 4. Create users and build ID maps
            Console.WriteLine("4. Creating users...");
            await CreateUsersAndBuildIdMaps(userManager);
            
            // 5. Create posts using ID maps
            Console.WriteLine("5. Creating posts...");
            await CreatePostsUsingIdMaps(context);
            
            // 6. Create comments
            Console.WriteLine("6. Creating comments...");
            await CreateCommentsUsingIdMaps(context);
            
            // 7. Create ratings
            Console.WriteLine("7. Creating ratings...");
            await CreateRatingsUsingIdMaps(context);
            
            // 8. Update average ratings
            Console.WriteLine("8. Updating averages...");
            await UpdateAverageRatings(context);
            
            Console.WriteLine("=== RANDOM SEED COMPLETE ===");
            PrintStatistics();
        }
        
        // PHASE 1: RAW DATA GENERATION (NO GUIDS)
        private void GenerateAllRawData()
        {
            GenerateUserData();
            GeneratePostData();
            GenerateInteractionData();
        }
        
        private void GenerateUserData()
        {
            // Admin (index 0)
            _adminData.Clear();
            _adminData.Add(new Dictionary<string, object>
            {
                ["username"] = "admin",
                ["email"] = "admin@gowheels.local",
                ["name"] = "Admin User",
                ["phone"] = "20000000",
                ["address"] = "Admin HQ"
            });
            
            // Expert (index 0)
            _expertData.Clear();
            _expertData.Add(new Dictionary<string, object>
            {
                ["username"] = "expert",
                ["email"] = "expert@gowheels.local",
                ["name"] = "Car Expert",
                ["phone"] = "21000000",
                ["address"] = "Expert Center"
            });
            
            // 5 Users
            _userData.Clear();
            for (int i = 0; i < UsersPopulationSize; i++)
            {
                string firstName = _firstNames[_random.Next(_firstNames.Length)];
                string lastName = _lastNames[_random.Next(_lastNames.Length)];
                
                _userData.Add(new Dictionary<string, object>
                {
                    ["username"] = $"user{i + 1}",
                    ["email"] = $"user{i + 1}@gowheels.local",
                    ["name"] = $"{firstName} {lastName}",
                    ["phone"] = RandomPhone(),
                    ["address"] = RandomAddress()
                });
            }
        }
        
        private void GeneratePostData()
        {
            _postData.Clear();
            
            for (int i = 0; i < PostsPopulationSize; i++)
            {
                string marque = _marques[_random.Next(_marques.Count)];
                DateOnly releaseDate = RandomDateOnly(2000);
                DateOnly purchaseDate = RandomDateOnly(releaseDate.Year);
                int kilometrage = RandomRange(0, 300000, 1000);
                
                var post = new Dictionary<string, object>
                {
                    ["status"] = _random.Next(0, 4), // 0-3 for PostStatus
                    ["type"] = _random.Next(2) == 0 ? "rent" : "sale",
                    ["marque"] = marque,
                    ["modelname"] = $"{RandomWord()} {RandomWord()}",
                    ["release_date"] = releaseDate,
                    ["purchase_date"] = purchaseDate,
                    ["kilometrage"] = kilometrage,
                    ["price"] = RandomRange(5000, 100000, 1000),
                    ["gearbox"] = _gearboxes[_random.Next(_gearboxes.Count)],
                    ["transmission"] = _transmissions[_random.Next(_transmissions.Count)],
                    ["energie"] = _energies[_random.Next(_energies.Count)],
                    ["puissance"] = RandomRange(5, 15),
                    ["carrosserie"] = _carrosseries[_random.Next(_carrosseries.Count)],
                    ["main"] = RandomRange(0, 5),
                    ["gouvernat"] = _gouvernorats[_random.Next(_gouvernorats.Count)],
                    ["createdat"] = purchaseDate.ToDateTime(TimeOnly.MinValue).AddDays(_random.Next(0, 365)),
                    ["owner_index"] = _random.Next(0, UsersPopulationSize)
                };
                _postData.Add(post);
            }
        }
        
        private void GenerateInteractionData()
        {
            _commentData.Clear();
            _postRatingData.Clear();
            _userRatingData.Clear();
            
            // Track rated pairs to avoid duplicates
            var ratedUserUser = new Dictionary<(int, int), bool>(); // (rater_index, rated_index) -> bool
            
            for (int postIndex = 0; postIndex < _postData.Count; postIndex++)
            {
                var post = _postData[postIndex];
                DateTime postDate = (DateTime)post["createdat"];
                
                // Get post owner info
                int ownerIndex = (int)post["owner_index"];
                
                // Both Users and Expert (0) can comment/rate posts
                var potentialInteractors = new List<(string type, int index)>();
                
                // Add all users
                for (int i = 0; i < UsersPopulationSize; i++)
                {
                    if (!(ownerIndex == i))
                    {
                        potentialInteractors.Add(("user", i));
                    }
                }
                potentialInteractors.Add(("expert", 0));
                
                foreach (var (interactorType, interactorIndex) in potentialInteractors)
                {
                    // COMMENTS: Try 3 times with 2% chance each time
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        if (_random.NextDouble() < 0.02)
                        {
                            _commentData.Add(new Dictionary<string, object>
                            {
                                ["body"] = string.Join(" ", Enumerable.Range(0, RandomRange(1, 11))
                                                    .Select(_ => RandomWord())),
                                ["creator_type"] = interactorType,
                                ["creator_index"] = interactorIndex,
                                ["post_index"] = postIndex,
                                ["createdat"] = postDate.AddDays(_random.Next(0, 30))
                            });
                        }
                    }
                    
                    // POST RATING: 20% chance exactly once
                    if (_random.NextDouble() < 0.20)
                    {
                        _postRatingData.Add(new Dictionary<string, object>
                        {
                            ["value"] = _random.Next(1, 6), // 1-5 stars
                            ["rater_type"] = interactorType,
                            ["rater_index"] = interactorIndex,
                            ["post_index"] = postIndex
                        });
                    }
                    
                    // USER RATING
                    var ratingKey = (interactorIndex, ownerIndex);
                    
                    if (!ratedUserUser.ContainsKey(ratingKey))
                    {
                        ratedUserUser[ratingKey] = false;
                    }
                    
                    // 5% chance to rate if not already rated
                    if (!ratedUserUser[ratingKey] && _random.NextDouble() < 0.05)
                    {
                        _userRatingData.Add(new Dictionary<string, object>
                        {
                            ["value"] = _random.Next(1, 6),
                            ["rater_type"] = interactorType,
                            ["rater_index"] = interactorIndex,
                            ["rated_index"] = ownerIndex
                        });
                        ratedUserUser[ratingKey] = true;
                    }
                }
            }
        }
        
        // PHASE 2: DATABASE CREATION WITH ID MAPPING
        private async Task CreateUsersAndBuildIdMaps(UserManager<ApplicationUser> userManager)
        {
            _adminIdMap.Clear();
            _expertIdMap.Clear();
            _userIdMap.Clear();
            
            // Create admin
            for (int i = 0; i < _adminData.Count; i++)
            {
                var userData = _adminData[i];
                var user = await CreateUserEntity(userManager, userData, "ADMIN");
                _adminIdMap[i] = user.Id;
            }
            
            // Create expert
            for (int i = 0; i < _expertData.Count; i++)
            {
                var userData = _expertData[i];
                var user = await CreateUserEntity(userManager, userData, "EXPERT");
                _expertIdMap[i] = user.Id;
            }
            
            // Create users
            for (int i = 0; i < _userData.Count; i++)
            {
                var userData = _userData[i];
                var user = await CreateUserEntity(userManager, userData, "USER");
                _userIdMap[i] = user.Id;
            }
        }
        
        private async Task<ApplicationUser> CreateUserEntity(UserManager<ApplicationUser> userManager, 
            Dictionary<string, object> userData, string role)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = (string)userData["username"],
                Email = (string)userData["email"],
                Name = (string)userData["name"],
                PhoneNumber = (string)userData["phone"],
                Address = (string)userData["address"],
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                NormalizedUserName = ((string)userData["username"]).ToUpper(),
                NormalizedEmail = ((string)userData["email"]).ToUpper()
            };
            
            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            
            return user;
        }
        
        private async Task CreatePostsUsingIdMaps(GoWheelsDbContext context)
        {
            _postIdMap.Clear();
            
            foreach (int postIndex in Enumerable.Range(0, _postData.Count))
            {
                var postData = _postData[postIndex];
                int kilometrage = (int)postData["kilometrage"];
                
                // Get owner ID from maps
                int ownerIndex = (int)postData["owner_index"];
                string ownerId = _userIdMap[ownerIndex];
                
                var post = new Post
                {
                    Id = Guid.NewGuid().ToString(),
                    Status = (PostStatus)(int)postData["status"],
                    IsForRent = (string)postData["type"] == "rent",
                    Constructor = (string)postData["marque"],
                    ModelName = (string)postData["modelname"],
                    ReleaseDate = (DateOnly)postData["release_date"],
                    PurchaseDate = (DateOnly)postData["purchase_date"],
                    Kilometrage = kilometrage,
                    Price = (decimal)(int)postData["price"],
                    OwnerId = ownerId,
                    CreatedAt = (DateTime)postData["createdat"],
                    Specifications = new Dictionary<string, string>
                    {
                        ["Gearbox"] = (string)postData["gearbox"],
                        ["Transmission"] = (string)postData["transmission"],
                        ["Kilométrage"] = $"{kilometrage:N0} km",
                        ["Énergie"] = (string)postData["energie"],
                        ["Puissance fiscale"] = $"{(int)postData["puissance"]} cv",
                        ["Carrosserie"] = (string)postData["carrosserie"],
                        ["État général"] = new[] { "Très bon", "Bon", "Normal" }[_random.Next(3)],
                        ["Anciens propriétaires"] = $"{(int)postData["main"]}ème main",
                        ["Gouvernorat"] = (string)postData["gouvernat"]
                    }
                };
                
                context.Posts.Add(post);
                _postIdMap[postIndex] = post.Id;
            }
            
            await context.SaveChangesAsync();
        }
        
        private async Task CreateCommentsUsingIdMaps(GoWheelsDbContext context)
        {
            foreach (var commentData in _commentData)
            {
                // Get creator ID
                string creatorType = (string)commentData["creator_type"];
                int creatorIndex = (int)commentData["creator_index"];
                string creatorId = creatorType == "user" ? _userIdMap[creatorIndex] : _expertIdMap[creatorIndex];
                
                // Get post ID
                int postIndex = (int)commentData["post_index"];
                string postId = _postIdMap[postIndex];
                
                var comment = new Comment
                {
                    Id = Guid.NewGuid().ToString(),
                    Body = (string)commentData["body"],
                    UserId = creatorId,
                    PostId = postId,
                    CreatedAt = (DateTime)commentData["createdat"]
                };
                
                context.Comments.Add(comment);
            }
            
            await context.SaveChangesAsync();
        }
        
        private async Task CreateRatingsUsingIdMaps(GoWheelsDbContext context)
        {
            // Post ratings
            foreach (var ratingData in _postRatingData)
            {
                string raterType = (string)ratingData["rater_type"];
                int raterIndex = (int)ratingData["rater_index"];
                string raterId = raterType == "user" ? _userIdMap[raterIndex] : _expertIdMap[raterIndex];
                
                int postIndex = (int)ratingData["post_index"];
                string postId = _postIdMap[postIndex];
                
                var rating = new RatingPost
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = (int)ratingData["value"],
                    OwnerId = raterId,
                    RatedPostId = postId
                };
                
                context.PostsRatings.Add(rating);
            }
            
            // User ratings (only user->user)
            foreach (var ratingData in _userRatingData)
            {
                string raterType = (string)ratingData["rater_type"];
                int raterIndex = (int)ratingData["rater_index"];
                string raterId = raterType == "user" ? _userIdMap[raterIndex] : _expertIdMap[raterIndex];
                

                int ratedIndex = (int)ratingData["rated_index"];
                string ratedId = _userIdMap[ratedIndex];

                
                var rating = new RatingUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = (int)ratingData["value"],
                    OwnerId = raterId,
                    RatedUserId = ratedId
                };
                
                context.UsersRatings.Add(rating);
            }
            
            await context.SaveChangesAsync();
        }
        
        private async Task UpdateAverageRatings(GoWheelsDbContext context)
        {
            // Update post averages
            var postRatings = await context.PostsRatings
                .GroupBy(r => r.RatedPostId)
                .Select(g => new
                {
                    PostId = g.Key,
                    Average = g.Average(r => r.Value),
                    Count = g.Count()
                })
                .ToListAsync();
            
            foreach (var stat in postRatings)
            {
                var post = await context.Posts.FindAsync(stat.PostId);
                if (post != null)
                {
                    post.RateAverage = (float)Math.Round(stat.Average, 1);
                    post.RatingsCount = stat.Count;
                }
            }
            
            // Update user averages
            var userRatings = await context.UsersRatings
                .GroupBy(r => r.RatedUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Average = g.Average(r => r.Value),
                    Count = g.Count()
                })
                .ToListAsync();
            
            foreach (var stat in userRatings)
            {
                var user = await context.Users.FindAsync(stat.UserId);
                if (user != null)
                {
                    user.RateAverage = (float)Math.Round(stat.Average, 1);
                    user.RatingsCount = stat.Count;
                }
            }
            
            await context.SaveChangesAsync();
        }
        
        // HELPER METHODS
        private async Task ClearDatabase(GoWheelsDbContext context)
        {
            // Clear in correct order
            context.PostsRatings.RemoveRange(context.PostsRatings);
            context.UsersRatings.RemoveRange(context.UsersRatings);
            context.Comments.RemoveRange(context.Comments);
            context.PostImages.RemoveRange(context.PostImages);
            context.Posts.RemoveRange(context.Posts);
            
            // Clear non-built-in users
            var usersToDelete = await context.Users
                /*.Where(u => u.UserName != "admin" && u.UserName != "expert" && !u.UserName.StartsWith("user"))*/
                .ToListAsync();
            context.Users.RemoveRange(usersToDelete);
            
            await context.SaveChangesAsync();
        }
        
        private void PrintStatistics()
        {
            Console.WriteLine($"\n=== SEEDING STATISTICS ===");
            Console.WriteLine($"Admins: {_adminData.Count}");
            Console.WriteLine($"Experts: {_expertData.Count}");
            Console.WriteLine($"Users: {_userData.Count}");
            Console.WriteLine($"Posts: {_postData.Count}");
            Console.WriteLine($"Comments: {_commentData.Count}");
            Console.WriteLine($"Post Ratings: {_postRatingData.Count}");
            Console.WriteLine($"User Ratings: {_userRatingData.Count}");
            
            var statusCounts = _postData.GroupBy(p => (int)p["status"])
                .ToDictionary(g => ((PostStatus)g.Key).ToString(), g => g.Count());
            
            Console.WriteLine($"\nPosts by status:");
            foreach (var status in Enum.GetNames(typeof(PostStatus)))
            {
                int count = statusCounts.GetValueOrDefault(status, 0);
                Console.WriteLine($"  {status}: {count}");
            }
        }
    }
}