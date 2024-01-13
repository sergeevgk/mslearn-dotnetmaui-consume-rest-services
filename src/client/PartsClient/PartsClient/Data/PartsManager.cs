using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PartsClient.Data;

public static class PartsManager
{
	// TODO: Add fields for BaseAddress, Url, and authorizationKey
	static readonly string BaseAddress = "http://localhost:5210/";
	static readonly string Url = $"{BaseAddress}/api/";
	private static string authorizationKey;

	static HttpClient client;

	private static async Task<HttpClient> GetClient()
	{
		if (client != null)
			return client;

		client = new HttpClient();

		if (string.IsNullOrEmpty(authorizationKey))
		{
			authorizationKey = await client.GetStringAsync($"{Url}login");
			authorizationKey = JsonSerializer.Deserialize<string>(authorizationKey);
		}

		client.DefaultRequestHeaders.Add("Authorization", authorizationKey);
		client.DefaultRequestHeaders.Add("Accept", "application/json");

		return client;
	}

	public static async Task<IEnumerable<Part>> GetAll()
	{
		if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
			return new List<Part>();

		var client = await GetClient();
		var responseContent = await client.GetStringAsync($"{Url}parts");
		var options = new JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true
		};

		var result = JsonSerializer.Deserialize<List<Part>>(responseContent, options);

		return result;
	}

	public static async Task<Part> Add(string partName, string supplier, string partType)
	{
		if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
			return new Part();

		var part = new Part()
		{
			PartName = partName,
			Suppliers = new List<string>(new[] { supplier }),
			PartType = partType,
			PartAvailableDate = DateTime.Now.Date
		};

		var client = await GetClient();
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
		};

		var message = new HttpRequestMessage(HttpMethod.Post, $"{Url}parts");
		message.Content = JsonContent.Create(part, options: options);

		var response = await client.SendAsync(message);
		response.EnsureSuccessStatusCode();

		var returnedJson = await response.Content.ReadAsStringAsync();
		

		var result = JsonSerializer.Deserialize<Part>(returnedJson, options);

		return result;
	}

	public static async Task Update(Part part)
	{
		if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
			return;

		HttpRequestMessage msg = new(HttpMethod.Put, $"{Url}parts/{part.PartID}");
		msg.Content = JsonContent.Create(part);
		var client = await GetClient();
		var response = await client.SendAsync(msg);
		response.EnsureSuccessStatusCode();
	}

	public static async Task Delete(string partID)
	{
		if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
			return;

		HttpRequestMessage msg = new(HttpMethod.Delete, $"{Url}parts/{partID}");
		var client = await GetClient();
		var response = await client.SendAsync(msg);
		response.EnsureSuccessStatusCode();
	}
}
