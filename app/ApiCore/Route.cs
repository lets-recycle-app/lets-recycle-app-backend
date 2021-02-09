using System;
using System.Collections.Generic;
using Bogus;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;

namespace ApiCore
{
    public static class Route
    {
        private static readonly Random Random = new Random();
        static readonly decimal singleKMX = (decimal) 0.01453433399999998; // 1 km longitude unit
        static readonly decimal singleKMY = (decimal) 0.00898315300000263; // 1 km latitude unit


        public static string Map(IDictionary<string, string> query)
        {
            int driverIdSel = -1;
            int depotIdSel = -1;
            int dayNoSel = -1;
            string routeDateSel = "";

            if (query.Count > 0)
            {
                foreach (var (key, value) in query)
                {
                    switch (key.Trim())
                    {
                        case "depotId":
                            depotIdSel = int.Parse(value);
                            break;
                        case "driverId":
                            driverIdSel = int.Parse(value);
                            break;
                        case "dayNo":
                        {
                            dayNoSel = int.Parse(value);
                            if (dayNoSel >= 0 && dayNoSel <= 20)
                            {
                                routeDateSel = GetFutureDate(dayNoSel);
                            }
                            else
                            {
                                return Result(232, "no route found, dayNo is in the past or more than 20 days ahead",
                                    null);
                            }

                            break;
                        }
                    }
                }
            }

            if (depotIdSel < 0 || driverIdSel < 0 || dayNoSel < 0)
            {
                return Result(200,
                    $"incorrect usage, route-map?depotId={depotIdSel} & driverId = {driverIdSel} & dayNo = {dayNoSel}",
                    null);
            }


            // results are always filtered by depot and and date,
            // all drivers (driverId=0) or specific driver can also be specified


            string sqlText;

            if (driverIdSel == 0)
            {
                sqlText = $"select * from drivers where depotId = {depotIdSel}";
            }
            else
            {
                sqlText = $"select * from drivers where depotId = {depotIdSel} and driverId = {driverIdSel}";
            }


            JToken allDrivers = Database.SqlMultiRow(sqlText);
            JArray mapData = new JArray();

            foreach (var driverRow in allDrivers)
            {
                mapData.Add(new JObject
                {
                    {"weight", 0.5},
                    {"color", "red"},
                    {"opacity", 1},
                    {"mode", "driving"},
                    {"locations", new JArray()}
                });

                JToken driverId = driverRow["driverId"];

                string sqlRoute =
                    $"select routeSeqNo, latitude,longitude from routes where depotId = {depotIdSel} and driverId = {driverId} and routeDate = str_to_date('{routeDateSel}','%Y-%m-%d') order by routeSeqNo";

                JToken routeRow = Database.SqlMultiRow(sqlRoute);

                JArray geoArray = new JArray();

                foreach (var stop in routeRow)
                {
                    geoArray.Add(new JArray {stop["latitude"] ?? 0.00, stop["longitude"] ?? 0.00});
                }

                if (mapData.Last != null) mapData.Last["locations"] = geoArray;
            }

            return Result(200,
                $"route map completed for depotId={depotIdSel} & routeDate={routeDateSel} & driverId={driverIdSel}",
                mapData);
        }

        public static string Markers(IDictionary<string, string> query)
        {
            int driverIdSel = -1;
            int depotIdSel = -1;
            int dayNoSel = -1;
            string routeDateSel = "";

            if (query.Count > 0)
            {
                foreach (var (key, value) in query)
                {
                    switch (key.Trim())
                    {
                        case "depotId":
                            depotIdSel = int.Parse(value);
                            break;
                        case "driverId":
                            driverIdSel = int.Parse(value);
                            break;
                        case "dayNo":
                        {
                            dayNoSel = int.Parse(value);
                            if (dayNoSel >= 0 && dayNoSel <= 20)
                            {
                                routeDateSel = GetFutureDate(dayNoSel);
                            }
                            else
                            {
                                return Result(232, "no route found, dayNo is in the past or more than 20 days ahead",
                                    null);
                            }

                            break;
                        }
                    }
                }
            }

            if (depotIdSel < 0 || driverIdSel < 0 || dayNoSel < 0)
            {
                return Result(200,
                    $"incorrect usage, route-map?depotId={depotIdSel} & driverId = {driverIdSel} & dayNo = {dayNoSel}",
                    null);
            }


            // results are always filtered by depot and and date,
            // all drivers (driverId=0) or specific driver can also be specified


            string sqlText;

            if (driverIdSel == 0)
            {
                sqlText = $"select * from drivers where depotId = {depotIdSel}";
            }
            else
            {
                sqlText = $"select * from drivers where depotId = {depotIdSel} and driverId = {driverIdSel}";
            }


            JToken allDrivers = Database.SqlMultiRow(sqlText);
            JArray mapData = new JArray();


            foreach (var driverRow in allDrivers)
            {
                JToken driverId = driverRow["driverId"];

                string sqlRoute =
                    $"select routeAction, latitude, longitude, addressPostcode from routes where depotId = {depotIdSel} and driverId = {driverId} and routeDate = str_to_date('{routeDateSel}','%Y-%m-%d') order by routeSeqNo";

                JToken routeRow = Database.SqlMultiRow(sqlRoute);


                // generate a marker object for every stop

                foreach (var stop in routeRow)
                {
                    JObject toolTip = new JObject
                    {
                        {"text", stop["addressPostcode"]}
                    };
                    
                    mapData.Add(new JObject
                    {
                        {"routeAction", stop["routeAction"]},
                        {"location", new JArray {stop["latitude"] ?? 0.00, stop["longitude"] ?? 0.00}},
                        {"tooltip", toolTip}
                    });
                }

                ;
            }

            return Result(200,
                $"route markers completed for depotId={depotIdSel} & routeDate={routeDateSel} & driverId={driverIdSel}",
                mapData);
        }

