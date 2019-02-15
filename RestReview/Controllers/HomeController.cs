﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web;
using System.Web.Mvc;
using Yelp.Api;
using Yelp.Api.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using RestReview.Models;
using Newtonsoft.Json.Linq;
using RestReview.Models.data_consistency;
using RestReview.Models.data_consi_phone;
using System.Text.RegularExpressions;
using RestReview.Models.phone_data;
using RestReview.Models.FB;
using RestReview.Models.PhotoCountFB;
using RestReview.Models.ReviewClass;

namespace RestReview.Controllers
{
    public class HomeController : Controller
    {
        public static string API_KEY = WebConfigurationManager.AppSettings["yelp_api"];
        public static string FACEBOOK_ACCESS_TOKEN = WebConfigurationManager.AppSettings["facebook_access_token"];
        Client client = new Client(API_KEY);
        public static SearchResponse searchResponse;
        static List<BusinessResponse> SearchList = new List<BusinessResponse>();


        public ActionResult Index()
        {
            ViewBag.businessList = null;
            ViewBag.closeresult1 = null;
            ViewBag.closeresult2 = null;
            ViewBag.closeresult3 = null;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Review(string CustomerName, string CustomerId)
        {
            ViewBag.businessList = null;
            ViewBag.closeresult1 = null;
            ViewBag.closeresult2 = null;
            ViewBag.closeresult3 = null;
            try
            {
                client = new Client(API_KEY);
                var results = await client.GetBusinessAsync(CustomerId);

                List<BusinessMatch> closematches = new List<BusinessMatch>();

                if (!string.IsNullOrEmpty(CustomerName) && !string.IsNullOrEmpty(CustomerId) && results.Location != null)
                {
                    string name = results.Name;
                    string address1 = results.Location.Address1;
                    string city = results.Location.City;
                    string state = results.Location.State;
                    string country = results.Location.Country;

                    using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
                        client.BaseAddress = new Uri("https://api.yelp.com/v3/businesses/matches/");
                        HttpResponseMessage response = client.GetAsync("lookup?name=" + name + "&address1=" + address1 + "&city=" + city + "&state=" + state + "&country=" + country + "").Result;
                        response.EnsureSuccessStatusCode();
                        string result = response.Content.ReadAsStringAsync().Result;
                        var res = JsonConvert.DeserializeObject<Example>(result);
                        var count = res.businesses.Count();
                        for (int i = 0; i < count; i++)
                        {
                            BusinessMatch business = new BusinessMatch();
                            business.id = res.businesses[i].id;
                            closematches.Add(business);
                        }
                    }


                    var businesses = (from business in SearchList
                                      where (business.Id != CustomerId)
                                      select new
                                      {
                                          name = business.Name,
                                          id = business.Id,
                                          rate = business.Rating,
                                          ratecount = business.ReviewCount,
                                          ImageUrl = business.ImageUrl,
                                          Phone = business.DisplayPhone,
                                          Location = business.Location.DisplayAddress.FirstOrDefault() + " " + business.Location.City + ", " + business.Location.Country,
                                          Url = business.Url
                                      }).Distinct().ToList();

                    List<biz_data> obj = new List<biz_data>();
                    foreach (var item in businesses)
                    {
                        biz_data b1 = new biz_data();
                        b1.id = item.id;
                        b1.name = item.name;
                        b1.rate = item.rate;
                        b1.ratecount = item.ratecount;
                        b1.imageurl = item.ImageUrl;
                        b1.Location = item.Location;
                        b1.Phone = item.Phone;
                        b1.Url = item.Url;
                        obj.Add(b1);
                    }

                    List<CloseMatchData> closematchdata = new List<CloseMatchData>();
                    foreach (var item in closematches)
                    {
                        CloseMatchData b2 = new CloseMatchData();
                        var clsmatch = await client.GetBusinessAsync(item.id);
                        b2.id = clsmatch.Id;  //added by dipa
                        b2.name = clsmatch.Name;
                        b2.rate = clsmatch.Rating;
                        b2.ratecount = clsmatch.ReviewCount;
                        b2.imageurl = clsmatch.ImageUrl;
                        b2.Location = clsmatch.Location.Address1 + " " + clsmatch.Location.City + " " + clsmatch.Location.State + " " + clsmatch.Location.Country;
                        b2.Phone = clsmatch.Phone;
                        b2.Url = clsmatch.Url;
                        closematchdata.Add(b2);
                    }

                    if (closematchdata.Count == 1)
                    {
                        ViewBag.closeresult1 = closematchdata;
                        ViewBag.closeresult2 = obj;
                        ViewBag.closeresult3 = obj;
                    }
                    else if (closematchdata.Count == 2)
                    {
                        ViewBag.closeresult1 = closematchdata;
                        ViewBag.closeresult2 = closematchdata;
                        ViewBag.closeresult3 = obj;
                    }
                    else
                    {
                        ViewBag.closeresult1 = closematchdata;
                        ViewBag.closeresult2 = closematchdata;
                        ViewBag.closeresult3 = closematchdata;
                    }

                    ViewBag.businessList = obj;
                    ViewBag.Message = "CustomerName: " + CustomerName + " CustomerId: " + CustomerId;
                }
                else
                {
                    var businesses = (from business in SearchList
                                      where (business.Id != CustomerId)
                                      select new
                                      {
                                          name = business.Name,
                                          id = business.Id,
                                          rate = business.Rating,
                                          ratecount = business.ReviewCount,
                                          ImageUrl = business.ImageUrl,
                                          Phone = business.DisplayPhone,
                                          Location = business.Location.DisplayAddress.FirstOrDefault() + " " + business.Location.City + ", " + business.Location.Country,
                                          Url = business.Url
                                      }).Distinct().ToList();

                    List<biz_data> obj = new List<biz_data>();
                    foreach (var item in businesses)
                    {
                        biz_data b1 = new biz_data();
                        b1.id = item.id;
                        b1.name = item.name;
                        b1.rate = item.rate;
                        b1.ratecount = item.ratecount;
                        b1.imageurl = item.ImageUrl;
                        b1.Location = item.Location;
                        b1.Phone = item.Phone;
                        b1.Url = item.Url;
                        obj.Add(b1);
                    }
                    ViewBag.closeresult1 = obj;
                    ViewBag.closeresult2 = obj;
                    ViewBag.closeresult3 = obj;
                    ViewBag.businessList = obj;
                }
                SearchList.Clear();
                return View();
            }
            catch (Exception ex)
            {
                SearchList.Clear();
                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }


        public ActionResult Contact(string id)
        {
            ViewBag.Message = "Your contact page.";
            ViewBag.ratingData = 0;  //total 40
            ViewBag.dataConsistency = 0;  //total 10
            ViewBag.phoneRate = 0;  //total 5
            ViewBag.RestaurantName = "";
            ViewBag.reviewCount = 0;
            ViewBag.reviewRating = 0;
            ViewBag.website = 0;
            ViewBag.likeCount = 0;
            ViewBag.photoCount = 0;
            ViewBag.socialMediaEng = 0;
            var website = "";
            var resturantName = "";


            //  var res = new Example();
            var google_place_id = "";
            var fbID = "";
            int like_count = 0;
            int photo_count = 0;
            string Phone = "";
            string requestURL = "";
            //rating Data
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                if (id != null)
                {
                    ReviewExample res_review = new ReviewExample();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
                    client.BaseAddress = new Uri("https://api.yelp.com/v3/businesses/");
                    HttpResponseMessage response = client.GetAsync(id + "").Result;

                    if (response.IsSuccessStatusCode)
                    {                        
                        response.EnsureSuccessStatusCode();
                        string result = response.Content.ReadAsStringAsync().Result;
                        res_review = JsonConvert.DeserializeObject<ReviewExample>(result);

                        if (res_review.id != null)
                        {
                            resturantName = res_review.name;


                            //rate count code
                            Phone = res_review.phone;
                            decimal rate = (Convert.ToDecimal(res_review.rating) * 20) / 5;
                            decimal rate_count = (Convert.ToInt16(res_review.review_count) * 20) / 1738;
                            if (rate_count > 20)
                                rate_count = 20;
                            else if (rate_count == 0)
                                rate_count = 1;
                            ViewBag.ratingData = (rate + rate_count);
                            ViewBag.RestaurantName = res_review.name;
                            ViewBag.reviewCount = res_review.review_count;
                            ViewBag.reviewRating = res_review.rating;
                        }
                    }
                    else
                    {                        
                        requestURL = response.RequestMessage.RequestUri.OriginalString;                        
                    }

                   
                }
            }

            if (requestURL != "")
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                {
                    if (id != null)
                    {
                        ReviewExample res_review = new ReviewExample();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
                        string uri = requestURL.Split(',')[0];
                        client.BaseAddress = new Uri(uri);
                        HttpResponseMessage response = client.GetAsync("").Result;

                        if (response.IsSuccessStatusCode)
                        {
                            response.EnsureSuccessStatusCode();
                            string result = response.Content.ReadAsStringAsync().Result;
                            res_review = JsonConvert.DeserializeObject<ReviewExample>(result);

                            if (res_review.id != null)
                            {
                                resturantName = res_review.name;


                                //rate count code
                                Phone = res_review.phone;
                                decimal rate = (Convert.ToDecimal(res_review.rating) * 20) / 5;
                                decimal rate_count = (Convert.ToInt16(res_review.review_count) * 20) / 1738;
                                if (rate_count > 20)
                                    rate_count = 20;
                                else if (rate_count == 0)
                                    rate_count = 1;
                                ViewBag.ratingData = (rate + rate_count);
                                ViewBag.RestaurantName = res_review.name;
                                ViewBag.reviewCount = res_review.review_count;
                                ViewBag.reviewRating = res_review.rating;
                            }
                        }           


                    }
                }

            }
            //data consistency
            using (var client_consistency = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                if (Phone != null)
                {
                    //data cosistency code
                    string original_phone = new string(Phone.Where(char.IsDigit).ToArray());
                    client_consistency.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/place/findplacefromtext/json");
                    HttpResponseMessage response_consistency = client_consistency.GetAsync("?input=%2B" + original_phone + "&inputtype=phonenumber&fields=place_id,name&key=AIzaSyBEnSTOkBqHX2LDW-yuzXD9Zi3lnQvbe9Y").Result;
                    response_consistency.EnsureSuccessStatusCode();
                    string result_consistency = response_consistency.Content.ReadAsStringAsync().Result;
                    var res_consistency = JsonConvert.DeserializeObject<TestExample>(result_consistency);

                    if (res_consistency.candidates.Count > 0)
                    {
                        // var actual_nm = res.businesses[0].name;
                        //data consistency full value is 10


                        var res1_nm = res_consistency.candidates[0].name;
                        google_place_id = res_consistency.candidates[0].place_id;

                        string[] actual_nm_array = resturantName.Split(' ');
                        string[] diff1 = res1_nm.Split(' ');


                        int count1 = 0;

                        for (int i = 0; i < actual_nm_array.Length; i++)
                        {
                            if (String.Compare(actual_nm_array[i], diff1[i]) != 0)
                            {
                                count1 += 1;
                            }
                        }
                        if (count1 == 0 && (diff1.Length == actual_nm_array.Length))
                        {
                            ViewBag.dataConsistency = 10;       //data consistency full value is 10
                        }

                        if (ViewBag.dataConsistency == 0)
                        {
                            if (actual_nm_array.Length == diff1.Length)
                            {
                                int diff = 10 - count1;   //data consistency full value is 10
                                if (diff < 0)
                                    diff = 2;
                                ViewBag.dataConsistency = diff;        //data consistency full value is 10
                            }
                            else
                            {
                                Random randm = new Random();
                                ViewBag.dataConsistency = randm.Next(2, 7);
                            }
                        }
                    }
                }
            }

