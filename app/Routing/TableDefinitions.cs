using System.Collections.Generic;

namespace Routing
{
    public class TableDefinitions
    {
        private readonly List<Table> _allTables = new List<Table>();

        public TableDefinitions()
        {
            CreateTable("depots", "depotId (int), depotName, postCode, fleetSize (int)");
            CreateTable("drivers", "driverId (int),  depotId (int),  driverName, truckSize (int), userName, apiKey");
            CreateTable("routes",
                "depotId (int), driverId (int), routeDate, routeSeqNo (int), addressId (int), routeAction, itemType, status, refNo from routes");
            CreateTable("postcodes",
                "postcodeId (int), postcode, latitude (dec), longitude (dec)");
        }

        private void CreateTable(string tableName, string fieldString)
        {
            _allTables.Add(new Table(tableName, fieldString));
        }

        public Table FetchTable(string tableName)
        {
            return _allTables.Find(table => table.TableName == tableName);
        }
    }
}