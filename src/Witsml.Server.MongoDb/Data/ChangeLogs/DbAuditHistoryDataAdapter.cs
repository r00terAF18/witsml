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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Energistics.Datatypes;
using LinqToQuerystring;
using PDS.Framework;
using PDS.Witsml.Data.ChangeLogs;
using PDS.Witsml.Server.Configuration;

namespace PDS.Witsml.Server.Data.ChangeLogs
{
    /// <summary>
    /// Data adapter that encapsulates CRUD functionality for <see cref="DbAuditHistory" />
    /// </summary>
    /// <seealso cref="PDS.Witsml.Server.Data.MongoDbDataAdapter{DbAuditHistory}" />
    [Export(typeof(IWitsmlDataAdapter<DbAuditHistory>))]
    [Export(typeof(IWitsml141Configuration))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DbAuditHistoryDataAdapter : MongoDbDataAdapter<DbAuditHistory>, IWitsml141Configuration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbAuditHistoryDataAdapter" /> class.
        /// </summary>
        /// <param name="container">The composition container.</param>
        /// <param name="databaseProvider">The database provider.</param>
        [ImportingConstructor]
        public DbAuditHistoryDataAdapter(IContainer container, IDatabaseProvider databaseProvider)
            : base(container, databaseProvider, "dbAuditHistory")
        {
            Logger.Debug("Instance created.");
        }

        /// <summary>
        /// Gets the supported capabilities for the <see cref="DbAuditHistory"/> object.
        /// </summary>
        /// <param name="capServer">The capServer instance.</param>
        public void GetCapabilities(CapServer capServer)
        {
            Logger.DebugFormat("Getting the supported capabilities for ChangeLog data version {0}.", capServer.Version);

            capServer.Add(Functions.GetFromStore, ObjectTypes.ChangeLog);
        }

        /// <summary>
        /// Retrieves data objects from the data store using the specified parser.
        /// </summary>
        /// <param name="parser">The query template parser.</param>
        /// <param name="context">The response context.</param>
        /// <returns>A collection of data objects retrieved from the data store.</returns>
        public override List<DbAuditHistory> Query(WitsmlQueryParser parser, ResponseContext context)
        {
            var entities = base.Query(parser, context);
            var returnElements = parser.ReturnElements();

            // Only return full changeHistory if requested
            if (!OptionsIn.ReturnElements.All.Equals(returnElements) && !parser.Contains("changeHistory"))
            {
                entities.ForEach(x => x.ChangeHistory = null);
            }

            return entities;
        }

        /// <summary>
        /// Gets or creates the audit history for the changed entity.
        /// </summary>
        /// <param name="uri">The URI of the changed entity.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="changeType">Type of the change.</param>
        /// <returns>A new or existing DbAuditHistory for the entity.</returns>
        public DbAuditHistory GetAuditHistory(EtpUri uri, object entity, ChangeInfoType changeType)
        {
            var abstractObject = entity as Energistics.DataAccess.WITSML200.AbstractObject;
            var commonDataObject = entity as ICommonDataObject;
            var dataObject = entity as IDataObject;
            var wellObject = entity as IWellObject;
            var wellboreObject = entity as IWellboreObject;

            var uriLower = uri.Uri.ToLowerInvariant();
            var changeInfo = $"{changeType:G} {uri.ObjectType}";
            var auditHistory = GetQuery().FirstOrDefault(x => x.Uri == uriLower);

            // Creating audit history entry
            if (auditHistory == null)
            {
                auditHistory = new DbAuditHistory
                {
                    ObjectType = uri.ObjectType,
                    LastChangeInfo = changeInfo,
                    LastChangeType = changeType,
                    CommonData = new CommonData(),
                    ChangeHistory = new List<ChangeHistory>(),
                    SourceName = commonDataObject?.CommonData?.SourceName ?? abstractObject?.Citation?.Originator,
                    UidWellbore = wellboreObject?.UidWellbore,
                    UidWell = wellObject?.UidWell ?? wellboreObject?.UidWell,
                    UidObject = uri.ObjectId,
                    Uri = uriLower
                };
            }
            else // Updating existing entry
            {
                auditHistory.LastChangeInfo = changeInfo;
                auditHistory.LastChangeType = changeType;
            }

            // Keep audit history name properties in sync with the entity name properties.
            auditHistory.NameWellbore = wellboreObject?.NameWellbore;
            auditHistory.NameWell = wellObject?.NameWell ?? wellboreObject?.NameWell;
            auditHistory.NameObject = dataObject?.Name ?? abstractObject?.Citation?.Title;

            // Keep date/time of last change in sync with the entity
            auditHistory.CommonData.DateTimeLastChange = GetDateTimeLastChange(
                commonDataObject?.CommonData?.DateTimeLastChange ?? 
                (DateTimeOffset?)abstractObject?.Citation?.LastUpdate, 
                changeType);

            // Make sure date/time created matches first date/time last change
            auditHistory.CommonData.DateTimeCreation = auditHistory.CommonData.DateTimeCreation ??
                                                       auditHistory.CommonData.DateTimeLastChange;

            // Update current ChangeHistory entry to match the ChangeLog header
            var changeHistory = GetCurrentChangeHistory();
            changeHistory.ChangeInfo = auditHistory.LastChangeInfo;
            changeHistory.ChangeType = auditHistory.LastChangeType;
            changeHistory.DateTimeChange = auditHistory.CommonData.DateTimeLastChange;

            // Append current ChangeHistory entry
            auditHistory.ChangeHistory.Add(changeHistory);

            // Remove ChangeHistory entry from current context
            WitsmlOperationContext.Current.ChangeHistory = null;

            return auditHistory;
        }

        /// <summary>
        /// Gets or creates the change history entry for the current operation.
        /// </summary>
        /// <returns>The <see cref="ChangeHistory"/> entry for the current operation.</returns>
        public ChangeHistory GetCurrentChangeHistory()
        {
            return WitsmlOperationContext.Current.ChangeHistory ??
                   (WitsmlOperationContext.Current.ChangeHistory = new ChangeHistory { Uid = Guid.NewGuid().ToString() });
        }

        /// <summary>
        /// Gets a collection of data objects related to the specified URI.
        /// </summary>
        /// <param name="parentUri">The parent URI.</param>
        /// <returns>A collection of data objects.</returns>
        public override List<DbAuditHistory> GetAll(EtpUri? parentUri)
        {
            Logger.DebugFormat("Fetching all ChangeLogs; Parent URI: {0}", parentUri);

            return GetAllQuery(parentUri)
                .OrderBy(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{DbAuditHistory}" /> instance to by used by the GetAll method.
        /// </summary>
        /// <param name="parentUri">The parent URI.</param>
        /// <returns>An executable query.</returns>
        protected override IQueryable<DbAuditHistory> GetAllQuery(EtpUri? parentUri)
        {
            var query = GetQuery().AsQueryable();

            if (parentUri != null)
            {
                //var uidWellbore = parentUri.Value.ObjectId;
                //query = query.Where(x => x.Wellbore.Uuid == uidWellbore);

                if (!string.IsNullOrWhiteSpace(parentUri.Value.Query))
                    query = query.LinqToQuerystring(parentUri.Value.Query);
            }

            return query;
        }

        /// <summary>
        /// Gets a list of the property names to project during a query.
        /// </summary>
        /// <param name="parser">The WITSML parser.</param>
        /// <returns>A list of property names.</returns>
        protected override List<string> GetProjectionPropertyNames(WitsmlQueryParser parser)
        {
            var returnElements = parser.ReturnElements();

            return OptionsIn.ReturnElements.IdOnly.Equals(returnElements)
                ? new List<string> { IdPropertyName, NamePropertyName, "UidWell", "NameWell", "UidWellbore", "NameWellbore", "UidObject", "NameObject" }
                : OptionsIn.ReturnElements.Requested.Equals(returnElements)
                ? new List<string>()
                : null;
        }

        /// <summary>
        /// Audits the entity. Override this method to adjust the audit record
        /// before it is submitted to the database or to prevent the audit.
        /// </summary>
        /// <param name="auditHistory">The audit history.</param>
        /// <param name="exists">if set to <c>true</c> the entry exists.</param>
        protected override void AuditEntity(DbAuditHistory auditHistory, bool exists)
        {
            // Excluding DbAuditHistory from audit history
        }

        /// <summary>
        /// Gets the date time last change based on the current change type.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="changeType">The type of change.</param>
        /// <returns>The date time of last change.</returns>
        private DateTimeOffset? GetDateTimeLastChange(DateTimeOffset? date, ChangeInfoType changeType)
        {
            if (changeType == ChangeInfoType.delete)
            {
                date = null;
            }

            return date ?? DateTimeOffset.UtcNow;
        }
    }
}