            //data consistency for website
            using (var client_consistency = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {

                //data cosistency with website from google
                client_consistency.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/place/details/json");
                HttpResponseMessage response_consistency = client_consistency.GetAsync("?key=AIzaSyBEnSTOkBqHX2LDW-yuzXD9Zi3lnQvbe9Y&placeid=" + google_place_id + "&fields=name,rating,formatted_phone_number,website,international_phone_number").Result;
                response_consistency.EnsureSuccessStatusCode();
                string result_consistency = response_consistency.Content.ReadAsStringAsync().Result;
                var res_consistency = JsonConvert.DeserializeObject<Data_consi_Example>(result_consistency);

                if (res_consistency.result != null)
                {
                    if (res_consistency.result.website != "")
                    {
                        ViewBag.website = 15;
                        website = res_consistency.result.website;
                    }
                }
            }

            //facebook Like Count
            using (var client_consistency = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {

                if (website != "" && website != null)
                {
                    //facebook
                    client_consistency.BaseAddress = new Uri("https://graph.facebook.com/");
                    HttpResponseMessage response_consistency = client_consistency.GetAsync("?ids=" + website + "").Result;
                    response_consistency.EnsureSuccessStatusCode();
                    string result_consistency = response_consistency.Content.ReadAsStringAsync().Result;


                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    dynamic item = serializer.Deserialize<dynamic>(result_consistency);
                    var like = item[website]["share"]["share_count"];

                    like_count = (Convert.ToInt16(like) * 20) / 2500;
                    if (like_count > 20)
                        like_count = 20;
                    else if (like_count < 0)
                        like_count = 0;
                }
            }

            //facebook id generator
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                if (resturantName != "")
                {
                    var idRes = new FBidExample();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", FACEBOOK_ACCESS_TOKEN);
                    client.BaseAddress = new Uri("https://graph.facebook.com/pages/search");
                    HttpResponseMessage response = client.GetAsync("?q=" + resturantName + "").Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;
                    idRes = JsonConvert.DeserializeObject<FBidExample>(result);

                    fbID = idRes.data[0].id;
                }
            }

            //facebook photo count api
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                if (fbID != "")
                {
                    var idRes = new FBidExample();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", FACEBOOK_ACCESS_TOKEN);
                    client.BaseAddress = new Uri("https://graph.facebook.com/v3.2/" + fbID + "/albums");
                    HttpResponseMessage response = client.GetAsync("?fields=count,name").Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;
                    idRes = JsonConvert.DeserializeObject<FBidExample>(result);

                    //photo count code
                    int count = idRes.data.Count;
                    int photos = 0;
                    for (int i = 0; i < count; i++)
                    {
                        photos += idRes.data[i].count;
                    }

                    photo_count = (Convert.ToInt16(photos) * 15) / 100;
                    if (photo_count > 15)
                        photo_count = 15;
                    else if (photo_count < 0)
                        photo_count = 0;

                    ViewBag.likeCount = like_count;
                    ViewBag.photoCount = photo_count;
                    ViewBag.socialMediaEng = like_count + photo_count;
                }
            }


