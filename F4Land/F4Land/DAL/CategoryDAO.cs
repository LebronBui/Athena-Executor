using RealEstateAuction.Models;

namespace RealEstateAuction.DAL
{
    public class CategoryDAO
    {
        public List<Category> GetCategories()
        {
            using (RealEstateContext db = new RealEstateContext())
            {
                return db.Categories
                    .Where(c => c.DeleteFlag == false)
                    .ToList();
            }
        }

        public Category GetCategoryById(int id)
        {
            using (RealEstateContext db = new RealEstateContext())
            {
                return db.Categories.Find(id);
            }
        }


    }
}
