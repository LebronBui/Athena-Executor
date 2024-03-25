using RealEstateAuction.Models;

namespace RealEstateAuction.DAL
{
    public class BankDAO
    {
        private readonly RealEstateContext context;
        public BankDAO()
        {
            context = new RealEstateContext();
        }

        public List<Banking> listBankings()
        {
            return context.Bankings.ToList();
        }

        public Banking bankDetail(int id)
        {
            return context.Bankings.SingleOrDefault(b => b.Id == id);
        }
    }
}
