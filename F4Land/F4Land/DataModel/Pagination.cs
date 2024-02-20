namespace RealEstateAuction.DataModel
{
    public class Pagination
    {
        const int maxRecordPerPage = 50;
        public int PageNumber { get; set; } = 1;

        public static int _recordPerPage = 5;
        public int RecordPerPage
        {
            get { return _recordPerPage; }
            set { _recordPerPage = (value > maxRecordPerPage) ? maxRecordPerPage : value; }
        }
    }
}
