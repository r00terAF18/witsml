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

using System;
using System.ComponentModel.Composition;
using System.Data;
using Energistics.DataAccess;
using PDS.Framework;
using PDS.Witsml.Server.Models;

namespace PDS.Witsml.Server.Converters
{
    /// <summary>
    /// Converts timestamps to and from provider specific data types.
    /// </summary>
    /// <seealso cref="PDS.Witsml.Server.Converters.DbValueConverter" />
    [ExportType(typeof(Timestamp), typeof(IDbValueConverter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TimestampConverter : DbValueConverter
    {
        /// <summary>
        /// Converts the supplied value from a provider specific data type.
        /// </summary>
        /// <param name="mapping">The data object mapping.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnValue">The column value.</param>
        /// <returns>The converted value.</returns>
        public override object ConvertFromDb(ObjectMapping mapping, IDataReader dataReader, string columnName, object columnValue)
        {
            if (columnValue == null) return null;

            if (columnValue.GetType().IsNumeric())
            {
                var unix = Convert.ToInt64(columnValue);
                if (unix < 0) return null;

                return new Timestamp(DateTimeOffset.FromUnixTimeSeconds(unix));
            }

            return new Timestamp(Convert.ToDateTime(columnValue));
        }
    }
}
