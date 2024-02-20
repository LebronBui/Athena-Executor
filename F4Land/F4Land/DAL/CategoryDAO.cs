using RealEstateAuction.Models;

namespace RealEstateAuction.DAL
{
    public class CategoryDAO
    {
        public static List<Category> GetCategories()
        {
            using (RealEstateContext db = new RealEstateContext())
            {
                return db.Categories.ToList();
            }
        }

        public static Category GetCategoryById(int id)
        {
            using (RealEstateContext db = new RealEstateContext())
            {
                return db.Categories.Find(id);
            }
        }


    }
}
