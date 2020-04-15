using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace AzureSearchSample
{
    public class Character
    {
        public Guid CharacterId { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Gender { get; set; }
        public string EyeColor { get; set; }
        public int Height { get; set; }
    }

    
    public class AzureSampleTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISearchIndexClient searchIndexClient;

        private string searchServiceName = "search-service-name";
        private string apiKey = "search-api-key";
        private string indexName = "search-index-name";

        public AzureSampleTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            var serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            searchIndexClient = serviceClient.Indexes.GetClient(indexName);
            searchIndexClient.SerializationSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        [Fact]
        public async Task ShouldInsertDocumentsIntoSearchIndex_OneMethod()
        {
            // create a list of IndexActions
            var actions = new[]
            {
                IndexAction.Upload( new Character
                {
                    CharacterId = Guid.NewGuid(),
                    FirstName = "Yoda",
                    LastName = "",
                    Gender = "male",
                    EyeColor = "brown",
                    Height = 66
                }),
                IndexAction.Upload( new Character
                {
                    CharacterId = Guid.NewGuid(),
                    FirstName = "Boba",
                    LastName = "Frett",
                    Gender = "male",
                    EyeColor = "brown",
                    Height = 183
                }),
                IndexAction.Upload(new Character
                {
                    CharacterId = Guid.NewGuid(),
                    FirstName = "Leia",
                    LastName = "Organa",
                    Gender = "female",
                    EyeColor = "brown",
                    Height = 150
                })
            };

            await searchIndexClient.Documents.IndexAsync(IndexBatch.New(actions));

        }


        [Fact]
        public async Task ShouldInsertDocumentsIntoSearchIndex_AlternativeMethod()
        {
            // create a standard list of objects
            var characters = new List<Character>
            {
                new Character
                {
                    CharacterId = Guid.NewGuid(),
                    FirstName = "Yoda",
                    LastName = "",
                    Gender = "male",
                    EyeColor = "brown",
                    Height = 66
                },
                new Character
                {
                    CharacterId = Guid.NewGuid(),
                    FirstName = "Boba",
                    LastName = "Frett",
                    Gender = "male",
                    EyeColor = "brown",
                    Height = 183
                },
                new Character
                {
                    CharacterId = Guid.NewGuid(),
                    FirstName = "Leia",
                    LastName = "Organa",
                    Gender = "female",
                    EyeColor = "brown",
                    Height = 150
                },
            };

            var serializeObject = JsonConvert.SerializeObject(characters);

            var mergeOrUploadActions = characters.Select(IndexAction.MergeOrUpload);
            
            var indexBatch = IndexBatch.New(mergeOrUploadActions);

            await searchIndexClient.Documents.IndexAsync(indexBatch);
        }

        [Fact]
        public async Task ShouldSearchForDocuments()
        {
            var searchResults = await searchIndexClient.Documents.SearchAsync<Character>("*");

            foreach (var searchResult in searchResults.Results)
            {

                _outputHelper.WriteLine($"{searchResult.Document.FirstName} {searchResult.Document.LastName}");
            }

            Assert.True(searchResults.Results.Any());

        }

    }
}
