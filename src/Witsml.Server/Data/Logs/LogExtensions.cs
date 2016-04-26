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
using System.Linq;
using PDS.Framework;
using Witsml131 = Energistics.DataAccess.WITSML131;
using Witsml141 = Energistics.DataAccess.WITSML141;

namespace PDS.Witsml.Server.Data.Logs
{
    /// <summary>
    /// Provides common helper methods for Log data objects.
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// Ensures the index curve is in the first position.
        /// </summary>
        /// <param name="list">The list of log curves.</param>
        /// <param name="mnemonic">The index curve mnemonic.</param>
        public static void MoveToFirst(this List<Witsml131.ComponentSchemas.LogCurveInfo> list, string mnemonic)
        {
            if (list == null || !list.Any() || string.IsNullOrWhiteSpace(mnemonic)) return;
            if (list[0].Mnemonic.EqualsIgnoreCase(mnemonic)) return;

            var indexCurve = list.FirstOrDefault(x => mnemonic.EqualsIgnoreCase(x.Mnemonic));
            if (indexCurve == null) return;

            list.Remove(indexCurve);
            list.Insert(0, indexCurve);
        }

        /// <summary>
        /// Ensures the index curve is in the first position.
        /// </summary>
        /// <param name="list">The list of log curves.</param>
        /// <param name="mnemonic">The index curve mnemonic.</param>
        public static void MoveToFirst(this List<Witsml141.ComponentSchemas.LogCurveInfo> list, string mnemonic)
        {
            if (list == null || !list.Any() || string.IsNullOrWhiteSpace(mnemonic)) return;
            if (list[0].Mnemonic.Value.EqualsIgnoreCase(mnemonic)) return;

            var indexCurve = list.FirstOrDefault(x => mnemonic.EqualsIgnoreCase(x.Mnemonic?.Value));
            if (indexCurve == null) return;

            list.Remove(indexCurve);
            list.Insert(0, indexCurve);
        }

        /// <summary>
        /// Gets the log curve information mnemonics.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetLogCurveInfoMnemonics(this WitsmlQueryParser parser)
        {
            var mnemonics = Enumerable.Empty<string>();
            var logCurveInfos = parser.Properties("logCurveInfo").ToArray();

            if (logCurveInfos.Any())
            {
                var mnemonicList = parser.Properties(logCurveInfos, "mnemonic").ToArray();

                if (mnemonicList.Any())
                {
                    mnemonics = mnemonicList.Select(x => x.Value);
                }
            }

            return mnemonics;
        }

        /// <summary>
        /// Gets the log data mnemonics.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetLogDataMnemonics(this WitsmlQueryParser parser)
        {
            var mnemonics = Enumerable.Empty<string>();
            var logData = parser.Property("logData");

            if (logData != null)
            {
                var mnemonicList = parser.Properties(logData, "mnemonicList")
                    .Take(1)
                    .ToArray();

                if (mnemonicList.Any())
                {
                    mnemonics = mnemonicList.First().Value.Split(',');
                }
            }

            return mnemonics;
        }

    }
}