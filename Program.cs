using Newtonsoft.Json; //added JSON.NET with Nuget
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AddressSplitter
{
    class Program
    {
        const string apiKey = "AIzaSyBOcpx4Fbxt_FiXaV4FrbPBk-tt3IZYsKM"; //paste your API KEY HERE 
        static string baseUrlGC = "https://maps.googleapis.com/maps/api/geocode/json?address="; // part1 of the URL for GeoCoding
        static string plusUrl="&key="+apiKey+"&sensor=false"; // part2 of the URL

        static public int DisplayMenu() // I add a menu for selecting between 1 - Address Splitter / 2 - Exit
        {
            Console.WriteLine("ADRRESS SPLITTER TUTORIAL (Select and hit Enter):");
            Console.WriteLine();
            Console.WriteLine("1. Address Splitter via Regex"); // 1 for Address Splitter with Google Map
            Console.WriteLine("2. Address Splitter via Google Map"); // 2 for Address Splitter
            Console.WriteLine("3. Exit"); // 3 for Exit
            Console.WriteLine();
            var result = Console.ReadLine(); //waiting for an integer input for the menu; value can between 1-3
            return Convert.ToInt32(result); //converting result to an integer for the menu
        }

        static void Main(string[] args)
        {
            int menuInput = 0;
            do // do-while statement for the menu, it loops until the input is 3 (Exit) 
            {
                Console.ForegroundColor = ConsoleColor.Green; // changing color for the console to green 
                menuInput=DisplayMenu(); //getting the result of the input for the menu
                Console.ForegroundColor = ConsoleColor.Gray; // changing to default color
                string inputAddress;
                switch (menuInput.ToString()) //switch statement for checking if input is 1 or 2
                {
                    case "1":    //if the input for menu is 1, then call the regex function
                        Console.WriteLine("===== WELCOME =====");
                        Console.WriteLine("Enter an address for extraction result: ");
                        inputAddress = Console.ReadLine();
                        Console.WriteLine("-------------------------------------");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("RESULT: \n" + Parse(inputAddress));
                        Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                    case "2":    //if the input for menu is 2, then call the GeoCoding function
                        Console.WriteLine("===== WELCOME =====");
                        Console.WriteLine("Enter an address for extraction result: ");
                        inputAddress = Console.ReadLine();
                        Console.WriteLine("-------------------------------------");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("RESULT: \n" + GeoCoding(inputAddress));
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                }
            } while (menuInput != 3); 
        }

        //case 1 : split addess with regex
        public static string Parse(string address)
        {
            string result = String.Empty;
            var input = address.ToUpper().Replace(",", " ").Replace(".", " ");

            var re = new Regex(BuildPattern());
            if (re.IsMatch(input))
            {
                var m = re.Match(input);
                result = "Apt No : " + m.Groups["AptNo"].Value + ", \n"
                       + "City : " + m.Groups["City"].Value + ", \n"
                       + "State : " + m.Groups["State"].Value + ", \n"
                       + "Postcode : " + m.Groups["Postcode"].Value + ", \n"
                       + "Street : " + m.Groups["Street"].Value + ", \n"
                       + "Section : " + m.Groups["Section"].Value + ", \n";
            }
            else
            {
                result = "Invalid address : " + input;
            }
            return result;
        }

        private static string BuildPattern()
        {
            var pattern = "^" +                          // beginning of string
              "(?<AptNo>\\d+)" +                         // 1 or more digits
              "(?:" +                                    // group (optional) {
              "(?:\\s+(?<City>" + GetCity() + "))?" +    //   whitespace + valid street suffix (optional)
              "(?:\\s+(?<State>" + GetState() + "))?" +  //   whitespace + valid street suffix (optional)
              "(?:\\s+(?<Postcode>\\d{5}))?" +           // 5 digits
              "(?:\\s+(?<Section>.*))?" +                //   whitespace + anything (optional)
              ")?" +                                     // }
              "$";                                       // end of string

            return pattern;
        }

        private static string GetCity()
        {
            return "KUALA TERENGGANU|KUALA LUMPUR|KAJANG|BANGI|DAMANSARA|PETALING JAYA|PUCHONG|SUBANG JAYA|CYBERJAYA|PUTRAJAYA|MANTIN|KUCHING|SEREMBAN";
        }

        private static string GetState()
        {
            return "SELANGOR|TERENGGANU|PAHANG|KELANTAN|MELAKA|PULAU PINANG|KEDAH|JOHOR|PERLIS|SABAH|SARAWAK";
        }

        private static string GetPostCode()
        {
            return "SELANGOR|TERENGGANU|PAHANG|KELANTAN|MELAKA|PULAU PINANG|KEDAH|JOHOR|PERLIS|SABAH|SARAWAK";
        }

        // case 2: find address and split address with google map api
        static string GeoCoding(string address)
        {
            var json = new WebClient().DownloadString(baseUrlGC + address.Replace(" ", "+") 
                + plusUrl);//concatenate URL with the input address and downloads the requested resource
            GoogleGeoCodeResponse jsonResult = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(json); //deserializing the result to GoogleGeoCodeResponse

            string status = jsonResult.status; // get status 

            string geoLocation = String.Empty;

            if (status == "OK") //check if status is OK
            {
                for (int i = 0; i < jsonResult.results.Length; i++) //loop throught the result 
                {

                    for (var x = 0; x < jsonResult.results[0].address_components.Length; x++)
                    {
                        var addr = jsonResult.results[0].address_components[x];
                        string getStreetNumber;
                        string getRoute;
                        string getLocality;
                        string getAdministrative_1;
                        string getCountry;
                        string getPostalCode;

                        if (addr.types[0] == "street_number")
                        {
                            getStreetNumber = addr.long_name;
                            geoLocation = "AptNo : " + getStreetNumber + ", \n" ;
                        }
                        if (addr.types[0] == "route")
                        {
                            getRoute = addr.long_name;
                            geoLocation += "Street : " + getRoute + ", \n";
                        }
                        if (addr.types[0] == "locality")
                        {
                            getLocality = addr.long_name;
                            geoLocation += "State : " + getLocality + ", \n";
                        }
                        if (addr.types[0] == "administrative_area_level_1")
                        {
                            getAdministrative_1 = addr.long_name;
                            geoLocation += "Section : " + getAdministrative_1 + ", \n";
                        }
                        if (addr.types[0] == "country")
                        {
                            getCountry = addr.long_name;
                            geoLocation += "Country : " + getCountry + ", \n";
                        }
                        if (addr.types[0] == "postal_code")
                        {
                            getPostalCode = addr.long_name;
                            geoLocation += "Postal Code : " + getPostalCode + ", \n";
                        }
                    }                   
            }
                return geoLocation; //return result
            }
            else 
            {
                return status; //return status / error if not OK
            }
        }
    }
}
