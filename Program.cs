using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using Camera_Trafikverket;

namespace Trafikverket_Camera
{
    class Program
    {
        static void Main(string[] args)
        {

            WebClient webClient = new WebClient();

            webClient.UploadStringCompleted += (obj, arguments) =>
            {
                if (arguments.Cancelled == true)
                {
                    Console.WriteLine("Request Cancelled by user");
                }
                else if (arguments.Error != null)
                {
                    Console.WriteLine(arguments.Error.Message);
                    Console.WriteLine("Request Failed");
                }
                else
                {
                    //Skickar resultat till funktion saveCamera   
                    var cameraList = saveCamera(arguments.Result);
                    //Lista med CameraObject räkans i function.
                    var cameraCount = cameraCounting(cameraList);

                    Console.WriteLine("Camera count: " + cameraCount.cct + " / " + cameraCount.ccd9 + " with direction 90'");
                    Console.WriteLine("Cameras without direction: " + cameraCount.ccdn);
                    Console.WriteLine("Done");
                }
            };

            try
            {
                Uri address = new Uri("https://api.trafikinfo.trafikverket.se/v2/data.xml");
                string requestBody =
                        "<REQUEST>" +
                            "<LOGIN authenticationkey ='34eb91629cf24525857f0dcc59ab3aaf'/>" +
                                "<QUERY objecttype = 'Camera' schemaversion = '1'>" +
                                    "<FILTER>" +
                                    "</FILTER>" +
                                "</QUERY>" +
                        "</REQUEST>";


                webClient.Headers["Content-Type"] = "text/xml";
                webClient.UploadStringAsync(address, "POST", requestBody);

            }
            catch (UriFormatException)
            {
                Console.WriteLine("Malformed url, press X to exit");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("An error occured, press X to exit.");
            }

            char keychar = ' ';
            while (keychar != 'X')
            {
                keychar = Char.ToUpper(Console.ReadKey().KeyChar);
                if (keychar == 'C')
                {
                    webClient.CancelAsync();
                }
            }
        }

        // XML => List<Camera>
        public static List<Camera> saveCamera(string xmlString)
        {

            List<Camera> cameraList = new List<Camera>();
            var rootElement = XElement.Parse(xmlString);

            if (rootElement != null)
            {
                foreach (var item in rootElement.Descendants("Camera"))
                {
                    if (item != null)
                    {
                        var camId = item.Element("Id").Value.ToString();
                        var camName = item.Element("Name").Value.ToString();
                        var camDirection = "666";

                        if (item.Element("Direction") != null)
                        {
                            camDirection = item.Element("Direction").Value.ToString();
                        }

                        Camera cam = new Camera { Id = camId, Name = camName, Diriction = Int32.Parse(camDirection) };
                        cameraList.Add(cam);
                    }
                }
            }
            return (cameraList);
        }

        // Count List<Camera>
        public static (int cct, int ccd9, int ccdn) cameraCounting(List<Camera> cameraList)
        {
            var countCameraTotal = cameraList.Count;
            var countCameraDirection90 = 0;
            var countCameraDirectionNull = 0;

            foreach (var item in cameraList)
            {
                if (item.Diriction == 90)
                {
                    countCameraDirection90++;

                }
                else if (item.Diriction == 666)
                {
                    countCameraDirectionNull++;
                }
            }
            return (countCameraTotal, countCameraDirection90, countCameraDirectionNull);
        }
    }
}
