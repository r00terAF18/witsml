//----------------------------------------------------------------------- 
// PDS.Witsml.Server, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Energistics.DataAccess.WITSML131;
using Energistics.DataAccess.WITSML131.ComponentSchemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDS.Framework;
using PDS.Witsml.Data.Trajectories;

namespace PDS.Witsml.Server.Data.Trajectories
{
    /// <summary>
    /// Trajectory131DataAdapterGetTests
    /// </summary>
    [TestClass]
    public partial class Trajectory131DataAdapterGetTests : Trajectory131TestBase
    {
        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Header_Return_Elements_All()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory without stations
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(Trajectory);

            DevKit.AssertNames(result, Trajectory);
            Assert.AreEqual(Trajectory.ServiceCompany, result.ServiceCompany);
            Assert.IsNotNull(result.CommonData);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Data_Return_Elements_All()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory without stations
            Trajectory.TrajectoryStation = DevKit.TrajectoryStations(5, 0, inCludeExtra: true);
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(Trajectory);

            DevKit.AssertNames(result, Trajectory);
            Assert.AreEqual(Trajectory.ServiceCompany, result.ServiceCompany);

            AssertTrajectoryStations(Trajectory.TrajectoryStation, result.TrajectoryStation);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Header_Return_Elements_Id_Only()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory without stations
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(Trajectory, optionsIn: OptionsIn.ReturnElements.IdOnly);

            DevKit.AssertNames(result, Trajectory);
            Assert.IsNull(result.ServiceCompany);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Header_Return_Elements_Default()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory without stations
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(Trajectory, optionsIn: string.Empty);

            DevKit.AssertNames(result);
            Assert.IsNull(result.ServiceCompany);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Header_Return_Elements_Requested()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory without stations
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var queryIn = string.Format(BasicXMLTemplate, Trajectory.UidWell, Trajectory.UidWellbore, Trajectory.Uid, "<serviceCompany />");
            var results = DevKit.Query<TrajectoryList, Trajectory>(ObjectTypes.Trajectory, queryIn, null, OptionsIn.ReturnElements.Requested);
            var result = results.FirstOrDefault();

            Assert.IsNotNull(result);
            DevKit.AssertNames(result);
            Assert.AreEqual(Trajectory.ServiceCompany, result.ServiceCompany);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Data_Return_Elements_Requested()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory with stations
            Trajectory.TrajectoryStation = DevKit.TrajectoryStations(5, 0, inCludeExtra: true);
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var result = DevKit.GetAndAssertWithXml(BasicXMLTemplate, Trajectory, "<serviceCompany /><trajectoryStation />", optionsIn: OptionsIn.ReturnElements.Requested);

            DevKit.AssertNames(result);
            Assert.AreEqual(Trajectory.ServiceCompany, result.ServiceCompany);

            AssertTrajectoryStations(Trajectory.TrajectoryStation, result.TrajectoryStation, true);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Data_Return_Elements_Data_Only()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory with stations
            Trajectory.TrajectoryStation = DevKit.TrajectoryStations(5, 0, inCludeExtra: true);
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(Trajectory, optionsIn: OptionsIn.ReturnElements.DataOnly);

            DevKit.AssertNames(result);
            Assert.IsNull(result.ServiceCompany);

            AssertTrajectoryStations(Trajectory.TrajectoryStation, result.TrajectoryStation, true);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Data_Return_Elements_Station_Location_Only()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory with stations
            Trajectory.TrajectoryStation = DevKit.TrajectoryStations(5, 0, inCludeExtra: true);
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(Trajectory, optionsIn: OptionsIn.ReturnElements.DataOnly);

            DevKit.AssertNames(result);
            Assert.IsNull(result.ServiceCompany);

            AssertTrajectoryStations(Trajectory.TrajectoryStation, result.TrajectoryStation);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Data_By_Md_Min()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory with stations
            Trajectory.TrajectoryStation = DevKit.TrajectoryStations(20, 10, inCludeExtra: true);
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            const int start = 15;
            var query = new Trajectory
            {
                Uid = Trajectory.Uid,
                UidWell = Trajectory.UidWell,
                UidWellbore = Trajectory.UidWellbore,
                MDMin = new MeasuredDepthCoord { Uom = Trajectory131Generator.MdUom, Value = start }
            };
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(query, queryByExample: true);

            DevKit.AssertNames(result, Trajectory);