        public static string Simulate(IDictionary<string, string> query)
        {
            Randomizer.Seed = new Random(58932234);

            // accept a query that defines a depotId/dayNo

            int depotId = 0;
            int fleetSize;
            string routeDate = GetFutureDate(1);

            string sqlText;

            if (query.Count > 0)
            {
                foreach (var (key, value) in query)
                {
                    switch (key.Trim())
                    {
                        case "depotId":
                            depotId = int.Parse(value);
                            break;
                        case "dayNo":
                        {
                            int dayNoSelect = int.Parse(value);
                            if (dayNoSelect >= 0 && dayNoSelect <= 20)
                            {
                                routeDate = GetFutureDate(dayNoSelect);
                            }
                            else
                            {
                                return Result(232, "no route simulated, dayNo is incorrect", null);
                            }

                            break;
                        }
                    }
                }
            }

            if (depotId == 0)
            {
                // must supply a depotId
                return Result(232, "no route simulated, depotId is missing", null);
            }

            JToken depotRow = Database.SqlSingleRow($"select * from depots where depotId = {depotId}");
            decimal depotLat;
            decimal depotLong;

            if (depotRow.HasValues)
            {
                string depotPostCode = depotRow["postcode"].ToString();
                fleetSize = int.Parse(depotRow["fleetSize"]?.ToString());

                sqlText = $"select latitude, longitude from postcodes where postcode = '{depotPostCode}' limit 1";


                JToken geoLocation = Database.SqlSingleRow(sqlText);

                if (geoLocation.HasValues)
                {
                    depotLat = decimal.Parse(geoLocation["latitude"].ToString());
                    depotLong = decimal.Parse(geoLocation["longitude"]?.ToString());
                }
                else
                {
                    return Result(232, $"no route simulated, depot postcode {depotPostCode} is not valid", null);
                }
            }

            else
            {
                return Result(232, "no route simulated, depotId is not valid", null);
            }

            int noRoutesToday = fleetSize;
            sqlText = $"select * from drivers where depotId = {depotId};";
            JToken allDrivers = Database.SqlMultiRow(sqlText);
            Database.SqlTransaction(
                $"delete from routes where depotId = {depotId} and routeDate = str_to_date('{routeDate}','%Y-%m-%d')");

            foreach (var driverRow in allDrivers)
            {
                SingleRoute(routeDate, depotRow, driverRow, depotLat, depotLong);
            }

            return Result(232, $"{noRoutesToday} driver routes simulated for day {routeDate}", null);
        }

        public static string Distance(IDictionary<string, string> query)
        {
            // receive a list of postcodes in the query string and calculate the distances
            // between the route stops.
            foreach (var pair in query)
            {
                Console.WriteLine($"<{pair.Key}>, <{pair.Value}>");
            }

            List<StopData> postCodeList = new List<StopData>();
            if (query.Count > 0)
            {
                int seqNo = 0;
                foreach (var (_, postCode) in query)
                {
                    StopData stopData = new StopData
                    {
                        SeqNo = seqNo,
                        PostCode = postCode,
                        Valid = false,
                        Latitude = 0,
                        Longitude = 0,
                        Distance = 0
                    };

                    postCodeList.Add(stopData);
                    seqNo += 1;
                }
            }

            foreach (StopData r in postCodeList)
            {
                (bool isValid, decimal latitude, decimal longitude) = FetchGeoPosition(r.PostCode);

                if (!isValid) continue;

                r.Valid = true;
                r.Latitude = latitude;
                r.Longitude = longitude;
            }

            // now calculate distances

            StopData lastData = null;
            foreach (StopData r in postCodeList)
            {
                if (lastData != null)
                {
                    r.Distance = HaversineDistance(lastData.Latitude, lastData.Longitude, r.Latitude, r.Longitude);
                }

                Console.WriteLine($"{r.SeqNo}, {r.Valid}, {r.PostCode}, {r.Latitude}, {r.Longitude}, {r.Distance}");


                lastData = r;
            }

            return Result(200, "route simulated.", null);
        }


