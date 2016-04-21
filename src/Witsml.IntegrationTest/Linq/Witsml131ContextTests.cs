﻿//----------------------------------------------------------------------- 
// PDS.Witsml, 2016.1
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

using System;
using System.Linq;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML131;
using Energistics.DataAccess.WITSML131.ComponentSchemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.Witsml.Linq
{
    [TestClass]
    public class Witsml131ContextTests
    {
        private const string WitsmlStoreUrl = "http://localhost/Witsml.Web/WitsmlStore.svc";
        //private const string WitsmlStoreUrl = "http://localhost:5050/WitsmlStore.svc";

        private Witsml131Context _context;

        [TestInitialize]
        public void TestSetUp()
        {
            _context = new Witsml131Context(WitsmlStoreUrl);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _context.Dispose();
        }

        [TestMethod]
        public void Get_server_version()
        {
            var response = _context.Connection.GetVersion();
            Console.WriteLine(response);
        }

        [TestMethod]
        public void Get_server_capabilities()
        {
            var response = _context.Connection.GetCap<CapServers>();
            Console.WriteLine(WitsmlParser.ToXml(response));
        }

        [TestMethod]
        public void Query_for_all_wells()
        {
            foreach (var well in _context.Wells.With(OptionsIn.ReturnElements.IdOnly))
            {
                Console.WriteLine("{0} - Uid: {1}; Name: {2}", well.GetType().Name, well.Uid, well.Name);
            }
        }

        [TestMethod]
        public void Query_for_well_by_uid()
        {
            var well = _context.Wells.GetByUid(Guid.NewGuid().ToString());
            Console.WriteLine("Result: {0}", well);
        }

        [TestMethod]
        public void Query_for_well_by_name()
        {
            foreach (var well in _context.Wells.With(OptionsIn.ReturnElements.All).Where(w => w.Name == "Well 1"))
            {
                Console.WriteLine("{0} - Uid: {1}; Name: {2}", well.GetType().Name, well.Uid, well.Name);
            }
        }

        [TestMethod]
        public void Query_for_all_wellbores()
        {
            foreach (var well in _context.Wellbores.With(OptionsIn.ReturnElements.IdOnly))
            {
                Console.WriteLine("{0} - Uid: {1}; Name: {2}", well.GetType().Name, well.Uid, well.Name);
            }
        }

        [TestMethod]
        public void Query_for_wellbores_by_well_name()
        {
            foreach (var wellbore in _context.Wellbores.Where(w => w.NameWell == "Well 0"))
            {
                Console.WriteLine("{0} - Uid: {1}; Name: {2}", wellbore.GetType().Name, wellbore.Uid, wellbore.Name);
            }
        }

        [TestMethod]
        public void Query_for_well_hiererchy()
        {
            foreach (var well in _context.Wells.With(OptionsIn.ReturnElements.IdOnly))
            {
                Console.WriteLine("{0} - Uid: {1}; Name: {2}", well.GetType().Name, well.Uid, well.Name);

                foreach (var wellbore in _context.Wellbores.With(OptionsIn.ReturnElements.IdOnly).Where(x => x.UidWell == well.Uid))
                {
                    Console.WriteLine("  {0} - Uid: {1}; Name: {2}", wellbore.GetType().Name, wellbore.Uid, wellbore.Name);

                    foreach (var log in _context.Logs.With(OptionsIn.ReturnElements.IdOnly).Where(x => x.UidWell == well.Uid && x.UidWellbore == wellbore.Uid))
                    {
                        Console.WriteLine("    {0} - Uid: {1}; Name: {2}", log.GetType().Name, log.Uid, log.Name);
                    }
                }
            }
        }

        [TestMethod]
        public void Query_for_log_header()
        {
            var log = _context.Logs
                .Include(x => x.LogCurveInfo = _context.One<LogCurveInfo>())
                .With(OptionsIn.ReturnElements.HeaderOnly)
                .Where(x => x.NameWell == "Well 0" && x.NameWellbore == "Wellbore 0-1" && x.Name == "Depth Log 0-1")
                .FirstOrDefault();

            Console.WriteLine("{0} - Uid: {1}; Name: {2}", log.GetType().Name, log.Uid, log.Name);
        }

        //[TestMethod]
        //public void Query_for_log_data()
        //{
        //    var log = _context.Logs
        //        .Include(x => x.LogData = _context.One<LogData>())
        //        .With(OptionsIn.ReturnElements.DataOnly)
        //        .Where(x => x.NameWell == "Well 0" && x.NameWellbore == "Wellbore 0-1" && x.Name == "Time Log 0-1" &&
        //                    x.StartIndex == new GenericMeasure(0, "m") && x.EndIndex == new GenericMeasure(1000, "m"))
        //        .FirstOrDefault();

        //    Console.WriteLine("{0} - Uid: {1}; Name: {2}", log.GetType().Name, log.Uid, log.Name);
        //}
    }
}