            var stations = Trajectory.TrajectoryStation.Where(s => s.MD.Value >= start).ToList();
            AssertTrajectoryStations(stations, result.TrajectoryStation);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Can_Retrieve_Data_By_Md_Min_Max()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory with stations
            Trajectory.TrajectoryStation = DevKit.TrajectoryStations(20, 10, inCludeExtra: true);
            DevKit.AddAndAssert(Trajectory);

            // Get trajectory
            const int start = 15;
            const int end = 20;
            var query = new Trajectory
            {
                Uid = Trajectory.Uid,
                UidWell = Trajectory.UidWell,
                UidWellbore = Trajectory.UidWellbore,
                MDMin = new MeasuredDepthCoord { Uom = Trajectory131Generator.MdUom, Value = start },
                MDMax = new MeasuredDepthCoord { Uom = Trajectory131Generator.MdUom, Value = end }
            };
            var result = DevKit.GetAndAssert<TrajectoryList, Trajectory>(query, queryByExample: true);

            DevKit.AssertNames(result, Trajectory);

            var stations = Trajectory.TrajectoryStation.Where(s => s.MD.Value >= start && s.MD.Value <= end).ToList();
            AssertTrajectoryStations(stations, result.TrajectoryStation);
        }

        [TestMethod]
        public void Trajectory131DataAdapter_GetFromStore_Filters_Results_With_No_Data()
        {
            // Add well and wellbore
            AddParents();

            // Add trajectory with stations
            Trajectory.TrajectoryStation = DevKit.TrajectoryStations(20, 10, inCludeExtra: true);
            DevKit.AddAndAssert(Trajectory);

            // Query end range before the trajectory structure
            var end = 9;
            var query = new Trajectory
            {
                Uid = Trajectory.Uid,
                UidWell = Trajectory.UidWell,
                UidWellbore = Trajectory.UidWellbore,
                MDMax = new MeasuredDepthCoord { Uom = Trajectory131Generator.MdUom, Value = end }
            };
            DevKit.GetAndAssert<TrajectoryList, Trajectory>(query, queryByExample: true, isNotNull: false);

            // Query start range after the trajectory structure
            var start = 100;
            query = new Trajectory
            {
                Uid = Trajectory.Uid,
                UidWell = Trajectory.UidWell,
                UidWellbore = Trajectory.UidWellbore,
                MDMin = new MeasuredDepthCoord { Uom = Trajectory131Generator.MdUom, Value = start },
            };
            DevKit.GetAndAssert<TrajectoryList, Trajectory>(query, queryByExample: true, isNotNull: false);

            // Query range outside the trajectory structure
            start = 2;
            end = 5;
            query = new Trajectory
            {
                Uid = Trajectory.Uid,
                UidWell = Trajectory.UidWell,
                UidWellbore = Trajectory.UidWellbore,
                MDMin = new MeasuredDepthCoord { Uom = Trajectory131Generator.MdUom, Value = start },
                MDMax = new MeasuredDepthCoord { Uom = Trajectory131Generator.MdUom, Value = end }
            };
            DevKit.GetAndAssert<TrajectoryList, Trajectory>(query, queryByExample: true, isNotNull: false);
        }

        private void AssertTrajectoryStations(List<TrajectoryStation> stations, List<TrajectoryStation> results, bool fullStation = false)
        {
            Assert.AreEqual(stations.Count, results.Count);

            foreach (var station in stations)
            {
                var result = results.FirstOrDefault(s => s.Uid == station.Uid);
                Assert.IsNotNull(result);
                Assert.AreEqual(station.TypeTrajStation, result.TypeTrajStation);
                Assert.AreEqual(station.MD?.Value, result.MD?.Value);
                Assert.AreEqual(station.Tvd?.Value, result.Tvd?.Value);
                Assert.AreEqual(station.Azi?.Value, result.Azi?.Value);
                Assert.AreEqual(station.Incl?.Value, result.Incl?.Value);
                Assert.AreEqual(station.DateTimeStn.ToUnixTimeMicroseconds(), result.DateTimeStn.ToUnixTimeMicroseconds());

                if (!fullStation)
                    continue;

                Assert.AreEqual(station.Mtf?.Value, result.Mtf?.Value);
                Assert.AreEqual(station.MDDelta?.Value, result.MDDelta?.Value);
                Assert.AreEqual(station.StatusTrajStation, result.StatusTrajStation);
            }
        }
    }
}