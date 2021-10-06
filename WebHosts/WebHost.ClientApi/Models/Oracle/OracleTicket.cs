using System;

namespace WebHost.ClientApi.Models.Oracle
{
    public class OracleTicket
    {
        public string MatrixUrl { get; set; }
        public string Ticket { get; set; }
        public string Datacenter { get; set; }
        public OperatorOverride OperatorOverride { get; set; }
        public Guid SessionId { get; set; }
        public string Hostname { get; set; }
        public string Country { get; set; }
    }
}