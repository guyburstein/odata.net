//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation. All rights reserved.  
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace Microsoft.Data.Spatial
{
    using System.Collections.ObjectModel;
    using Microsoft.Spatial;

    /// <summary>
    /// Helper methods for the geography type.
    /// </summary>
    internal static class GeographyHelperMethods
    {
        /// <summary>
        /// Sends the current spatial object to the given pipeline with a figure that represents this LineString
        /// </summary>
        /// <param name="lineString">GeographyLineString instance to serialize.</param>
        /// <param name="pipeline">The pipeline to populate to</param>
        internal static void SendFigure(this GeographyLineString lineString, GeographyPipeline pipeline)
        {
            ReadOnlyCollection<GeographyPoint> points = lineString.Points;
            for (int i = 0; i < points.Count; ++i)
            {
                if (i == 0)
                {
                    pipeline.BeginFigure(new GeographyPosition(points[i].Latitude, points[i].Longitude, points[i].Z, points[i].M));
                }
                else
                {
                    pipeline.LineTo(new GeographyPosition(points[i].Latitude, points[i].Longitude, points[i].Z, points[i].M));
                }
            }

            if (points.Count > 0)
            {
                pipeline.EndFigure();
            }
        }
    }
}
