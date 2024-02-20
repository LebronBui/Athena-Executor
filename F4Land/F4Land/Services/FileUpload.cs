namespace RealEstateAuction.Services
{
    public class FileUpload
    {
        public static string UploadImageProduct(IFormFile file)
        {
            try
            {
                //generate random name for image to advoid duplicate name file using uuid
                Guid newGuid = Guid.NewGuid();
                string uuidString = newGuid.ToString();

                //cretae path to save image 
                var folderName = Path.Combine("wwwroot", "img");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                //check path image eixst or not
                bool exists = System.IO.Directory.Exists(pathToSave);
                if (!exists)
                    //if not create image in wwwroot
                    System.IO.Directory.CreateDirectory(pathToSave);
                //validate if there is empty file or not
                if (file.Length > 0)
                {
                    //create file name with extension
                    var fileName = newGuid + Path.GetExtension(file.FileName);

                    //create path to save url to database
                    var fullPath = Path.Combine(pathToSave, fileName);

                    //create path to upload image to folder
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(dbPath, FileMode.OpenOrCreate))
                    {
                        //upload image to folder
                        file.CopyTo(stream);
                    }

                    //return the path of image
                    return "~/img" + "/" + fileName;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
