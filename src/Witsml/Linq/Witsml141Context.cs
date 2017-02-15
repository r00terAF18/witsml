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

using System.Collections.Generic;
using System.Linq;
using System.Security;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.Datatypes;

namespace PDS.Witsml.Linq
{
    /// <summary>
    /// Manages the context for version 141 of WITSML connections and data.
    /// </summary>
    /// <seealso cref="PDS.Witsml.Linq.WitsmlContext" />
    public class Witsml141Context : WitsmlContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Witsml141Context"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="timeoutInMinutes">The timeout in minutes.</param>
        public Witsml141Context(string url, double timeoutInMinutes = 1.5)
            : base(url, timeoutInMinutes, WMLSVersion.WITSML141)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Witsml141Context"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="timeoutInMinutes">The timeout in minutes.</param>
        public Witsml141Context(string url, string username, string password, double timeoutInMinutes = 1.5)
            : base(url, username, password, timeoutInMinutes, WMLSVersion.WITSML141)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Witsml141Context"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="timeoutInMinutes">The timeout in minutes.</param>
        public Witsml141Context(string url, string username, SecureString password, double timeoutInMinutes = 1.5)
            : base(url, username, password, timeoutInMinutes, WMLSVersion.WITSML141)
        {
        }

        /// <summary>
        /// Gets the data schema version.
        /// </summary>
        public override string DataSchemaVersion => OptionsIn.DataVersion.Version141.Value;

        /// <summary>
        /// Gets the wells.
        /// </summary>
        public IWitsmlQuery<Well> Wells => CreateQuery<Well, WellList>();

        /// <summary>
        /// Gets the wellbores.
        /// </summary>
        public IWitsmlQuery<Wellbore> Wellbores => CreateQuery<Wellbore, WellboreList>();

        /// <summary>
        /// Gets the rigs.
        /// </summary>
        public IWitsmlQuery<Rig> Rigs => CreateQuery<Rig, RigList>();

        /// <summary>
        /// Gets the logs.
        /// </summary>
        public IWitsmlQuery<Log> Logs => CreateQuery<Log, LogList>();

        /// <summary>
        /// Gets the trajectories.
        /// </summary>
        public IWitsmlQuery<Trajectory> Trajectories => CreateQuery<Trajectory, TrajectoryList>();

        /// <summary>
        /// Gets all wells.
        /// </summary>
        /// <returns>The wells.</returns>
        public override IEnumerable<IDataObject> GetAllWells()
        {
            return Wells
                .With(OptionsIn.ReturnElements.IdOnly)
                .ToList() // execute query before sorting
                .OrderBy(x => x.Name);
        }

        /// <summary>
        /// Gets the wellbores.
        /// </summary>
        /// <param name="uri">The parent URI.</param>
        /// <returns>The wellbores.</returns>
        public override IEnumerable<IWellObject> GetWellbores(EtpUri uri)
        {
            return Wellbores
                .With(OptionsIn.ReturnElements.IdOnly)
                .Where(x => x.UidWell == uri.ObjectId)
                .ToList() // execute query before sorting
                .OrderBy(x => x.Name);
        }
    }
}
