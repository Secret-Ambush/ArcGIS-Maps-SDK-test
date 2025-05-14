//   Copyright 2022 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI;
using System.Threading.Tasks;

namespace QueryAFeatureLayerSQL
{

    class MapViewModel : INotifyPropertyChanged
    {

        public MapViewModel()
        {
            SetupMap();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Map? _map;
        public Map? Map
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        private SelectionProperties? _selectionProps;
        public SelectionProperties? SelectionProps { 
            get { return _selectionProps; }
            set 
            { 
                _selectionProps = value; 
                OnPropertyChanged(); 
            }
        }

        private void SetupMap()
        {
            // Create a new map with a 'topographic vector' basemap.
            Map map = new Map(BasemapStyle.ArcGISTopographic);

            // Set the initial viewpoint around the Santa Monica Mountains in California.
            var mapCenterPoint = new MapPoint(-118.805, 34.027, SpatialReferences.Wgs84);
            map.InitialViewpoint = new Viewpoint(mapCenterPoint, 100000);

            // Set the view model's Map property with the map.
            Map = map;

            // Add a layer that shows parcels for Los Angeles County.
            Uri parcelsUri = new Uri("https://services3.arcgis.com/GVgbJbqm8hXASVYi/arcgis/rest/services/LA_County_Parcels/FeatureServer/0");
            FeatureLayer parcelsFeatureLayer = new FeatureLayer(parcelsUri); 
            
            // Give the layer an ID so we can easily find it later, then add it to the map.
            parcelsFeatureLayer.Id = "Parcels"; 
            Map.OperationalLayers.Add(parcelsFeatureLayer);

            // Create selection properties (bound to the MapView).
            SelectionProperties selectionProps = new SelectionProperties();
            selectionProps.Color = System.Drawing.Color.Yellow;
            this.SelectionProps = selectionProps;

        }

        public async Task QueryFeatureLayer(string layerId, string whereExpression, Envelope queryExtent)
        {
            // Get the layer based on its Id.
            var featureLayerToQuery = _map?.OperationalLayers[layerId] as FeatureLayer;

            // Get the feature table from the feature layer.
            var featureTableToQuery = featureLayerToQuery?.FeatureTable;
            if(featureTableToQuery == null) { return; }
            // Clear any existing selection.
            featureLayerToQuery?.ClearSelection();

            // Create the query parameters using the where expression and extent passed in.
            QueryParameters queryParams = new QueryParameters
            {
                Geometry = queryExtent,
                ReturnGeometry = true,
                WhereClause = whereExpression,
            };

            // Query the table and get the list of features in the result.
            var queryResult = await featureTableToQuery.QueryFeaturesAsync(queryParams);

            // Loop over each feature from the query result.
            foreach (Feature feature in queryResult)
            {
                // Select each feature.
                featureLayerToQuery!.SelectFeature(feature);
            }
        }

    }
}