        private static (decimal, decimal) FetchNewGeoPosition(double distanceKm, double angle, decimal latitude,
            decimal longitude)
        {
            // find a new latitude/longitude x km away from the current lat/long and in a direction of a certain degree (0-360)
            const double radiusKm = 6378.137;
            const double pi = Math.PI;
            double distanceY = Math.Round(distanceKm * Math.Cos(angle / 360 * 2 * pi));
            double distanceX = Math.Round(distanceKm * Math.Sin(angle / 360 * 2 * pi));

            // North+ South- latitude (y axis)
            double newLatitude = Math.Round((double) latitude + distanceY / radiusKm * (180 / pi), 9);

            // East+ West- longitude (x axis)
            double newLongitude = Math.Round((double) longitude +
                                             distanceX / radiusKm * (180 / pi) / Math.Cos(newLatitude * pi / 180),
                9);
            return ((decimal) newLatitude, (decimal) newLongitude);
        }

        private static (bool, decimal, decimal) FetchGeoPosition(string lookupPostCode)
        {
            Dictionary<string, string> codeQuery = new Dictionary<string, string>
            {
                {"postcode", lookupPostCode}
            };

            JObject postCode = JObject.Parse(TableName.Get("postcodes", codeQuery));
            foreach (JToken code in postCode["result"])
            {
                return (true, decimal.Parse(code["latitude"]?.ToString() ?? string.Empty),
                    decimal.Parse(code["longitude"]?.ToString()));
            }

            return (false, 0, 0);
        }

        private static string GetFutureDate(int daysForward)
        {
            // return yyyy-mm-dd n days forward from today 
            DateTime currentDay = DateTime.Now.Date;
            return $"{currentDay.AddDays(daysForward):yyyy-MM-dd}";
        }

        private static int RandomNumber(int min, int max)
        {
            return Random.Next(min, max + 1);
        }


        private static decimal HaversineDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double radiusKm = 6378.137;
            double latitudeDelta = ToRadians((double) (lat2 - lat1));
            double longitudeDelta = ToRadians((double) (lon2 - lon1));

            double a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                       Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2) *
                       Math.Cos(ToRadians((double) lat1)) * Math.Cos(ToRadians((double) lat2));

