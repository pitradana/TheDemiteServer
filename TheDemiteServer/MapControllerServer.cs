﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace TheDemiteServer
{
    class MapControllerServer
    {
        private string format = "geojson";                                                                                 // format data of mapzen vector tile
        //private string url = "http://167.205.7.235:8080/geoserver/gwc/service/tms/1.0.0/mapproject%3Abdg_planet_osm_roads@EPSG%3A900913@geojson/{0}/{1}/{2}.{3}";
        //private string urlPolygon = "http://167.205.7.235:8080/geoserver/gwc/service/tms/1.0.0/mapproject%3Abandung_planet_osm_polygon_polygons@EPSG%3A900913@geojson/{0}/{1}/{2}.{3}";
        //private string urlPoint = "http://167.205.7.235:8080/geoserver/gwc/service/tms/1.0.0/mapproject%3Abdg_planet_osm_point@EPSG%3A900913@geojson/{0}/{1}/{2}.{3}";
        //private string mapzenApiKey = "mapzen-KhT9o6J";                                                                 // mapzen api key. get the api key by sign up to mapzen
        //private string mapzenUrl = "http://tile.mapzen.com/mapzen/vector/v1/{0}/{1}/{2}/{3}.{4}?api_key={5}";           // mapzen vector tile url. 0 => layers, 1 => zoom level, 2 => x tile coordinate, 3 => y tile coordinate, 4 => vector tile data format, 5 => mapzen api key
        private int zoom = 18;

        private string url = "http://vectormap.pptik.id:8080/geoserver/gwc/service/tms/1.0.0/map%3Abandung_planet_osm_line_lines@EPSG%3A900913@geojson/{0}/{1}/{2}.{3}";
        private string urlLine = "http://vectormap.pptik.id:8080/geoserver/gwc/service/tms/1.0.0/map%3Abandung_planet_osm_line_lines@EPSG%3A900913@geojson/{0}/{1}/{2}.{3}";
        private string urlPolygon = "http://vectormap.pptik.id:8080/geoserver/gwc/service/tms/1.0.0/map%3Abandung_planet_osm_polygon_polygons@EPSG%3A900913@geojson/{0}/{1}/{2}.{3}";
        private string urlPoint = "http://vectormap.pptik.id:8080/geoserver/gwc/service/tms/1.0.0/map%3Abandung_planet_osm_point_points@EPSG%3A900913@geojson/{0}/{1}/{2}.{3}";


        private float centerMercatorX;
        private float centerMercatorY;
        private float posX;
        private float posY;
        private int tileX;
        private int tileY;

        private float centerPosX;
        private float centerPosY;

        private HttpClient http;
        //private dynamic completeMapData;
        private ListMapData listMapData;
        private bool mapReady;

        private bool firstAcess;

        public MapControllerServer()
        {
            this.centerMercatorX = float.MinValue;
            this.centerMercatorY = float.MinValue;
            this.posX = float.MinValue;
            this.posY = float.MinValue;
            this.tileX = int.MinValue;
            this.tileY = int.MinValue;

            this.centerPosX = float.MinValue;
            this.centerPosY = float.MinValue;

            this.http = new HttpClient();

            this.listMapData = new ListMapData();
            this.listMapData.listBuildingData = new List<BuildingData>();
            this.listMapData.listRoadData = new List<RoadData>();
            this.mapReady = false;

            this.firstAcess = true;
        }

        public bool ConvertLocationAndCheck(float latitude, float longitude)
        {
            bool result = false;

            float[] mercator = GeoConverter.GeoCoorToMercatorProjection(latitude, longitude);
            float[] pixel = GeoConverter.MercatorProjectionToPixel(mercator, zoom);
            int[] tile = GeoConverter.PixelToTileCoordinate(pixel);

            if (firstAcess)
            {
                centerMercatorX = mercator[0];
                centerMercatorY = mercator[1];
                firstAcess = false;
            }

            if (tile[0] == tileX && tile[1] == tileY)
            {
                result = false;
            }
            else
            {
                centerMercatorX = mercator[0];
                centerMercatorY = mercator[1];

                tileX = tile[0];
                tileY = tile[1];
                result = true;
            }

            this.posX = mercator[0] - centerMercatorX;
            this.posY = mercator[1] - centerMercatorY;
            this.centerPosX = centerMercatorX; //mercator[0];
            this.centerPosY = centerMercatorY; //mercator[1];

            return result;
        }

        public async void CreateMap()
        {
            this.mapReady = false;

            //access road
            string urlRequest = string.Format(url, zoom.ToString(), tileX.ToString(), tileY.ToString(), format);
            dynamic mapData = await this.ProcessMapData(urlRequest);
            this.ConvertRoadData(mapData, "road");

            urlRequest = string.Format(urlLine, zoom.ToString(), tileX.ToString(), tileY.ToString(), format);
            mapData = await this.ProcessMapData(urlRequest);
            this.ConvertRoadData(mapData, "line");

            //access polygon
            urlRequest = string.Format(urlPolygon, zoom.ToString(), tileX.ToString(), tileY.ToString(), format);
            mapData = await this.ProcessMapData(urlRequest);
            this.ConvertBuildingData(mapData);

            //acess point
            urlRequest = string.Format(urlPoint, zoom.ToString(), tileX.ToString(), tileY.ToString(), format);
            mapData = await this.ProcessMapData(urlRequest);
            this.ConvertPOIData(mapData);

            this.mapReady = true;
        }

        private async Task<dynamic> ProcessMapData(string url)
        {
            var response = await this.http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            dynamic mapData = JsonConvert.DeserializeObject(result);

            return mapData;
        }

        private void ConvertBuildingData(dynamic building)
        {
            this.listMapData.listBuildingData.Clear();

            for (int i = 0; i < building.features.Count; i++)
            {
                var tempData = building.features[i];
                //Debug.WriteLine(tempData.properties.code);
                if (tempData.properties.building != "")
                {
                    if (tempData.geometry.type == "Polygon")
                    {
                        BuildingData buildingData = new BuildingData();
                        buildingData.listCoordinate = new List<Coordinate>();

                        for (int j = 0; j < tempData.geometry.coordinates[0].Count; j++)
                        {
                            var coordinate = tempData.geometry.coordinates[0][j];


                            //float[] mercator = GeoConverter.GeoCoorToMercatorProjection((float)Convert.ToDouble(coordinate[1]), (float)Convert.ToDouble(coordinate[0]));
                            /*
                            float tempX = mercator[0] - this.centerMercatorX;
                            float tempY = mercator[1] - this.centerMercatorY;
                            */

                            float tempX = (float)Convert.ToDouble(coordinate[0]) - this.centerMercatorX;
                            float tempY = (float)Convert.ToDouble(coordinate[1]) - this.centerMercatorY;

                            //coordinate[1] = tempX;
                            //coordinate[0] = tempY;
                            Coordinate coor = new Coordinate();
                            coor.latitude = tempX;
                            coor.longitude = tempY;
                            buildingData.listCoordinate.Add(coor);
                        }

                        buildingData.buildingName = tempData.properties.name;

                        buildingData.buildingCode = tempData.properties.code;
                        this.listMapData.listBuildingData.Add(buildingData);

                        //add centeroid of polygon
                        BuildingData centeroidData = new BuildingData();
                        centeroidData.listCoordinate = new List<Coordinate>();

                        float[] centeroid = this.FindCenteroid(buildingData.listCoordinate);
                        Coordinate coorCenteroid = new Coordinate();
                        coorCenteroid.latitude = centeroid[0];
                        coorCenteroid.longitude = centeroid[1];
                        centeroidData.listCoordinate.Add(coorCenteroid);
                        centeroidData.buildingName = tempData.properties.name;
                        this.listMapData.listBuildingData.Add(centeroidData);
                    }

                    if (tempData.geometry.type == "Point")
                    {
                        BuildingData buildingData = new BuildingData();
                        buildingData.listCoordinate = new List<Coordinate>();

                        var coordinate = tempData.geometry.coordinates;
                        float[] mercator = GeoConverter.GeoCoorToMercatorProjection((float)Convert.ToDouble(coordinate[1]), (float)Convert.ToDouble(coordinate[0]));
                        float tempX = mercator[0] - this.centerMercatorX;
                        float tempY = mercator[1] - this.centerMercatorY;
                        //coordinate[1] = tempX;
                        //coordinate[0] = tempY;
                        Coordinate coor = new Coordinate();
                        coor.latitude = tempX;
                        coor.longitude = tempY;
                        buildingData.listCoordinate.Add(coor);

                        buildingData.buildingName = tempData.properties.name;
                        this.listMapData.listBuildingData.Add(buildingData);
                    }
                }
            }
        }

        private float[] FindCenteroid(List<Coordinate> poly)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                float temp = poly[i].latitude + poly[j].longitude - poly[j].latitude * poly[i].longitude;
                accumulatedArea += temp;
                centerX += (poly[i].latitude + poly[j].latitude) * temp;
                centerY += (poly[i].longitude + poly[j].longitude) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
                return null; //avoid division by zero

            accumulatedArea *= 3f;
            return new float[] { centerX / accumulatedArea, centerY / accumulatedArea };
        }

        private void ConvertRoadData(dynamic road, string type)
        {
            if (type == "road")
            {
                this.listMapData.listRoadData.Clear();
            }

            for (int i = 0; i < road.features.Count; i++)
            {
                var tempData = road.features[i];
                if (tempData.geometry.type == "LineString")
                {
                    RoadData roadData = new RoadData();
                    roadData.listCoordinate = new List<Coordinate>();

                    for (int j = 0; j < tempData.geometry.coordinates.Count; j++)
                    {
                        var coordinate = tempData.geometry.coordinates[j];
                        /*
                        float[] mercator = GeoConverter.GeoCoorToMercatorProjection((float)Convert.ToDouble(coordinate[1]), (float)Convert.ToDouble(coordinate[0]));
                        float tempX = mercator[0] - this.centerMercatorX;
                        float tempY = mercator[1] - this.centerMercatorY;
                        */
                        //Console.WriteLine(coordinate[0] + " ===latitude=== " + this.centerMercatorX);
                        //Console.WriteLine(coordinate[1] + " ===longitude=== " + this.centerMercatorY);
                        float tempX = (float)Convert.ToDouble(coordinate[0]) - this.centerMercatorX;
                        float tempY = (float)Convert.ToDouble(coordinate[1]) - this.centerMercatorY;
                        //Console.WriteLine(tempX + " ======= " + tempY);
                        //coordinate[1] = tempX;
                        //coordinate[0] = tempY;

                        Coordinate coor = new Coordinate();
                        coor.latitude = tempX;
                        coor.longitude = tempY;
                        roadData.listCoordinate.Add(coor);
                    }

                    roadData.roadName = tempData.properties.name;
                    this.listMapData.listRoadData.Add(roadData);
                }
            }
        }

        private void ConvertPOIData(dynamic poi)
        {
            if (this.listMapData.listBuildingData == null)
            {
                this.listMapData.listBuildingData = new List<BuildingData>();
            }

            for (int i = 0; i < poi.features.Count; i++)
            {
                var tempData = poi.features[i];

                BuildingData buildingData = new BuildingData();
                buildingData.listCoordinate = new List<Coordinate>();

                var coordinate = tempData.geometry.coordinates;
                float[] mercator = GeoConverter.GeoCoorToMercatorProjection((float)coordinate[1], (float)coordinate[0]);
                /*
                float tempX = mercator[0] - this.centerMercatorX;
                float tempY = mercator[1] - this.centerMercatorY;
                */

                float tempX = (float)Convert.ToDouble(coordinate[0]) - this.centerMercatorX;
                float tempY = (float)Convert.ToDouble(coordinate[1]) - this.centerMercatorY;

                //coordinate[1] = tempX;
                //coordinate[0] = tempY;
                Coordinate coor = new Coordinate();
                coor.latitude = tempX;
                coor.longitude = tempY;
                buildingData.listCoordinate.Add(coor);

                buildingData.buildingName = tempData.properties.name;
                this.listMapData.listBuildingData.Add(buildingData);
            }
        }

        
        public List<Coordinate> StartRoute(float latitude, float longitude, string destination)
        {
            RouteManagement route = new RouteManagement(this.centerMercatorX, this.centerMercatorY, latitude, longitude, this.http);
            route.StartRouting(destination);

            while (!route.GetRoutingDone()) ;

            return route.GetFinalRoute();
        }
        

        public void SetPosX(float posX)
        {
            this.posX = posX;
        }

        public float GetPosX()
        {
            return this.posX;
        }

        public void SetPosY(float posY)
        {
            this.posY = posY;
        }

        public float GetPosY()
        {
            return this.posY;
        }

        public void SetTileX(int tileX)
        {
            this.tileX = tileX;
        }

        public int GetTileX()
        {
            return this.tileX;
        }

        public void SetTileY(int tileY)
        {
            this.tileY = tileY;
        }

        public int GetTileY()
        {
            return this.tileY;
        }

        public void SetListMapData(ListMapData listMapData)
        {
            this.listMapData = listMapData;
            //this.completeMapData = completeMapData;
        }

        public dynamic GetListMapData()
        {
            return this.listMapData;
            //return this.completeMapData;
        }

        public void SetMapReady(bool mapReady)
        {
            this.mapReady = mapReady;
        }

        public bool GetMapReady()
        {
            return this.mapReady;
        }

        public float GetCenterPosX()
        {
            return this.centerPosX;
        }

        public float GetCenterPosY()
        {
            return this.centerPosY;
        }
    }
}
