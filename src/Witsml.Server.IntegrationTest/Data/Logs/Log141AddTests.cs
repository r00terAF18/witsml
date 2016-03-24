﻿using System.Collections.Generic;
using System.Linq;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.Witsml.Server.Data.Logs
{
    [TestClass]
    public class Log141AddTests
    {
        private DevKit141Aspect DevKit;
        private Well _well;
        private Wellbore _wellbore;

        [TestInitialize]
        public void TestSetUp()
        {
            DevKit = new DevKit141Aspect();

            DevKit.Store.CapServerProviders = DevKit.Store.CapServerProviders
                .Where(x => x.DataSchemaVersion == OptionsIn.DataVersion.Version141.Value)
                .ToArray();

            _well = new Well { Name = DevKit.Name("Well 01"), TimeZone = DevKit.TimeZone };
            _wellbore = new Wellbore()
            {
                NameWell = _well.Name,
                Name = DevKit.Name("Wellbore 01")
            };
        }

        [TestMethod]
        public void Log_can_be_added_with_depth_data()
        {
            var response = DevKit.Add<WellList, Well>(_well);

            _wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(_wellbore);

            var log = new Log()
            {
                UidWell = _wellbore.UidWell,
                NameWell = _well.Name,
                UidWellbore = response.SuppMsgOut,
                NameWellbore = _wellbore.Name,
                Name = DevKit.Name("Log 01")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10);
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);
        }

        [TestMethod]
        public void Log_can_be_added_with_time_data()
        {
            var response = DevKit.Add<WellList, Well>(_well);

            _wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(_wellbore);

            var log = new Log()
            {
                UidWell = _wellbore.UidWell,
                NameWell = _well.Name,
                UidWellbore = response.SuppMsgOut,
                NameWellbore = _wellbore.Name,
                Name = DevKit.Name("Log 01")
            };

            DevKit.InitHeader(log, LogIndexType.datetime);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10, 1, false);
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);
        }

        [TestMethod]
        public void Test_append_log_data()
        {
            var response = DevKit.Add<WellList, Well>(_well);

            _wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(_wellbore);

            var log = new Log()
            {
                UidWell = _wellbore.UidWell,
                NameWell = _well.Name,
                UidWellbore = response.SuppMsgOut,
                NameWellbore = _wellbore.Name,
                Name = DevKit.Name("Log 01"),
                StartIndex = new GenericMeasure(5, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10);
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;
            log = new Log()
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog,
                StartIndex = new GenericMeasure(17, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6);
            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = new Log
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog
            };

            var results = DevKit.Query<LogList, Log>(query, optionsIn: "returnElements=all");
            Assert.AreEqual(1, results.Count);
            var result = results.First();
            var logData = result.LogData.FirstOrDefault();
            Assert.IsNotNull(logData);
            Assert.AreEqual(16, logData.Data.Count);
        }

        [TestMethod]
        public void Test_prepend_log_data()
        {
            var response = DevKit.Add<WellList, Well>(_well);

            _wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(_wellbore);

            var log = new Log()
            {
                UidWell = _wellbore.UidWell,
                NameWell = _well.Name,
                UidWellbore = response.SuppMsgOut,
                NameWellbore = _wellbore.Name,
                Name = DevKit.Name("Log 01"),
                StartIndex = new GenericMeasure(17, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10);
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;
            log = new Log()
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog,
                StartIndex = new GenericMeasure(5, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6);
            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = new Log
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog
            };

            var results = DevKit.Query<LogList, Log>(query, optionsIn: "returnElements=all");
            Assert.AreEqual(1, results.Count);
            var result = results.First();
            var logData = result.LogData.FirstOrDefault();
            Assert.IsNotNull(logData);
            Assert.AreEqual(16, logData.Data.Count);
        }

        [TestMethod]
        public void Test_update_overlapping_log_data()
        {
            var response = DevKit.Add<WellList, Well>(_well);

            _wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(_wellbore);

            var log = new Log()
            {
                UidWell = _wellbore.UidWell,
                NameWell = _well.Name,
                UidWellbore = response.SuppMsgOut,
                NameWellbore = _wellbore.Name,
                Name = DevKit.Name("Log 01"),
                StartIndex = new GenericMeasure(1, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 8);
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;
            log = new Log()
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog,
                StartIndex = new GenericMeasure(4.1, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 3, 0.9);
            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = new Log
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog
            };

            var results = DevKit.Query<LogList, Log>(query, optionsIn: "returnElements=all");
            Assert.AreEqual(1, results.Count);
            var result = results.First();
            var logData = result.LogData.FirstOrDefault();
            Assert.IsNotNull(logData);
            Assert.AreEqual(9, logData.Data.Count);
        }

        [TestMethod]
        public void Test_overwrite_log_data_chunk()
        {
            var response = DevKit.Add<WellList, Well>(_well);

            _wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(_wellbore);

            var log = new Log()
            {
                UidWell = _wellbore.UidWell,
                NameWell = _well.Name,
                UidWellbore = response.SuppMsgOut,
                NameWellbore = _wellbore.Name,
                Name = DevKit.Name("Log 01"),
                StartIndex = new GenericMeasure(17, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6);
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;
            log = new Log()
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog,
                StartIndex = new GenericMeasure(4.1, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 3, 0.9);
            var logData = log.LogData.First();
            logData.Data.Add("21.5, 1, 21.7");
            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = new Log
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog
            };

            var results = DevKit.Query<LogList, Log>(query, optionsIn: "returnElements=all");
            Assert.AreEqual(1, results.Count);
            var result = results.First();
            logData = result.LogData.FirstOrDefault();
            Assert.IsNotNull(logData);
            Assert.AreEqual(5, logData.Data.Count);
        }

        [TestMethod]
        public void Test_update_log_data_with_different_range_for_each_channel()
        {
            var response = DevKit.Add<WellList, Well>(_well);

            _wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(_wellbore);

            var log = new Log()
            {
                UidWell = _wellbore.UidWell,
                NameWell = _well.Name,
                UidWellbore = response.SuppMsgOut,
                NameWellbore = _wellbore.Name,
                Name = DevKit.Name("Log 01"),
                StartIndex = new GenericMeasure(15, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 8);
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;
            log = new Log()
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog,
                StartIndex = new GenericMeasure(13, "m")
            };

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6, 0.9);
            var logData = log.LogData.First();
            logData.Data.Clear();
            logData.Data.Add("13,13.1,");
            logData.Data.Add("14,14.1,");
            logData.Data.Add("15,15.1,");
            logData.Data.Add("16,16.1,");
            logData.Data.Add("17,17.1,");
            logData.Data.Add("20,20.1,20.2");
            logData.Data.Add("21,,21.2");
            logData.Data.Add("22,,22.2");
            logData.Data.Add("23,,23.2");
            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = new Log
            {
                UidWell = _wellbore.UidWell,
                UidWellbore = uidWellbore,
                Uid = uidLog
            };

            var results = DevKit.Query<LogList, Log>(query, optionsIn: "returnElements=all");
            Assert.AreEqual(1, results.Count);
            var result = results.First();
            logData = result.LogData.FirstOrDefault();
            Assert.IsNotNull(logData);
            Assert.AreEqual(11, logData.Data.Count);
            var data = logData.Data;
            Assert.AreEqual("15,15.1,15", data[2]);
        }
    }
}
