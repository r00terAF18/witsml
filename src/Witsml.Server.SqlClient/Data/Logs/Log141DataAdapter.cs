﻿//----------------------------------------------------------------------- 
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
using System.ComponentModel.Composition;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using PDS.Witsml.Server.Configuration;
using PDS.Witsml.Server.Properties;

namespace PDS.Witsml.Server.Data.Logs
{
    [Export(typeof(IWitsmlDataAdapter<Log>))]
    [Export(typeof(IWitsml141Configuration))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Log141DataAdapter : SqlWitsmlDataAdapter<Log>, IWitsml141Configuration
    {
        private static readonly int MaxDataNodes = Settings.Default.MaxDataNodes;
        private static readonly int MaxDataPoints = Settings.Default.MaxDataPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log141DataAdapter" /> class.
        /// </summary>
        /// <param name="databaseProvider">The database provider.</param>
        [ImportingConstructor]
        public Log141DataAdapter(IDatabaseProvider databaseProvider) : base(databaseProvider, ObjectNames.Log141)
        {
        }

        /// <summary>
        /// Gets the supported capabilities for the <see cref="Log"/> object.
        /// </summary>
        /// <param name="capServer">The capServer instance.</param>
        public void GetCapabilities(CapServer capServer)
        {
            if (!IsObjectMappingAvailable) return;

            var dataObject = new ObjectWithConstraint(ObjectTypes.Log)
            {
                MaxDataNodes = MaxDataNodes,
                MaxDataPoints = MaxDataPoints
            };

            capServer.Add(Functions.GetFromStore, dataObject);
            //capServer.Add(Functions.AddToStore, dataObject);
            //capServer.Add(Functions.UpdateInStore, dataObject);
            //capServer.Add(Functions.DeleteFromStore, ObjectTypes.Log);
        }

        /// <summary>
        /// Retrieves data objects from the data store using the specified parser.
        /// </summary>
        /// <param name="parser">The query template parser.</param>
        /// <returns>
        /// A collection of data objects retrieved from the data store.
        /// </returns>
        public override List<Log> Query(WitsmlQueryParser parser)
        {
            if (OptionsIn.RequestObjectSelectionCapability.True.Equals(parser.RequestObjectSelectionCapability()))
            {
                return CreateQueryTemplateList();
            }

            var logs = base.Query(parser);

            if (parser.IncludeLogData())
            {
                ValidateGrowingObjectDataRequest(parser, logs);

                logs.ForEach(l =>
                {
                    var logHeader = GetLogHeader(l);
                    var mnemonics = GetMnemonicList(logHeader, parser);

                    QueryLogDataValues(l, logHeader, parser, mnemonics);
                    FormatLogHeader(l, mnemonics);
                });
            }
            else if (!OptionsIn.RequestObjectSelectionCapability.True.Equals(parser.RequestObjectSelectionCapability()))
            {
                logs.ForEach(l =>
                {
                    var logHeader = GetLogHeader(l);
                    var mnemonics = GetMnemonicList(logHeader, parser);
                    FormatLogHeader(l, mnemonics);
                });
            }

            return logs;
        }

        private Log GetLogHeader(Log log)
        {
            return log;
        }

        private IDictionary<int, string> GetMnemonicList(object log, WitsmlQueryParser parser)
        {
            return new Dictionary<int, string>();
        }

        private void QueryLogDataValues(Log log, Log logHeader, WitsmlQueryParser parser, IDictionary<int, string> mnemonics)
        {
        }

        private void FormatLogHeader(Log log, IDictionary<int, string> mnemonics)
        {
        }

        protected override List<Log> CreateQueryTemplateList()
        {
            return new Log()
            {
                UidWell = "abc",
                NameWell = "abc",
                UidWellbore = "abc",
                NameWellbore = "abc",
                Uid = "abc",
                Name = "abc"
            }
            .AsList();
        }
    }
}