            double c = 2 * Math.Asin(Math.Sqrt(a));
            return (decimal) Math.Round(radiusKm * c, 4); // km
        }


        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private static void SingleRoute(string date, JToken depotRow, JToken driverRow, decimal depotLat,
            decimal depotLong)
        {
            var depotPostCode = depotRow["postcode"].ToString();
            Console.WriteLine($"{depotRow["depotPostCode"]} {depotLat} {depotLong} {driverRow["driverName"]}");
            string[] itemArray = {"fridge", "freezer", "washer", "dryer", "oven", "cooker", "dishwasher"};
            string stopPostCode = depotPostCode;
            decimal latitude = depotLat;
            decimal longitude = depotLong;
            int directionAngle = RandomNumber(0, 360);
            int addressId = 0;
            int noStops = RandomNumber(10, 14);
            int collectionStart = noStops - RandomNumber(1, 4);

            List<FakeCustomer> customers = GetFakeCustomers(noStops);
            for (int routeSeqNo = 0;
                routeSeqNo <= noStops;
                routeSeqNo += 1)
            {
                string routeAction = "delivery";

                if (routeSeqNo == 0)
                {
                    routeAction = "depot";
                }
                else if (routeSeqNo >= collectionStart)
                {
                    routeAction = "recycle";
                }

                decimal distance;

                // find the distance to the next route stop

                (string newPostCode, decimal newLatitude, decimal newLongitude) =
                    GetNextPostCode(directionAngle, latitude, longitude);

                if (newPostCode.Length == 0 || newPostCode == stopPostCode || routeSeqNo == noStops)
                {
                    // this is last stop or we failed to find a new postcode, so stop the
                    // the route after this stop as we might have hit the sea or a mountain range.

                    distance = 0; // km
                }
                else
                {
                    stopPostCode = newPostCode;
                    distance = HaversineDistance(latitude, longitude, newLatitude,
                        newLongitude); // km                    
                }

                if (routeSeqNo > 0)
                {
                    addressId = InsertNewAddress(stopPostCode, routeAction, customers[routeSeqNo - 1]);
                }

                JObject newRouteStop = new JObject
                {
                    {"depotId", depotRow["depotId"]},
                    {"driverId", driverRow["driverId"]},
                    {"routeDate", date},
                    {"routeSeqNo", routeSeqNo},
                    {"distance", distance},
                    {"addressId", addressId},
                    {"addressPostcode", stopPostCode},
                    {"latitude", latitude},
                    {"longitude", longitude},
                    {"routeAction", routeAction},
                    {"itemType", routeSeqNo == 0 ? "depot" : itemArray[RandomNumber(0, itemArray.Length - 1)]},
                    {"status", "pending"},
                    {
                        "refNo",
                        $"D{depotRow["depotId"]:0#}R{driverRow["driverId"]:0#}S{routeSeqNo:0#}T{date.Substring(date.Length - 2)}"
                    }
                };

                InsertRouteStop(newRouteStop);

                if (distance == 0)
                {
                    return;
                }

                // set the new stop to be the previously calculated next stop geolocation

                latitude = newLatitude;
                longitude = newLongitude;
            }
        }

        private static (string, decimal, decimal) GetNextPostCode(int directionAngle, decimal latitude,
            decimal longitude)
        {
            // find next postcode on the stops List by moving the
            // geolocation forward by a set amount of km.

            double kmDistance = 1.5 + (double) RandomNumber(-100, 100) / 100;
            double deviation = RandomNumber(-5, 5);
            string nextPostCode = "";
            (decimal newLat, decimal newLong) =
                FetchNewGeoPosition(kmDistance, directionAngle + deviation, latitude, longitude);

            // now find a nearby postcode

            string sqlText =
                $"select postcode from postcodes where (latitude between {latitude - singleKMY} and {latitude + singleKMY}) and (longitude between {longitude - singleKMX} and {longitude + singleKMX}) limit 1";

            JObject newGeoArea = JObject.Parse(Database.GetSqlSelect(sqlText));

            JToken result = newGeoArea["result"];
            if (result.HasValues)
            {
                nextPostCode = result[0]["postcode"].ToString();
            }

            return (nextPostCode, newLat, newLong);
        }

        private static string InsertRouteStop(JObject routeStop)
        {
            JObject postInfo =
                JObject.Parse(TableName.Post("routes", routeStop.ToObject<Dictionary<string, string>>()));
            return postInfo["status"].ToString() != "200" ? postInfo.ToString() : postInfo.ToString();
        }

        private static int InsertNewAddress(string routePostCode, string routeAction, FakeCustomer customer)
        {
            string[] notesList =
            {
                "Please ring doorbell",
                "On the front porch",
                "In the front garden",
                "Appliance at back gate",
                "Next to the garages on the left",
                "behind the car park"
            };

            string notes = "";
            if (routeAction == "recycle")
            {
                notes = notesList[RandomNumber(0, notesList.Length - 1)];
            }

            int houseNo = RandomNumber(1, 99);
            if (RandomNumber(1, 3) == 3)
            {
                houseNo = RandomNumber(100, 175);
            }

            JObject newAddress = new JObject
            {
                {"postcode", routePostCode},
                {"customerName", customer.FullName},
                {"customerEmail", customer.Email},
                {"locationType", "private property"},
                {"houseNo", houseNo},
                {"street", customer.StreetName},
                {"townAddress", customer.Town},
                {"notes", notes}
            };

            Console.WriteLine(newAddress);

            JObject postInfo =
                JObject.Parse(TableName.Post("addresses", newAddress.ToObject<Dictionary<string, string>>()));
            if (postInfo["status"].ToString() != "200")
            {
                Console.WriteLine(postInfo);
                return 0;
            }

            try
            {
                int addressId = int.Parse(postInfo["result"][0]["insertId"].ToString());
                return addressId;
            }
            catch
            {
            }

            return 0;
        }

        private static List<FakeCustomer> GetFakeCustomers(int noCustomers)
        {
            var fakeCustomer = new Faker<FakeCustomer>("en_BORK")
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (f, u) => u.FirstName + " " + u.LastName)
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.StreetName, (f, u) => f.Address.StreetName())
                .RuleFor(u => u.Town, (f, u) => f.Address.City());
            return fakeCustomer.Generate(noCustomers);
        }

        class FakeCustomer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string StreetName { get; set; }
            public string Town { get; set; }
        }


        class StopData
        {
            public int SeqNo { get; set; }
            public string PostCode { get; set; }
            public bool Valid { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public decimal Distance { get; set; }
            public JObject RowDetails { get; set; }
        }
    }
}