            TempData["ratingData"] = ViewBag.ratingData;
            TempData["dataConsistency"] = ViewBag.dataConsistency;
            TempData["phoneRate"] = ViewBag.RestaurantName;  //ViewBag.phoneRate
            TempData["RestaurantName"] = ViewBag.RestaurantName;
            TempData["reviewCount"] = ViewBag.reviewCount;
            TempData["reviewRating"] = ViewBag.reviewRating;
            TempData["website"] = ViewBag.website;
            TempData["photoCount"] = ViewBag.photoCount;
            TempData["likeCount"] = ViewBag.likeCount;
            TempData["socialMediaEng"] = ViewBag.socialMediaEng;
            return View();
        }


        public ActionResult Analyze()
        {
            ViewBag.Message = "Your Analyze page.";
            ViewBag.dataConsistency = Convert.ToInt32(TempData["dataConsistency"]);
            ViewBag.RestaurantName = TempData["RestaurantName"];
            ViewBag.phoneRate = TempData["RestaurantName"];
            ViewBag.ratingData = Convert.ToInt32(TempData["ratingData"]);
            ViewBag.reviewCount = TempData["reviewCount"];
            ViewBag.reviewRating = TempData["reviewRating"];
            ViewBag.website = TempData["website"];
            ViewBag.photoCount = TempData["photoCount"];
            ViewBag.likeCount = TempData["likeCount"];
            ViewBag.socialMediaEng = TempData["socialMediaEng"];

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetAllData(string xlocation)
        {

            SearchList.Clear();
            //client = new Client(API_KEY);
            //var request = new Yelp.Api.Models.SearchRequest();
            //request.Location = xlocation;
            //request.Term = "pl";
            //request.MaxResults = 50;

            //int count = 1;


            //while (count < 1000)
            //{
            //    request.ResultsOffset = count;
            //    searchResponse = await client.SearchBusinessesAllAsync(request);

            //    foreach (var b in searchResponse.Businesses)
            //    {
            //        if (!SearchList.Contains(b))
            //            SearchList.Add(b);
            //    }
            //    count += 50;

            //}

            //using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            //{
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            //    client.BaseAddress = new Uri("https://api.yelp.com/v3/openql/businesses/search");
            //    var location = "North Kingstown";
            //    HttpResponseMessage response = client.GetAsync("lookup?term=oak&location=" + location + "").Result;
            //    response.EnsureSuccessStatusCode();
            //    string result = response.Content.ReadAsStringAsync().Result;
            //    var res = JsonConvert.DeserializeObject<Example>(result);
            //    var count = res.businesses.Count();

            //}

            return await Task.FromResult(Json("hi", JsonRequestBehavior.AllowGet));
        }

        [HttpPost]
        public async Task<JsonResult> AutoComplete(string prefix, string location)
        {

            //  SearchList.Clear();
            client = new Client(API_KEY);
            var request = new Yelp.Api.Models.SearchRequest();
            request.Location = location;
            request.Term = prefix;
            request.MaxResults = 50;
            request.Categories = "Restaurants";


            searchResponse = await client.SearchBusinessesAllAsync(request);

            foreach (var b in searchResponse.Businesses)
            {
                if (!SearchList.Contains(b))
                    SearchList.Add(b);
            }


            var customers = (from customer in SearchList
                             where ((customer.Name.ToLower()).Contains(prefix.ToLower()))
                             select new
                             {
                                 label = customer.Name,
                                 val = customer.Id
                             }).Distinct().ToList();

            return await Task.FromResult(Json(customers, JsonRequestBehavior.AllowGet));
        }

        [HttpPost]
        public async Task<JsonResult> GetBusiness(string ID)
        {
            var results = await client.GetBusinessAsync(ID);//, request.Latitude, request.Longitude);
            return await Task.FromResult(Json(results, JsonRequestBehavior.AllowGet));
        }

        public class biz_data
        {
            public string id { get; set; }
            public string name { get; set; }
            public float rate { get; set; }
            public int ratecount { get; set; }
            public string imageurl { get; set; }
            public string Phone { get; set; }
            public string Location { get; set; }
            public string Url { get; set; }
        }

        public class CloseMatchData
        {
            public string id { get; set; }
            public string name { get; set; }
            public float rate { get; set; }
            public int ratecount { get; set; }
            public string imageurl { get; set; }
            public string Phone { get; set; }
            public string Location { get; set; }
            public string Url { get; set; }
        }
    }
}